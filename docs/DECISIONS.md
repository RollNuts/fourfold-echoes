# FOURFOLD ECHOES Decisions

This log is for the private commercial Unity repository.

| ID | Decision | Status | Rationale |
| --- | --- | --- | --- |
| D-001 | Keep the commercial Unity project private. | Accepted | The public repository exists mainly to demonstrate Veripsa Core coordination. Runtime implementation, visual polish, audio direction, asset pipeline, and Steam-facing product work should not reveal avoidable commercial detail. |
| D-002 | Unity 6.3 LTS is the current runtime target. | Accepted | The game now needs fixed-angle 3D feel, animation, lighting, input, controller, build, and Steam-readiness evidence. Unity is the right spike path for that evidence. |
| D-003 | Gate A proves one playable room before expanding scope. | Accepted | Movement, attack, dodge, phase interaction, enemy feedback, gate claim, camera, and placeholder audio must feel coherent before networking, asset production, or Steam work expands. |
| D-004 | Prefer Unity's official MCP bridge and reject third-party MCP by default. | Accepted | Unity MCP can expose high-trust editor automation to an AI client. Use Unity's official `com.unity.ai.assistant` MCP path first. Do not install third-party bridges or enable external clients until localhost binding, client approval behavior, generated config, and prompt-injection risk are reviewed. Runtime evidence still requires running the game. |
| D-005 | Keep third-party and generated assets out until provenance is recorded. | Accepted | Steam sale requires asset rights discipline. Gate A uses Unity primitives and procedural placeholder tones only. |
| D-006 | Use CLI/file-contract as the stable Unity gate and MCP only as an acceleration layer. | Accepted | PR and CI evidence must not depend on an interactive editor bridge. MCP may speed up local editor inspection and `Tools/FOURFOLD/...` execution, but the same operation must remain reachable through `-batchmode -executeMethod` and file artifacts. |
| D-007 | Do not claim branch-protection-enforced required checks on the private repo yet. | Accepted | `gh api repos/RollNuts/fourfold-echoes/branches/main/protection` currently returns HTTP 403 requiring GitHub Pro or public visibility. Until that changes, Veripsa/CI can inform merges but cannot be enforced as GitHub required checks on this private repo. |
