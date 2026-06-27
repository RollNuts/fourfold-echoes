# Compact Action Art Direction

Status: canonical after D-020.

The stricter production-line contract is:

- `docs/Art/FOLDED_RELIQUARY_STYLE_BIBLE.md`

When this file and the style bible differ, the style bible wins for production
3D model generation, prompt contracts, and validation gates.

## 正規アート軸

**折り目遺物の箱庭模型**

FOURFOLD ECHOES の3Dモデルは、地域ごとに別ジャンルを作るのでは
なく、すべてを「折られた小さな遺物」の変奏として扱う。根、炉、
水晶、王冠などの単語は素材や状態の説明に留め、画面の第一印象は
常に同じプロダクトファミリーに見えること。

このプロダクトファミリーは、トップダウンで読む、低い折り石板・
四分割象嵌・厚い欠けタブ・一本の機能シグナル糸で統一された、
商用安全な箱庭遺物アクションアドベンチャーとして固定する。R01は
森ジャンル、R02は工場ジャンル、R03は水晶ジャンルではない。同じ
商品ラインが、摩耗、熱、冷気で変質しているだけでなければならない。

外部ゲームの公式スクリーンショットは、市場品質を測るための集計
ベンチマークだけに使う。明度、コントラスト、エッジ密度、遠景での
可読性、画面密度は比較してよいが、形状、配色セット、構図、
キャラクター、敵、宝箱、ゲート、UI、ロゴ、固有モチーフを設計指示
へ還元してはいけない。複数作品の平均を取って「別物」とする設計も
禁止する。

## 造形DNA

| Rule | Required Treatment |
| --- | --- |
| Folded plinth | 主要モデルは低い台座、折れた縁、ずれた板面を持つ。 |
| Split inlay | 四分割、45度前後の切れ目、短い象嵌線を共通記号にする。 |
| Signal thread | ツール、報酬、ロック、危険面は一本の発光糸またはソケットで読む。 |
| Chunk tab | 細い装飾ではなく、厚みのある欠け、爪、段差で密度を出す。 |
| Shared family | 敵は欠けた遺物、ボスは大型化した遺物、ツールは完全形の遺物として作る。 |

## アートピラー

1. **Readable Stylized 3D**
   - Top-down distance must read instantly.
   - Shapes, color blocks, lighting, and animation matter more than surface
     detail.
   - Low-poly leaning is allowed; cheap-looking primitive assembly is not.

2. **One Tool, One Visual Hook**
   - The exploration tool must be the most recognizable non-character object.
   - Its idle silhouette, active glow, target response, and reward feedback must
     be understandable without sound or text.

3. **Compact Regional Contrast**
   - Hub and three regions must be clearly different by color, shape, lighting,
     and prop families.
   - Reuse base meshes where possible, but make screenshots distinguishable.

## 禁止事項

- Photorealism.
- MMO armor/detail density.
- Gray blockout as market-facing art.
- Thin decorative clutter that cannot be read from top-down view.
- High vertical walls that hide combat.
- Region palettes that differ only by post-process tint.
- Important interactables that rely on text labels.
- VFX that hides enemy tells.
- AI-looking fantasy noise without edited silhouettes.
- Extra art language for systems outside the MVP.

## 予算表

This table budgets the market-validation vertical slice. The full MVP cap is
still 1 hub, 3 regions, 4 bosses, and 1 exploration tool; do not read the slice
boss count as a lower final-MVP cap.

| Asset Class | Vertical Slice Upper Bound | Material Budget | Texture Budget | Notes |
| --- | ---: | ---: | ---: | --- |
| Hero | 1 model | 3 materials | 1024 max | readable tool socket |
| Exploration tool | 1 model | 2 materials | 512 max | glow/emission variant |
| Normal enemy | 2 models | 2 each | 512 max | silhouette-first |
| Miniboss | 1 model | 3 | 1024 max | larger tell surfaces |
| Boss | 1 model | 4 | 1024-2048 max | weak/read points visible |
| Hub kit | 12-18 props | shared atlas | 1024 atlas | safe, warm, clear |
| Region kit | 16-24 props per region | shared atlas | 1024 atlas | reuse base geometry |
| Gimmick room objects | 6-10 | 2 each | 512 max | tool reaction states |
| Chest/reward | 1 chest, 2 relics | 2 each | 512 max | reward glow allowed |
| VFX | 12 max in slice | n/a | 256-512 flipbooks | tool, combat, reward, boss |
| UI icons | 12 max in slice | n/a | 256 max | no icon bloat |

