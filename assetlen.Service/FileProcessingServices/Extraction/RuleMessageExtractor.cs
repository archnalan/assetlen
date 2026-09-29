using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using assetlen.Shared.Models.Models.RemoteSite;

namespace assetlen.Service.FileProcessingServices.Extraction;

/// <summary>
/// The deterministic extractor: narrow triggers over money, materials, dates and
/// decisions, and nothing else (assetlen.md Law 3).
/// <para>
/// It is the floor the product stands on with no model and no network — Law 0
/// applied to extraction. Each rule is named on the proposal it produces, because
/// the accept rate is measured per rule and a rule that falls under two-thirds is
/// narrowed rather than trusted (plan.md P5).
/// </para>
/// </summary>
public sealed class RuleMessageExtractor : IMessageExtractor
{
    public const string EngineName = "rules-v1";

    public string Engine => EngineName;

    public Task<ExtractionOutput> ExtractAsync(IReadOnlyList<ExtractionInput> messages, CancellationToken ct = default)
        => Task.FromResult(Extract(messages));

    private const RegexOptions I = RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant;

    // ─── Cleaning ────────────────────────────────────────────────────────

    private static readonly Regex Mention = new("@⁨[^⁩]*⁩|@[\\w.]+", RegexOptions.Compiled);
    private static readonly Regex EditedMarker = new(@"<\s*This message was edited\s*>", I);
    private static readonly Regex Deleted = new(@"^(?:this message was deleted|you deleted this message|<media omitted>|null)$", I);
    private static readonly Regex UrlOnly = new(@"^\s*https?://\S+\s*$", I);
    private static readonly Regex Bullet = new(@"^\s*(?:\d{1,2}[.)]|[-•*–])\s*", RegexOptions.Compiled);
    private static readonly Regex NumberedBullet = new(@"^\s*\d{1,2}[.)]\s*", RegexOptions.Compiled);

    private static readonly Regex SummaryHeader = new(
        @"^(?:quick\s+|site\s+|today'?s\s+|daily\s+)?(?:summary|progress|update|in\s+progress)s?(?:\s+(?:summary|update|updates|activities|progress))*(?:\s+(?:for\s+)?today)?[\s.:!]*$", I);

    private static readonly Regex AckStart = new(
        @"^(?:ok(?:ay)?|noted|thanks?|thank\s+you|good|welcome|nice|great|correct|alright|all\s+right|sure|yes|yeah|no|hello|hi|hey|oh|well\s+done|congratulations|amen|cool|fine|perfect|morning|evening|afternoon)\b", I);

    // ─── Rule vocabularies ───────────────────────────────────────────────

    private static readonly Regex MoneyContext = new(
        @"\b(?:labou?r|cost|costs|costed|price|quote|quoted|quotation|pay|paid|payment|receive|received|sent|send|budget|balance|amount|total|fee|charge|transfer|deposit|invoice|receipt|release|advance|contract|cleared|carried\s+forward|install?ment|asking)\b", I);

    private static readonly Regex Dispute = new(@"\b(?:too\s+(?:high|much|expensive)|expensive|overpriced|reduce\s+(?:it|the)|negotiate|can'?t\s+afford)\b", I);

    private static readonly Regex Hedge = new(@"\b(?:if|hopefully|maybe|perhaps|probably|God\s+willing)\b", I);

    private static readonly Regex FutureCue = new(
        @"\b(?:will|shall|should|going\s+to|gonna|scheduled|schedule|plan(?:ning)?\s+to|expect(?:ed|ing)?|intend|begin|start|starting|commenc(?:e|es|ing)|resume|proceed|ready|complete|completion|finish|done|deliver(?:ed|y)?|due|deadline|cast|casting|coming|confirmed\s+to|set\s+up)\b|'ll\b", I);

    private static readonly Regex ModalFuture = new(@"\b(?:will|shall|should|going\s+to|gonna|scheduled)\b|'ll\b", I);

    private static readonly Regex PastReport = new(@"\b(?:yesterday|was|were|did|had|started|begun|began|have\s+been|has\s+been)\b", I);

