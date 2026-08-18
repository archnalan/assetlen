# Assetlen

**Remote asset monitoring for developers who run their projects on trust, a verbal
agreement and a phone.**

---

## 0. Who pays, and what follows from it

**Peter buys Assetlen. Nalan is a guest in Peter's project.**

This is the decision everything else in this document obeys. It was made on 2026-08-12
against [whatsapp-evidence.md](whatsapp-evidence.md), and it reverses several earlier
assumptions. Peter funds the work, carries the risk, chases the updates, runs several
projects at once, and is the only party with a recurring reason to pay. The contractor is a
one-man practice with high switching cost and no budget line for software.

Four consequences, each binding:

| # | Decision | Because |
|---|---|---|
| **D1** | **The project belongs to Peter's account.** Contractors are invited into it and can be replaced without Peter losing the record. | A commitments register that dies when the builder stops paying is not a register. |
| **D2** | **Assetlen must be worth paying for on day one with Nalan absent.** | Peter is the buyer; Nalan is still the adoption risk. If the product only works once the contractor adopts it, the buyer churns before that ever happens. |
| **D3** | **WhatsApp is not replaced. It is ingested.** | Nalan will not move. Fighting for the conversation loses; taking a copy of it wins. |
| **D4** | **Contractor participation is an upgrade, never a precondition.** | Three tiers, §7. Every tier must stand alone. |
| **D5** | **Peter appoints the mediator; the mediator staffs the delivery side. Peter sees *that* they exist, never their traffic.** | §10.1. A guest may not silently add people to the owner's account, but the owner has no business reading the crew's operational chatter. |

> **The trap this avoids.** The obvious build is: contractor captures → system drafts →
> contractor curates → developer reads. Every step depends on the person with the least
> incentive. Peter pays for an empty page and leaves in week three. **Nothing in this
> document may assume Nalan logs in.**

---

## 1. The problem, stated precisely

Peter and Nalan do not have a communication problem. They communicate constantly. They
have a **retrieval and continuity problem**, and it has one cause:

> **WhatsApp stores messages. Construction runs on commitments.**

A message has a position in a stream. Positions expire the moment new messages arrive.
So nothing can be pointed at — it can only be found again and re-sent. Hence:

- The same receipt appears five times in the thread.
- A markup is a screenshot, orphaned from the document it marks up.
- An answer is a message, so next month it is the next thing to scroll for.
- An idea shared in June is invisible in September.
- There is one volume dial for two audiences, so the client either drowns or is in the dark.

Every symptom in the year-long thread traces back to that one line. The seven failure
modes and their citations are in [whatsapp-evidence.md](whatsapp-evidence.md) §3.

---

## 2. The vision

Assetlen turns a message stream Peter cannot search into a **commitments register he
owns**.

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

The boundary-wall idea enters as an **Idea parked against External Works**. It is not
noise and not a decision. Assetlen holds it, lets it accumulate references and estimates,
and hands it back at stage kickoff — three things a stream structurally cannot do, because
a stream only knows *now*.

### Query state on settled items

```
Cleared  →  Query raised  →  Resolved
```

Cleared is not closed. Peter can reopen a paid item without it reading as an accusation,
and when it resolves **the item updates** — *"revised to 4 bags, +UGX 480,000, agreed
6 Aug"* — not just a message somewhere. Otherwise the answer becomes the next thing to
scroll for and we have rebuilt WhatsApp with better fonts.

### Where a commitment comes from

Every commitment records its **source channel**, because the most expensive ones on a real
project were never typed anywhere ([evidence](whatsapp-evidence.md) F5):

```
App  |  Ingested  |  Verbal  |  Meeting
```

A `Verbal` commitment is one tap — *"agreed on the call: retaining-wall labour at UGX Xm,
11 Jun"* — attributed to both parties. The counterparty sees it next and can **Confirm** or
say **That's not what we said**, which flips it to `Query raised`. It never silently
becomes truth.

---

## 4. The design laws

### Law 0 — Assetlen must work when the contractor does not.

The newest law and the one that outranks the rest. Every surface has a path that runs on
material Peter forwards himself. If a feature only functions once Nalan adopts, it is a
tier-2 feature and must be labelled as such.

