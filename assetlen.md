# Assetlen

**Remote asset monitoring for developers who run their projects on trust, a verbal
agreement and a phone.**

---

## 1. The problem, stated precisely

Peter and David do not have a communication problem. They communicate constantly. They
have a **retrieval and continuity problem**, and it has one cause:

> **WhatsApp stores messages. Construction runs on commitments.**

A message has a position in a stream. Positions expire the moment new messages arrive.
So nothing can be pointed at — it can only be found again and re-sent. Hence:

- The same receipt appears five times in the thread.
- A markup is a screenshot, orphaned from the document it marks up.
- An answer is a message, so next month it is the next thing to scroll for.
- An idea shared in June is invisible in September.
- There is one volume dial for two audiences, so the client either drowns or is in the dark.

Every symptom in this project traces back to that one line.

---

## 2. The vision

Assetlen replaces the message stream with a **commitments register** that both parties
write to without noticing.

Peter's four searches — the promise document, the photo proving a promise, the material
agreed, the sketch-and-ask — are not four features. They are one act: *holding a past
commitment up against present reality.* Assetlen exists to make that act take twenty
seconds.

---

## 3. The one object

Everything in Assetlen is a **Commitment**: a spec, a price, a date, a material, a choice.
Each carries who agreed, when, and the evidence.

Commitments live inside **funded stages**, each holding five to eight checklist deliverables.
Every photo, comment, decision, invoice and query attaches to a line item. Nothing floats.

### Maturity states

```
Idea  →  In discussion  →  Agreed  →  Delivered  →  Verified
```

The Pinterest boundary wall enters as an **Idea parked against External Works**. It is not
noise and not a decision. Assetlen holds it, lets it accumulate references and estimates,
and hands it back at stage kickoff — three things a stream structurally cannot do, because
a stream only knows *now*.

### Query state on settled items

```
Cleared  →  Query raised  →  Resolved
```

Cleared is not closed. Peter can reopen a paid item without it reading as an accusation,
and when it resolves **the item updates** — *"revised to 4 bags, +£X, agreed 6 Aug"* — not
just a message somewhere. Otherwise the answer becomes the next thing to scroll for and
we have rebuilt WhatsApp with better fonts.

---

## 4. The four design laws

### Law 1 — Diverge at retrieval. Stay identical at capture.

Different is *worse* wherever they have muscle memory: sending a photo, replying, voice
notes, notification speed. Different is *better* only where WhatsApp offers nothing at
all — and it offers nothing for retrieval. That blank space is the safe ground to be
radical on.

### Law 2 — One canonical artifact, many pointers.

The receipt is uploaded once and lives at a permanent address. Every later mention is a
link, never a copy. Re-uploads are hash-matched: *"this is already Receipt R-014."*
Annotation is a **layer on the original**, versioned and attributed, never a new image.
Repetition is a tax on the absence of addressing. Fix addressing and it disappears.

### Law 3 — Capture stays dumb. Extraction gets smart.

David and the clerk post exactly as they do today. Assetlen reads it — OCR on every
receipt and invoice, plus extraction of what is being committed to — and proposes
structure. One tap to confirm. **David never fills a form; he approves a guess.**
Extract only on money, materials, dates and decisions, or confirmation fatigue will make
the register confidently wrong.

### Law 4 — Speak only when waiting costs something.

Never nag an Idea. Surface a parked item when money, a lead time or a physical dependency
creates a real deadline:

> *Boundary wall is parked for External Works, but the gate power duct must be laid before
> the driveway is poured. Decide gate position by 20 Sept?*

Lead times compute the deadline backwards from the stage start. Everything else stays
silent. A muted Assetlen is just WhatsApp with extra steps.

---

## 5. Two surfaces, one set of facts

WhatsApp's structural failure with a live site is one channel serving two audiences.

**The Site Log** — David's operational record. Complete, unsanitised, private. The clerk
of works posts here without judgement. Concurrent crews mean three destinations, not three
threads.

**The Client Brief** — one page per day for Peter, **grouped by deliverable, not by time**.
Auto-drafted: same-vantage-point shots, anything on a funded item, anything that changes a
date or a cost. David edits by exception in three minutes. **It publishes at the cutoff
whether or not he touches it.**

