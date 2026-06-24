# No-BS Readiness Rubric

Use this rubric before calling any branch a Steam-facing demo candidate. It is
deliberately stricter than "the build runs."

Scoring:

- 0: absent or misleading.
- 1: technically present but not player-readable.
- 2: readable as a prototype, not yet sellable.
- 3: credible for private demo-candidate review.
- 4: strong enough to survive public-facing scrutiny after legal and platform
  checks.

| Area | 0-1 means | 2 means | 3 means | 4 means |
| --- | --- | --- | --- | --- |
| Truthfulness | Claims outrun the build | Prototype limits are stated | All capture and copy match the playable branch | External-facing claims survive legal, platform, and QA review |
| First 10 seconds | Viewer cannot identify player, goal, or threat | Basic room read works with HUD help | Player, threat, objective, and route are clear from the scene | The first screen sells the loop without explanation |
| Fun-first ARPG feel | Movement and combat look tedious | Basic actions function but lack impact | Move, dodge, hit, enemy reaction, and defeat look satisfying in a short clip | The loop looks replayable before any extra systems are explained |
| Visual identity | Proof shapes dominate | Key objects are distinct but rough | Player, enemy, altar, and gate have intentional silhouettes | Art direction feels coherent across room, UI, and effects |
| Combat readability | Hits, tells, and dodges are unclear | Main actions are readable in motion | Threat, dodge, hit, stagger, and defeat are clear in a short clip | Combat reads at thumbnail size and feels responsive when played |
| Phase system | Phase is a label, color swap, or puzzle symbol burden | Phase difference is visible but secondary | Phase choice improves combat feel or decision clarity | Phase identity is memorable without slowing the room |
| Objective loop | Completion condition is unclear or symbol-heavy | Altar/gate loop is understandable | Room entry to reward claim feels complete and simple | Completion creates clear desire for another room |
| Audio | Silent or distracting | Event coverage exists as placeholder | Timing supports action, threat, objective, and reward | Audio identity improves mood and feedback without masking play |
| UI and prompts | Debug text carries the experience | HUD explains the prototype | Capture-friendly UI supports play without looking like a test harness | UI feels like part of the game and scales to store capture |
| Build and capture | Cannot reliably produce evidence | Manual screenshots are possible | Repeatable screenshot/clip/playable evidence exists | Evidence flow is routine enough for release-candidate review |
| Asset rights | Unknown-source assets are present | Placeholder-only or reviewed assets | Provenance is recorded for anything non-procedural | Rights, receipts, and license constraints are release-audit ready |

## Minimum Bar For Demo-Candidate Review

Do not use "Steam-ready" for this project at the current stage. For a private
demo-candidate review, require:

- Every area scores at least 3, except areas explicitly marked as deliberate
  placeholder work in the PR.
- Truthfulness, fun-first ARPG feel, build and capture, and asset rights score
  at least 3 with no exceptions.
- One uninterrupted room-completion clip exists.
- The screenshot set covers first read, movement, combat, simple objective, gate,
  and reward.
- The PR states exactly what is placeholder and what is intended direction.

## Automatic Red Flags

- Store-facing copy mentions features not visible in the branch.
- Capture hides the roughest interaction instead of improving it.
- The player needs a written walkthrough to understand the first room.
- The next visible PR adds puzzle-symbol complexity before basic combat feels
  good.
- Finality is implied for placeholder art, audio, UI, balance, or economy.
- Any asset lacks a clear source and commercial-use record.
