# WORLD-011-B — Biome Placement Configuration

- **Status:** IMPLEMENTATION DELIVERED — Unity EditMode execution and live prefab assignment pending
- **Parent work package:** WORLD-011 — Procedural Environment Placement
- **Scope:** data model and Inspector configuration for deterministic Plains / Forest placement
- **Not included:** runtime placement algorithm, bootstrap integration, prefab asset creation, or Unity validation

## 1. Objective

Define one reusable, editable configuration for environment placement. Biome rules and individual prefab rules must be data-driven so that density, scale, spacing, and geographic constraints can be tuned without changing the placement algorithm.

The configuration consumes the existing WORLD-002 geographic fields. It must not generate or modify terrain, climate, water, or biome data.

## 2. Proposed asset and code locations

Runtime data model:

`Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementSettings.cs`

Suggested configuration asset:

`Assets/Project/Content/Configuration/World/CFG_SphericalEnvironmentPlacement.asset`

The asset folder is a proposed location only. Confirm the live project's configuration-folder convention before creating the asset. The settings class should use `[CreateAssetMenu]` so the asset can be created from Unity after the code is installed.

Namespace:

`Pokemon.Project.World.Spherical.Environment`

The existing spherical runtime assembly is `Pokemon.Project.World.Spherical`; no new assembly definition is required for this data model.

## 3. Configuration hierarchy

Use a `ScriptableObject` containing an array of biome rules. Each biome rule contains an array of prefab placement rules.

Conceptual structure:

```text
SphericalEnvironmentPlacementSettings
└── BiomeRule[]
    ├── biome: Plains | Forest
    ├── enabled
    └── PrefabRule[]
        ├── prefab reference
        ├── enabled
        ├── density / selection weight
        ├── minimum spacing
        ├── scale range
        ├── yaw range
        ├── slope range
        ├── optional altitude range
        ├── optional moisture range
        ├── optional temperature range
        ├── surface offset
        └── exclusion radius
```

Use serializable nested classes (or equivalent serializable data types) so Unity can edit the values directly in the Inspector. Keep runtime settings data-only: no scene references, generated instances, or mutable world state should be stored in the asset.

## 4. Root settings

| Field | Type | Purpose | Initial value / rule |
|---|---|---|---|
| `schemaVersion` | `int` | Supports future serialized migrations | `1` |
| `plainsEnabled` / biome rule enabled | `bool` | Enables Plains placement | `true` |
| `forestEnabled` / biome rule enabled | `bool` | Enables Forest placement | `true` |
| `globalSeedOffset` | `int` | Optional stable variation without changing the terrain seed | `0` |
| `globalDensityMultiplier` | `float` | Global density tuning | `1.0`, clamped to non-negative |
| `maxInstances` | `int` | Hard safety cap for one generation pass | Explicit configurable cap; choose after Tiny World profiling |
| `defaultSurfaceOffsetMeters` | `float` | Default lift from sampled terrain surface | Small non-negative value; tune after prefab pivot audit |

Do not duplicate the planet radius or world seed in this asset. The generator receives the existing `SphericalGeographicFieldSet`, which already exposes `Seed` and `PlanetRadiusMeters`.

## 5. Per-prefab rule fields

| Field | Type | Semantics |
|---|---|---|
| `id` | `string` | Stable identifier used by deterministic selection; must be unique within the asset |
| `prefab` | `GameObject` | Reference to the generated environment prefab; must not be null for an enabled rule |
| `enabled` | `bool` | Allows disabling an asset without deleting its configuration |
| `selectionWeight` | `float` | Relative chance when multiple rules compete for the same accepted candidate; non-negative |
| `targetDensityPerSquareKilometre` | `float` | Desired approximate count per km² before spacing and geographic filters; non-negative |
| `minimumSpacingMeters` | `float` | Minimum separation from another instance governed by the same spacing group; non-negative |
| `spacingGroup` | `string` | Allows different prefab types to share an exclusion/spacing constraint |
| `minimumScale` / `maximumScale` | `float` | Uniform scale range; both positive and maximum must be >= minimum |
| `minimumYawDegrees` / `maximumYawDegrees` | `float` | Rotation around local radial up; supports wrap-safe range normalization |
| `maximumSlopeDegrees` | `float` | Rejects steeper terrain; range 0–90 |
| `minimumAltitudeMeters` / `maximumAltitudeMeters` | `float` | Optional altitude filter; disabled unless explicitly enabled |
| `minimumMoisture` / `maximumMoisture` | `float` | Optional normalized moisture filter; range 0–1 |
| `minimumTemperature` / `maximumTemperature` | `float` | Optional normalized temperature filter; range 0–1 |
| `surfaceOffsetMeters` | `float` | Per-prefab override; non-negative |
| `exclusionRadiusMeters` | `float` | Additional local clearance around this object; non-negative |

