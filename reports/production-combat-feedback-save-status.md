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
- Restored completed reward progress now reports a player-visible `Saved reward restored` event.
- Low health now gets a red edge warning and critical-health label while the slice is in play.
- Automated coverage now includes a fresh-start-equivalent save path: complete the reward, persist to disk, unload the production scene, reload it, and restore the completed state from disk.

## Verification

- `git diff --check -- <changed exact paths>`: passed.
- `node Scripts/Validation/validate_repo.mjs`: passed.
- `node Scripts/Validation/check_public_repo_hygiene.mjs`: passed.
- Sanitization scan over changed C# and test files: passed.
- Unity production scene validation in an isolated worktree: exit code 0; validation passed.
- Unity PlayMode was attempted twice in the isolated worktree, but Unity shut down without producing Test Runner XML. No latest PlayMode pass is claimed.
- Unity Windows build smoke reached the production build entry point, then exited 1 because this editor does not have `StandaloneWindows64` support installed. No artifact was produced.
- Earlier `ProductionCombatSlice` scene verifier and 30-second smoke evidence remain recorded in `reports/scene-ProductionCombatSlice.md`.
- The fresh-start-equivalent PlayMode test still needs a serialized Test Runner rerun with XML output and exit code 0.

## Warnings

- Unity emitted non-failing local environment noise around metrics cache, licensing entitlement lookup, ADB shutdown, and missing local dotnet build-server during shutdown.
- Several imported character FBX assets reported rig configuration warnings during the temporary project import. They are asset-import warnings outside this lane and were not gameplay-bound here.

## Remaining Gap

- Manual controller-device playthrough is still required; automated PlayMode proves the route state and save flags, not controller feel.
- Current Unity Test Runner XML evidence still needs to be regenerated.
- Windows Standalone build support is required before the slice can produce a release-candidate Windows artifact.

## Asset Library Note

No `unity-game-asset-library` assets were imported or bound in this lane. The work was limited to existing runtime feedback and tests.
