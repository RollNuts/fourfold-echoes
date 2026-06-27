# Commercial Progress: Title Pause Retry Clear Proof

Date: 2026-06-27

## 1. Final Goal Support

This pass reduces risk on the highest-priority commercial route:
Title -> Game -> Pause -> Retry -> Clear. The route now has a deterministic
PlayMode smoke path that can complete the production slice without fake input
timing or broad new systems.

## 2. Systems Touched

- Production slice run state and progression hooks
- Production slice UI readability and save-state HUD feedback
- Low-health presentation warning for live play
- Production slice PlayMode smoke coverage

No inventory, crafting, quest log, social system, open world, networking
dependency, or new input package was added.

## 3. Files Added/Changed

- Changed: `Assets/Scripts/ProductionCombatSliceController.cs`
- Added: `Assets/Scripts/ProductionCombatLowHealthWarning.cs`
- Changed: `Assets/Scripts/ProductionCombatOnboardingHint.cs`
- Changed: `Assets/Scripts/ProductionCombatSliceUi.cs`
- Added: `Assets/Tests/EditMode/ProductionCombatLowHealthWarningTests.cs`
- Changed: `Assets/Tests/PlayMode/SliceSceneSmokeTests.cs`
- Changed: `docs/test-plan.md`
- Changed: `reports/commercial-gap-map.md`
- Added: `reports/commercial-progress-title-pause-clear-2026-06-27.md`
- Added: `reports/production-combat-feedback-save-status.md`
- Changed: `reports/scene-ProductionCombatSlice.md`

Existing dirty files outside this lane were left untouched.

## 4. Implementation

- Added narrow public progression methods:
  - `ClearMinorWardens`
  - `ClearBossGate`
  - `ClaimReward`
- Routed the in-scene reward interaction through `ClaimReward` so gameplay and
  tests use the same completion transition.
- Added a PlayMode smoke test that proves:
  - title state starts correctly
  - begin run enters gameplay
  - pause enters paused state
  - retry returns to gameplay with transient progress reset
  - exploration tool opens the shortcut
  - wardens, boss gate, reward, completed state, save flags, and return to
    title all connect in one route
- Added a scene reload PlayMode proof that claims the reward, reloads
  `ProductionCombatSlice`, and verifies restored shortcut, boss, reward,
  completed state, reward pad visibility, and save status.
- Added a fresh-start-equivalent PlayMode proof that writes reward completion to
  disk, unloads the production scene through `D020VerticalSlice`, reloads
  `ProductionCombatSlice`, and verifies the reward state restores from disk.
- Updated `reports/commercial-gap-map.md` so the commercial audit reflects the
  deterministic route proof and scene reload proof as executed.
- Added clear title/onboarding copy for controller and keyboard routes.
- Added HUD save status text so the player can see local save readiness,
  restoration, saved progress, or save failure.
- Added a low-health screen-edge warning and critical-health label in the
  production slice only.

## 5. Tests

Static checks executed:

- `git diff --check` on the exact D-030 lane paths
  - Result: passed.
- `git diff --cached --check` on the exact D-030 lane paths
  - Result: passed.
- `node Scripts/Validation/validate_repo.mjs`
  - Result: passed. Required reset files present: 57.
- `node Scripts/Validation/check_public_repo_hygiene.mjs`
  - Result: passed. Scanned tracked/untracked files: 6556.
- Sanitization scan over the changed exact paths
  - Result: passed. No personal local path, credential assignment, private key,
    credentialed URL, or database URL pattern found.

Unity batchmode executed against a temporary project copy to avoid launching a
second Unity process against the open working checkout:

- EditMode `FourfoldEchoes.EditModeTests`: exit code 0, 20 total, 20 passed,
  0 failed.
- PlayMode `FourfoldEchoes.PlayModeTests`: exit code 0, 9 total, 9 passed,
  0 failed.

After that run, this lane added one fresh-start-equivalent PlayMode test. The
checkout currently has `Temp/UnityLockfile`, so Unity was not relaunched against
the same project. Rerun PlayMode before merging to count the new test.

A later isolated-worktree Unity attempt did not reach Test Runner results
because licensing initialization failed first. Treat current Unity XML evidence
as pending until licensing recovers and the updated suite is rerun.

Warnings observed and treated as shutdown/environment noise after passing test
results: Unity CoreBusinessMetrics SQLite cache lock messages, .NET SDK
build-server shutdown text, ADB/input-system shutdown messages.

The final PlayMode run includes the scene reload proof.

## 6. Acceptance Conditions

Accepted for this pass when:

- The production controller exposes deterministic progression transitions
  without adding a new game system.
- Reward claiming uses the same method for gameplay and PlayMode proof.
- The PlayMode test covers Title -> Game -> Pause -> Retry -> Clear and verifies
  the saved shortcut, boss, and reward flags.
- A scene reload PlayMode proof exists for saved reward restoration.
- A fresh-start-equivalent PlayMode proof exists for saved reward restoration
  after unloading the production scene.
- Static validation passes.
- Unity EditMode and PlayMode test assemblies pass with exit code 0 for the
  earlier 9-test PlayMode run. The latest added fresh-start test still needs the
  next serialized Unity rerun after licensing recovers.

## 7. Next Smallest Useful Task

Run the updated PlayMode suite in serialized Unity, then run a Windows build
smoke for the playable slice.