    private static readonly Regex MaterialWord = new(
        @"\b(?:cement|sand|aggregates?|hardcore|murram|marrum|ballast|gravel|stones?|blocks?|bricks?|iron\s+bars?|rebars?|steel|timber|nails|pipes?|bitumen|paint|tiles?|terrazz?o|epoxy|alumin(?:i)?um|lime|conduits?|cables?|maxpans?|primer|sealant|grout|materials?)\b", I);

    private static readonly Regex Rebar = new(@"\b(?<n>\d{1,4})\s*(?:x\s*|×\s*|no\.?\s*)?(?<t>[TRY])(?<d>\d{1,2})\b", RegexOptions.Compiled);

    private static readonly Regex Quantity = new(
        @"\b\d+(?:\.\d+)?\s*(?:bags?|trucks?|sinotrucks?|lorr(?:y|ies)|tippers?|tonnes?|tons?|trips?|pieces?|pcs|lengths?|litres?|liters?|drums?|rolls?|sheets?|m2|m3|sqm|cubes?|boxes|cartons?)\b", I);

    private static readonly Regex SpecCue = new(
        @"\b(?:procure|buy|order|supply|use|using|need|needs|should|must|only|heavy[\s-]?duty|machine|grade|class|brand|minimum|at\s*least|atleast|don'?t|dont|do\s+not)\b", I);

    private static readonly Regex ProcureCue = new(@"\b(?:procure|buy|order|supply)\b", I);

    private static readonly Regex DeliveredCue = new(
        @"\b(?:delivered|arrived|on\s+site|at\s+(?:the\s+)?site\s+already|confirmed\s+on\s+site|have\s+been\s+confirmed)\b", I);

    private static readonly Regex ClientDirective = new(
        @"^(?:(?:also|and|but|actually|so|again)\s*,?\s*)?(?:i|we)\s+(?:want|prefer|would\s+(?:like|prefer)|choose|chose|have\s+chosen|have\s+decided|decided)\b(?!\s+to\s+(?:know|see|get|hear|be))(?!.*\b(?:done|completed|finished|closed|updates?)\b)" +
        @"|^(?:please\s+|kindly\s+)?(?:maintain|keep|go\s+with|let'?s\s+go\s+with|proceed\s+with)\b", I);

    private static readonly Regex FinishChoice = new(
        @"\bshould\s+be\s+(?:grey|gray|white|black|cream|beige|brown|matt|matte|gloss|glossy|plain|smooth|rough|\w+\s+colou?r)\b" +
        @"|\b(?:one|single|just\s+one|only\s+one)\s+colou?r\b|\bno\s+(?:black|white|grey|gray|patterns?)\b|\bno\s+changes\b|\bshould\s+be\s+as\s+(?:in|per)\b", I);

    private static readonly Regex AgreedCue = new(
        @"\b(?:we\s+(?:have\s+)?agreed|agreed\s+with|(?:have\s+)?settled\s+on|we\s+(?:have\s+)?decided|chosen|approved|go\s+ahead|finali[sz]ed\s+(?:on\s+)?the)\b", I);

    private static readonly Regex EngineerCleared = new(@"\bengineer\s+(?:has\s+)?(?:confirmed|approved|cleared|advised)\b", I);

    private static readonly Regex SpecChange = new(
        @"\binstead\s+of\b|\bchanging\s+\w+(?:\s+\w+)?\s+(?:into|to)\b|\balterations?\b|\b(?:exceeds|raised|increased|extended|lowered)\s+by\b|\breduced\s+(?:by|to)\b|\bthe\s+proposal\s+(?:is|was)\b|\b(?:added|adding)\s+(?:a|an)\s+(?:new|extra|additional)\b|\binstructed\s+(?:them\s+|the\s+\w+\s+)?to\b", I);

    private static readonly Regex Recommendation = new(@"\b(?:i|we)\s+(?:recommend|suggest|propose)\b|\boption\s+(?:for|of)\b", I);

