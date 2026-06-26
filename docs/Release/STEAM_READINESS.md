# Steam Readiness

Status: D-020 draft.

## Store Promise

Explore three compact fantasy regions from one hub, master one exploration tool,
open shortcuts, find relic rewards, and defeat four readable bosses.

## Core Hook Sentence

FOURFOLD ECHOES is a single-player top-down action-adventure built around a
small rule set: readable combat, one exploration tool, compact rooms,
shortcuts, and bosses.

## Short Description Draft

FOURFOLD ECHOES is a compact top-down fantasy action-adventure with one hub,
three handcrafted regions, one exploration tool, and four bosses. No bloated map
sprawl, no live-service systems, and no crafting treadmill.

## Screenshot Pillars

| Shot | Purpose |
| --- | --- |
| Hub gates | shows structure and safe return |
| Tool use | shows the core exploration verb |
| Room problem | shows a compact adventure puzzle/gimmick |
| Shortcut open | shows navigation reward |
| Sword combat | shows attack, dodge, hit confirm |
| Miniboss | shows escalation |
| Boss arena | shows large readable threat |
| Relic reward | shows reward without inventory bloat |

## Trailer Shot List

1. Hero leaves hub toward Region 01.
2. Top-down movement and camera show the path clearly.
3. Exploration tool pulse reveals or activates a route object.
4. First enemy fight shows attack, dodge, and hit confirm.
5. Gimmick room uses the same tool differently.
6. Shortcut opens and changes the route.
7. Relic reward appears and is collected.
8. Boss telegraph forces movement, dodge, and tool timing.
9. Title and wishlist callout.

## Demo Acceptance

- Hub 1.
- Exploration area 1.
- Normal enemies 2.
- Miniboss 1.
- Boss 1.
- Exploration tool 1.
- Shortcut 1.
- Gimmick rooms 2.
- Relic rewards 2.
- UI minimum.
- BGM 2 tracks.
- Minimum SFX set.
- Local save/load.
- Controller support.
- Stable Windows build.
- No placeholder hero, tool, boss, UI, or audio in store captures.

## Hardware Messaging

- Steam Windows first.
- Steam Deck target only after measurement.
- macOS build is development evidence only unless a later product decision says
  otherwise.
- PS5/Switch/Xbox are later feasibility targets, not launch promises.

## Steam発売前チェックリスト

| Readiness Item | Public-Repo Check | Ship Gate |
| --- | --- | --- |
| Product scope | copy says single-player, Steam-first, buy-to-play | no co-op, live-service, extraction, hack-and-slash loot, or open-world claims |
| Windows launch | clean install starts from Steam client path | no confirmed P0 launch failure |
| Offline play | launch, save, load, and core completion path work without a live server | no server dependency for shipped gameplay |
| Controller path | menus and gameplay are completable with controller | no mouse-only blocker |
| Save reliability | restart, continue, and corruption fallback are tested | no confirmed save-loss P0 |
| Store assets | screenshots and trailer use current gameplay | no placeholder or uncommitted feature shown |
| Legal/support basics | licenses, credits, support route, and build version are documented | no unknown shipped rights issue |

## Steam Deckテスト表

| Area | Readiness Evidence | Messaging Rule |
| --- | --- | --- |
| Device environment | record device, SteamOS, Proton/build details | describe as tested environment, not universal promise |
| 1280x800 UI | screenshots for HUD, pause, settings, prompts, dialogue | no official compatibility wording from screenshots alone |
| Controller | full menu and gameplay smoke with default layout | do not claim final layout until measured |
| Suspend/resume | gameplay and menu resume checks | report observed result only |
| Offline play | launch/load/short slice without live server | required for D-020 scope |
| Stability | 30-minute gameplay note for crash, audio, and frame symptoms | state target only until current evidence exists |

## 回帰テスト表

| Area | Required Before Release Candidate | Blocker Threshold |
| --- | --- | --- |
| Controller | menu, gameplay, pause, reconnect, no mouse-only blocker | P0/P1 if default controller blocks completion |
| Resolution | 1280x800, 1920x1080, fullscreen/windowed if present | P1 if critical UI is clipped or unreadable |
| Save corruption | missing, old, truncated, interrupted-save cases | P0 if data loss or crash blocks continue |
| Language switch | English/Japanese switch and fallback review | P1 if critical text disappears |
| Crash | launch, scene transition, boss retry loop, quit | P0/P1 by frequency and progression impact |
| Boss progression | death, retry, simultaneous death, defeat flag, reload | P0 if completion can be permanently blocked |

## コンソール事前対応表

| Area | Prepare Without Expanding Scope | Later Risk Reduced |
| --- | --- | --- |
| Input | controller-first UI and gameplay checks | platform control rework |
| Save | versioned local save behavior and recovery checks | storage lifecycle issues |
| Suspend/resume | pause-safe recovery from gameplay and menus | handheld/console lifecycle issues |
| Display | safe-area and 1280x800/16:9 readability checks | TV/handheld clipping |
| Localization | string coverage for player-facing text | late localization extraction |
| Platform coupling | no live server or Steam-only gameplay dependency | portability risk |

These are general preparedness checks for a public planning repo. They do not
claim any platform approval, certification result, or console release date.

## 30日運用計画

| Period | Release Management Focus | Public Communication |
| --- | --- | --- |
| Day 0-3 | triage P0 launch, save, crash, and progression reports daily | acknowledge confirmed blockers and workarounds |
| Day 4-7 | batch P1 controller, Deck, boss retry, readability, and language fixes | publish concise hotfix notes |
| Day 8-14 | prepare stability patch from verified clusters | update known issues before patch release |
| Day 15-30 | close duplicate clusters and keep patch scope limited to verified fixes | avoid roadmap promises beyond committed fixes |

Hotfixes must be based on the shipped release branch, include changed-area
regression, and keep previous public build rollback options where practical.

## ユーザー告知テンプレート

### Launch Known Issues

We are tracking confirmed issues that affect launching, saves, progression,
crashes, controller play, and Steam Deck testing. Please include your build
version, device, input method, selected language, and where the issue happened.

### Hotfix Live

Hotfix `<version>` is live. This update fixes `<short verified fix summary>`.
Please restart Steam to receive the patch. We are continuing to prioritize
launch, save, progression, crash, and controller issues first.

### Fix Still In Testing

The fix for `<issue>` needs more verification because it touches saves,
progression, or launch stability. Current workaround: `<workaround or none>`.

## バグ優先度表

| Priority | Release Definition | Release Action |
| --- | --- | --- |
| P0 | cannot launch, save is damaged, or main progression is impossible | block release or ship immediate hotfix candidate |
| P1 | crash, common controller lockout, boss/tool progression risk | next patch priority after focused regression |
| P2 | serious readability, missing required cue, common non-blocking defect | scheduled patch after P0/P1 |
| P3 | typo, rare cosmetic issue, minor polish | batch when low risk |

## AI Disclosure Considerations

Do not ship visibly AI-generated identity assets without human review, terms
proof, and disclosure decision. Procedural internal assets are allowed when
repository-authored and documented.
