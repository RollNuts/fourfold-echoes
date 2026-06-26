# Compact Action Art Direction

Status: canonical after D-020.

## アートピラー

1. **Readable Stylized 3D**
   - Top-down distance must read instantly.
   - Shapes, color blocks, lighting, and animation matter more than surface
     detail.
   - Low-poly leaning is allowed; cheap-looking primitive assembly is not.
   - Final assets need authored proportions, bevels, color separation, and
     deliberate silhouettes. A gray cube, capsule, stick figure, or unedited
     kitbash is never final art.

2. **One Tool, One Visual Hook**
   - The exploration tool must be the most recognizable non-character object.
   - Its idle silhouette, active glow, target response, and reward feedback must
     be understandable without sound or text.
   - Tool-related materials and VFX should share one visual accent so the player
     can identify valid targets before interacting.

3. **Compact Regional Contrast**
   - Hub and three regions must be clearly different by color, shape, lighting,
     and prop families.
   - Reuse base meshes where possible, but make screenshots distinguishable.
   - A region pass is accepted only when a thumbnail crop still communicates
     where the player is.

4. **Premium Compact Craft**
   - The game can be small, but each shipped object should look intentionally
     made for this camera and world.
   - Prioritize a few polished readable assets over many unfinished variations.
   - Stylization means simplified and designed, not unfinished.

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
- Open-world vista language, distant biomes, mount-scale landmarks, or traversal
  art implying a larger scope than the compact hub and three regions.
- Co-op readability language such as player-color ownership rings, shared loot
  callouts, or split-party markers.
- Loot-rarity color ladders, extraction markers, or hack-and-slash item spam.
- Stick-figure characters, plain capsules, unmodified primitives, or default
  placeholder materials in any final or public-facing capture.
- Text labels as the only way to understand a tool target, boss weak point,
  chest, shortcut, or hazard.

## 予算表

This table budgets the market-validation vertical slice. The full MVP cap is
still 1 hub, 3 regions, 4 bosses, and 1 exploration tool; do not read the slice
boss count as a lower final-MVP cap.

| Asset Class | Vertical Slice Upper Bound | Material Budget | Texture Budget | VFX Upper Bound | Animation Density Upper Bound | Notes |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Hero | 1 model | 3 materials | 1024 max | 2 attached | 10 gameplay clips | readable tool socket, no plain mannequin |
| Exploration tool | 1 model | 2 materials | 512 max | 4 | 5 clips/states | idle, aim/active, valid target, success |
| Normal enemy | 2 models | 2 each | 512 max | 2 each | 6 clips each | silhouette-first, clear anticipation |
| Miniboss | 1 model | 3 | 1024 max | 3 | 8 clips | larger tell surfaces |
| Boss | 1 model in slice, 4 max MVP | 4 | 1024-2048 max | 5 each | 12 clips each | weak/read points visible |
| Hub kit | 12-18 props | shared atlas + 2 accent mats | 1024 atlas | 2 ambient | 2 moving props | safe, warm, clear |
| Region kit | 16-24 props per region | shared atlas + 3 accent mats | 1024 atlas | 3 regional | 3 moving props | reuse base geometry |
| Gimmick room objects | 6-10 | 2 each | 512 max | 3 total | 4 clips/states | tool reaction states |
| Chest/reward | 1 chest, 2 relics | 2 each | 512 max | 2 | 3 clips | reward glow allowed |
| Combat VFX | 8 max in slice | n/a | 256-512 flipbooks | 8 | n/a | hit, damage, guard, boss tell |
| Tool/Reward VFX | 4 max in slice | n/a | 256-512 flipbooks | 4 | n/a | pulse, target reaction, unlock, reward |
| UI icons | 12 max in slice | n/a | 256 max | 2 UI pulses | 2 states per icon | no icon bloat |

Budget rules:

- Count material variants as materials when they require separate tuning,
  keywords, or authored texture sets.
- Prefer shared atlases for environment props. Unique textures are reserved for
  hero, bosses, the exploration tool, and major reward objects.
