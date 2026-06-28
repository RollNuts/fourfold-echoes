# Version Display Evidence

Generated UTC: `2026-06-28T15:06:24.540Z`

QA gate: Build version and commit SHA are visible in debug or credits

## Build Identity

| Field | Value |
| --- | --- |
| Version | `1.0` |
| Commit | `786d4fa7e1e4` |
| Full commit | `786d4fa7e1e4f0e9b9d3e3799d805b3de85a2eb2` |
| Branch | `codex/build-evidence-link-20260628` |
| Title line | `Build 1.0 / commit 786d4fa7e1e4` |

## Runtime Contracts

### `Assets/Scripts/FourfoldBuildInfo.cs`

- Exists: true
- Required snippets:
  - present: `Resources.Load<TextAsset>(ResourceName)`
  - present: `Build {BuildVersion} / commit {ShortCommitSha}`

### `Assets/Scripts/TitleSceneController.cs`

- Exists: true
- Required snippets:
  - present: `FourfoldBuildInfo.TitleBuildLine`
  - present: `BuildInfoRect`

### `Assets/Editor/FourfoldUnityBuild.cs`

- Exists: true
- Required snippets:
  - present: `FOURFOLD_BUILD_VERSION`
  - present: `FOURFOLD_COMMIT_SHA`
  - present: `FourfoldBuildInfo.txt`

## Status

`evidence_ready`

## Follow Up

- Capture a clean-launch player run and confirm the title screen line is readable in the built player.
- Attach the same build identity to future clean-launch and store-capture manifests.
