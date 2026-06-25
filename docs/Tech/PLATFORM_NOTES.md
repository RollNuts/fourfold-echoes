# Platform Notes

## Steam First

Steam Windows is the first shipping target. The repo currently supports Unity macOS editor-side validation and macOS standalone build for evidence.

## Future PS5

PS5 is a future port candidate, not v1 scope.

Keep port-readiness by:

- avoiding custom backend dependency
- avoiding platform-specific gameplay code
- keeping controller-first UX
- maintaining asset LOD discipline
- separating save/platform adapters from gameplay

## Current Engine Facts

- Unity version: `6000.3.18f1`
- Render pipeline: Built-in, confirmed by `customRenderPipeline: 0`
- Input: current prototype input is not the final controller-first action layer
- Addressables: not present
- Networking packages: not present
