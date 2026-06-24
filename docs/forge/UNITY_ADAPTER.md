# Unity Adapter

The Unity Adapter converts Game IR into Unity-generated assets and evidence.

It does not make game design decisions. If an operation asks for an Ember altar,
the Adapter wires the corresponding known components and reports missing
references. It must not invent unrelated enemies, puzzles, or effects.

## Responsibilities

- Parse Game IR.
- Validate schemas.
- Maintain stable ID to Unity asset mapping.
- Generate scenes in the generated region.
- Generate proof prefabs.
- Wire known components.
- Assign registered materials.
- Configure proof camera and lights.
- Run EditMode / PlayMode validation.
- Capture screenshots.
- Generate reports.

## Fail Closed

The Adapter must fail instead of guessing when:

- Unity version does not match the project target.
- A semantic ID is duplicated.
- A referenced asset is missing.
- A manual asset would be overwritten.
- A generated asset mapping is ambiguous.
- Console errors appear during verification.
- A required component type is unsupported.

## Generated Region

Initial generated path:

```text
Assets/Generated/VeripsaForge/
```

The current Gate A prototype still uses simple generated C# scene construction.
Forge should migrate generated scene output into this region before production
content expands.

## Manual Region

Manual path:

```text
Assets/Manual/
```

Forge may read or reference manual assets only through registered semantic IDs.
It must not rewrite them.

## Evidence Output

Evidence is local by default and can be attached to PRs later:

```text
$TMPDIR/fourfold-echoes-evidence/
```

Evidence should include:

- Scene ID.
- Camera ID.
- Commit SHA.
- Operation ID.
- Unity version.
- Adapter version.
- Screenshot path.
- Console error status.
- Test result.