### The truth floor

David controls emphasis, not truth. These reach Peter regardless of curation:

1. Anything that moves money
2. Anything that moves a date
3. Anything that changes an agreed spec
4. Blockers and decisions Peter owes

Stated to both parties once, plainly. Never quietly widened. This is the clause that makes
a filtered feed trustworthy.

---

## 6. Industry standard, hidden inside simplicity

The professional discipline Peter is missing is delivered through workflow, not vocabulary.
**David never sees the word "valuation."**

| Industry practice | Assetlen form |
|---|---|
| Schedule of works + valuations | Funded stages with 5–8 checklist items |
| Progress claim / QS certification | "Claim stage" with attached evidence, Peter approves |
| RFI register | Decisions Peter owes, with by-when and consequence |
| Variation / change order register | Extras list: what, why, cost, approved yes/no |
| Drawing register with revisions | Current version pinned; superseded archived, not deleted |
| Snagging / punch list | Defects list at stage close |
| Retention | One field: % held back |
| Procurement lead times | Backwards-computed "decide by" dates |

---

## 7. Scope

### Build

- Funded stages with deliverable checklists
- Three-tap capture against a deliverable (clerk-capable)
- Commitment extraction with one-tap confirm
- Markup as a layer on the original, attributed and versioned
- Query state on cleared items, resolving back into the item
- Backlinks in both directions and a provenance strip: *agreed → evidence → invoiced →
  cleared → queried → resolved*
- OCR search across images and documents
- Parked ideas with dependency- and lead-time-driven surfacing
- Stage kickoff brief
- Site log and auto-drafted client brief with cutoff publishing
- Current drawing revision
- A deliberately second-class informal channel with **"file this to an item"** on every
  message

### Do not build

| Cut | Reason |
|---|---|
| **Holding or moving money** | Escrow licensing will consume the entire runway before anyone has posted a photo. Record that a stage was funded and released; the transfer happens at the bank. |
| Gantt charts and critical path | Peter thinks in stages, not networks |
| Bills of quantities | Reintroduces the machinery he is avoiding |
| Accounting integrations | Not the bottleneck |
| Multi-project portfolio dashboards | One project must work first |
| Roles beyond developer / contractor / clerk | Permissions complexity, no user value yet |
| Threaded general chat as a primary surface | Rebuilds the problem |
| Anything that adds a tap to capture | Directly causes churn back to WhatsApp |

**Rule for every future feature:** *Does it help someone hold a past commitment against
present reality?* If not, it does not ship.

---

## 8. Non-negotiable parity

These are not features. They are the price of entry, and failing any one of them sends
both users back to the green app:

- Notification speed and reliability equal to WhatsApp
- Voice notes, with transcription
- Share-sheet target — post straight from the camera roll, or from WhatsApp itself
- Works on a bad site connection; queues and syncs
- Three taps to post a photo

---

## 9. Validation before building

Three cheap tests, in order:

1. **The extraction test.** Take one finished stage's real WhatsApp thread. Hand-extract
   every commitment in it — perhaps twenty lines. Show Peter the page. *Would this have
   saved the scrolling?* If he still wants to scroll the thread, the commitment model is
   wrong and this is a search problem.

2. **The two-surface test.** Take one real site day. Hand-build the full log and the
   three-block brief. If David says *"I'd have dropped two of those"* and Peter says
   *"this is what I wanted,"* the curation model holds.

3. **The bad-week test.** Run one live project with David posting to a shared album with a
   checklist, manually, for three weeks. The risk was never scope — it is whether David
   survives a bad week on site without drifting back. Nothing else matters if this fails.

---

## 10. Is this too much for an MVP?

Cut as above: **one project, three users, five screens.** Six to ten weeks.

It is only too much if Assetlen touches money movement. Everything else here is one data
model — every artifact has an ID, every comment carries a parent ID — from which
backlinks, provenance and search fall out for free.

---

## The positioning

> **WhatsApp is where the conversation happens. Assetlen is where the agreement lives.**
