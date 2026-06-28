# Version Display Evidence

Generated UTC: `2026-06-28T15:00:17.227Z`

QA gate: Build version and commit SHA are visible in debug or credits

## Build Identity

| Field | Value |
| --- | --- |
| Version | `1.0` |
| Commit | `bcc5ebb6fdf0` |
| Full commit | `bcc5ebb6fdf0efbc978adb8d68f13d9d9a86b83f` |
| Branch | `codex/build-evidence-link-20260628` |
| Title line | `Build 1.0 / commit bcc5ebb6fdf0` |

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
