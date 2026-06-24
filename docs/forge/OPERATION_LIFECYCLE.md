# Operation Lifecycle

Every Forge change follows the same lifecycle:

```text
inspect -> plan -> apply -> verify -> evidence -> review -> merge -> reconcile
```

## Inspect

Read the current semantic state:

- Project.
- Scene.
- Entity.
- Component.
- Scenario.
- Generated asset mapping.

Inspect should not mutate Unity.

## Plan

Plan predicts the change before touching Unity:

- Semantic entities affected.
- Generated Unity files expected.
- Manual assets referenced.
- Tests to run.
- Screenshot cameras to capture.
- Veripsa lane suggestion.
- Rollback path.
- Risks.

## Apply

Apply runs through the Unity Adapter.

Rules:

- Snapshot before writing.
- Write only generated regions.
- Preserve stable ID mappings.
- Do not rewrite manual assets.
- Fail closed on missing references.

## Verify

Verification must include machine evidence:

- Schema validation.
- Duplicate stable ID check.
- Scene load.
- Required entity/component checks.
- Console error scan.
- Scenario result.
- Screenshot capture where visual change is claimed.

## Rollback

Rollback must know:

- Operation ID.
- Changed files.
- Previous stable ID mapping.
- Generated assets created.
- Snapshot location.

Git remains the final rollback safety net, but Forge should produce operation
level rollback instructions.
