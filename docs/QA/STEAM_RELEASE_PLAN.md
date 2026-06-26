# Steam Release QA Plan

Status: D-020 planning document.

References:

- Steam Store Graphical Assets: https://partner.steamgames.com/doc/store/assets
- Steam Deck Compatibility Review: https://partner.steamgames.com/doc/steamdeck/compat

## Steam発売前チェックリスト

| Gate | Owner | Status | Evidence |
| --- | --- | --- | --- |
| Windows build launches from a clean install | Release | Planned | `artifacts/test-results/launch-clean-install.md` |
| Steam client launch path is smoke-tested on a clean Windows account | Release | Planned | `artifacts/test-results/steam-launch-path.md` |
| Main menu, new game, continue, settings, and quit work with controller | QA | Planned | `artifacts/test-results/controller-menu.md` |
| 1280x800 and 1920x1080 layouts are readable | QA/UI | Planned | `artifacts/screenshots/ui-readability/` |
| Local save/load survives app restart | QA/Engineering | Planned | `artifacts/test-results/save-roundtrip.md` |
| Save corruption fallback is tested | QA/Engineering | Planned | `artifacts/test-results/save-corruption.md` |
| Controller and keyboard can complete the game | QA | Planned | `artifacts/test-results/full-clear-inputs.md` |
| No required mouse-only UI | QA/UI | Planned | `artifacts/test-results/no-mouse-only.md` |
| Offline play is verified for the shipped single-player build | QA | Planned | `artifacts/test-results/offline-play.md` |
| No live server dependency is required for launch, save, or completion | Release/Engineering | Planned | `artifacts/test-results/no-server-dependency.md` |
| No placeholder art/audio in store captures | Art/Audio | Planned | `artifacts/Reports/store-capture-readiness.md` |
| Store screenshots show real gameplay | Marketing | Planned | `artifacts/store/screenshots/manifest.md` |
| Trailer footage is captured from current gameplay | Marketing | Planned | `artifacts/store/trailer/source-captures.md` |
| BGM/SFX licenses are documented | Audio/Legal | Planned | `docs/Audio/ASSET_REGISTER.csv`, `docs/Legal/LICENSES.md` |
| Third-party asset licenses are documented | Art/Legal | Planned | `docs/ASSET_RIGHTS.md` |
| AI-use policy is reviewed for shipped content | Legal/Production | Planned | `docs/Legal/AI_USAGE_POLICY.md` if needed |
| Crash logs path and support contact are documented | Release | Planned | `docs/Release/SUPPORT.md` |
| Credits and licenses page exists | Release/Legal | Planned | in-game credits scene plus `docs/Legal/LICENSES.md` |
| Build version and commit SHA are visible in debug or credits | Engineering | Planned | `artifacts/test-results/version-display.md` |
| Store page wording matches D-020: single-player, Steam-first, buy-to-play | Release/Marketing | Planned | `artifacts/Reports/store-copy-check.md` |
| Store page avoids uncommitted console, co-op, open-world, extraction, or live-service claims | Release/Marketing | Planned | `artifacts/Reports/store-claim-check.md` |

Release gate rule: a public release candidate cannot ship while any confirmed
P0 remains open. P1 exceptions require a named owner, player-facing workaround,
and explicit release sign-off.

## Steam Deckテスト表

| Item | Test | Pass Condition | Status | Evidence |
| --- | --- | --- | --- | --- |
| Device profile | record device, SteamOS, Proton, build SHA | environment known | Not yet measured | `artifacts/test-results/deck-environment.md` |
| Resolution | 1280x800 | no clipped HUD or unreadable menus | Not yet measured | `artifacts/screenshots/deck-1280x800/` |
| Controller | default layout | all gameplay and menus reachable | Not yet measured | `artifacts/test-results/deck-controller.md` |
| Glyphs | controller prompts | no keyboard-only prompts in gameplay | Not yet measured | `artifacts/screenshots/deck-glyphs/` |
| Text | UI/HUD readability | no critical text below practical Deck readability | Not yet measured | `artifacts/screenshots/deck-text/` |
| Performance | gameplay slice | record FPS target and frame-time spikes | Not yet measured | `artifacts/Reports/deck-frame-time.md` |
| Suspend/resume | pause during gameplay, resume | state recovers without input loss | Not yet measured | `artifacts/test-results/deck-suspend-resume.md` |
| Save | save, close, relaunch | progress roundtrips | Not yet measured | `artifacts/test-results/deck-save.md` |
| Video settings | fullscreen/windowed equivalents | no broken aspect ratio | Not yet measured | `artifacts/test-results/deck-video.md` |
| Audio | speakers/headphones | no missing required cues | Not yet measured | `artifacts/test-results/deck-audio.md` |
| Offline mode | start Steam offline, launch, load, complete a short slice | no blocked launch, save, or progression path | Not yet measured | `artifacts/test-results/deck-offline.md` |
| Thermal/battery notes | 30-minute gameplay slice | record observed stability and major throttling symptoms | Not yet measured | `artifacts/Reports/deck-30min-stability.md` |
| On-screen keyboard | rename/input fields if present | no required text entry blocks progression | Not yet measured | `artifacts/test-results/deck-text-entry.md` |

