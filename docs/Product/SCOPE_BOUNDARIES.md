# Scope Boundaries

Status: canonical after D-020.

## 上限定義

| Area | MVP Limit |
| --- | --- |
| Hub | 1 |
| Regions | 3 maximum |
| Bosses | 4 maximum |
| Exploration tools | 1 |
| Normal enemies in vertical slice | 2 |
| Minibosses in vertical slice | 1 |
| Bosses in vertical slice | 1 |
| Gimmick rooms in vertical slice | 2 |
| Relic rewards in vertical slice | 2 |
| BGM in vertical slice | 2 tracks |
| Save slots | 1 simple local save path |

## 許可する深さ

- Same exploration tool used in new room layouts.
- Same enemy rules used in different terrain.
- Boss patterns that test movement, dodge, attack timing, and tool use.
- Shortcuts that make compact regions feel connected.
- Relic rewards that slightly change play without opening an inventory economy.
- Art, lighting, animation, VFX, and audio polish that make the simple rules feel
  satisfying.

## 明示的にスコープ外

| Feature | Reason |
| --- | --- |
| Open world | contradicts compact classic action-adventure target |
| Day/night cycle | adds content and QA without proving core fun |
| Fishing/farming/base building | different genre loops |
| Inventory/crafting | UI/economy cost is too high for MVP |
| Quest log | implies more narrative/content scale than allowed |
| Social systems | outside single-player scope |
| Multiplayer/co-op/PvP | multiplies QA and design work |
| Live service/backend | violates offline buy-to-play target |
| Multiple exploration tools | dilutes one-tool mastery |
| More than 3 regions or 4 bosses | outside MVP ceiling |
| Procedural world generation | weakens authored room mastery |

## 追加提案のルール

A new feature can be considered only when:

1. it doubles the value of the existing core loop;
2. it replaces or removes at least two lower-value items;
3. it improves trailer clarity or play value more than its implementation cost;
4. it does not delay the vertical slice.

Otherwise reject it.
