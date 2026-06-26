# Product Loop Player Completion Smoke

Generated UTC: `2026-06-26T16:21:39Z`

- Result: PASS
- Artifact: `Build/FourfoldEchoes/macos/FourfoldEchoes.app`
- Artifact size: `102M`
- Log: `artifacts/logs/player-smoke-product-loop.log`
- Sentinel: `FOURFOLD PLAYER SMOKE PASS scenes=Title>HubCrossroads>D020VerticalSlice>HubCrossroads>Title>HubCrossroads clearCount=1 relics=2/2 bestSeconds=91.0`

This report is intentionally sanitized for public commit. The raw player log is local evidence and is not committed.
The runtime smoke snapshots and restores the local save around its New Game route check.
Verified route: Title -> HubCrossroads -> D020VerticalSlice -> HubCrossroads -> Title -> HubCrossroads.
Verified outcome: D-020 clear persisted, both relic rewards banked, best time saved, and Continue returns to Hub.
