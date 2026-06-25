# Steam Release QA Plan

Status: D-020 planning document.

References:

- Steam Store Graphical Assets: https://partner.steamgames.com/doc/store/assets
- Steam Deck Compatibility Review: https://partner.steamgames.com/doc/steamdeck/compat

## Steam発売前チェックリスト

| Gate | Owner | Status | Evidence |
| --- | --- | --- | --- |
| Windows build launches from a clean install | Release | Planned | `artifacts/test-results/launch-clean-install.md` |
| Main menu, new game, continue, settings, and quit work with controller | QA | Planned | `artifacts/test-results/controller-menu.md` |
| 1280x800 and 1920x1080 layouts are readable | QA/UI | Planned | `artifacts/screenshots/ui-readability/` |
| Local save/load survives app restart | QA/Engineering | Planned | `artifacts/test-results/save-roundtrip.md` |
| Save corruption fallback is tested | QA/Engineering | Planned | `artifacts/test-results/save-corruption.md` |
| Controller and keyboard can complete the game | QA | Planned | `artifacts/test-results/full-clear-inputs.md` |
| No required mouse-only UI | QA/UI | Planned | `artifacts/test-results/no-mouse-only.md` |
| No placeholder art/audio in store captures | Art/Audio | Planned | `artifacts/Reports/store-capture-readiness.md` |
| Store screenshots show real gameplay | Marketing | Planned | `artifacts/store/screenshots/manifest.md` |
| Trailer footage is captured from current gameplay | Marketing | Planned | `artifacts/store/trailer/source-captures.md` |
| BGM/SFX licenses are documented | Audio/Legal | Planned | `docs/Audio/ASSET_REGISTER.csv`, `docs/Legal/LICENSES.md` |
| Third-party asset licenses are documented | Art/Legal | Planned | `docs/ASSET_RIGHTS.md` |
| AI-use policy is reviewed for shipped content | Legal/Production | Planned | `docs/Legal/AI_USAGE_POLICY.md` if needed |
| Crash logs path and support contact are documented | Release | Planned | `docs/Release/SUPPORT.md` |
| Credits and licenses page exists | Release/Legal | Planned | in-game credits scene plus `docs/Legal/LICENSES.md` |
| Build version and commit SHA are visible in debug or credits | Engineering | Planned | `artifacts/test-results/version-display.md` |

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

## 回帰テスト表

| Area | Cases | Evidence |
| --- | --- | --- |
| Controller | move, attack, dodge, tool, interact, pause; no guard unless implemented | `artifacts/test-results/regression-controller.md` |
| Resolution | 1280x800, 1920x1080, ultrawide safe check | `artifacts/test-results/regression-resolution.md` |
| Save corruption | missing save, old version save, truncated save | `artifacts/test-results/regression-save-corruption.md` |
| Language | English/Japanese switch, missing text fallback | `artifacts/test-results/regression-language.md` |
| Crash | launch, scene transition, boss retry, quit | `artifacts/test-results/regression-crash.md` |
| Boss progression | death/retry, defeat flag, shortcut unlock, reload after defeat | `artifacts/test-results/regression-boss-progression.md` |
| Exploration tool | no target, valid target, solved target, reload solved state | `artifacts/test-results/regression-tool.md` |
| Audio | all required cue categories fire once in slice | `artifacts/test-results/regression-audio.md` |
| UI | prompts, boss health, pause, settings, save indicator | `artifacts/test-results/regression-ui.md` |

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

## バグ優先度表

| Priority | Definition | Examples | Response Target |
| --- | --- | --- | --- |
| P0 | launch failure, save corruption, progression impossible | cannot start game, save deleted, boss door never opens | immediate hotfix candidate |
| P1 | crash, boss/tool progression break, controller lockout | crash on region load, tool node soft-lock, pause menu traps controller | next patch priority |
| P2 | serious readability, UI clipping, missing key SFX, common collision issue | Deck HUD clipped, boss tell unclear, required cue missing | scheduled patch |
| P3 | typo, rare visual issue, minor polish | minor text error, rare prop overlap | batch fix |

Duplicate and cannot-reproduce handling:

- Duplicate reports are linked to the oldest confirmed issue.
- Cannot-reproduce issues remain open if they mention save loss, crash, or
  progression. Otherwise they move to watchlist after two failed repro attempts.
