# Scope Control

Status: canonical after D-020.

この文書は D-020 のスコープ変更を止めるための運用表である。
D-020 はシングルプレイ専用、Steam 先行、買い切り、見下ろし視点、コンパクト、オープンワールドなし、探索ツール 1 つ、MVP 上限 1 ハブ/3 地域/4 ボスに固定する。
Echo Phase、co-op、ハクスラ、抽出、オープンワールド案は変更提案の根拠にしない。

## 変更管理テンプレート

変更提案は、採用前にこの表を埋める。空欄がある提案、D-020 上限を超える提案、削る既存項目がない新規システム提案は採用しない。

| Field | Entry |
| --- | --- |
| Change title |  |
| Problem solved |  |
| D-020 item affected | 必須 / 後回し / 不要 / スコープ外 |
| Trailer value | none / weak / strong |
| Play value | none / weak / strong |
| Implementation cost | low / medium / high |
| QA cost | low / medium / high |
| Art/audio impact | none / improves / delays |
| Existing item removed |  |
| Second item removed |  |
| Cap impact | none / needs trade / exceeds D-020 |
| Decision rule used | complete first / improve first / cut |
| Decision | accept / defer / reject |
| Decision owner |  |
| Date |  |

## 上限定義

| Category | Limit |
| --- | --- |
| Hub | 1 |
| Regions | maximum 3 |
| Bosses | maximum 4 |
| Exploration tools | 1 |
| Vertical-slice normal enemies | 2 |
| Vertical-slice minibosses | 1 |
| Vertical-slice bosses | 1 |
| Vertical-slice relic rewards | 2 |
| Vertical-slice gimmick rooms | 2 |
| Vertical-slice shortcuts | 1 minimum, no network expansion |
| Vertical-slice BGM | minimum 2 usable tracks |
| Save path | local save/load only |
| Multiplayer modes | 0 |
| Online backend/live service systems | 0 |
| New systems beyond the locked D-020 scope | 0 |

`New systems beyond the locked D-020 scope` means no additional gameplay
systems may be introduced outside the current canonical slice. It does not block
implementation of already-approved D-020 fundamentals such as movement, camera,
normal attack, dodge, one exploration tool, two gimmick rooms, shortcut,
save/load, required UI, BGM, or SFX.

上限は目標値ではなく最大値である。3 地域や 4 ボスまで作る権利ではなく、完成度、可読性、市場検証素材を優先した後に残る余力の上限として扱う。

## 却下基準

Reject a proposal when any of these are true:

- It adds inventory, crafting, quest log, social, online, large-map, or
  base-building behavior.
- It adds open world, multi-state world switching, multiplayer, or live-service
  behavior under any name.
- It reintroduces Echo Phase, co-op, hack-and-slash loot/builds, extraction, or
  open-world exploration from older concepts.
- It requires a second exploration tool.
- It does not improve trailer clarity.
- It does not improve the first 30 minutes.
- It increases QA scope more than play value.
- It lets art or audio remain placeholder longer.
- It delays the vertical slice.
- It solves a problem the current one-tool loop should solve.
- It uses "one in, two out" to justify a new system. That rule can only trade
  content or polish tasks, not add systems to D-020.
- It requires local paths, personal information, credentials, signing material, or
  non-public production details in public documentation.
- It creates a dependency on unapproved Unity assets, unavailable services, or
  non-shippable placeholder media for market-facing material.

## 週次レビュー表

| Review Item | Question | Evidence | Outcome |
| --- | --- | --- | --- |
| 15-minute play | Is the objective, control, danger, reward, and retry loop understandable? | build notes or captured clip | keep / fix / cut |
| Screenshot | Can one image explain the player, room, tool target, enemy, and route? | candidate screenshot | pass / fail |
| Audio | Are movement, attack, hit, dodge, tool, reward, UI, and boss cues audible and non-placeholder? | cue checklist | pass / fail |
| Scope | Did anything exceed 1 hub, 3 regions, 4 bosses, 1 tool, or the slice caps? | cap table diff | maintain / cut |
| Bugs | Are the top 5 player-facing blockers recorded and assigned? | bug list | next-week priority / defer |
| Trailer beat | Did this week create footage value from real gameplay? | 10-30 second capture | yes / no |
| Market truth | Do Steam-facing words match implemented or locked D-020 content? | store-copy check | match / revise |
| Public hygiene | Are docs free of local paths, personal data, credentials, and private details? | repository scan | pass / fix |

## 3分類表

この分類は毎週更新する。完成優先が未完了の間、改善優先は完成優先を支えるものだけに絞り、削除候補は原則として戻さない。

| Classification | Items | Decision Rule |
| --- | --- |
| 完成優先 | movement, top-down camera, normal attack, dodge, damage/death/retry, one exploration tool, exploration nodes, two gimmick rooms, room controller, one shortcut, local save/load, minimal HUD, core SFX, BGM routing, one miniboss, one boss | playable path and market-proof capture come before breadth |
| 改善優先 | enemy readability, boss tell timing, tool VFX, hub/region lighting, UI readability, BGM mix, reward presentation, screenshot composition, controller feel, Steam Deck readability | improve existing D-020 items before adding content |
| 削除候補 | any second tool, extra regions beyond 3, bosses beyond 4, inventory, crafting, quest log, co-op, online/backend, Echo Phase, hack-and-slash loot, extraction loop, open-world map, decorative-only props, extra UI screens, unapproved market-facing placeholders | cut when it delays completion, weakens clarity, or exceeds D-020 |