    private static readonly Regex DecisionOwed = new(
        @"\blet\s+(?:us|me)\s+know\s+if\s+you\s+(?:want|prefer|would)|\bwaiting\s+on\s+you\s+to\s+(?:confirm|decide|choose|approve)|\bshall\s+we\b|\bdo\s+you\s+want\b|\bwould\s+you\s+(?:like|prefer)\b|\byou\s+(?:could|can)\s+choose\b|\b(?:please|kindly)\s+(?:confirm|choose|decide|approve)\b|\byour\s+(?:decision|approval|go-?ahead)\b|\blet\s+(?:us|me)\s+know\s+if\s+(?:this|the|it)\b[^.?!]*\b(?:ok|okay|fine|good|works|covers)\b", I);

    private static readonly Regex BlockerCue = new(
        @"\b(?:danger(?:ous)?|unsafe|hazard(?:ous)?|blocked|stuck|delays?|delayed|postpone(?:d)?|didn'?t\s+show\s+up|did\s+not\s+show\s+up|no[\s-]?show|unable\s+to\s+make\s+it|communication\s+challenge|power\s+cuts?|missing|not\s+(?:yet\s+)?(?:been\s+)?fabricated|not\s+expected\s+in\s+the\s+shipment|slowed|no\s+work|out\s+of\s+stock|shortage)\b", I);

    private static readonly Regex Party = new(
        @"\b(?:the|our|their)\s+(?<p>(?:[A-Za-z]+\s+){0,2}?(?:team|guys|contractor|fixers?|supplier|fabricators?|engineer|machinery|machines))\b|\b(?<p>[A-Za-z]+\s+(?:team|guys))\b", I);

    private static readonly Regex Affirmation = new(@"^(?:yes|yeah|yep|correct|agreed|approved|go\s+ahead|sure|definitely)\b[\s,.!]*", I);

    private static readonly Regex QuestionStart = new(
        @"^\s*(?:why|what|how|where|when|which|who|is\s+(?:it|this|that|there)|are\s+(?:we|you|they)|can\s+(?:you|we|i)|could\s+you|did\s+(?:you|we)|do\s+(?:you|we))\b", I);

    private static readonly Regex WhenQuestion = new(@"\b(?:when|how\s+long|what\s+time|by\s+when)\b", I);

    private static readonly Regex Percent = new(@"(?<p>\d{1,3}(?:\.\d+)?)\s*%", RegexOptions.Compiled);

    private static readonly Regex SubjectFiller = new(
        @"(?:\s+(?:is|are|at|about|around|approximately|roughly|currently|already|now|over|nearly|almost|in\s+progress|progress(?:ing)?))+\s*$", I);

    private static readonly Regex SentenceSplit = new(@"(?<=[.!?])\s+(?=[A-Z0-9@])|\.{2,}\s*|\r?\n+", RegexOptions.Compiled);

    // ─── The pass ────────────────────────────────────────────────────────

    private sealed record Text(int Index, ExtractionInput Message, string Body);

    public static ExtractionOutput Extract(IReadOnlyList<ExtractionInput> messages)
    {
        var output = new ExtractionOutput();

        var texts = new List<Text>();
        for (var i = 0; i < messages.Count; i++)
        {
            var m = messages[i];
            if (m.IsSystem || string.IsNullOrWhiteSpace(m.Body)) continue;
            var body = Clean(m.Body);
            if (body.Length < 2 || Deleted.IsMatch(body) || UrlOnly.IsMatch(body)) continue;
            texts.Add(new Text(i, m, body));
        }

        // Recommendations answered "yes, …" become one agreed decision, not two items.
        var answeredRecommendations = new HashSet<string>();
        var recentMoney = new List<(ExtractionCandidate C, DateTime At, string Author)>();

        for (var t = 0; t < texts.Count; t++)
        {
            var current = texts[t];
            var m = current.Message;
            var body = current.Body;

            if (TryReply(texts, t, output, answeredRecommendations)) continue;
            if (IsAck(body)) continue;

            var units = SplitUnits(body, out var isList, out var isSummary);
            var materialLines = new List<string>();
            var numbered = isList && !isSummary;

            for (var u = 0; u < units.Count; u++)
            {
                var unit = units[u].Text;
                if (IsAck(unit) || SummaryHeader.IsMatch(unit)) continue;

                ReadPercent(unit, units.Take(u).Select(x => x.Text).ToList(), m, output);

                // A bullet stands alone; only a sentence on the same line, or in running prose, lends its subject.
                var context = u > 0 && (!isList || units[u - 1].Line == units[u].Line) ? units[u - 1].Text : null;
                var candidate = Classify(unit, context, m, numbered, isSummary, recentMoney, output);
                if (candidate is null) continue;

                if (candidate.Rule == "material-quantity" && !numbered)
                {
                    materialLines.Add(Tidy(unit));
                    continue;
                }

                Add(output, candidate);
                if (candidate.Kind == ProposalKind.Price)
                    recentMoney.Add((candidate, m.SentAt, m.Author));
            }

            // Several quantity lines in one plain message are one order, not five nags.
            if (materialLines.Count > 0)
            {
                Add(output, new ExtractionCandidate
                {
                    MessageId = m.Id,
                    Kind = ProposalKind.Material,
                    Title = Cap(materialLines.Count == 1 ? materialLines[0] : string.Join("; ", materialLines)),
                    Quantity = Cap(string.Join("; ", materialLines), 100),
                    Rule = "material-quantity",
                    Confidence = 0.75
                });
            }
        }

        SuppressAnswered(output, texts, answeredRecommendations);
        return output;
    }

