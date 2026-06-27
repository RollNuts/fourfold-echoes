# CI and Build Pipeline

This repository uses GitHub Actions for pull request validation, default branch
builds, and security scanning. The pipeline is intentionally small because
FOURFOLD ECHOES is still a compact Unity slice: CI should prove the current
runtime path without adding broad release machinery before the MVP needs it.

## Workflows

| Workflow | Trigger | Purpose |
| --- | --- | --- |
| `.github/workflows/validate.yml` | `pull_request` to `main`, `workflow_dispatch` | Repository guard scripts plus Unity EditMode and PlayMode tests. |
| `.github/workflows/build.yml` | `push` to `main`, `workflow_dispatch` | Automatic default-branch D-020 Windows slice build. |
| `.github/workflows/security.yml` | `pull_request`, `push` to `main`, weekly schedule, `workflow_dispatch` | CodeQL C# analysis. |

## Unity Version

GameCI is configured with `unityVersion: auto`, so the editor version is read
from `ProjectSettings/ProjectVersion.txt`. At the time this CI was added, that
file selects `6000.3.18f1`.

Do not hard-code a separate Unity version in workflow YAML unless the project
intentionally changes away from the version file. If the editor version changes,
update `ProjectSettings/ProjectVersion.txt` first and let cache keys rotate from
that file.

## Pull Request Validation

`validate.yml` has two layers:

- `repo-guards` runs the public hygiene scanner, repository validation, Forge
  contract check, validation-script syntax checks, and PR diff whitespace check.
- `unity-tests` runs Unity `EditMode` and `PlayMode` tests through GameCI.

Unity test results and logs are uploaded on every run attempt from:

```text
artifacts/unity-tests/EditMode/
artifacts/unity-tests/PlayMode/
```

The Unity test job uses a `Library` cache keyed by operating system, test mode,
`ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, optional
`Packages/packages-lock.json`, and core project settings. The cache stores only
generated Unity import state and can be deleted without losing source assets.

## Default Branch Build

`build.yml` builds automatically on pushes to `main`. The current build target
is the D-020 Windows slice through:

```text
FourfoldEchoes.Editor.FourfoldUnityBuild.BuildCurrentD020Slice
```

The workflow uploads build logs and build output from:

```text
artifacts/unity-build/
Build/CI/
```

Windows is the default because Steam Windows is the first shipping target.
macOS builds remain development evidence only unless a later product decision
promotes them to release scope.

## Security

`security.yml` runs CodeQL for C# with `build-mode: none`. This keeps the scan
independent from Unity activation and avoids granting extra permissions to a
security workflow. CodeQL results upload to GitHub code scanning and the SARIF
output is also retained as an artifact under:

```text
artifacts/codeql/
```

## Secrets and Permissions

Required Unity secret:

```text
UNITY_LICENSE
```

Keep secrets scoped to the single step that needs them. Do not place Unity,
OpenAI, store, signing, or release credentials in workflow-level or job-level
`env`. Do not print secrets or derived token values with `echo`.

Repository permissions stay minimal:

- Validation and build workflows use `contents: read`.
- Security uses `contents: read`, `actions: read`, and `security-events: write`
  for CodeQL upload.
- Future autofix jobs must be isolated in their own job and grant
  `contents: write` or `pull-requests: write` only to that job.

## Action Pinning

Actions are pinned by full commit SHA. The SHAs currently correspond to these
human-readable upstream refs:

| Action | Upstream ref | Pinned SHA |
| --- | --- | --- |
| `actions/checkout` | `v4` | `34e114876b0b11c390a56381ad16ebd13914f8d5` |
| `actions/cache` | `v4` | `0057852bfaa89a56745cba8c7296529d2fc39830` |
| `actions/upload-artifact` | `v4` | `ea165f8d65b6e75b540449e92b4886f43607fa02` |
| `game-ci/unity-test-runner` | `v4` | `0ff419b913a3630032cbe0de48a0099b5a9f0ed9` |
| `game-ci/unity-builder` | `v4` | `1d4ee0697f193f54668e98961d79907911f4b4f2` |
| `github/codeql-action` | `v3` | `b0c4fd77f6c559021d78430ec4d0d169ae74a4eb` |

Before production hardening or periodic dependency refresh, resolve refs again
with:

```bash
git ls-remote --refs https://github.com/actions/checkout.git refs/tags/v4
git ls-remote --refs https://github.com/actions/cache.git refs/tags/v4
git ls-remote --refs https://github.com/actions/upload-artifact.git refs/tags/v4
git ls-remote --refs https://github.com/game-ci/unity-test-runner.git refs/tags/v4
git ls-remote --refs https://github.com/game-ci/unity-builder.git refs/tags/v4
git ls-remote --refs https://github.com/github/codeql-action.git refs/tags/v3
```

Treat SHA updates like dependency upgrades: review upstream release notes,
update this table, and verify the workflows on a branch before merging.

## Codex Review and Autofix Extension

The workflows leave room for future Codex review or autofix without mixing AI
credentials into the main validation jobs.

When adding it:

- Add a separate job after `repo-guards` or after all validation jobs, depending
  on whether it comments only or writes changes.
- Use step-level `env` for `OPENAI_API_KEY`; never place it at workflow or job
  scope.
- Give write permissions only to the autofix job, and only the minimum needed:
  usually `pull-requests: write` for comments or `contents: write` for commits.
- Upload any review logs as artifacts, but redact prompts, raw secrets, local
  absolute paths, and proprietary asset payloads first.
- Prefer draft suggestions or patches over direct pushes until branch protection
  and review policy are explicit.