### Law 1 — Diverge at retrieval. Stay identical at capture.

Different is *worse* wherever there is muscle memory: sending a photo, replying, voice
notes, notification speed. Different is *better* only where WhatsApp offers nothing at
all — and it offers nothing for retrieval. That blank space is the safe ground to be
radical on.

### Law 2 — One canonical artifact, many pointers.

The receipt is uploaded once and lives at a permanent address. Every later mention is a
link, never a copy. Re-uploads are hash-matched: *"this is already Receipt R-014."*
Annotation is a **layer on the original**, versioned and attributed, never a new image.
Repetition is a tax on the absence of addressing. Fix addressing and it disappears.

### Law 3 — Extraction is the product, not a garnish.

Under the old thesis Nalan posted structure and extraction merely tidied it. Under D2,
**extraction is the only path from raw forwarded material to a register.** OCR every
receipt and invoice; read every ingested message for what is being committed to; propose
structure. One tap to confirm.

Extract only on **money, materials, dates and decisions**. A real thread is hundreds of
*"Okay"*, *"Noted"*, *"Good progress"* — those must produce nothing. Ask ten times a day
and whoever is confirming taps through blindly, and the register becomes confidently wrong,
which is worse than no register at all.

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

**The Site Diary** — the complete, unsanitised operational record. Everything ingested or
captured lands here.

**The Client Brief** — one page per day for Peter, **grouped by deliverable, not by time**.
Same-vantage-point shots, anything on a funded item, anything that changes a date or a cost.

**The brief assembles itself.** When Nalan participates he edits by exception in three
minutes and publishes at the cutoff whether or not he touches it. **When he does not
participate, it still publishes** — assembled from ingested material, uncurated, because
an uncurated brief of Peter's own forwarded content is exactly as private as his phone.

> Curation is a feature Nalan unlocks for himself. It is not a dependency Peter inherits.

### Curation and the truth floor

Where a contractor *is* participating, one — at most two — **mediators** stand between the
two sides. On the real project this was the architect: retained by the client, directing
the build, the only person who saw both surfaces, and **the single accountable name on
everything that crossed** (§10.1). A mediator controls **emphasis, not truth.** These reach
Peter regardless of curation:

1. Anything that moves money
2. Anything that moves a date
3. Anything that changes an agreed spec
4. Blockers and decisions Peter owes

Stated to both parties once, plainly. Never quietly widened. This is the clause that makes
a filtered feed trustworthy — and it is enforceable only because the register is Peter's,
so the filter runs in the house of the person being filtered *for*, not *by*.

---

## 6. Industry standard, hidden inside simplicity

The professional discipline Peter is missing is delivered through workflow, not vocabulary.
**Nobody ever sees the word "valuation."**

| Industry practice | Assetlen form |
|---|---|
| Schedule of works + valuations | Funded stages with 5–8 checklist items |
| Progress claim / QS certification | "Claim stage" with attached evidence, Peter approves |
| Cost report | Per stage: **funded → claimed → cleared → carried forward** |
| Variation / change order register | Extras list: what, why, cost delta, approved yes/no |
| RFI register | Decisions Peter owes, with by-when and consequence |
| Drawing register with revisions | Current version pinned; superseded archived, not deleted |
| Snagging / punch list | Defects list at stage close |
| Retention | One field: % held back |
| Procurement lead times | Backwards-computed "decide by" dates |

The cost report and the variation register are promoted to the top of this table because
the single worst hour in the corpus was a reconciliation failure caused by eight unlogged
variations ([evidence](whatsapp-evidence.md) F3, F4), settled only by meeting in person.

---

## 7. Three tiers

Each tier must be worth its price alone. Nothing in a lower tier degrades when the next one
never arrives.

**Tier 1 — Peter alone.** He forwards a WhatsApp export, a share-sheet photo, an email
attachment. Assetlen extracts commitments, OCRs receipts, builds the register, tracks
funded-vs-claimed per stage, and answers *"what did I approve on the balustrade?"* This
tier is the product. If it does not sell on its own, nothing later saves it.

**Tier 2 — Peter's side.** Dinah joins. She has her own emphasis — specs, finishes, choices
she owes — over the same facts, and she reports site conditions Peter cannot see.