    private static ExtractionCandidate? Classify(
        string unit, string? previousUnit, ExtractionInput m, bool numbered, bool isSummary,
        List<(ExtractionCandidate C, DateTime At, string Author)> recentMoney, ExtractionOutput output)
    {
        var question = IsQuestion(unit);
        var title = TitleFor(unit, previousUnit);

        // A decision the contractor is waiting on is worth holding even when asked as a question.
        if (m.AuthorSide != ProjectSide.Client && DecisionOwed.IsMatch(unit))
            return new ExtractionCandidate
            {
                MessageId = m.Id, Kind = ProposalKind.Choice, Title = title,
                Maturity = CommitmentMaturity.InDiscussion,
                OwedBySide = Other(m.AuthorSide) ?? ProjectSide.Client,
                Rule = "decision-owed", Confidence = 0.6
            };

        if (question)
        {
            var statement = StatementBeforeQuestion(unit);
            if (statement is null) return null;
            unit = statement;
            title = TitleFor(unit, null);
        }

        var money = MoneyReader.FindAll(unit);
        if (money.Count > 0 && (MoneyContext.IsMatch(unit) || money.Any(h => h.Explicit)))
        {
            var hit = money[0];
            if (Dispute.IsMatch(unit))
            {
                var earlier = recentMoney.LastOrDefault(r => r.C.Amount == hit.Amount
                                                            && r.Author != m.Author
                                                            && (m.SentAt - r.At).TotalDays <= 7);
                if (earlier.C is not null)
                {
                    earlier.C.Contested = true;
                    earlier.C.ContestNote = Cap($"{m.Author}: {Tidy(unit)}", 500);
                    return null;
                }
            }

            var due = DatePhraseReader.Find(unit, m.SentAt);
            return new ExtractionCandidate
            {
                MessageId = m.Id, Kind = ProposalKind.Price, Title = title,
                Amount = hit.Amount, Currency = hit.Currency,
                DueDate = due is { IsToday: false } && due.Date >= m.SentAt.Date ? due.Date : null,
                DateText = due?.Text,
                Maturity = Dispute.IsMatch(unit) ? CommitmentMaturity.InDiscussion : CommitmentMaturity.Agreed,
                Contested = Dispute.IsMatch(unit),
                Rule = "money", Confidence = 0.8
            };
        }

        var date = DatePhraseReader.Find(unit, m.SentAt);
        if (date is not null && IsPromise(unit, date, m.SentAt))
            return new ExtractionCandidate
            {
                MessageId = m.Id, Kind = ProposalKind.Date, Title = title,
                DueDate = date.Date, DateText = date.Text,
                Rule = "date-promise", Confidence = 0.7
            };

        var materialish = MaterialWord.IsMatch(unit);

        if (!isSummary && (materialish || Regex.IsMatch(unit, @"\bitems\b", I)) && DeliveredCue.IsMatch(unit))
            return new ExtractionCandidate
            {
                MessageId = m.Id, Kind = ProposalKind.Material, Title = title,
                Maturity = CommitmentMaturity.Delivered,
                Rule = "material-delivered", Confidence = 0.7
            };

        var decision = Decision(unit, title, m);
        if (decision is not null) return decision;

        if (!isSummary)
        {
            var rebar = Rebar.Matches(unit);
            var quantity = Quantity.Match(unit);
            var specified = materialish && (quantity.Success || SpecCue.IsMatch(unit)) && !PastReport.IsMatch(unit);
            var listed = numbered && (materialish || ProcureCue.IsMatch(unit));

            if (rebar.Count > 0 || specified || listed)
                return new ExtractionCandidate
                {
                    MessageId = m.Id, Kind = ProposalKind.Material, Title = title,
                    Quantity = rebar.Count > 0 || quantity.Success
                        ? Cap(string.Join(", ", rebar.Select(r => r.Value).Append(quantity.Success ? quantity.Value : null).Where(v => v is not null)!), 100)
                        : null,
                    Rule = numbered ? "material-spec" : "material-quantity",
                    Confidence = 0.75
                };
        }

        if (BlockerCue.IsMatch(unit))
        {
            var party = Party.Match(unit);
            return new ExtractionCandidate
            {
                MessageId = m.Id, Kind = ProposalKind.Blocker, Title = title,
                PartyName = party.Success ? Capitalise(party.Groups["p"].Value.Trim())
                          : Regex.IsMatch(unit, @"power\s+cuts?", I) ? "Power supply" : null,
                Rule = "blocker", Confidence = 0.6
            };
        }

        return null;
    }

