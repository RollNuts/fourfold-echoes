# D021 Scope, QA, Release, And Store

Status: required release discipline.

## Change-Control Template

| Field | Entry |
| --- | --- |
| Change title |  |
| Problem solved |  |
| Existing D021 item affected |  |
| Trailer value | none / weak / strong |
| First-30-minute value | none / weak / strong |
| Implementation cost | low / medium / high |
| QA cost | low / medium / high |
| Art/audio impact | improves / neutral / delays |
| Existing item removed |  |
| Second item removed |  |
| Cap impact | none / trade required / exceeds cap |
| Decision | accept / defer / reject |
| Owner/date |  |

Rule: empty fields, no removed items, or cap breach means reject/defer.

## Weekly Review

| Review | Question | Evidence | Outcome |
| --- | --- | --- | --- |
| Play | Is objective, danger, reward, retry clear in 15 minutes? | capture/build notes | keep/fix/cut |
| Store | Can one screenshot explain player, tool, route, enemy, reward? | candidate shot | pass/fail |
| QA | Top blockers assigned? | bug list | fix/defer |
| Scope | Any cap exceeded? | cap diff | maintain/cut |
| Audio/Art | Any placeholder in public-facing path? | checklist | pass/fix |
| Trailer | Did the week produce real footage value? | 10-30s clip | yes/no |

## Completion, Improvement, Delete

| Category | Examples | Rule |
| --- | --- | --- |
| Complete | title, hub, Region 01, tool, boss, save, required UI/audio | finish before adding |
| Improve | enemy readability, boss tell, tool target, HUD, pause/settings, SFX mix | improve if it raises clarity |
| Delete | extra systems, stale D020 copy, open-world claims, extraction loss language, placeholder market media | remove when it confuses product |

## Steam Release Checklist

| Area | Required Before Public Release |
| --- | --- |
| Build | clean Windows install launches through Steam; version/build SHA visible |
| Flow | title, new game, continue, settings, pause, quit are controller-accessible |
| Save | local save/load survives restart; corrupt save has safe fallback |
| Input | controller and keyboard complete the slice |
| Display | 1280x800, 1920x1080, fullscreen/windowed safe checks pass |
| Audio/Art | no placeholder art/audio in screenshots or trailers |
| Store | copy matches implemented or locked scope only |
| Support | crash/log path, support contact, known-issues format |
| Legal | asset rights, AI-use policy if applicable, licenses, credits checked |

## Steam Deck Tests

| Test | Pass Condition |
| --- | --- |
| 1280x800 | HUD, prompts, pause/settings, boss HP readable |
| Default controller | start, play, pause, quit, retry, tool, interact all reachable |
| Glyphs | prompts match active controller path |
| Suspend/resume | no lost input or save damage |
| Offline | launch, load, play, save, quit without server dependency |
| Performance | 30-minute slice records frame pacing and audio stability |
| Text | JP/EN priority strings readable |
| Save | Deck save roundtrip and interrupted-session checks pass |

Do not claim Steam Deck Verified or Playable until measured and accepted through
the actual Steam process.

## Regression Tests

| Area | Cases |
| --- | --- |
| Controller | menu navigation, movement, attack, dodge, tool, interact, pause, reconnect |
| Resolution | 1280x800, 1920x1080, fullscreen/windowed, ultrawide safe framing |
| Save corruption | missing, old version, truncated, interrupted save |
| Language | EN/JP switch or fallback, missing string fallback |
| Crash | launch, scene transition, boss retry x10, quit, reload after death |
| Boss softlock | death/retry, defeat flag, simultaneous death, shortcut reload |
| Tool | valid target, no target, solved target, reload solved state |
| Audio/UI | required cues fire; boss HP, save indicator, prompts readable |

## 30-Day Hotfix Plan

| Window | Focus |
| --- | --- |
| Day 0-3 | P0 only: launch failure, save damage, progression blockers, repeatable crashes |
| Day 4-7 | P1 clusters: controller lockout, Deck blockers, boss retry, language-critical issues |
| Day 8-14 | stability patch, frequent P2 issues, readability fixes, support FAQ updates |
| Day 15-30 | known-issues cleanup, final stability pass, no scope additions |

## Bug Priorities

| Priority | Definition |
| --- | --- |
| P0 | cannot launch, cannot continue, save corruption/loss, main progression impossible |
| P1 | crash, common controller lockout, boss/tool progression risk, repeated softlock |
| P2 | serious readability, Deck clipping, missing required cue, language fallback issue |
| P3 | typo, rare cosmetic issue, minor polish |

## Announcement Templates

Early update:

```text
FOURFOLD ECHOES is now focused on a compact single-player top-down adventure:
one hub, three regions, four bosses, and one exploration tool. We are building
toward a Steam-first buy-to-play release and will only show features that are in
the playable build.
```

Patch note:

```text
This update improves the playable slice: clearer controller flow, safer save
handling, more readable UI at 1280x800, and updated wording that matches the
current product direction.
```

Known issue:

```text
Known issue: [issue]. Workaround: [steps]. Priority: [P0-P3]. We will update
this note after the next verified build.
```
