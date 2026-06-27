# Concept Art Gate Report

Generated: 2026-06-26

## Verdict

3D production is paused until assets pass a concept-first gate.

The current acceptable direction is not "average stylized fantasy". It is
**Folded Reliquary Miniatures**: folded shell-stone leaves, brass hinges, teal
signal grooves, wax seams, and dark underside support masses.

## Reference Images

| Image | Status | Use |
| --- | --- | --- |
| `artifacts/Concepts/FoldedReliquary/FE_CONCEPT_FoldedReliquary_StyleBoard_v001.png` | accepted with notes | overall material, object, and grouped presentation reference |
| `artifacts/Concepts/FoldedReliquary/FE_CONCEPT_FirstFold_KeyArt_v001.png` | conditional | gameplay relationship reference; do not copy the circular arena/floor layout directly |
| `artifacts/Concepts/FoldedReliquary/FE_CONCEPT_RouteSurface_ModelSheet_v001.png` | rejected as primary | too close to rectangular modular floor language; kept as negative comparison |
| `artifacts/Concepts/FoldedReliquary/FE_CONCEPT_RouteSurface_ModelSheet_v002.png` | accepted as primary route reference | next modeling source for route surfaces and `FE_ENV_R01_ShortcutBridge_01` |

## Anti-Derivative Review

Prompt inputs deliberately avoided named external games and franchise style
phrases. User taste was translated into abstract traits only: readable
stylization, controlled saturation, warm/cool lighting, layered object
hierarchy, and current-market polish.

Rejected or limited visual shortcuts:

- treasure chest
- crystal tower
- square tile grid
- medieval cobblestone
- sword/shield/wand silhouettes
- slime/goblin/dragon mascot silhouettes
- symmetrical generic fantasy relics

## Modeling Notes

`RouteSurface_ModelSheet_v002` is the strongest source because it moves away
from a standard floor kit and toward a proprietary construction language:

- petal/leaf plate silhouettes
- hinge spines and clasp teeth
- teal enamel grooves as functional route state
- red wax seams as locked/danger state
- dark underside pieces for readable depth and contact
- asymmetrical chipped tabs instead of scattered decoration

When modeling, do not reproduce the decorative border or sheet presentation
lines. Extract only the physical asset forms.

## Next Target

Build `FE_ENV_R01_ShortcutBridge_01` from `RouteSurface_ModelSheet_v002`.

Required modeling brief:

- asymmetrical folded-shell bridge
- pivot at route connection center
- two visible connection sockets
- teal opened signal groove
- one broken edge cluster, not evenly distributed chips
- dark underside support mass
- no rectangular tile bridge silhouette

## Multi-Agent Review Consensus

Three review passes agreed on the same priority:

- route surfaces have the highest visual leverage because they occupy the most
  screen area and repeat across the game
- current route/floor assets read too much like procedural slabs and broad gray
  workbench surfaces
- detail should become thick functional signal seams, hinge parts, sockets, and
  localized broken clusters, not scattered noise
- market-facing renders need stronger lighting, contrast, saturation, contact
  shadows, and tighter camera framing, but external games remain metric
  references only

This supports making `FE_ENV_R01_ShortcutBridge_01` the first concept-driven
3D rebuild.
