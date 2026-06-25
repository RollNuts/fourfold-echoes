# Technical Asset Pipeline

Status: D-020 support document.

## Current Pipeline

```text
Art direction -> Blender/procedural or manual source -> Unity import -> prefab/material validation -> screenshot/playtest evidence
```

Current proof tooling is useful only when it supports the D-020 target:

- `tools/Blender/generate_pilot_assets.py`
- `tools/AssetPipeline/validate_generated_assets.mjs`
- D-020 scene generation and validation scripts in later lanes

Generated assets are evidence until a human visual review accepts them as
production-intent assets.

## Required Capabilities

| Capability | Status |
| --- | --- |
| naming enforcement | partial |
| pivot normalization | partial |
| scale normalization | partial |
| preview render generation | implemented for pilot |
| contact/turnaround sheet generation | pilot |
| Unity prefab generation | partial |
| LOD hook | partial |
| collider hook | partial |
| missing material scan | required |
| region variant linking | planned via material/prop families |
| build validation | later D-020 evidence lane |

## Folder Ownership

| Folder | Owner | Current Rule |
| --- | --- | --- |
| `Assets/Art/Generated/D020/` | D-020 scene evidence | can support validation but is not final production art |
| `Assets/Art/Generated/BlenderPilot/` | optional asset-pipeline proof | land only if pipeline evidence remains useful |
| `tools/Blender/` | technical art automation | scripts must be repeatable from CLI |
| `tools/AssetPipeline/` | generated asset validation | no Unity scene ownership |
| `Scripts/Validation/` | repo validation and reports | no gameplay logic |

Do not create asset-pipeline folders for removed systems such as broad-world
streaming, multi-state world switching, inventory, crafting, social, or
multiplayer.
