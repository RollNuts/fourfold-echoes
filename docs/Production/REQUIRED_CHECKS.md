# Required Checks

This repository can be made public only after required checks protect `main`.

## Required Contexts

Configure branch protection for `main` to require:

- `repo-contract`
- `public-hygiene`
- Veripsa Core check for PR coordination

`repo-contract` should run:

```bash
node Scripts/Validation/validate_repo.mjs
node tools/forge/check.mjs
node --check Scripts/Validation/write_veripsa_split_report.mjs
git diff --check
```

`public-hygiene` should run:

```bash
node Scripts/Validation/check_public_repo_hygiene.mjs
```

## Workflow Note

The current connector token cannot push `.github/workflows/*` because it lacks
GitHub `workflow` scope. Add the workflow from a token or GitHub App with
workflow permission, then mark the contexts above as required in branch
protection.

## Public Hygiene Policy

- Do not commit credentials, tokens, private keys, private relay URLs, or local
  machine paths.
- Do not track Unity local generated folders such as `Library/`, `Temp/`,
  `Logs/`, `UserSettings/`, `Build/`, or `Builds/`.
- Public docs may describe local-first development, but must not include a real
  user home path.
- Unity batchmode evidence may stay local until artifacts are intentionally
  reviewed for public release.
