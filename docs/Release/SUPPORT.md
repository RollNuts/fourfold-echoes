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
