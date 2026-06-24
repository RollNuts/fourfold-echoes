# Asset Rights And Repo Hygiene

FOURFOLD ECHOES is a private commercial Unity project. Treat every committed
asset and every Unity `.meta` GUID as part of the commercial source record.

Current Gate A status:

| Path or ID | Source | Rights status | Notes |
| --- | --- | --- | --- |
| Gate A primitive geometry and procedural placeholder tones | Repository-authored | Approved | No third-party production art, animation, audio, model, font, or texture assets are committed. |
| `Assets/Scripts/FourfoldProofAudio.cs` runtime tones | Repository-authored procedural synthesis | Approved | Attack, hit, dodge, altar heat, gate open, room clear, enemy tell, player hit, phase, and reward cues are generated in Unity at runtime from sine-wave sample data. No external audio files or AI-generated audio assets are committed. |

## Intake Rules

- Do not commit third-party, generated, marketplace, contractor, or reference
  assets until commercial-use rights are reviewed and recorded.
- Record the asset path or semantic ID, source/vendor, license or contract,
  commercial-use permission, team/seat limits, modification rights,
  redistribution limits, receipt/proof location, reviewer, and review date.
- Keep confidential receipts or vendor account details outside this repository;
  the repo record should identify where approved proof lives.
- Track Unity `.meta` files beside every committed Unity asset. Missing,
  duplicated, or regenerated GUIDs are review blockers. Generate `.meta` files
  through Unity import, not by hand-authoring GUIDs in routine changes.
- Place reviewed human-authored production assets under `Assets/Manual/`.
  Shared runtime assets should be registered by stable semantic ID before Forge
  references them.
- Keep generated proof output under the generated/evidence paths documented in
  `docs/forge/UNITY_ADAPTER.md`; generated artifacts are not source of truth.
- Large binary assets must match `.gitattributes` and be stored through Git LFS
  when intentionally tracked. `git lfs version` must pass before committing a
  binary asset intake change.
- Do not commit Unity packages, shipping builds, recordings, screenshots,
  imported caches, MCP client config, credentials, or unknown-source assets
  unless a separate review explicitly approves the exception.

## AI-Generated Asset Rule

AI-generated art, audio, animation, texture, model, or font assets need the same
rights review as marketplace or contractor assets. Record the tool/service,
terms snapshot date, prompt summary, human edits, and any style, trademark, or
likeness constraints before committing the asset.