### Density and selection semantics

`targetDensityPerSquareKilometre` is a tuning target, not a promise that exactly that many objects will appear. The actual count is reduced by land/biome eligibility, slope and optional environmental filters, spacing, and the global instance cap.

The deterministic candidate selector should convert the target density into a stable selection probability using the represented surface area of a candidate cell. Avoid a fixed probability per cube-sphere lattice sample: changing `faceResolution` must not silently change the expected world density.

`selectionWeight` only resolves competition between eligible prefab rules. It is not a substitute for density. If the implementation chooses one asset per candidate, weights must be normalized over the eligible rules at that candidate.

## 6. Initial biome rule catalogue

The following is the initial asset catalogue. Exact density values are intentionally left as tuning values to set after prefab scale/pivot inspection and the first Play Mode placement pass; inventing numerical density targets before that would conflate asset scale with visual frequency.

### Plains

| Prefab role | Expected prefab | Initial frequency | Geographic constraints |
|---|---|---|---|
| Short grass | `PF_Grass_Short` | Common | Dry Plains; low slope |
| Wild grass | `PF_Grass_Wild` | Medium | Dry Plains; low-to-moderate slope |
| Tall grass | `PF_Grass_Tall` | Uncommon | Dry Plains; low-to-moderate slope |
| Small stone cluster | `PF_Stone_Cluster_Small` | Sparse | Dry Plains; slope-limited |
| Large stone cluster | `PF_Stone_Cluster_Large` | Very sparse | Dry Plains; stricter spacing |
| Reed clump | `PF_Reed_Clump` | Conditional / uncommon | Moist lowland, near water; land cells only |

### Forest

| Prefab role | Expected prefab | Initial frequency | Geographic constraints |
|---|---|---|---|
| Wild grass | `PF_Grass_Wild` | Common | Dry Forest; low-to-moderate slope |
| Tall grass | `PF_Grass_Tall` | Common | Dry Forest; low-to-moderate slope |
| Leaf litter | `PF_Leaf_Litter` | Common | Dry Forest; low slope |
| Red mushroom | `PF_Mushroom_Red` | Uncommon | Dry Forest; moisture preference |
| Fallen log | `PF_Fallen_Log` | Uncommon | Dry Forest; slope-limited |
| Tree stump | `PF_Tree_Stump` | Uncommon | Dry Forest; slope-limited |
| Small stone cluster | `PF_Stone_Cluster_Small` | Sparse | Dry Forest; slope-limited |
| Large stone cluster | `PF_Stone_Cluster_Large` | Very sparse | Dry Forest; stricter spacing |

The prefab names above are expected outputs of the previously delivered generator. They are not verified asset references in the current source snapshot. Assign actual prefab objects through the Inspector once the live generated folder is available.

Desert and Mountains must not be added to the first configuration asset. They remain deferred in WORLD-011.

## 7. Geographic eligibility contract

The configuration describes constraints; the future generator applies them against the existing field set:

1. `SampleWaterClass(direction)` must return `Land`. Ocean, lake/pond, and river cells are rejected.
2. `SampleBiome(direction)` must match the containing biome rule.
3. `Sample(SlopeDegrees, direction)` must fall within the configured slope range.
4. Optional altitude, moisture, and temperature filters are evaluated only when enabled.
5. Candidate position is calculated from `direction * (PlanetRadiusMeters + sampledAltitude + surfaceOffset)`.

