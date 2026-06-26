# Required Checks

Status: public guardrail after D-020.

This project can publish planning reports and dogfood evidence, but public
branches must not expose local paths, personal information, credentials, private
keys, private package URLs, or unlicensed assets.

## Local Required Checks

Run these before opening or updating a PR that changes product docs, tooling,
Unity scenes, generated assets, or release evidence:

```bash
node Scripts/Validation/check_public_repo_hygiene.mjs
node Scripts/Validation/validate_repo.mjs
node tools/forge/check.mjs
node --check Scripts/Validation/check_public_repo_hygiene.mjs
node --check Scripts/Validation/validate_repo.mjs
node --check Scripts/Validation/write_veripsa_split_report.mjs
git diff --check
```

Unity validation is also required for PRs that change scenes, prefabs, runtime
C# behavior, editor generation, build scripts, or captured gameplay evidence:

```bash
Scripts/Validation/run_all.sh
```

If Unity licensing or local editor startup blocks the full run, the PR must state
that Unity validation is not yet verified and include the exact failed command
and log artifact name, without publishing machine-local absolute paths.

## GitHub Required Statuses

When branch protection is configured, require equivalent checks for:

| Check | Required For | Purpose |
| --- | --- | --- |
| `public-hygiene` | all PRs | blocks local paths, credentials, private keys, and Unity local generated folders |
| `repo-validation` | all PRs | confirms canonical D-020 docs, Game IR, audio registers, and scope guards |
| `forge-check` | specs/tooling PRs | validates Game IR and Forge-facing contracts |
| `unity-validation` | Unity scene/runtime/editor PRs | validates scene generation, Unity product validator, and capture path |
| `diff-whitespace` | all PRs | blocks trailing whitespace and broken patch formatting |
| `veripsa-review` | all PRs | checks lane coupling and safe landing order |

Do not use a Veripsa clear verdict as proof of code correctness. Veripsa is the
coordination gate; tests, Unity validation, screenshots, and builds are the
quality evidence.

## Public Evidence Rules

Allowed in public PRs:

- Product, art, audio, QA, release, and marketing plans after personal data scan.
- Screenshots and captures from actual gameplay, if they do not show local user
  information or private paths.
- Generated reports that use repository-relative paths.
- License ledgers for assets with clear commercial permission.

Blocked from public PRs:

- Credentials, tokens, private keys, local machine paths, private package URLs.
- Raw Unity `Library`, `Logs`, `Temp`, or user settings.
- Third-party assets without commercial-use and redistribution status recorded.
- Store claims for features not implemented in the current build.
- AI-generated final assets whose tool terms or usage rights are not recorded.

## PR Evidence Template

Use this evidence block in PR bodies:

```md
## Evidence
- public-hygiene: pass/fail/not run
- repo-validation: pass/fail/not run
- forge-check: pass/fail/not run
- unity-validation: pass/fail/not run, reason if blocked
- screenshots/build artifacts: link or not applicable
- Veripsa verdict: clear/warn/pause/unknown
- Local/private data scan: pass/fail
```

If a check is not run, say so directly. Do not imply visual, audio, Unity, or
build validation from docs-only checks.
