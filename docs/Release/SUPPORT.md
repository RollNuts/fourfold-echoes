# Support Plan

Status: D-020 release planning document.

## Purpose

Define the support surface for the Steam-first release without adding live
service, accounts, telemetry, or always-online dependencies.

## Support Contact

| Field | Value |
| --- | --- |
| Public support email | TBD before Steam page publication |
| Steam forum | Planned for launch support |
| Bug report thread | Planned for demo and launch windows |
| Response language | English and Japanese first |

Do not publish a Steam page until the public support contact is filled. `TBD`
is acceptable only while this document is a planning artifact.

## Player Report Template

Ask players for:

- build version
- platform and device
- input device
- language
- last scene, room, or boss
- expected behavior
- actual behavior
- save file if relevant
- screenshot or short video if available

## Response Templates

### Initial Bug Acknowledgement

Thanks for the report. We are checking this against the current build. If you
can, please send your build version, platform, input device, language, last
scene or boss, and whether this happened after loading a save.

### Request For Save / Logs

We need one more piece of evidence to reproduce this safely. Please send the
save file, crash log if one exists, and the last room or boss entered before the
issue appeared. Do not delete the affected save until we confirm whether it is
recoverable.

### Workaround Reply

We have not shipped a fix yet. Current workaround: `<workaround or none>`. We
will update the known-issues post when a verified fix is scheduled.

### Duplicate Reply

This matches an issue we are already tracking as `<issue id>`. We linked your
details to the existing report and will update the main known-issues thread
when the status changes.

### Cannot Reproduce / Watchlist Reply

We could not reproduce this with the current information. Because it may affect
progress, saves, crashes, or controls, it remains on the watchlist. Additional
save files, logs, or short videos will help us verify it.

### Fixed In Next Build

We reproduced this and have a fix verified internally. It is scheduled for the
next build after regression checks for save, controller, and affected boss or
room progression pass.

### Post-Patch Closure

This issue should be fixed in build `<version>`. Please restart Steam to update.
If it still happens on that build, reply with your save file and build version
so we can reopen the report.

## Triage Routing

| Report Type | Priority Owner |
| --- | --- |
| launch failure | release |
| crash | engineering |
| save corruption | engineering |
| boss progression blocker | design/engineering |
| controller lockout | QA/engineering |
| missing required SFX or unreadable tell | audio/design |
| visual clipping or unreadable UI | art/UI |

## Boundaries

- No account recovery because v1 has no accounts.
- No server status page because v1 has no live server.
- No promises for new content in support replies.
- Hotfix replies should reference confirmed fixes only.
