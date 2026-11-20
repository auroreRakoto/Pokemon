# MOD-001-B — Dependency validation and activation report

- **Status:** IMPLEMENTED — Unity validation NOT RUN
- **Work package:** MOD-001 (APPROVED)
- **Increment:** MOD-001-B
- **Owner:** ARCH / World Composition
- **Owner decision:** missing dependency blocks only the dependent optional capability; unrelated modules continue; invalid World Core blocks world creation.
- **Plan milestone:** owner-reported pushed; exact commit hash not supplied.

## 1. Source authority and merge strategy

The implementation was based on these supplied snapshots:
- `Assets.zip` SHA-256: `1ab647ddb59bcf7022bc7314fe00b8724be9a1689d231dabd18258213d0078db`
- Latest MOD-001-A deliverable (`MOD-001-A_WORLD_MODULE_PROFILE_CONFIG_4.zip`) SHA-256: `17b32718e1bab7c5f88554cc6e12cbec8c11f2fa00b48d7d4f531f9c9d63930e`
- Approved MOD-001 plan source (`MOD-001_OPTIONAL_WORLD_MODULE_COMPOSITION_v2.zip`) SHA-256: `d70032df112bbeffef6c83249a88b4599dc46f0ab1ea3d835fa45822bb35cdec`

The latest MOD-001-A profile/module files were overlaid as the authoritative generated versions because they were created after the Assets archive. The Assets archive supplied the existing runtime assembly, EditMode test assembly, geographic/world contracts and bootstrap context. No older profile/module source was copied over the newer MOD-001-A files.

## 2. Implemented

- Added immutable provider-availability input `SphericalWorldModuleValidationContext`.
- Added stable module identifiers, statuses, per-module diagnostics and an immutable activation report.
- Added deterministic pure-C# validation in `SphericalWorldModuleValidator`.
- Added 10 EditMode test cases for World Core failure, independent optional failure, biome/terrain dependencies, environment catalog and policy dependencies, player providers and future Pokémon availability.
- Added the approved local-failure policy to DEC-049 and changed MOD-001 from PROPOSED to APPROVED.

## 3. Dependency rules implemented

| Capability | Validation rule |
|---|---|
| World Core | Required; invalid core blocks world creation and all requested capabilities |
| Terrain presentation | Requires valid World Core and an available terrain provider |
| Biome classification | Requires valid World Core |
| Biome ground presentation | Requires active terrain presentation, biome classification and biome presentation resources |
| Environment assets | Requires valid World Core and a valid prefab catalog; requires biome classification only when the selected placement policy declares it |
| Player | Requires valid World Core, a spawn query and movement configuration; does not require terrain rendering |
| Pokémon | Requires valid World Core and an implemented/available Pokémon runtime; otherwise only Pokémon is blocked |

Unrequested modules are `Disabled`; requested and satisfiable modules are `Enabled`; required World Core is `Required`; requested modules with missing dependencies are `Blocked`. Each decision includes a human-readable diagnostic.

## 4. Deliberate integration boundary

This increment validates a requested module set but does not yet alter the existing `SphericalBiomeInspectionBootstrap`, geography generator, field-set contract, environment placement generator, renderer or player lifecycle. No current runtime objects are being conditionally created yet. That wiring is a later MOD-001 increment and will consume this report at the world composition root.

The context is intentionally explicit: the future composition root must report whether the current environment placement policy requires biomes and whether each provider is actually available. It must not infer provider availability from null-reference exceptions.

## 5. Code-standard audit

| File | Functions/methods | Maximum logical body lines | Description comments |
|---|---:|---:|---|
| `SphericalWorldModuleValidationContext.cs` | 1 constructor | 14 | Present |
| `SphericalWorldModuleActivationReport.cs` | 3 | 8 | Present |
| `SphericalWorldModuleValidator.cs` | 10 | 15 | Present above every method |
| `SphericalWorldModuleValidatorTests.cs` | 10 | 13 | Present above every test |

No file exceeds 10 functions/methods. No function exceeds 50 logical code lines. The validator is deterministic, has no Unity object creation, no global mutable state, no Editor dependency and no random input.

## 6. Validation results

| Check | Status | Evidence |
|---|---|---|
| Source archive and MOD-001-A overlay inspected | PASS | Exact supplied ZIPs and relevant code inspected |
| Runtime assembly boundary | PASS — static | New files remain under existing Spherical runtime assembly; no asmdef edit |
| EditMode assembly boundary | PASS — static | Tests added under existing Spherical EditMode test assembly |
| Function count and line limit | PASS — static | See table above |
| Function description comments | PASS — static | Present above each constructor/method/test |
| New `.meta` files | PASS — static | Unique GUIDs and MonoImporter metadata authored |
| C# compile | NOT RUN | Unity Editor is owner-operated; no local Unity compile performed |
| EditMode tests | NOT RUN | Test Runner must be run by owner |
| Preview | N/A | Pure validation logic; no visual output in this increment |
| Runtime integration | NOT IMPLEMENTED | Deferred to later MOD-001 increment |
| PlayMode / target scene | NOT RUN | Runtime integration is not part of MOD-001-B |

## 7. Owner runbook

1. Extract the ZIP at the Unity project root, preserving paths.
2. Confirm Unity imports the four new C# scripts and their `.meta` files.
3. Run `SphericalWorldModuleValidatorTests` in EditMode.
4. Confirm all 10 tests pass and no new compile errors appear.
5. Send the Test Runner result and Console screenshot before MOD-001-C/integration proceeds.

## 8. Files changed

New runtime:
- `Assets/Project/Runtime/World/Spherical/Composition/SphericalWorldModuleValidationContext.cs`
- `Assets/Project/Runtime/World/Spherical/Composition/SphericalWorldModuleActivationReport.cs`
- `Assets/Project/Runtime/World/Spherical/Composition/SphericalWorldModuleValidator.cs`

New tests:
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalWorldModuleValidatorTests.cs`

New documentation:
- `Documentation/03_ENGINEERING/WORK_PACKAGES/MOD-001_OPTIONAL_WORLD_MODULE_COMPOSITION.md` (status and failure policy updated)
- `Documentation/00_GOVERNANCE/DECISIONS/DEC-049_OPTIONAL_INDEPENDENT_MODULES_DRAFT.md` (confirmed policy updated)
- `Documentation/06_AUDIT/MOD-001-B_IMPLEMENTATION_REPORT.md`
- `Documentation/03_ENGINEERING/TEST_PLANS/MOD-001-B_DEPENDENCY_VALIDATION_TEST_PLAN.md`

No existing source files, assets, scenes, prefabs, ScriptableObjects or `.meta` files were deleted or modified in this increment.

## 9. Next increment

MOD-001-C — separate World Core geographic fields from optional biome classification. This requires a separate caller/consumer audit of `SphericalGeographicFieldSet`, `SphericalGeographicFieldGenerator`, biome sampling, environment placement and movement queries before changing any public contract.

## 10. Checkpoint commit

`feat(world): add optional module dependency validation`

`Work-Package: MOD-001`
`Agent: ARCH`