    private static ExtractionCandidate? Decision(string unit, string title, ExtractionInput m)
    {
        var byClient = m.AuthorSide == ProjectSide.Client;

        if (byClient && (ClientDirective.IsMatch(unit) || FinishChoice.IsMatch(unit)))
            return new ExtractionCandidate
            {
                MessageId = m.Id, Kind = ProposalKind.Choice, Title = title,
                Rule = "decision-directive", Confidence = 0.7
            };

        if (AgreedCue.IsMatch(unit) || EngineerCleared.IsMatch(unit))
            return new ExtractionCandidate
            {
                MessageId = m.Id, Kind = ProposalKind.Choice, Title = title,
                Rule = "decision-agreed", Confidence = 0.65
            };

        if (SpecChange.IsMatch(unit))
            return new ExtractionCandidate
            {
                MessageId = m.Id, Kind = ProposalKind.Spec, Title = title,
                Maturity = byClient ? CommitmentMaturity.Agreed : CommitmentMaturity.InDiscussion,
                Rule = "spec-change", Confidence = 0.6
            };

        if (Recommendation.IsMatch(unit))
            return new ExtractionCandidate
            {
                MessageId = m.Id, Kind = ProposalKind.Choice, Title = title,
                Maturity = CommitmentMaturity.InDiscussion,
                OwedBySide = Other(m.AuthorSide),
                Rule = "recommendation", Confidence = 0.55
            };

        return null;
    }

    /// <summary>
    /// A date is a promise only when the sentence looks forward, is not hedged
    /// ("if possible"), and the day has not already gone. "Today" counts only with
    /// a future modal — "started today" is a report, not a commitment.
    /// </summary>
    private static bool IsPromise(string unit, DateHit date, DateTime sentAt)
    {
        if (Hedge.IsMatch(unit)) return false;
        if (date.Date < sentAt.Date) return false;
        if (date.IsToday) return ModalFuture.IsMatch(unit);
        if (!FutureCue.IsMatch(unit)) return false;
        return !PastReport.IsMatch(unit) || ModalFuture.IsMatch(unit);
    }

    // ─── Replies — context the single message does not carry ─────────────

