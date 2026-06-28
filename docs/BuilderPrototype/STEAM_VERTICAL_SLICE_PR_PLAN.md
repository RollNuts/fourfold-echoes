# Steam Vertical Slice PR Plan

## Purpose

Create PR-ready development lanes for an original Steam indie prototype that
combines block building, Diablo-like ARPG buildcraft, FF14-like positional
telegraph combat, Tarkov-like local run tension, addictive loot, and
streamer-friendly moments.

The old repository direction is not the product source of truth for this lane.
Implementation PRs should stay isolated under Builder Prototype files until
Veripsa and review comments say otherwise.

## Veripsa Merge Ordering

1. PR-01: Builder Prototype Spine
2. PR-02: Block Build/Edit Loop
3. PR-03: Positional Combat Arena
4. PR-04: Run Stakes And Extraction Tension
5. PR-05: Loot And Buildcraft Loop
6. PR-06: Streamer Moment Pass

Every PR must be small, reviewable, and ready for review. PR comments must not
be ignored: either implement the requested change or reply with a concrete
technical reason and get reviewer agreement.

## PR-01: Builder Prototype Spine

Goal: establish an isolated prototype scene, control assumptions, and testable
slice shell without touching shipping scenes.

Acceptance checks:
- Prototype opens/runs without changing existing production scenes.
- Player can move with keyboard and controller-equivalent bindings.
- Build, combat, loot, and run-state hooks are stubbed or documented.
- PR body includes Unity version, manual playtest path, and changed files.

Draft PR title: `Builder Prototype: add isolated vertical slice spine`

## PR-02: Block Build/Edit Loop

Goal: prove the builder core interaction: place, remove, preview, and validate
blocks in a compact combat-readable arena.

Acceptance checks:
- Player can place and remove prototype blocks.
- Invalid placements are rejected before commit.
- Blocks affect movement/collision consistently.
- The loop stays isolated from shipping scenes.

Draft PR title: `Builder Prototype: add block placement and edit loop`

## PR-03: Positional Combat Arena

Goal: prove FF14-like positional telegraph combat in a top-down ARPG format.

Acceptance checks:
- Enemy telegraphs are readable before damage resolves.
- Player can avoid attacks through movement and positioning.
- Front/flank/rear or safe/unsafe resolution is deterministic.
- Built-block interaction rules are documented.

Draft PR title: `Builder Prototype: add positional telegraph combat arena`

## PR-04: Run Stakes And Extraction Tension

Goal: add Tarkov-like pressure without networking: the player should feel
tension from carrying rewards, deciding whether to push deeper, and extracting
before temporary gains are lost.

Acceptance checks:
- Player can extract after earning a reward.
- Pushing deeper increases danger and potential reward.
- Failure has a visible local cost but does not corrupt saves or block testing.
- No multiplayer, backend, account, real-money economy, or live-service system.

Draft PR title: `Builder Prototype: add run stakes and extraction tension`

## PR-05: Loot And Buildcraft Loop

Goal: prove the Diablo-style reward loop without a heavy inventory: drops should
change moment-to-moment build choices and make replays feel different.

Acceptance checks:
- Clearing an encounter produces visible loot.
- Equipping loot changes combat, movement, or block behavior.
- Player can compare and replace current gear quickly.
- No stash, market, networking, or permanent economy is introduced.

Draft PR title: `Builder Prototype: add run-only loot and build modifiers`

## PR-06: Streamer Moment Pass

Goal: add repeatable spectacle hooks that create clips without expanding scope
into a content treadmill.

Acceptance checks:
- Moment triggers are deterministic or controllably seeded for QA.
- Effects are readable in a 10-20 second clip.
- Spectacle does not hide telegraphs, player state, or extraction risk.
- No chat integration, social feature, or external dependency is added.

Draft PR title: `Builder Prototype: add streamer-friendly vertical slice moments`

## Cross-PR Gate

- Confirm the branch only touches files agreed in that PR.
- Confirm unrelated modified files are not included.
- Confirm PR comments are resolved before merge.
- Confirm the PR body includes validation results.
- Confirm the Steam promise still reads clearly: build a space, fight through
  positional danger, decide whether to push or extract, earn build-changing
  loot, and create at least one memorable clip-worthy moment.
