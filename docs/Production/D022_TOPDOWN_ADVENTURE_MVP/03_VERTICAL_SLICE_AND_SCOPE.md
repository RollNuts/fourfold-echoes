# D022 Vertical Slice And Scope Control

## 完成条件チェックリスト

| Area | Completion Condition |
| --- | --- |
| Scope | hub 1, exploration area 1, normal enemies 2, miniboss 1, boss 1, tool 1, shortcut 1, gimmick rooms 2, reward relics 2 |
| Experience | first 30 minutes prove learning, application, tension, reward, boss defeat, return |
| Video | 30 muted seconds explain tool, room change, enemy tell, boss opening |
| Visual/audio | no graybox public capture, no placeholder SFX, BGM 2+, minimum SFX |
| UI | title, continue, HUD, boss HP, prompts, pause, settings, save, death/retry |
| Save | new game, continue, death retry, boss clear, corrupt fallback |
| Platform | Windows build path, controller, 1280x800, offline play |

## 実装順序

| Week | Focus | Done When |
| --- | --- | --- |
| 1 | Boot, title, save/load, input, hub shell | controller can start/continue and enter hub |
| 2 | Player move/combat/death/retry | basic enemy can kill player and be defeated |
| 3 | One exploration tool + gimmick room 1 | tool opens or exposes a route with VFX/SFX |
| 4 | Room gate, shortcut, reward relic 1 | progress saves and route change persists |
| 5 | Enemy 2 + gimmick room 2 + reward relic 2 | 30-minute path has learning/application |
| 6 | Miniboss + boss + boss HUD | boss has readable tell/opening/defeat |
| 7 | Art/audio polish for slice | no graybox/no temp SFX in captures |
| 8 | QA, Deck, screenshots, 45-sec trailer | market-validation package exists |

## 担当表

| Category | Responsibility | Deliverable |
| --- | --- | --- |
| Product | scope cap and rejection decisions | change log and weekly scope review |
| Gameplay | rooms, enemies, boss, tool | 30-minute playable slice |
| UI/UX | title, HUD, pause, settings, save feedback | controller-first product UI |
| Art | hub, region, enemies, tool, reward, VFX | non-graybox market captures |
| Audio | BGM 2, required SFX, mix | no-placeholder capture audio |
| QA/Release | Steam, Deck, regression, bug priority | release risk table |
| Marketing | JP/EN/ZH copy, screenshots, trailers | store-ready draft |

## リスク表

| Risk | Severity | Response |
| --- | --- | --- |
| Old scope leaks back in | high | reject open-world/co-op/crafting/loot language immediately |
| 30 minutes feels flat | high | tune room order, enemy tells, boss opening, reward beat; do not add systems |
| Video does not explain game | high | fix camera, VFX, UI, tells before adding features |
| Audio slips late | medium | SFX/BGM are slice completion gates |
| Steam Deck failure | medium | test 1280x800/controller-only early |
| Art remains mock-like | high | market capture blocked until room/tool/enemy/chest meet quality bar |

## 市場検証可能性メモ

The slice validates market pull only if a player can understand the product in
30 muted seconds and feel the loop in 30 minutes. The footage must show hub,
tool use, room transformation, enemy tell, reward relic, shortcut, boss opening,
and return/save feedback. If the slice needs explanation outside the video, the
product is not yet market-testable.

## 変更管理テンプレート

| Field | Value |
| --- | --- |
| Change name |  |
| Problem solved |  |
| Affected completion condition |  |
| 30-minute play value | none / weak / strong |
| 30-second video value | none / weak / strong |
| Implementation cost | low / medium / high |
| QA cost | low / medium / high |
| What gets removed |  |
| Cap impact | none / trade / exceeds |
| Decision | accept / defer / reject |
| Owner/date |  |

## 上限定義

| Item | Vertical Slice Cap | MVP Cap |
| --- | ---: | ---: |
| Hub | 1 | 1 |
| Exploration area / regions | 1 | 3 |
| Normal enemies | 2 | 4 |
| Miniboss | 1 | 3 max, region-contained |
| Boss | 1 | 4 |
| Exploration tool | 1 | 1 |
| Shortcuts | 1 | compact region-specific |
| Gimmick rooms | 2 | hand-authored only |
| Reward relics | 2 | small reward beats only |
| Online/crafting/quest/social/equipment build | 0 | 0 |

## 却下基準

Reject a proposal when it:

- exceeds hub/region/boss/tool caps
- requires inventory, crafting, quest log, social, online, or open-world systems
- does not improve 30-minute play or 30-second video clarity
- relies on graybox or placeholder audio for public proof
- adds a new system instead of improving tool, room, enemy, boss, UI, art, or audio
- cannot name two existing scope items to remove

## 週次レビュー表

| Question | Evidence | Decision |
| --- | --- | --- |
| Can the 30-minute route be completed? | play recording | pass/fix/cut |
| Does 30 muted seconds explain the game? | muted capture | pass/fix |
| Did scope exceed caps? | diff/scope table | keep/cut |
| Are art/audio public-ready? | screenshots/audio capture | pass/fix |
| Any P0/P1 blockers? | bug table | ship/block |

## 3分類表

| Category | Items | Rule |
| --- | --- | --- |
| 完成優先 | hub, R01, tool, enemy 2, miniboss, boss, UI, save, BGM2, SFX | finish before adding |
| 改善優先 | enemy tell, tool visibility, boss opening, HUD clarity, mix | improve if it raises readability |
| 削除候補 | extra regions, weapons, crafting, co-op, online, unapproved art/audio | cut immediately |
