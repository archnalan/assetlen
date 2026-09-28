using System.Globalization;
using System.Text;
using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.FileProcessingServices.Extraction;

/// <summary>
/// Extraction by Claude, behind the same interface as the rules — and never
/// trusted further than they are.
/// <para>
/// Off unless the deployment sets both an API key and <c>Extraction:UseClaude</c>:
/// sending a client's thread to an external service is the owner's decision
/// (works-report.md §6.4). Author names are not sent, only which side spoke.
/// </para>
/// <para>
/// Every item is checked against the message it cites before it may become a
/// proposal: the quote must be in the message, a figure must be one the money
/// reader finds there, a date must be one the date reader resolves there.
/// Numbers never come from the model (works-report.md §2). A window that fails,
/// or is refused, falls back to the rules, so extraction still works with the
/// model switched off — Law 0 applied to Law 3.
/// </para>
/// </summary>
public sealed class ClaudeMessageExtractor : IMessageExtractor
{
    private const int WindowSize = 60;

    private readonly ILogger<ClaudeMessageExtractor> _logger;
    private readonly string? _apiKey;
    private readonly bool _enabled;
    private readonly string _model;

    public ClaudeMessageExtractor(IConfiguration config, ILogger<ClaudeMessageExtractor> logger)
    {
        _logger = logger;
        _apiKey = config["Anthropic:ApiKey"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        _enabled = bool.TryParse(config["Extraction:UseClaude"], out var on) && on;
        _model = config["Extraction:Model"] ?? "claude-sonnet-5-5";
    }

    public string Engine => $"claude:{_model}";

    public bool IsAvailable => _enabled && !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<ExtractionOutput> ExtractAsync(IReadOnlyList<ExtractionInput> messages, CancellationToken ct = default)
    {
        var output = new ExtractionOutput();

        // Readings are numbers, and numbers come from the rules.
        output.Readings.AddRange(RuleMessageExtractor.Extract(messages).Readings);

        var client = new AnthropicClient { ApiKey = _apiKey };
        var texts = messages.Where(m => !m.IsSystem && !string.IsNullOrWhiteSpace(m.Body)
                                        && !m.Body!.Contains("<Media omitted>", StringComparison.OrdinalIgnoreCase)).ToList();

        for (var start = 0; start < texts.Count; start += WindowSize)
        {
            var window = texts.Skip(start).Take(WindowSize).ToList();
            try
            {
                var items = await AskAsync(client, window, ct);
                output.Candidates.AddRange(Validate(items, window, output.Notes));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Claude extraction failed for a window of {Count}; using the rules for it", window.Count);
                output.Notes.Add($"Model unavailable for {window.Count} messages from {window[0].SentAt:d MMM}; the rules read them instead.");
                output.Candidates.AddRange(RuleMessageExtractor.Extract(window).Candidates);
            }
        }

        return output;
    }

    private async Task<List<ModelItem>> AskAsync(AnthropicClient client, List<ExtractionInput> window, CancellationToken ct)
    {
        var transcript = new StringBuilder();
        foreach (var m in window)
            transcript.Append('[').Append(m.Id).Append("] ")
                      .Append(m.SentAt.ToString("yyyy-MM-dd ddd HH:mm", CultureInfo.InvariantCulture))
                      .Append(" | ").Append(m.AuthorSide?.ToString() ?? "Unknown").Append(" | ")
                      .AppendLine(m.Body!.Replace('\n', ' '));

        var response = await client.Messages.Create(new MessageCreateParams
        {
            Model = _model,
            MaxTokens = 16000,
            System = SystemPrompt,
            OutputConfig = new OutputConfig
            {
                Format = new JsonOutputFormat { Schema = Schema() }
            },
            Messages = [new() { Role = Role.User, Content = transcript.ToString() }]
        }, ct);

        if (response.StopReason == "refusal")
            throw new InvalidOperationException("The model declined this window.");

        var json = string.Concat(response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));
        var parsed = JsonSerializer.Deserialize<ModelReply>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return parsed?.Items ?? new List<ModelItem>();
    }