**Tier 3 — Nalan joins.** Capture lands directly instead of being forwarded. He curates the
brief by exception, exposes selected frames rather than forwarding batches, and gets paid
faster because claims carry their own evidence. Everything above still works if he stops.

---

## 8. Scope

### Build

- Funded stages with deliverable checklists
- **Ingest: WhatsApp export import, share-sheet target, email-in** — the tier-1 front door
- Commitment extraction with one-tap confirm, on money / materials / dates / decisions only
- **Verbal-decision capture** with counterparty confirm-or-dispute
- Per-stage cost report: funded → claimed → cleared → carried forward
- **Variation register**: what, why, cost delta, approved by whom and when
- OCR search across images and documents
- Markup as a layer on the original, attributed and versioned
- Query state on cleared items, resolving back into the item
- Backlinks in both directions and a provenance strip: *agreed → evidence → invoiced →
  cleared → queried → resolved*
- Parked ideas with dependency- and lead-time-driven surfacing; stage kickoff brief
- Site Diary and auto-assembling Client Brief with cutoff publishing
- Current drawing revision, superseded archived
- **Peter's multi-project home** — see the reversal below
- Three-tap capture against a deliverable, and bulk camera-roll import *(tier 3)*
- Per-frame exposure so a mediator shares three of eighteen, never the batch *(tier 3)*

### Do not build

| Cut | Reason |
|---|---|
| **Holding or moving money** | Escrow licensing will consume the entire runway before anyone has posted a photo. Funds on the real project moved through three agents, two banks and a third party's account — rails Assetlen could not see if it tried. Record that a stage was funded and released; the transfer happens at the bank. |
| **Any in-app informal channel** | **Cut harder than before.** Under D3 the conversation stays in WhatsApp and Assetlen ingests it. Building a second-class chat spends effort competing where we have decided not to compete, and splits the record in two. |
| **Voice notes and transcription as a launch item** | Parity aimed at a contractor who may never log in. Tier 3 at the earliest. |
| Gantt charts and critical path | Peter thinks in stages, not networks |
| Bills of quantities | Reintroduces the machinery he is avoiding; his own BoQ was cut down twice for being too heavy |
| Accounting integrations | Not the bottleneck |
| Roles beyond developer / representative / mediator / delivery | Permissions complexity, no user value |
| Anything that adds a tap to capture | Directly causes churn back to WhatsApp |

### Reversed

| Previously cut | Now build | Why |
|---|---|---|
| **Multi-project portfolio dashboard** | **Peter's home screen** | The cut read *"one project must work first"* — a sequencing note that hardened into a ban. Peter runs a main house, a guest wing, external works and a second site simultaneously. If he pays, the first thing he opens is all of them. One project must work first; it must not be the only thing that ever works. |

**Rule for every future feature:** *Does it help someone hold a past commitment against
present reality?* If not, it does not ship. **Second rule, from D2:** *does it still work
when the contractor is silent?* If not, it is tier 3 and cannot be counted toward launch.

---

## 9. Non-negotiable parity

Parity is now measured against **what Peter does today**, not against what a contractor
would need to switch.

- Forwarding into Assetlen must be no harder than forwarding inside WhatsApp
- Anything he can see in WhatsApp, he can find in Assetlen in one search
- Works on a bad connection; queues and syncs
- Notification speed and reliability equal to WhatsApp
- *(tier 3)* Three taps to post a photo; bulk camera-roll import; voice notes

---

## 10. Who is on a project

### 10.1 One accountable face

**Peter creates the project and appoints the mediator. The mediator staffs the delivery
side. The mediator is accountable for all of it.**

> **Nalan is accountable regardless of which subcontractors he uses.** Everything that
> crosses to Peter crosses as Nalan's word. He cannot later disown a claim because the
> steel team made it, or a spec because the fabricator misread it. One contract, one
> signature, one throat to choke — which is exactly the arrangement Peter already believes
> he has, and the reason he tolerates not knowing who is on site.

This produces three distinct lists, and conflating them is the mistake to avoid:

| List | Who sees it | Contents |
|---|---|---|
| **The accountable face** | Peter, on everything | One name. Every exposed frame, claim, commitment and answer is attributed to the mediator, whoever actually produced it. |
| **True authorship** | Delivery side only | Who captured what, on which day. This is the mediator's own defensible record — *"when a wall cracks in November"* — and it is never sanitised. |
| **The access roster** | Peter, always, as account owner | Every human holding a login on his project. Names and side, nothing more: no activity, no traffic, no Site Diary. |

The access roster is not negotiable. A guest cannot silently add people to the owner's
account — Peter pays the bill and carries the data. But the roster answers *"who has a
key"*, not *"who did what"*, and the two must never be merged into one screen.

**Delegation limits.** One mediator, or at most two — a project with three filters has no
accountable face. The mediator may add and remove **delivery-side** members only; the
client side is Peter's alone. Peter can replace the mediator at any time without losing a
single commitment, which is the whole point of D1.

### 10.3 Pricing — per project, by size

**Assetlen bills the developer per project, priced on floor area. Never per seat.**

Seat pricing was considered and rejected: Peter's bill would grow every time Nalan hired a
labourer, which taxes exactly the behaviour the product needs — the whole delivery side
capturing. It also hands the contractor a lever over the developer's invoice. Delivery-side
members are therefore free and uncapped.

Floor area is the right meter because it is **contractor-independent, verifiable from the
drawings, and does not move when the crew does.**

| Tier | Total floor area |
|---|---|
| **Small** | up to 250 m² — a single modest residence |
| **Medium** | 250–750 m² — a substantial residence, or one with a secondary building |
| **Large** | over 750 m² — multiple buildings, multi-unit or commercial |

Three rules, and they are the whole policy:

1. **The billable unit is the top-level project.** A guest wing is a sub-project: it
   enlarges the parent's area rather than becoming a second invoice.
2. **A tier can never rise without a person saying yes.** A measured increase is proposed,
   not applied. A decrease applies immediately — we do not keep billing a band the drawings
   no longer justify.
3. **An undeclared project bills at Small.** We never guess upward; a wrong guess in our own
   favour is the fastest way to lose the account.

**Later, not now:** read the area off the uploaded drawings and propose the tier
automatically — the artifact store already holds them and the revision chain already knows
which one is current. Until that exists, area is declared and the source is recorded, so an
automated reading can never silently overwrite a human one.

### 10.2 Ownership and durability

Because Peter buys (D1):

- **The account is Peter's.** Projects belong to it. Contractors, representatives and
  mediators are invited in and can be removed.
- **One human, one login, many accounts.** Nalan works for several developers and appears
  as a guest in each. The developer, not the contractor, is the account boundary.
- **Replacing the contractor does not touch the record.** This is the single strongest
  reason the register belongs on Peter's side of the table, and it is a sales argument as
  much as an architectural one.
- Assetlen still never holds or moves money.

---

## 11. Validation

Three cheap tests, in order. All three now measure Peter, because Peter pays.

1. **The extraction test.** Take one finished stage's real thread. Hand-extract every
   commitment — sixteen for the retaining wall, already drafted in
   [whatsapp-evidence.md](whatsapp-evidence.md) §7. Show Peter the page. *Would this have
   saved the scrolling?* If he still wants to scroll the thread, the commitment model is
   wrong and this is a search problem.

2. **The two-surface test.** Take one real site day. Hand-build the full log and the
   three-block brief. If a contractor says *"I'd have dropped two of those"* and Peter says
   *"this is what I wanted,"* the curation model holds.

3. **The silent-contractor test.** *(Replaces the bad-week test.)* Build tier 1 from a real
   WhatsApp export with **no contractor involvement at all**, and give it to Peter for three
   weeks. Would he pay for this alone? The old test asked whether Nalan survives a bad week.
   Under D2 that is no longer the thing that kills us — an empty register Peter paid for is.

---

## 12. Is this too much for an MVP?

Cut as above: **tier 1 only. One developer, one project, four screens** — register, brief,
stage money, search. Six to ten weeks.

It is only too much if Assetlen touches money movement. Everything else is one data model —
every artifact has an ID, every commitment carries a source and a parent — from which
backlinks, provenance and search fall out for free.

---

## The positioning

> **WhatsApp is where the conversation happens. Assetlen is where the agreement lives —
> and the agreement belongs to the person who paid for it.**