## 命名規則

| Category | Pattern | Example |
| --- | --- | --- |
| Character | `FE_CHAR_<ROLE>_<NAME>_##` | `FE_CHAR_PLAYER_Hero_01` |
| Props | `FE_PROP_<AREA>_<NAME>_##` | `FE_PROP_R01_RootGate_01` |
| Terrain | `FE_ENV_<AREA>_<TYPE>_##` | `FE_ENV_R02_FloorCracked_03` |
| Enemy | `FE_ENEMY_<ROLE>_<NAME>` | `FE_ENEMY_MELEE_Shardling` |
| Boss | `FE_BOSS_##_<NAME>` | `FE_BOSS_01_RootWarden` |
| Relic | `FE_RELIC_<NAME>_##` | `FE_RELIC_EmberSeed_01` |
| UI | `FE_UI_<PURPOSE>_<STATE>` | `FE_UI_Tool_Ready` |
| VFX | `FE_VFX_<SOURCE>_<ACTION>` | `FE_VFX_Tool_Pulse` |
| Material | `FE_MAT_<AREA_OR_ROLE>_<SURFACE>` | `FE_MAT_R03_CrystalGlow` |
| Texture | `FE_TEX_<AREA_OR_ROLE>_<SURFACE>_<MAP>` | `FE_TEX_R01_Stone_ALB` |

Use area codes:

- `HUB`
- `R01`
- `R02`
- `R03`
- `BOSS`
- `COMMON`

## 地域別ルック表

| Area | Color Script | Shape Language | Lighting | Gameplay Read |
| --- | --- | --- | --- | --- |
| Hub | ivory, warm gold, soft blue | clean folded plinths, quartered floor marks, low safe barriers | soft warm key, safe shadows | safety, return, orientation |
| Region 01 | moss green, pale stone, yellow flowers | weathered folded relics with rounded chip tabs and low growth accents | bright adventure light | first tool reads and basic enemies |
| Region 02 | rust red, charcoal, amber | scorched folded relics with bent tabs and amber inlay threads | hard side light, warm danger | shortcut and combat pressure |
| Region 03 | deep blue, violet, cold white | cold folded relics with sharp tabs and violet split inlays | cool contrast, clear glow | late route reads and boss foreshadow |

## 最低品質基準

| Asset | Acceptance |
| --- | --- |
| 1 enemy | readable body/front/attack origin at gameplay camera distance |
| 1 room | floor, walls, exit, hazard, interactable, reward route visible without labels |
| 1 gimmick pedestal | idle/active/solved states visible and tied to tool VFX |
| 1 chest | visible as reward, not confused with scenery, has open state |
| Hero | tool visible, facing readable, not a capsule or plain mannequin |
| Exploration tool | silhouette and glow readable in still screenshot |
| Region shot | area identity visible at thumbnail size |

## 制作フロー

1. **灰色ブロックアウト**
   - Prove camera distance, room scale, route, enemy spacing, and tool target.
   - Do not spend more than one sprint here.

2. **スタイル化**
   - Replace major shapes with stylized mesh families.
   - Add regional color blocks, prop language, and readable silhouettes.

3. **ライティング**
   - Establish hub and one region lighting before content quantity grows.
   - Check screenshot readability at 1280x800 and 1920x1080.

4. **VFX**
   - Add only gameplay-explaining effects first: tool pulse, target reaction,
     hit confirm, damage, reward, boss tell.

5. **最終磨き**
   - Remove clutter.
   - Adjust contrast.
   - Capture screenshots.
   - Fix what cannot be understood silently.

## 省略可能な表現

- Tiny decorative props.
- Multiple costume variants.
- Complex cloth simulation.
- High-frequency texture detail.
- Decorative idle VFX not tied to gameplay.
- Region-specific unique mesh for every small prop.

## 絶対に削ってはいけない表現

- Hero readability.
- Exploration tool silhouette, glow, and response VFX.
- Enemy tell visibility.
- Boss danger shape.
- Shortcut opening visual.
- Reward/chest readability.
- Region color/lighting distinction.
- Hit confirm and damage feedback.