Continuous fields must use `SphericalGeographicFieldSet.Sample`; categorical fields must use `SampleBiome` / `SampleWaterClass`. Do not interpolate enum IDs.

## 8. Determinism contract

The settings asset must not contain a mutable random generator. The future placement generator derives deterministic values from:

- `SphericalGeographicFieldSet.Seed`;
- `globalSeedOffset`;
- stable cube-sphere cell identity (face, x, y);
- stable prefab rule `id`.

Do not use `UnityEngine.Random` global state. Results must not depend on face iteration order, scene object order, or unrelated random calls elsewhere in the game.

Changing only visual tuning values may change placements for affected rules, but identical seed + identical settings must reproduce the same prefab choices, positions, rotations, and scales.

## 9. Validation responsibilities

Add a `Validate()` method to the settings asset (or a pure validator) that checks:

- supported `schemaVersion`;
- no duplicate or empty rule IDs;
- no null prefab on enabled rules;
- no negative density, weight, spacing, exclusion radius, or offsets;
- valid positive scale ranges;
- valid slope range;
- valid normalized moisture/temperature ranges;
- valid optional min/max pairs;
- no duplicate biome rule entries;
- no unsupported biome entries in the first pass (only Plains and Forest).

Validation should report all detected issues in one pass where practical. It must not silently invent missing prefab references or rewrite user-authored tuning values without an explicit migration.

## 10. Implementation sequence after review

1. Implement `SphericalEnvironmentPlacementSettings.cs` with serializable data types, `CreateAssetMenu`, schema version, and validation.
2. Add EditMode tests for validation and range handling.
3. Create the configuration asset in the verified project configuration folder.
4. Assign the actual generated prefab references in Unity Inspector.
5. Verify pivots, dimensions, orientation, materials, and bounds before tuning numeric densities and offsets.
6. Only then proceed to WORLD-011-C deterministic candidate sampling and placement.

## 11. Acceptance criteria for WORLD-011-B

- The settings structure can express all approved Plains and Forest prefab rules without algorithm-specific hardcoding.
- Every active prefab rule has a stable ID and a prefab reference.
- Density, scale, yaw, slope, optional environmental filters, spacing, and surface offset are editable.
- Invalid configuration is reported clearly.
- No Desert or Mountains placement rules are introduced.
- No placement runtime or bootstrap code is changed in this sub-task.
- Actual prefab references and final density numbers are not claimed as verified until inspected in Unity.

## 12. Commit policy

After implementation and validation, prepare a commit message describing the validated result. If implementation is delivered but Unity validation or prefab assignment remains outstanding, use a checkpoint commit message that states that limitation explicitly. Do not describe WORLD-011-B as implemented merely because this design document exists.


## 13. Implementation delivery (2026-11-19)

Implemented files:

- `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementSettings.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/Environment/SphericalEnvironmentPlacementSettingsTests.cs`

The runtime asset contains a versioned data model, Plains/Forest biome rule entries, per-prefab distribution/transform/geographic fields, aggregate validation (`GetValidationIssues`) and throwing validation (`Validate`). No prefab references, concrete density targets, runtime placement code, or bootstrap integration were fabricated. An empty configuration is valid so the asset can be authored incrementally in the Inspector.

The EditMode test source covers empty configuration, enabled/disabled prefab references, duplicate IDs and biome rules, unsupported biomes, invalid numeric ranges, non-finite values, and aggregated exception reporting. Tests have **not** been executed in Unity in this delivery; compilation and test execution remain to be confirmed in the Editor.

### Current implementation boundary

- Data model and validation source: delivered.
- EditMode test source: delivered, not executed.
- Unity configuration asset creation: pending live-project confirmation.
- Actual prefab assignments and numeric density tuning: pending prefab inspection.
- WORLD-011-C deterministic placement algorithm: not started.

### Checkpoint commit message

`feat(world): add spherical environment placement settings`

This checkpoint describes the delivered settings model and tests only; it does not claim Unity compilation, test execution, prefab assignment, or placement completion.
