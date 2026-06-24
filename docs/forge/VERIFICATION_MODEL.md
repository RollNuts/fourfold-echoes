# Verification Model

Forge exists to replace self-report with evidence.

## Evidence Types

- Schema validation.
- Stable ID validation.
- Unity scene load.
- Required entity checks.
- Required component checks.
- Console error scan.
- PlayMode scenario.
- Deterministic replay summary.
- Screenshot capture.
- Performance summary.
- Build result.

## Director-Facing Evidence

The director should see:

- What changed.
- Before/after screenshot or clip.
- Whether audio changed.
- How to play the artifact.
- Whether the run reached the expected state.
- Any warnings or missing verification.

The director should not need to inspect Unity hierarchy, components, or console
manually for normal review.

## Claim Rules

- Do not claim "playable" without running the scene or build.
- Do not claim "looks good" without screenshot or capture evidence.
- Do not claim "audio works" without runtime audio status or a played artifact.
- Do not claim "build-ready" without a build artifact.
- Do not claim "safe to merge" just because Veripsa Core is Clear.

Core coordination and Unity correctness are separate:

```text
Veripsa Core = coordination signal
Unity verify = runtime correctness signal
Human review = product judgment
```
