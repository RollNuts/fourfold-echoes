# Buyout 3D Tool Stack

Status: working decision for the next visual-quality pass.
Research date: 2026-06-29.

This document records the current buy-to-own 3D tool recommendation for
FOURFOLD ECHOES. It does not approve any asset by itself. A tool only matters
when it helps a top-down combat screenshot stop looking like a prototype.

## Goal

Raise the first playable combat screen from blockout / primitive assembly to
market-readable stylized 3D:

- the player silhouette reads in one second
- one enemy role reads from shape, not UI rings
- the exploration tool has a distinct active-end silhouette
- a room prop proves the Folded Reliquary construction language
- a Unity or Blender gameplay-camera render looks authored, not kitbashed

## Current Local Tool State

Checked locally on 2026-06-29:

| Tool | Local state | Use now |
| --- | --- | --- |
| Blender | Installed | Primary modeling, export, preview render |
| Aseprite | Installed | Hand-painted texture, VFX, UI, sprite-sheet work |
| PixelOver | Installed | Pixel-style post pass and 2D/VFX experiments |
| 3DCoat | Not found | Trial / purchase candidate |
| Plasticity | Not found | Purchase candidate after 3DCoat trial |
| Marmoset Toolbag | Not found | Later polish / bake candidate |
| Blockbench | Not found | Optional free roughing tool |
| Crocotile 3D | Not found | Optional paid tile-environment roughing tool |
| Cascadeur | Not found | Later animation-assist candidate |

## Recommendation

### 1. 3DCoat first

Use 3DCoat first if a paid tool is added.

Reason: the current visual failure is not render settings. It is weak authored
form: box characters, symbolic enemies, toy-like props, and unconvincing
surface treatment. 3DCoat directly helps sculpt, retopologize, unwrap, and
paint stylized characters, enemies, rocks, reliquary props, and boss pieces.

Adoption gate:

- one hero model, one enemy model, and one reliquary room prop can be built or
  improved faster than Blender-only work
- each asset keeps a readable top-down silhouette at 128 px height
- the exported asset can be cleaned in Blender and imported to Unity without
  material or scale confusion
- the resulting screen looks better before any store/marketing work continues

Official source: <https://3dcoat.com/buy/>

### 2. Plasticity second

Use Plasticity for hard-surface and architectural pieces after the character /
enemy problem is moving.

Best fit:

- exploration-tool casing
- folded doors, bridges, locks, shrines, sockets, rails
- boss-room architectural pieces
- clean bevelled relic mechanisms

Do not use it as the main character tool. It should feed Blender for game-ready
cleanup and Unity export.

Official source: <https://www.plasticity.xyz/>

### 3. Marmoset Toolbag later

Use Marmoset Toolbag only after the asset forms are worth polishing.

Best fit:

- bake checks
- material lookdev
- fast turntable / portfolio-style validation
- catching weak normal, roughness, and edge readability before Unity import

Do not buy it to rescue bad models. A polished blockout is still a blockout.

Official source: <https://marmoset.co/toolbag/>

## Secondary Tools

| Tool | Decision | Notes |
| --- | --- | --- |
| Aseprite | Use now | Hand-paint texture accents, UI, VFX sheets, damage/hit frames. |
| PixelOver | Use now, but not as the core 3D solution | Useful for pixel-style exports and VFX tests; it does not replace better 3D form. |
| Blockbench | Optional free roughing | Useful for fast low-poly enemies or props, but avoid letting the final game read as blocky voxel work. |
| Crocotile 3D | Optional environment roughing | Useful for floor/wall tile experiments; not a final character path. |
| Cascadeur | Later animation assist | Consider for attack, recoil, dodge, and boss tells after the model silhouette is good. |
| ArmorPaint | Later low-cost texture fallback | Attractive price, but alpha status makes it secondary to 3DCoat for now. |

## Do Not Use

Do not make these the core path for the next slice:

- generic modular asset assemblers for the hero, enemies, or bosses
- tile-only tools for final character art
- render/lookdev tools before the models have strong silhouettes
- broad marketplace packs that create license or visual-identity debt
- tools that push the game away from the canonical compact top-down
  action-adventure direction

## Next Test

Before any further Steam-facing screenshot claim, run a small asset test:

1. Make or improve `Hero / Exploration Tool / Enemy / Reliquary Prop`.
2. Render them together from the gameplay camera.
3. Check the screen at 1280x800 and 1920x1080.
4. Reject the pass if the hero, attack direction, enemy role, and room identity
   cannot be read silently in one second.
5. Only then import or PR the production-facing art.

The test can start with Blender + Aseprite + PixelOver immediately. 3DCoat
should be added only if it materially improves this exact test.
