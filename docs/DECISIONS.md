# FOURFOLD ECHOES Decisions

This log is for the private commercial Unity repository.

| ID | Decision | Status | Rationale |
| --- | --- | --- | --- |
| D-001 | Keep the commercial Unity project private. | Accepted | The public repository exists mainly to demonstrate Veripsa Core coordination. Runtime implementation, visual polish, audio direction, asset pipeline, and Steam-facing product work should not reveal avoidable commercial detail. |
| D-002 | Unity 6.3 LTS is the current runtime target. | Accepted | The game now needs fixed-angle 3D feel, animation, lighting, input, controller, build, and Steam-readiness evidence. Unity is the right spike path for that evidence. |
| D-003 | Gate A proves one playable room before expanding scope. | Accepted | Movement, attack, dodge, phase interaction, enemy feedback, gate claim, camera, and placeholder audio must feel coherent before networking, asset production, or Steam work expands. |
| D-004 | Treat Unity MCP as a candidate, not a default dependency. | Accepted | Unity MCP can expose high-trust editor automation to an AI client. Do not install or enable it by default until source, license, localhost binding, remote-access behavior, generated config, and prompt-injection risk are reviewed. Runtime evidence still requires running the game. |
| D-005 | Keep third-party and generated assets out until provenance is recorded. | Accepted | Steam sale requires asset rights discipline. Gate A uses Unity primitives and procedural placeholder tones only. |
