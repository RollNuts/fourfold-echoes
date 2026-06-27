# Free Asset Ingestion

Status: active sourcing lane.

## Decision

Free assets can be used, but only when the license is clear enough for
commercial use, modification, and storage in a private project vault.

The priority is not to collect everything. The priority is to collect usable
source material that can be edited into Fourfold Echoes without license or style
risk.

## Preferred Sources

Preferred:

- CC0 / public-domain-equivalent assets.
- Assets with explicit commercial use and modification permission.
- Assets with downloadable source formats: `.fbx`, `.glb`, `.blend`, `.obj`,
  `.png`, `.wav`, `.ogg`.

Good first checks:

- Poly Haven: CC0 models, textures, and HDRIs.
- Creative Commons CC0: copy, modify, distribute, and commercial use are allowed
  without asking permission, but trademark, patent, privacy, and endorsement
  issues still need care.

Conditional:

- Unity Asset Store free assets. They can be useful, but they are licensed, not
  sold, may have provider-specific terms, may be seat-limited for extensions,
  and should not be treated like CC0 source material.
- OpenGameArt assets. Use only if the individual asset license is acceptable.
  Prefer CC0. Avoid NC, ND, unclear attribution chains, and copyleft spillover
  unless there is a deliberate reason.

Avoid:

- ripped game assets
- fan models
- marketplace uploads with no license text
- "free for personal use" assets
- AI training datasets from marketplace assets
- assets that require attribution in UI/credits before the project has a
  credits pipeline

## Intake Checklist

For every external asset, record:

- source URL
- download date
- author/provider
- license name
- license URL or bundled license file
- commercial use allowed: yes/no/unknown
- modification allowed: yes/no/unknown
- redistribution/vault storage allowed: yes/no/unknown
- attribution required: yes/no
- original file path
- edited derivative path
- notes on style changes made

If any field is unknown, the asset stays in `hold_legal_review`.

## Edit Strategy

External assets should be treated as raw material, not final art.

Allowed edits:

- simplify topology
- recolor to project palettes
- swap materials
- bevel/round hard silhouettes
- convert terrain into block-field modules
- remove logos/text
- remove recognizable motifs
- atlas textures
- add LODs and collision

Reject after edit if the asset still looks like a recognizable downloaded pack
or if multiple packs clash visually in the same scene.
