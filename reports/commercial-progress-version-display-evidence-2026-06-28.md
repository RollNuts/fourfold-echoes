# Commercial Progress: Version Display Evidence

Date: 2026-06-28

## Goal Support

This lane links the title-screen build identifier to generated QA evidence so
release, support, and store-capture work can cite the same version and commit
instead of relying on memory or raw Unity logs.

## Systems Touched

- Market and QA report generation
- Version-display QA evidence
- Generated final status and performance reports

## Files Added Or Changed

- `Scripts/Validation/write_market_reports.mjs`
- `artifacts/.gitignore`
- `artifacts/test-results/version-display.json`
- `artifacts/test-results/version-display.md`
- `artifacts/Reports/performance-snapshot.json`
- `artifacts/Reports/performance-snapshot.md`
- `artifacts/Reports/final-status-report.json`
- `artifacts/Reports/final-status-report.md`
- `artifacts/Reports/audio-inventory.json`
- `artifacts/Reports/audio-inventory.md`
- `reports/commercial-progress-version-display-evidence-2026-06-28.md`

## Implementation

- Added build identity extraction to `write_market_reports.mjs`.
- Generated `artifacts/test-results/version-display.*` from git commit,
  project bundle version, and runtime display contract checks.
- Linked the version-display report from final status and stamped build
  version / commit into the performance snapshot.
- Allowed structured `.md` and `.json` files under `artifacts/test-results/`
  while keeping raw artifact directories ignored by default.

## Tests

- `git diff --check` on the exact changed paths: passed.
- `node Scripts/Validation/write_market_reports.mjs`: passed.
- `node Scripts/Validation/validate_repo.mjs`: passed.
- `node Scripts/Validation/check_public_repo_hygiene.mjs`: passed.
- `node tools/forge/check.mjs`: passed.
- Secret/private-path scan on the exact changed text paths: passed.
- Unity 6000.3.18f1 batchmode
  `FourfoldEchoes.Editor.FourfoldD022ProductContractVerifier.VerifyD022Contract`:
  exit code 0.
- Unity log error-pattern scan: no compile/error/null-reference/assertion
  matches.

## Acceptance Conditions

- `node Scripts/Validation/write_market_reports.mjs` produces
  `artifacts/test-results/version-display.md` and `.json`.
- The generated final status report points to the version-display evidence.
- The generated performance snapshot includes build version and commit.
- No raw logs or private machine-specific material is added.

## Next Smallest Useful Task

Use the version-display evidence in a clean-launch smoke run once a disposable
player build is available for the current slice.
