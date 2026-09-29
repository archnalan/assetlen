using System.Text;
using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.FileProcessingServices.Report;

/// <summary>
/// The works report's descriptions, drafted by Claude (works-report.md §6) —
/// and never trusted further than <see cref="NarrativeValidator"/> allows.
/// <para>
/// Off unless the deployment sets an API key and <c>Report:UseClaude</c>, and the
/// project's owner has enabled drafting: the facts leave the deployment only on
/// both of those. They are sent already filtered to the reader's side, as
/// snippets, never the thread. Cards go to Sonnet; the cover, which carries the
/// answer to "on time or not, and by how much", goes to Opus. A refusal or an
/// outage falls back to the template, so the report issues regardless (Law 0).
/// </para>
/// </summary>
public sealed class ClaudeReportNarrator : IReportNarrator
{
    private readonly ILogger<ClaudeReportNarrator> _logger;
    private readonly string? _apiKey;
    private readonly bool _enabled;
    private readonly string _cardModel;
    private readonly string _coverModel;

    public ClaudeReportNarrator(IConfiguration config, ILogger<ClaudeReportNarrator> logger)
    {
        _logger = logger;
        _apiKey = config["Anthropic:ApiKey"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        _enabled = bool.TryParse(config["Report:UseClaude"], out var on) && on;
        _cardModel = config["Report:CardModel"] ?? "claude-sonnet-5-5";
        _coverModel = config["Report:CoverModel"] ?? "claude-opus-5-5";
    }

    public string Engine => $"claude:{_cardModel}+{_coverModel}";

    public bool IsAvailable => _enabled && !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<List<NarrativeTargetDto>> DraftAsync(IReadOnlyList<NarrativeRequest> targets, CancellationToken ct = default)
    {
        var client = new AnthropicClient { ApiKey = _apiKey };
        var drafted = new List<NarrativeTargetDto>();

        var cover = targets.Where(t => t.Kind is NarrativeTargetKind.Cover or NarrativeTargetKind.Answer).ToList();
        var cards = targets.Except(cover).ToList();

        foreach (var (group, model) in new[] { (cards, _cardModel), (cover, _coverModel) })
        {
            // Small batches keep one refused or failed call from costing the whole page.
            foreach (var batch in group.Chunk(25))
            {
                try
                {
                    drafted.AddRange(await AskAsync(client, model, batch, ct));
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex, "Report drafting failed for {Count} targets on {Model}; the template stands", batch.Length, model);
                }
            }
        }
        return drafted;
    }

    private static async Task<List<NarrativeTargetDto>> AskAsync(AnthropicClient client, string model, NarrativeRequest[] batch, CancellationToken ct)
    {
        var prompt = new StringBuilder();
        foreach (var t in batch)
        {
            prompt.Append("## target ").Append(t.TargetId).Append(" (").Append(t.Kind).Append(", at most ")
                  .Append(t.MaxSentences).Append(" sentences, ").Append(t.MaxWords).AppendLine(" words)");
            prompt.AppendLine("facts:");
            foreach (var f in t.Facts) prompt.Append("- ").AppendLine(f);
            prompt.AppendLine("sources you may cite:");
            foreach (var id in t.SourceIds) prompt.Append("- ").AppendLine(id);
            if (t.Snippets.Count > 0)
            {
                prompt.AppendLine("source words:");
                foreach (var (id, text) in t.Snippets) prompt.Append("- [").Append(id).Append("] ").AppendLine(text.Replace('\n', ' '));
            }
            prompt.AppendLine();
        }

        var response = await client.Messages.Create(new MessageCreateParams
        {
            Model = model,
            MaxTokens = 16000,
            System = SystemPrompt,
            OutputConfig = new OutputConfig { Format = new JsonOutputFormat { Schema = Schema() } },
            Messages = [new() { Role = Role.User, Content = prompt.ToString() }]
        }, ct);

        if (response.StopReason == "refusal")
            throw new InvalidOperationException("The model declined to draft these descriptions.");

        var json = string.Concat(response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));
        var reply = JsonSerializer.Deserialize<Reply>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return (reply?.Targets ?? new()).Select(t => new NarrativeTargetDto
        {
            TargetId = t.TargetId ?? "",
            Sentences = (t.Sentences ?? new()).Select(s => new NarrativeSentenceDto { Text = s.Text ?? "", SourceIds = s.SourceIds ?? new() }).ToList()
        }).ToList();
    }

    private static Dictionary<string, JsonElement> Schema()
    {
        var sentence = new
        {
            type = "object",
            additionalProperties = false,
            required = new[] { "text", "source_ids" },
            properties = new Dictionary<string, object>
            {
                ["text"] = new { type = "string" },
                ["source_ids"] = new { type = "array", items = new { type = "string" } }
            }
        };
        var target = new
        {
            type = "object",
            additionalProperties = false,
            required = new[] { "target_id", "sentences" },
            properties = new Dictionary<string, object>
            {
                ["target_id"] = new { type = "string" },
                ["sentences"] = new { type = "array", items = sentence }
            }
        };
        return new Dictionary<string, JsonElement>
        {
            ["type"] = JsonSerializer.SerializeToElement("object"),
            ["additionalProperties"] = JsonSerializer.SerializeToElement(false),
            ["required"] = JsonSerializer.SerializeToElement(new[] { "targets" }),
            ["properties"] = JsonSerializer.SerializeToElement(new { targets = new { type = "array", items = target } })
        };
    }

    private const string SystemPrompt = """
        You write the few sentences on a construction works report that a project's funder reads.
        Each target gives facts that are already final, the source ids you may cite, and the words of those sources.

        For every target write at most the sentences and words it allows, and cite at least one of its source ids on every sentence.
        Describe; do not sell. Plain and specific: "Terrazzo strips and screed are down on the terrace; grinding has started at the water tank."
        Never "great progress", never an exclamation, never reassurance.
        Use only numbers, percentages, dates and names that appear in that target's facts or source words.
        Do not forecast, promise, or say who is to blame unless the facts say so.
        For a stage: what changed in the window, then what is outstanding.
        For the cover: a status in three sentences. For the answer: whether the project is on course for its date, and by how much.
        Return every target you were given, using its target id exactly.
        """;

    private sealed class Reply
    {
        public List<ReplyTarget>? Targets { get; set; }
    }

    private sealed class ReplyTarget
    {
        [System.Text.Json.Serialization.JsonPropertyName("target_id")] public string? TargetId { get; set; }
        public List<ReplySentence>? Sentences { get; set; }
    }

    private sealed class ReplySentence
    {
        public string? Text { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("source_ids")] public List<string>? SourceIds { get; set; }
    }
}