Deck status must be described as a target until measured on device. Do not imply
official compatibility, verification, or performance tiers until there is
current evidence.

## 回帰テスト表

| Area | Cases | Pass Condition | Evidence |
| --- | --- | --- | --- |
| Controller | move, attack, dodge, tool, interact, pause | every required action is reachable from default controller mapping | `artifacts/test-results/regression-controller.md` |
| Controller | disconnect/reconnect during gameplay and menus | input recovers or shows a clear pause-safe state | `artifacts/test-results/regression-controller-reconnect.md` |
| Resolution | 1280x800, 1920x1080, ultrawide safe check | no clipped HUD, unreadable menus, or blocked command prompts | `artifacts/test-results/regression-resolution.md` |
| Resolution | fullscreen/windowed toggle if present | aspect ratio and input focus recover after switching | `artifacts/test-results/regression-display-mode.md` |
| Save corruption | missing save, old version save, truncated save | game does not crash; player receives a recoverable path | `artifacts/test-results/regression-save-corruption.md` |
| Save corruption | quit during save indicator | next launch preserves last valid state or falls back safely | `artifacts/test-results/regression-save-interrupt.md` |
| Language | English/Japanese switch, missing text fallback | language changes without restart-only dead ends or missing critical strings | `artifacts/test-results/regression-language.md` |
| Language | controller glyphs after language switch | input prompts remain correct for the active device | `artifacts/test-results/regression-language-glyphs.md` |
| Crash | launch, scene transition, boss retry, quit | no crash; logs include build version if a crash is reproduced | `artifacts/test-results/regression-crash.md` |
| Crash | repeated retry loop for 10 attempts | memory, audio, and input remain stable enough to continue | `artifacts/test-results/regression-retry-loop.md` |
| Boss progression | death/retry, defeat flag, shortcut unlock, reload after defeat | boss cannot become permanently unbeatable or already-defeated incorrectly | `artifacts/test-results/regression-boss-progression.md` |
| Boss progression | defeat during simultaneous player death | result is deterministic and reload state is valid | `artifacts/test-results/regression-boss-edge.md` |
| Exploration tool | no target, valid target, solved target, reload solved state | tool feedback and solved-state persistence are clear | `artifacts/test-results/regression-tool.md` |
| Audio | all required cue categories fire once in slice | no missing critical combat, reward, or UI cues | `artifacts/test-results/regression-audio.md` |
| UI | prompts, boss health, pause, settings, save indicator | state is readable with controller and at Deck resolution | `artifacts/test-results/regression-ui.md` |

## コンソール事前対応表

| Concern | Prepare Now | Console Risk Avoided |
| --- | --- | --- |
| Input | controller-first architecture, no keyboard-only actions | platform controller certification churn |
| Save | save service abstraction, versioned data, no direct platform calls | storage API rewrite |
| Suspend/resume | deterministic pause/recover path from every scene | console lifecycle failure |
| Performance | texture, material, VFX, and mesh budgets recorded | late memory/perf collapse |
| Display | safe-area aware UI and 1280x800-first checks | TV/handheld clipping |
| Glyphs | action IDs independent from displayed button art | platform-specific prompt rewrite |
| Platform services | achievements/cloud wrappers only when needed | gameplay coupled to Steam |
| Localization | string IDs for player-facing UI | late text extraction |
| Certification risk | no hardcoded personal paths, no network dependency | immediate console rejection |
| Build identity | clear version string and platform label in support reports | ambiguous external bug reports |
| Entitlements | keep purchase/platform checks outside core progression | platform-specific storefront coupling |
| Age/rating materials | record content descriptors from actual shipped content | late store submission churn |
| Patch discipline | release notes tied to fixed issue IDs | unverifiable update history |

