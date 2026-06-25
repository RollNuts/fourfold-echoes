# Validation Report

Status: D-020 planning and evidence ledger.

This file records what has actually been verified for the current D-020 target.
Old prototype harnesses are historical and must not be used as proof that the
current game is fun, marketable, or visually ready.

## Current Verified Baseline

| Check | Result | Evidence |
| --- | --- | --- |
| Product canon validation | PASS | `node Scripts/Validation/validate_repo.mjs` |
| Forge/Game IR check | PASS | `node tools/forge/check.mjs` |
| Product lane split | PASS locally | `artifacts/Reports/veripsa-current-split.md` |
| D-020 art/audio docs | PASS locally | PR-B validation and D-020 registers |
| D-020 playable smoke | PASS locally | `artifacts/Reports/unity-product-validation.md`; movement, dodge, attack hit, enemy hit, tool activation |

## Not Yet Verified

| Area | Required Evidence |
| --- | --- |
| Production-styled D-020 Region 01 room | Unity playthrough capture from styled current scene |
| Controller and keyboard input parity | controller and keyboard smoke test |
| Full exploration tool behavior | valid target, invalid target, cooldown/ready, route response |
| Two gimmick rooms | same tool used in two different room contexts |
| Shortcut | shortcut opens, remains open after save/load |
| Two enemies | readable tells, hit confirm, defeat, retry loop |
| Miniboss and boss | progression, death/retry, defeat flag, no softlock |
| Two relic rewards | reward feedback without inventory UI |
| Minimum UI | health, tool state, prompt, boss health, pause/settings |
| BGM 2 and minimum SFX | cues fire in actual gameplay, not only in registers |
| Save/load | versioned local save roundtrip and corruption fallback |
| Store capture readiness | screenshots/video from real gameplay, no gray-box art/audio |

## Current D-020 Non-Goals Guard

Validation must reject new current-scope claims for:

- large seamless-world structure
- multiple exploration tools
- inventory, crafting, quest log, or social systems
- multiplayer, matchmaking, online accounts, dedicated servers, or live service
- day/night, fishing, farming, base building, survival loops
- placeholder art/audio in market-validation captures

## Evidence Rules

- A passing document check is not proof of gameplay quality.
- A generated scene is not proof of market readiness.
- A screenshot must come from the current D-020 playable build to count for
  store-readiness review.
- Audio is not accepted until the cue is heard in gameplay context.
- Historical prototype evidence may prove tooling, but not product direction.
