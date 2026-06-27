# UI Localization-Ready HUD Prompt Pack

Date: 2026-06-27

## Goal Support

This pack supports the compact top-down action-adventure target by making the combat HUD, input prompts, status badges, modal, title/menu shell, and world prompts readable under combat pressure and resilient to longer Japanese strings. It does not add inventory, crafting, quest log, social, network, or open-world UI.

## Lane

- Lane: `ui-localization-ready-hud-prompt`
- Base: `origin/codex/production-combat-slice-scene-readability`
- Veripsa ACK reason: local Veripsa report says narrow docs/assets are Core-friendly and new runtime/editor C# should be split. This change adds isolated UI assets, localization data, and preview artifacts only.

## Scope Notes

- `asset_request.yaml` was not present in the checkout. Inputs were inferred from the session request and canonical product docs.
- Inventory is recorded as a localization boundary only because MVP scope explicitly excludes runtime inventory.
- Screen-space UI and world-space UI are separated in UXML and in `asset.json`.

## Acceptance

- HUD priority is visible through placement, size, and grouping.
- Icons are generated at 128 px source size and reviewed at 24 px and 32 px in the preview.
- Status differences use shape, border, notch, and symbol changes, not color alone.
- Japanese max string assumptions are recorded in `localization.csv` and `asset.json`.
- Gamepad and keyboard prompts have separate icon/text rows.

## Checks

- Passed: `node Scripts/Validation/validate_repo.mjs`
- Passed: `node Scripts/Validation/check_public_repo_hygiene.mjs`
- Passed: UXML parse, localization CSV parse, `asset.json` parse, and icon static QC.
- Passed: generated text asset scan for local paths, token markers, private-key markers, and raw file URLs.
- Passed: Unity batchmode `FourfoldProductValidator.RunAll`, exit code 0.
- Unity warnings: prefab assets have no LODGroup components; active validation scene has no LODGroup components. These are existing production-art/readiness warnings, not UI pack blockers.
- Preview reviewed in Codex app; no Unity/game browser visual inspection is claimed.
- Passed: `git diff --check` on the exact UI pack path set.
- Not yet run: staging, commit, push, PR creation, remote checks, merge.