    private static bool TryReply(List<Text> texts, int index, ExtractionOutput output, HashSet<string> answered)
    {
        var current = texts[index];
        var earlier = OtherPartyBurst(texts, index);
        var prev = earlier.FirstOrDefault();
        if (prev is null) return false;
        var m = current.Message;
        var body = current.Body;
        var gap = m.SentAt - prev.Message.SentAt;
        var prevQuestion = prev.Body.TrimEnd().EndsWith('?');

        // "When do you intend to do that?" — "Tomorrow".
        if (WordCount(body) <= 5 && prevQuestion && WhenQuestion.IsMatch(prev.Body) && gap.TotalHours <= 6)
        {
            var date = DatePhraseReader.Find(body, m.SentAt);
            if (date is not null && !date.IsToday && date.Date >= m.SentAt.Date)
            {
                Add(output, new ExtractionCandidate
                {
                    MessageId = m.Id, Kind = ProposalKind.Date,
                    Title = Cap($"{Tidy(LastQuestion(prev.Body))} — {Tidy(body)}"),
                    DueDate = date.Date, DateText = date.Text,
                    Rule = "date-reply", Confidence = 0.6
                });
                return true;
            }
        }

        // "Where is the burglar going to be fixed?" — "It will be on the inside".
        if (prevQuestion && gap.TotalHours <= 3
            && Regex.IsMatch(LastQuestion(prev.Body), @"^\s*(?:where|which)\b", I)
            && Regex.IsMatch(body, @"\b(?:will\s+be|shall\s+be|goes?\s+on|go\s+in)\b", I))
        {
            Add(output, new ExtractionCandidate
            {
                MessageId = m.Id, Kind = ProposalKind.Spec,
                Title = Cap($"{Tidy(LastQuestion(prev.Body))} — {Tidy(body)}"),
                Rule = "spec-reply", Confidence = 0.55
            });
            return true;
        }

        // "We recommend terrazzo in the laundry too" — "Yes, all the space up there".
        // A "yes" answers a yes/no question or a proposal, never "What of the septic tank?".
        var affirmation = Affirmation.Match(body);
        if (affirmation.Success && WordCount(body[affirmation.Length..]) >= 2)
        {
            var asked = earlier.FirstOrDefault(e =>
                (m.SentAt - e.Message.SentAt).TotalHours <= 12
                && (IsYesNoQuestion(LastQuestion(e.Body)) || Recommendation.IsMatch(e.Body) || DecisionOwed.IsMatch(e.Body)));
            if (asked is null) return false;
            prev = asked;
            prevQuestion = prev.Body.TrimEnd().EndsWith('?');

            var date = DatePhraseReader.Find(prev.Body, prev.Message.SentAt);
            var title = Cap($"{Tidy(prevQuestion ? LastQuestion(prev.Body) : prev.Body)} — {Tidy(body)}");

            Add(output, date is not null && !date.IsToday
                ? new ExtractionCandidate
                {
                    MessageId = m.Id, Kind = ProposalKind.Date, Title = title,
                    DueDate = date.Date, DateText = date.Text,
                    Rule = "date-restated", Confidence = 0.6
                }
                : new ExtractionCandidate
                {
                    MessageId = m.Id, Kind = ProposalKind.Choice, Title = title,
                    Rule = "decision-affirmed", Confidence = 0.65
                });

            answered.Add(prev.Message.Id);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Drop the open item once the thread closes it: a recommendation that was
    /// answered "yes", and a decision owed that the other side made within two
    /// days. Otherwise the queue asks Peter to confirm a question he already answered.
    /// </summary>
    private static void SuppressAnswered(ExtractionOutput output, List<Text> texts, HashSet<string> answered)
    {
        var byId = texts.ToDictionary(t => t.Message.Id, t => t.Message);

        output.Candidates.RemoveAll(c => c.Rule == "recommendation" && answered.Contains(c.MessageId));

        var decisions = output.Candidates
            .Where(c => c.Rule is "decision-directive" or "decision-agreed" or "decision-affirmed" or "spec-change")
            .Select(c => byId[c.MessageId])
            .ToList();

        output.Candidates.RemoveAll(c =>
        {
            if (c.Rule != "decision-owed" || !byId.TryGetValue(c.MessageId, out var asked)) return false;
            return decisions.Any(d => d.Author != asked.Author
                                      && d.SentAt >= asked.SentAt
                                      && (d.SentAt - asked.SentAt).TotalHours <= 48);
        });

        // One author asking for the same decision twice in a quarter of an hour is one ask.
        var owed = output.Candidates.Where(c => c.Rule == "decision-owed").ToList();
        for (var i = 1; i < owed.Count; i++)
        {
            var a = byId[owed[i - 1].MessageId];
            var b = byId[owed[i].MessageId];
            if (a.Author == b.Author && (b.SentAt - a.SentAt).TotalMinutes <= 15)
                output.Candidates.Remove(owed[i]);
        }
    }

    // ─── Progress readings ───────────────────────────────────────────────

    private static void ReadPercent(string unit, List<string> earlierUnits, ExtractionInput m, ExtractionOutput output)
    {
        foreach (Match p in Percent.Matches(unit))
        {
            if (!decimal.TryParse(p.Groups["p"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var pct)) continue;
            if (pct is < 0 or > 100 || MoneyContext.IsMatch(unit)) continue;

            // "90% is already done" names nothing; the subject is the nearest
            // earlier sentence that does not itself start with "it".
            // A one-word subject is still a subject — "Terrazzo is at 45%" is the
            // terrazzo's, not the previous sentence's (works-report R4). Only a
            // pronoun or filler borrows.
            var subject = Subject(unit[..p.Index]);
            for (var e = earlierUnits.Count - 1; IsBareSubject(subject) && e >= 0; e--)
            {
                if (Regex.IsMatch(earlierUnits[e], @"^(?:it|they|this|that)\b", I)) continue;
                subject = Subject(Percent.Replace(earlierUnits[e], "").TrimEnd(' ', '.', ','));
            }
            if (IsBareSubject(subject)) continue;

            output.Readings.Add(new ExtractedReading(m.Id, Cap(subject, 200), pct, m.SentAt));
        }
    }

    private static string Subject(string text)
    {
        var s = Tidy(text);
        s = Regex.Replace(s, @"[,;:\-–]+$", "").Trim();
        s = SubjectFiller.Replace(s, "").Trim();
        return s;
    }

    // ─── Text handling ───────────────────────────────────────────────────

    private static string Clean(string body)
    {
        var s = Mention.Replace(body, "");
        s = EditedMarker.Replace(s, "");
        s = s.Replace("‎", "").Replace("‏", "").Replace("⁨", "").Replace("⁩", "")
             .Replace(' ', ' ').Replace(' ', ' ')
             .Replace('’', '\'').Replace('‘', '\'').Replace('“', '"').Replace('”', '"');
        return s.Trim();
    }

    private sealed record Unit(string Text, int Line);

    private static List<Unit> SplitUnits(string body, out bool isList, out bool isSummary)
    {
        var lines = body.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
        var bulletLines = lines.Count(l => Bullet.IsMatch(l));
        isList = bulletLines >= 2;

        var dashed = lines.Count(l => Bullet.IsMatch(l) && !NumberedBullet.IsMatch(l));
        isSummary = lines.Count > 0 && SummaryHeader.IsMatch(lines[0]) || (isList && dashed >= bulletLines);

        var units = new List<Unit>();
        for (var l = 0; l < lines.Count; l++)
        {
            var line = Bullet.Replace(lines[l], "").Trim();
            foreach (var part in SentenceSplit.Split(line))
            {
                var p = part.Trim();
                if (p.Length > 1) units.Add(new Unit(p, l));
            }
        }

        return units;
    }

    /// <summary>
    /// "We have settled on Epoxy, what is next?" is a decision followed by a
    /// question. Keep the statement; drop the question tail.
    /// </summary>
    private static string? StatementBeforeQuestion(string unit)
    {
        if (!unit.TrimEnd().EndsWith('?')) return null;
        var clause = Regex.Matches(unit, @"[,;](?=\s)");
        if (clause.Count == 0) return null;
        var head = unit[..clause[^1].Index].Trim();
        return head.Contains('?') || WordCount(head) < 3 || QuestionStart.IsMatch(head) ? null : head;
    }

    /// <summary>"Okay", "Noted", "Thanks" — read, and worth nothing to a register or a brief.</summary>
    public static bool IsAcknowledgement(string text) => IsAck(text);

    private static bool IsAck(string text)
    {
        var t = text.Trim();
        if (t.Length == 0) return true;
        if (Regex.IsMatch(t, @"^thank", I)) return true;
        return WordCount(t) <= 4 && AckStart.IsMatch(t);
    }

    /// <summary>The other party's messages immediately before this one, newest first — the thing being answered.</summary>
    private static List<Text> OtherPartyBurst(List<Text> texts, int index)
    {
        var author = texts[index].Message.Author;
        var burst = new List<Text>();
        string? other = null;

        for (var i = index - 1; i >= 0 && i >= index - 12 && burst.Count < 4; i--)
        {
            var candidate = texts[i];
            if (candidate.Message.Author == author)
            {
                if (burst.Count > 0) break;
                continue;
            }
            if (IsAck(candidate.Body)) continue;
            other ??= candidate.Message.Author;
            if (candidate.Message.Author != other) break;
            burst.Add(candidate);
        }

        return burst;
    }

    private static bool IsQuestion(string unit) =>
        unit.TrimEnd().EndsWith('?') || QuestionStart.IsMatch(unit);

    private static bool IsYesNoQuestion(string text) =>
        text.TrimEnd().EndsWith('?') && !Regex.IsMatch(text, @"^\s*(?:(?:okay|so|then|and|also|hi\s+\w+)\s*,?\s*)?(?:what|where|when|why|how|which|who)\b", I);

    private static string LastQuestion(string body)
    {
        var parts = SentenceSplit.Split(body).Select(p => p.Trim()).Where(p => p.EndsWith('?')).ToList();
        return parts.Count > 0 ? parts[^1] : body;
    }

    /// <summary>
    /// "It will be complete this week" says nothing on its own; the sentence
    /// before it names what "it" is. Short or pronoun-led units borrow it.
    /// </summary>
    private static string TitleFor(string unit, string? previousUnit)
    {
        var t = Tidy(unit);
        if (previousUnit is not null
            && (WordCount(t) < 6 || Regex.IsMatch(t, @"^(?:it|they|this|that|these|those|them)\b", I))
            && !SummaryHeader.IsMatch(previousUnit))
            t = $"{Tidy(previousUnit)} — {t}";
        return Cap(t);
    }

    private static string Tidy(string text)
    {
        var t = Bullet.Replace(text, "");
        t = Regex.Replace(t, @"^(?:kindly|please)\s+", "", I);
        t = Regex.Replace(t, @"\s+", " ").Trim();
        t = t.TrimEnd('.', ',', ';', ':', ' ');
        return t.Length > 0 ? char.ToUpperInvariant(t[0]) + t[1..] : t;
    }

    private static string Capitalise(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];

    private static string Cap(string text, int max = 160)
    {
        if (text.Length <= max) return text;
        var cut = text[..max];
        var space = cut.LastIndexOf(' ');
        return (space > max / 2 ? cut[..space] : cut).TrimEnd(',', ';', ' ') + "…";
    }

    private static readonly HashSet<string> BareWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "it", "this", "that", "they", "we", "which", "work", "works", "progress", "all", "overall",
        "now", "so", "and", "but", "also", "total", "about", "done", "completed", "complete"
    };

    private static bool IsBareSubject(string subject) =>
        WordCount(subject) == 0 || (WordCount(subject) == 1 && BareWords.Contains(subject.Trim(' ', '.', ',', ':')));

    private static int WordCount(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Count(w => w.Any(char.IsLetterOrDigit));

    private static ProjectSide? Other(ProjectSide? side) => side switch
    {
        ProjectSide.Client => ProjectSide.Contractor,
        ProjectSide.Contractor => ProjectSide.Client,
        _ => null
    };

    private static void Add(ExtractionOutput output, ExtractionCandidate candidate)
    {
        var key = Normalise(candidate.Title);
        if (output.Candidates.Any(c => c.MessageId == candidate.MessageId && c.Kind == candidate.Kind && Normalise(c.Title) == key))
            return;
        output.Candidates.Add(candidate);
    }

    internal static string Normalise(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (var ch in text.ToLowerInvariant())
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
        return sb.ToString();
    }
}