- VFX counts include looping ambience, bursts, trails, hit flashes, and UI
  pulses. Temporary debug effects do not ship.
- Animation density means authored gameplay clips or states, not blend-tree
  permutations.
- Public captures must use assets that meet these budgets and quality bars; gray
  blockout captures are internal only.

## 命名規則

| Category | Pattern | Example |
| --- | --- | --- |
| Props | `FE_PROP_<AREA>_<NAME>_##` | `FE_PROP_R01_RootGate_01` |
| Terrain | `FE_ENV_<AREA>_<TYPE>_##` | `FE_ENV_R02_FloorCracked_03` |
| Enemy | `FE_ENEMY_<ROLE>_<NAME>` | `FE_ENEMY_MELEE_Shardling` |
| Boss | `FE_BOSS_##_<NAME>` | `FE_BOSS_01_RootWarden` |
| UI | `FE_UI_<PURPOSE>_<STATE>` | `FE_UI_Tool_Ready` |
| VFX | `FE_VFX_<SOURCE>_<ACTION>` | `FE_VFX_Tool_Pulse` |
| Material | `FE_MAT_<AREA_OR_ROLE>_<SURFACE>` | `FE_MAT_R03_CrystalGlow` |
| Texture | `FE_TEX_<AREA_OR_ROLE>_<SURFACE>_<MAP>` | `FE_TEX_R01_Stone_ALB` |

Required role/type labels:

| Family | Approved Labels | Example |
| --- | --- | --- |
| Props | `Gate`, `Chest`, `Pedestal`, `Switch`, `Pillar`, `Cover`, `Debris`, `Reward` | `FE_PROP_HUB_Pedestal_01` |
| Terrain | `Floor`, `Wall`, `Cliff`, `Bridge`, `Stair`, `Trim`, `Hazard`, `Door` | `FE_ENV_R02_CliffAngled_02` |
| Enemy | `MELEE`, `RANGED`, `HEAVY`, `SUMMON`, `BOSS` | `FE_ENEMY_HEAVY_AshBrute` |
| UI | `Tool`, `Health`, `Boss`, `Map`, `Prompt`, `Reward` plus state | `FE_UI_Prompt_Interact` |
| VFX | `Tool`, `Enemy`, `Boss`, `Reward`, `Env`, `UI` plus action | `FE_VFX_Boss_TellSlam` |

State suffixes:

- Use `_Idle`, `_Active`, `_Solved`, `_Open`, `_Closed`, `_Ready`, `_Hit`,
  `_Break`, or `_Loop` when an object has authored state variants.
- Keep numbers two digits for repeated environment pieces: `_01`, `_02`, `_03`.
- Do not encode feature ideas, prototype names, or deprecated modes in asset
  names.

Use area codes:

- `HUB`
- `R01`
- `R02`
- `R03`
- `BOSS`
- `COMMON`

## 地域別ルック表

| Area | Color Script | Shape Language | Lighting | Prop/Surface Notes | Gameplay Read |
| --- | --- | --- | --- | --- | --- |
| Hub | ivory, warm gold, soft blue | circular plaza, low walls, clean stones | soft warm key, safe shadows, low contrast | worn stone, cloth banners, repaired wood, brass accents | safety, return, orientation |
| Region 01 | moss green, pale stone, yellow flowers | roots, rounded ruins, shallow slopes | bright adventure light, soft leaf shadows | root arches, round stones, shallow water, natural tool targets | first tool reads and basic enemies |
| Region 02 | rust red, charcoal, amber | broken tile, angled cliffs, metal frames | hard side light, warm danger, sharper shadows | cracked masonry, scorched metal, ash, angular hazard rims | shortcut and combat pressure |
| Region 03 | deep blue, violet, cold white | crystals, narrow bridges, smooth dark stone | cool contrast, rim glow, readable darkness | crystal clusters, polished black stone, thin luminous seams | late route reads and boss foreshadow |

Regional rules:

