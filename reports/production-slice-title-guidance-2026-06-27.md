# Production Slice Title Guidance - 2026-06-27

## Final Goal Support

This change moves the production combat slice closer to Steam-first usability by making the first visible screen state the room goal and the controller-first input path before the run starts.

## Systems Touched

- Production combat slice runtime UI.
- QA evidence for the title guidance lane.

## Files Added/Changed

- `Assets/Scripts/ProductionCombatSliceUi.cs`
- `reports/production-slice-title-guidance-2026-06-27.md`

## Implementation

- Replaced the generic title copy with concrete route objectives: clear two wardens, use the Echo Tool shortcut, break the boss gate, and claim the reward.
- Added compact controller and keyboard input guidance to the title overlay.
- Did not add new mechanics, inventory, crafting, quest, social, networking, or open-world systems.

## Tests

- `git diff --check -- Assets/Scripts/ProductionCombatSliceUi.cs reports/production-slice-title-guidance-2026-06-27.md`
- `node Scripts/Validation/validate_repo.mjs`
- `node Scripts/Validation/check_public_repo_hygiene.mjs`
- Unity batchmode was not launched for this branch because existing Unity batchmode jobs were already active. This preserves the one-Unity-validation-at-a-time rule. Unity exit code for this branch was therefore not produced.

## Acceptance Conditions

- The production slice title screen states the immediate player objective.
- Controller-first input is visible before starting the run.
- The change remains scoped to UI copy and QA reporting.
- No secrets, raw Unity logs, personal local paths, or private URLs are included.

## Next Smallest Useful Task

Run a real visual pass on the production slice title screen and tune layout if any title text wraps poorly at Steam Deck and 1080p desktop resolutions.