    private static IEnumerable<ExtractionCandidate> Validate(List<ModelItem> items, List<ExtractionInput> window, List<string> notes)
    {
        var byId = window.ToDictionary(m => m.Id);
        var rejected = 0;

        foreach (var item in items)
        {
            if (item.MessageId is null || !byId.TryGetValue(item.MessageId, out var message)) { rejected++; continue; }
            if (!Enum.TryParse<ProposalKind>(item.Kind, true, out var kind)) { rejected++; continue; }

            var body = message.Body ?? "";
            var quote = item.Quote?.Trim() ?? "";
            if (quote.Length < 3 || !RuleMessageExtractor.Normalise(body).Contains(RuleMessageExtractor.Normalise(quote)))
            {
                rejected++;
                continue;
            }

            // A figure is kept only if the money reader finds that same figure in the message.
            decimal? amount = null;
            string? currency = null;
            if (decimal.TryParse(item.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var said))
            {
                var hit = MoneyReader.FindAll(body).FirstOrDefault(h => h.Amount == said);
                if (hit is not null) { amount = hit.Amount; currency = hit.Currency; }
            }

            DateTime? due = null;
            string? dateText = null;
            if (DateTime.TryParseExact(item.DueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var claimed))
            {
                var found = DatePhraseReader.Find(quote, message.SentAt) ?? DatePhraseReader.Find(body, message.SentAt);
                if (found is not null && found.Date == claimed.Date) { due = found.Date; dateText = found.Text; }
            }

            // A title may not carry a number the message does not.
            var title = (item.Title ?? quote).Trim();
            if (title.Where(char.IsDigit).Any() && !DigitsAppearIn(title, body)) title = quote;
            if (title.Length > 200) title = title[..199] + "…";

            yield return new ExtractionCandidate
            {
                MessageId = message.Id,
                Kind = kind,
                Title = title,
                Amount = amount,
                Currency = currency,
                DueDate = due,
                DateText = dateText,
                Maturity = Enum.TryParse<CommitmentMaturity>(item.Maturity, true, out var mat) ? mat : CommitmentMaturity.Agreed,
                OwedBySide = Enum.TryParse<ProjectSide>(item.OwedBy, true, out var owed) ? owed : null,
                PartyName = string.IsNullOrWhiteSpace(item.Party) ? null : item.Party.Trim(),
                Rule = "claude",
                Confidence = 0.7
            };
        }

        if (rejected > 0) notes.Add($"{rejected} model suggestions were dropped for not matching the message they cited.");
    }

    private static bool DigitsAppearIn(string title, string body)
    {
        foreach (var run in System.Text.RegularExpressions.Regex.Matches(title, @"\d+").Select(m => m.Value))
            if (!body.Contains(run, StringComparison.Ordinal)) return false;
        return true;
    }

    private static Dictionary<string, JsonElement> Schema()
    {
        var item = new
        {
            type = "object",
            additionalProperties = false,
            required = new[] { "message_id", "kind", "title", "quote", "amount", "due_date", "maturity", "owed_by", "party" },
            properties = new Dictionary<string, object>
            {
                ["message_id"] = new { type = "string" },
                ["kind"] = new { type = "string", @enum = new[] { "Spec", "Price", "Date", "Material", "Choice", "Blocker" } },
                ["title"] = new { type = "string" },
                ["quote"] = new { type = "string" },
                ["amount"] = new { type = "string" },
                ["due_date"] = new { type = "string" },
                ["maturity"] = new { type = "string", @enum = new[] { "Idea", "InDiscussion", "Agreed", "Delivered" } },
                ["owed_by"] = new { type = "string", @enum = new[] { "", "Client", "Contractor" } },
                ["party"] = new { type = "string" }
            }
        };

        return new Dictionary<string, JsonElement>
        {
            ["type"] = JsonSerializer.SerializeToElement("object"),
            ["additionalProperties"] = JsonSerializer.SerializeToElement(false),
            ["required"] = JsonSerializer.SerializeToElement(new[] { "items" }),
            ["properties"] = JsonSerializer.SerializeToElement(new { items = new { type = "array", items = item } })
        };
    }

    private const string SystemPrompt = """
        You read a construction project's group chat and list what it commits someone to.
        Each line is: [message id] date time | side (Client or Contractor) | text.

        List only four things, plus blockers:
        - money: a price, a payment, a release, a disputed figure (kind Price)
        - materials: a brand, grade, quantity or delivery of a material (kind Material)
        - dates: a promised or demanded completion or start date, including "tomorrow", "by Tuesday", "this week" (kind Date)
        - decisions: a choice made, a specification agreed or changed, or a decision one side is waiting on (kind Choice or Spec)
        - blockers: something stopping work, with who has to move (kind Blocker)

        Acknowledgements, greetings, thanks, progress reports without a commitment, and questions that commit nobody produce nothing.
        "Okay", "Noted", "Good progress" produce nothing. When unsure, leave it out: a wrong item costs more than a missing one.

        For each item give the message id it came from and a quote copied exactly from that message.
        Title: one plain sentence, no more than 20 words, no reassurance, no names.
        amount: digits only in full units (12M becomes 12000000), or "" when the message states none.
        due_date: yyyy-MM-dd resolved against the message's own date, or "" when there is none.
        maturity: Agreed for a statement of what will happen, InDiscussion for a proposal or a contested point, Delivered for something reported delivered.
        owed_by: for a decision one side is waiting on, the side that owes it; otherwise "".
        party: for a blocker, who has to move ("the window team"); otherwise "".
        """;

    private sealed class ModelReply
    {
        public List<ModelItem>? Items { get; set; }
    }

    private sealed class ModelItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("message_id")] public string? MessageId { get; set; }
        public string? Kind { get; set; }
        public string? Title { get; set; }
        public string? Quote { get; set; }
        public string? Amount { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("due_date")] public string? DueDate { get; set; }
        public string? Maturity { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("owed_by")] public string? OwedBy { get; set; }
        public string? Party { get; set; }
    }
}