- Each area needs one dominant color, one secondary support color, and one
  restrained accent. Do not solve regional identity with full-screen tint.
- Tool targets must keep a shared accent across all regions so interaction
  language stays consistent.
- Combat floors should remain lower contrast than enemies, hazards, and tool
  targets.
- Decorative props should frame routes and rewards; they must not create false
  interactable silhouettes.

## 最低品質基準

| Asset | Acceptance |
| --- | --- |
| 1 enemy | readable body/front/attack origin at gameplay camera distance; has authored idle, move, anticipation, attack, hit, and defeat; no stick limbs or capsule-only body |
| 1 room | floor, walls, exit, hazard, interactable, and reward route visible without labels; silhouette remains readable in a thumbnail |
| 1 gimmick pedestal | idle/active/solved states visible and tied to tool VFX; target face is clear from top-down view; solved state changes shape, light, or motion |
| 1 chest | visible as reward, not confused with scenery, has closed/open states, readable hinge or lid motion, and restrained reward VFX |
| Hero | tool visible, facing readable, not a capsule or plain mannequin |
| Exploration tool | silhouette and glow readable in still screenshot |
| Region shot | area identity visible at thumbnail size |

Asset-specific minimum bars:

- Enemy: one strong silhouette feature, one readable attack source, one color or
  value separation from the floor, and a tell that starts before damage.
- Room: at least three clear value layers: walkable floor, boundary, and
  gameplay object. Exits and shortcuts must be visible before the player reaches
  them.
- Gimmick pedestal: idle cannot look broken, active cannot look solved, and
  solved cannot require UI text to understand.
- Chest: must be attractive enough to read as reward, but less visually dominant
  than the exploration tool or boss weak point.

## 制作フロー

1. **灰色ブロックアウト**
   - Prove camera distance, room scale, route, enemy spacing, and tool target.
   - Do not spend more than one sprint here.
   - Exit this pass only after a room can be played and understood with simple
     labeled primitives. Do not use this pass for marketing screenshots.

2. **スタイル化**
   - Replace major shapes with stylized mesh families.
   - Add regional color blocks, prop language, and readable silhouettes.
   - Lock the silhouette and color hierarchy before adding surface detail.

3. **ライティング**
   - Establish hub and one region lighting before content quantity grows.
   - Check screenshot readability at 1280x800 and 1920x1080.
   - Verify that enemy tells, tool targets, exits, hazards, and rewards remain
     readable without post-process tricks.

4. **VFX**
   - Add only gameplay-explaining effects first: tool pulse, target reaction,
     hit confirm, damage, reward, boss tell.
   - Tune opacity, size, and lifetime from gameplay camera distance. Effects
     must support tells, not cover them.

5. **最終磨き**
   - Remove clutter.
   - Adjust contrast.
   - Capture screenshots.
   - Fix what cannot be understood silently.
   - Final polish includes mesh bevels, authored material breakup, simple contact
     shadows, state readability, and cleanup of placeholder naming.

## 省略可能な表現

- Tiny decorative props.
- Multiple costume variants.
- Complex cloth simulation.
- High-frequency texture detail.
- Decorative idle VFX not tied to gameplay.
- Region-specific unique mesh for every small prop.
- Extra hero costumes, weapon skins, or cosmetic variants.
- Unique breakage states for every prop.
- Secondary ambient creatures or non-gameplay set dressing animation.
- Complex decals that do not clarify routes, hazards, or interaction.
- Cinematic-only lighting setups that differ from playable rooms.

## 絶対に削ってはいけない表現

- Hero readability.
- Exploration tool silhouette, glow, and response VFX.
- Enemy tell visibility.
- Boss danger shape.
- Shortcut opening visual.
- Reward/chest readability.
- Region color/lighting distinction.
- Hit confirm and damage feedback.
- At least one polished representative enemy, one polished room, one polished
  gimmick pedestal, and one polished chest before public-facing capture.
- Tool target readability across all three regions.
- Playable-camera validation for every final art pass.
