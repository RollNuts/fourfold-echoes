# D021 Vertical Slice Plan

Status: build-order contract.

## Completion Checklist

| Area | Acceptance |
| --- | --- |
| Title | controller-accessible new game, continue, settings, quit |
| Hub | one gate, one mission briefing, result summary, safe pause |
| Region 01 | two tool rooms, one shortcut, two normal enemies, one miniboss, one boss |
| Player | movement, normal attack, dodge, hit confirm, damage, death/retry |
| Tool | ready, pulse, valid target, solved target, fail state, cooldown |
| Boss | readable tells, boss HP, one tool-created opening, defeat event |
| Reward | two visible reward beats, no inventory screen |
| Save | local save/load, backup fallback, settings persistence |
| Audio | required SFX and hub/region/boss music cues |
| UI | 1280x800 and 1920x1080 readable |
| Evidence | real gameplay screenshots, no placeholder claim |

## Weekly Plan

| Week | Goal | Done When |
| --- | --- | --- |
| 1 | D021 canon, UI copy, validation gates | old direction no longer appears in player-facing flow |
| 2 | Bootstrap/title/hub/persistent systems | new game, continue, settings, hub loop work |
| 3 | Region 01 movement/combat/tool room | one enemy and one tool room are playable |
| 4 | Shortcut, reward, miniboss | route change and reward beat are visible |
| 5 | Boss 01 | boss defeated and return progress saved |
| 6 | UI/UX polish | controller, pause, settings, failure, result screens are shippable |
| 7 | Audio pass | required cues and minimum BGM are present |
| 8 | Visual/audio evidence pass | market screenshots/trailer beats use real gameplay |

## Task Ownership

| Lane | Owns |
| --- | --- |
| Product | caps, accepted scope, store promises |
| Gameplay | movement, combat, tool, enemies, boss, room flow |
| UI/UX | title, HUD, prompts, pause, settings, save/failure/result |
| Art | hero, enemy, region, tool, reward, lighting, VFX |
| Audio | SFX, BGM, cue routing, mix |
| QA/Release | Deck, regression, save, build, Steam evidence |
| Veripsa | lane coupling and safe land order |

## Risks

| Risk | Response |
| --- | --- |
| Old D020 language returns | run contract validator and reject player-facing stale copy |
| Art lane blocks playability | use readable temporary visuals but never market them |
| UI grows into menus | reject inventory, quest, build, or social screens |
| Boss becomes reaction-test only | use position, tool opening, and tells |
| Audio delayed | required cue table gates the slice |
| Steam claims outrun build | store-copy check before public capture |

## Market-Validation Memo

The first market test is not a feature list. It is a 45-75 second capture that
shows:

1. A readable hero in hub.
2. A clear tool target.
3. A room state changing.
4. A readable enemy tell and dodge.
5. A reward beat.
6. A boss tell, opening, and punish.
7. A return/result beat.

If the clip cannot explain the game without voice-over, improve UI, visuals,
audio, room layout, or camera before adding content.
