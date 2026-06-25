# Scope Control

Status: canonical after D-020.

## 変更管理テンプレート

| Field | Entry |
| --- | --- |
| Change title |  |
| Problem solved |  |
| Trailer value | none / weak / strong |
| Play value | none / weak / strong |
| Implementation cost | low / medium / high |
| QA cost | low / medium / high |
| Existing item removed |  |
| Second item removed |  |
| Decision | accept / defer / reject |
| Decision owner |  |
| Date |  |

## 上限定義

| Category | Limit |
| --- | --- |
| Hub | 1 |
| Regions | 3 |
| Bosses | 4 |
| Exploration tools | 1 |
| Vertical-slice normal enemies | 2 |
| Vertical-slice minibosses | 1 |
| Vertical-slice bosses | 1 |
| Vertical-slice relic rewards | 2 |
| Vertical-slice gimmick rooms | 2 |
| New systems per milestone | 0 |

## 却下基準

Reject a proposal when any of these are true:

- It adds inventory, crafting, quest log, social, online, large-map, or
  base-building behavior.
- It adds open world, multi-state world switching, multiplayer, or live-service
  behavior under any name.
- It requires a second exploration tool.
- It does not improve trailer clarity.
- It does not improve the first 30 minutes.
- It increases QA scope more than play value.
- It lets art or audio remain placeholder longer.
- It delays the vertical slice.
- It solves a problem the current one-tool loop should solve.
- It uses "one in, two out" to justify a new system. That rule can only trade
  content or polish tasks, not add systems to D-020.

## 週次レビュー表

| Review Item | Question | Outcome |
| --- | --- | --- |
| 15-minute play | Is the game understandable and improving? | keep / fix / cut |
| Screenshot | Can one image explain the room/tool/enemy? | pass / fail |
| Audio | Are new interactions audible and non-placeholder? | pass / fail |
| Scope | Did anything exceed the cap? | accept / cut |
| Bugs | What are the top 5 player-facing blockers? | next-week priority |
| Trailer beat | Did this week create footage value? | yes / no |

## 3分類表

| Classification | Items |
| --- | --- |
| 完成優先 | movement, camera, normal attack, dodge, one tool, two rooms, shortcut, save/load, core SFX |
| 改善優先 | enemy readability, boss tell timing, tool VFX, hub/region lighting, UI readability, BGM mix |
| 削除候補 | any second tool, extra regions, optional systems, decorative-only props, extra UI screens |
