# Manual QA Checklist

## Product Read

- [ ] Can a new viewer describe the game from a screenshot?
- [ ] Can a new viewer identify player, enemy, reward, route, and danger?
- [ ] Does the first 20 minutes communicate the hook?
- [ ] Does the build read as a single-player action-adventure, not co-op,
      extraction, hack-and-slash loot, open world, or live-service?
- [ ] Does store-capture content match current gameplay and avoid uncommitted
      platform promises?

## Steam発売前チェックリスト

| Check | Pass Condition | Evidence |
| --- | --- | --- |
| Clean Windows launch | new install opens to title/menu without manual file edits | `artifacts/test-results/manual-steam-launch.md` |
| Single-player flow | new game, continue, settings, pause, quit require no account or server | `artifacts/test-results/manual-single-player-flow.md` |
| Buy-to-play expectation | no in-game wording implies subscriptions, seasons, or online operations | `artifacts/test-results/manual-monetization-copy.md` |
| Store capture sanity | screenshots/trailer show shipped mechanics and final-intent assets only | `artifacts/Reports/manual-store-capture.md` |
| Support basics | build version, crash reporting instructions, and support route are findable | `artifacts/test-results/manual-support-basics.md` |

## Controls

- [ ] Controller can navigate all core gameplay.
- [ ] Keyboard/mouse has complete fallback.
- [ ] Dodge/attack/interact are responsive.
- [ ] Camera does not hide critical action.
- [ ] Controller disconnect/reconnect returns to a playable or pause-safe state.
- [ ] No progression-critical UI requires a mouse-only action.

## Steam Deckテスト表

| Check | Pass Condition | Evidence |
| --- | --- | --- |
| 1280x800 readability | HUD, pause, settings, prompts, and dialogue are readable | `artifacts/screenshots/manual-deck-1280x800/` |
| Default controller path | title to gameplay to quit is reachable without keyboard/mouse | `artifacts/test-results/manual-deck-controller.md` |
| Glyphs/prompts | active prompts match the controller input path | `artifacts/screenshots/manual-deck-glyphs/` |
| Suspend/resume | gameplay resumes without lost input, audio failure, or save damage | `artifacts/test-results/manual-deck-suspend.md` |
| Offline play | launch, load, and short-session completion do not require a live server | `artifacts/test-results/manual-deck-offline.md` |
| 30-minute stability | record major frame pacing, thermal, audio, or crash symptoms | `artifacts/Reports/manual-deck-stability.md` |

Deck findings are evidence for readiness planning only. Do not label the game as
officially compatible or verified from this checklist alone.

## Combat

- [ ] Enemy tells are readable.
- [ ] Boss attacks explain themselves visually and audibly.
- [ ] Player knows why they were hit.
- [ ] Retry is fast.
- [ ] Boss defeat, simultaneous death, retry, and reload preserve valid progress.
- [ ] Boss rooms do not allow a permanent soft-lock after a failed attempt.

## Visual

- [ ] No missing/pink materials.
- [ ] No obvious primitive placeholder treated as final.
- [ ] Hero and boss survive close screenshot.
- [ ] Rewards are visible.
- [ ] Landmarks are memorable.

## Audio

- [ ] Major player actions have SFX.
- [ ] Discovery/reward cues are satisfying.
- [ ] Loops do not click.
- [ ] No license-unknown sounds.

## 回帰テスト表

| Area | Manual Cases | Pass Condition |
| --- | --- | --- |
| Controller | menu navigation, movement, attack, dodge, tool, interact, pause, reconnect | all required actions remain reachable with default mapping |
| Resolution | 1280x800, 1920x1080, fullscreen/windowed if present | no clipped critical UI, unreadable prompts, or input focus loss |
| Save corruption | missing save, truncated save, quit during save indicator | no crash; last valid state or clear recovery path is available |
| Language switch | English/Japanese switch from menu and during restart path | no missing critical text; prompts remain correct |
| Crash | launch, scene transition, boss retry loop, quit | no crash in smoke pass; crash reports include build version when reproduced |
| Boss progression | defeat, death/retry, simultaneous death, reload after defeat | no permanent progression block or invalid defeated-state reset |

## コンソール事前対応表

| Area | Manual Check | Why It Matters Later |
| --- | --- | --- |
| Input | controller can complete core flow without keyboard-only actions | reduces platform-specific control rework |
| Save | player progress survives restart and interrupted-session checks | reduces storage lifecycle risk |
| Suspend/resume | pause and resume from gameplay, menu, and boss retry | prepares for handheld/console lifecycle expectations |
| Display | safe area and readability checks include 1280x800 and TV-like 16:9 | reduces late UI clipping fixes |
| Localization | all player-facing text has English/Japanese coverage or fallback | reduces late text extraction churn |
| Network independence | launch, save, and completion work offline | matches D-020 no-live-server scope |

These checks are only advance hygiene. They are not a commitment to any console
release window or platform feature.

## 30日運用計画

| Period | Manual QA Focus | Exit Signal |
| --- | --- | --- |
| Day 0-3 | reproduce P0 reports: launch, save, progression, crash | every P0 has owner, repro status, and hotfix decision |
| Day 4-7 | controller, Deck, boss retry, readability, language reports | P1 clusters are verified and regression scope is named |
| Day 8-14 | patch candidate smoke and changed-area regression | patch notes match verified fixes |
| Day 15-30 | stability pass and known-issues cleanup | unresolved issues are prioritized P1-P3 with workarounds where possible |

## ユーザー告知テンプレート

### Bug Report Request

Please include your build version, Windows or Steam Deck environment, input
device, selected language, where the issue happened, and whether it still
happens after restarting the game.

### Known Issue

We are investigating `<brief issue>`. Current status:
`investigating / fixed internally / fix scheduled`. Current workaround:
`<workaround or none>`.

### Hotfix Note

Hotfix `<version>` is live. This update fixes `<launch/save/progression/crash/controller summary>`.
Please restart Steam to receive the patch.

## バグ優先度表

| Priority | Manual QA Definition | Examples |
| --- | --- | --- |
| P0 | cannot launch, cannot continue, save damage, or main progression blocked | startup failure, corrupted save, boss gate never opens |
| P1 | crash, common controller lockout, boss/tool progression risk | crash on area load, retry loop soft-lock, controller trapped in menu |
| P2 | serious readability, missing required cue, common non-blocking gameplay defect | Deck HUD clipped, boss tell unclear, language fallback missing |
| P3 | typo, rare cosmetic issue, minor polish | text error, rare visual overlap, small audio imbalance |
