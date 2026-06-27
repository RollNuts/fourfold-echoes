# Production Slice HUD Tool Prompt

Date: 2026-06-27

## Player-visible change

- The in-run HUD now names the Echo Tool and shows its controller-first input hint while the tool is ready or recovering.
- This keeps the production slice readable after leaving the title screen, without adding inventory, quest, progression, or new combat systems.

## Scope

- Changed: `Assets/Scripts/ProductionCombatSliceUi.cs`
- Added: `reports/production-slice-hud-tool-prompt-2026-06-27.md`
- Existing dirty work in other lanes is intentionally out of scope.

## Validation

- `git diff --check -- Assets/Scripts/ProductionCombatSliceUi.cs reports/production-slice-hud-tool-prompt-2026-06-27.md`: passed
- `node Scripts/Validation/validate_repo.mjs`: passed
- `node Scripts/Validation/check_public_repo_hygiene.mjs`: passed
- Exact file secret/private path scan: passed; broad keyword scan only matched this report's policy wording.
- Unity batchmode: not launched because active batchmode jobs were already running; exit code not produced.
- Local Veripsa Core: unavailable in this checkout; ready PR is expected to receive GitHub App Veripsa review.

## Acceptance

- HUD tool state remains readable in both ready and recovering states.
- No scene, package, asmdef, Animator, NavMesh, Physics, save, or asset import changes are included.
- No raw Unity logs, local personal paths, tokens, secrets, or private URLs are committed.