These items are preparatory only. They do not commit the project to a console
date, platform feature, or certification claim.

## 30日運用計画

| Period | Focus | Release Branch Rule |
| --- | --- | --- |
| Day 0-3 | P0 launch failures, crashes, save corruption, progression blockers | hotfix branch from shipped tag; one fix per commit |
| Day 4-7 | controller, Deck, boss retry, major readability fixes | batch P1 fixes after regression smoke |
| Day 8-14 | frequent P1/P2 bugs, small QoL, store FAQ updates | patch notes drafted before build upload |
| Day 15-30 | stability patch, known issues update, roadmap only for committed fixes | no scope additions, only verified fixes |

Intake channels:

- Steam forum bug thread
- support email
- crash logs attached by user
- known-issues page maintained from confirmed reports only

Daily release operations:

| Cadence | Action | Owner |
| --- | --- | --- |
| Daily during Day 0-7 | triage new reports into P0-P3 and duplicates | QA lead |
| Daily during Day 0-7 | update known issues when a confirmed player-impacting issue changes state | Release |
| Twice weekly during Day 8-30 | review unresolved P1/P2 clusters and patch readiness | QA/Release |
| Before every hotfix | run the launch, save, controller, crash, and changed-area regression set | QA |
| After every hotfix | verify download, launch, version string, and public notes | Release |

Hotfix verification:

1. Reproduce or classify as credible high-risk report.
2. Patch on release branch.
3. Run launch, save, controller, boss progression, and changed-area tests.
4. Update known issues and patch notes.
5. Keep previous public build available if a rollback path is needed.

## ユーザー告知テンプレート

Today's update focuses on stability, progression, controls, and save reliability.
We fixed issues that could block progress or make combat/tool feedback unclear.
We are still tracking reported issues and will prioritize fixes that affect
completion, crashes, and controller play first.

### Known Issue Template

We are investigating an issue where `<brief issue>`. Current status:
`investigating / fixed internally / fix scheduled`. If you encounter it, please
send your save file, build version, input device, and the last room or boss you
entered.

### Hotfix Shipped Template

Hotfix `<version>` is live. This update fixes `<P0/P1 summary>`. Please restart
Steam to receive the patch. We will continue tracking reports that affect saves,
progression, crashes, and controller play first.

### Hotfix Delayed Template

The fix for `<issue>` needs additional verification because it touches saves or
progression. We will not ship it until it passes the regression checks. Current
workaround: `<workaround or none>`.

### Known Issues Update Template

Known issues updated for `<date>`:

- Fixed in latest build: `<issue list or none>`
- Still investigating: `<issue list or none>`
- Workarounds: `<short workaround or none>`

We are prioritizing issues that affect launching, saves, progression, crashes,
and controller play.

### Support Request Template

When reporting a bug, please include the build version, Windows or Steam Deck
environment, input device, selected language, where it happened, whether it
reproduces after restart, and any crash log or save file you are comfortable
sending.

## バグ優先度表

| Priority | Definition | Examples | Response Target |
| --- | --- | --- | --- |
| P0 | launch failure, save corruption, progression impossible | cannot start game, save deleted, boss door never opens | immediate hotfix candidate |
| P1 | crash, boss/tool progression break, controller lockout | crash on region load, tool node soft-lock, pause menu traps controller | next patch priority |
| P2 | serious readability, UI clipping, missing key SFX, common collision issue | Deck HUD clipped, boss tell unclear, required cue missing | scheduled patch |
| P3 | typo, rare visual issue, minor polish | minor text error, rare prop overlap | batch fix |

Priority modifiers:

- Raise one level when the issue affects save integrity, completion, launch, or
  the default controller path.
- Raise one level when multiple independent reports include the same clear repro.
- Lower only after QA confirms a reliable workaround that does not require
  editing files or using developer tools.
- Never lower a crash, save, or progression issue solely because it is rare.

Duplicate and cannot-reproduce handling:

- Duplicate reports are linked to the oldest confirmed issue.
- Cannot-reproduce issues remain open if they mention save loss, crash, or
  progression. Otherwise they move to watchlist after two failed repro attempts.
