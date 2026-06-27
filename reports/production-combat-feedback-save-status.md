# Production Combat Feedback Save Status

Date: 2026-06-27

## Goal Support

This lane moves `ProductionCombatSlice` closer to a commercial vertical slice by making combat state, reward progression, save feedback, and low-health danger easier for a player to read without adding inventory, quest, crafting, open-world, or network systems.

## Systems Touched

- Runtime combat slice controller
- Runtime UI Toolkit HUD
- Runtime IMGUI onboarding and low-health overlays
- PlayMode smoke coverage for the production slice progression path
- PlayMode save restoration coverage after unloading the production scene

## Player-Visible Change

- The title and onboarding text now describe the actual compact room loop: clear two wardens, use the Echo Tool shortcut, break the boss gate, and claim the reward.
- The HUD exposes local save status such as ready, restored, saved, and save failure.
- Low health now gets a red edge warning and critical-health label while the slice is in play.
- Automated coverage now includes a fresh-start-equivalent save path: complete the reward, persist to disk, unload the production scene, reload it, and restore the completed state from disk.

## Verification

- `git diff --check -- <changed exact paths>`: passed.
- `node Scripts/Validation/validate_repo.mjs`: passed.
- `node Scripts/Validation/check_public_repo_hygiene.mjs`: passed.
- Sanitization scan over changed C# and test files: passed.
- Unity batchmode EditMode was attempted against an isolated worktree, but Unity licensing initialization failed before the Test Runner started. No result XML was produced.
- Unity PlayMode was not started after the EditMode licensing failure.
- Earlier `ProductionCombatSlice` scene verifier and 30-second smoke evidence remain recorded in `reports/scene-ProductionCombatSlice.md`.
- The fresh-start-equivalent PlayMode test still needs the next serialized Unity rerun after the licensing channel recovers.

## Warnings

- Unity emitted licensing initialization errors before test execution in the current verification attempt. No compile or test failure was produced by this attempt.
- Prior Unity runs emitted non-failing local environment noise around metrics cache, licensing entitlement lookup, ADB shutdown, and missing local dotnet build-server during shutdown.
- Several imported character FBX assets reported rig configuration warnings during the temporary project import. They are asset-import warnings outside this lane and were not gameplay-bound here.

## Remaining Gap

- Manual controller-device playthrough is still required; automated PlayMode proves the route state and save flags, not controller feel.
- Current Unity Test Runner XML evidence still needs to be regenerated after the licensing channel recovers.
- Windows build smoke is still required before the slice can be called release-candidate ready.

## Asset Library Note

No `unity-game-asset-library` assets were imported or bound in this lane. The work was limited to existing runtime feedback and tests.
