# Commercial Progress: Saved Reward Restore Event

Date: 2026-06-27

## Goal Support

This lane improves the production combat slice's local-save path by making a restored claimed reward visible as a saved-progress event instead of a generic already-claimed state. It supports the Steam-first MVP requirement that local progress recovery is understandable after returning to the game.

## Systems Touched

- Production combat slice runtime state
- Local save restore feedback
- PlayMode smoke coverage

## Files Added Or Changed

- `Assets/Scripts/ProductionCombatSliceController.cs`
- `Assets/Tests/PlayMode/SliceSceneSmokeTests.cs`
- `reports/commercial-progress-saved-reward-restore-event-2026-06-27.md`

## Implementation

- Added a single runtime event string for saved reward restoration.
- When local save data restores a claimed reward, the controller now reports `Saved reward restored`.
- Existing scene reload and preseeded-save PlayMode paths now assert both `Progress restored` and the restored reward event.

## Tests

- Static validation is expected for the exact changed files.
- Unity PlayMode validation is blocked while another Unity batchmode process is active in this project family; it must be run serially before a final release claim.

## Acceptance Conditions

- Loading a save with the reward already claimed enters the completed state.
- The shortcut, boss gate, and reward pad remain restored.
- The player-visible event line reports `Saved reward restored`.

## Next Smallest Useful Task

Run the targeted PlayMode test once Unity batchmode is clear, then add a fresh app-start packaged smoke for the same saved reward restore path.
