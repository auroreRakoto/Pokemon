# WORLD-011 — Procedural Environment Placement

- **Status:** PROPOSAL — ready for review
- **Priority:** High
- **Estimated effort:** 12–16 hours for the first validated prototype
- **Depends on:** WORLD-002 geographic fields; WORLD-003 playable biome inspection; WORLD-010 biome rendering; generated Environment prefabs
- **Target scene:** `WORLD-002_BiomeInspection`
- **Initial scope:** deterministic placement for Plains and Forest only

## 1. Goal

Populate the playable spherical planet with the existing environment prefabs. Objects must follow the spherical surface, respect the generated geographic fields and remain reproducible for a given world seed.

This work package adds a first layer of environmental dressing. It does not yet implement a complete ecosystem, vegetation growth, biome transitions for all four biomes, streaming, or large-scale vegetation rendering optimizations.

## 2. Repository findings

The current playable inspection bootstrap already generates and retains a `SphericalGeographicFieldSet` in `_fields`.

The field set exposes the data needed for an initial placement pass:

- `SampleBiome(direction)` and `GetBiome(face, x, y)` for the categorical biome.
- `SampleWaterClass(direction)` and `GetWaterClass(face, x, y)` to reject ocean and other water cells.
- `Sample(SphericalGeographicFieldType.AltitudeMeters, direction)` for terrain elevation.
- `Sample(SphericalGeographicFieldType.SlopeDegrees, direction)` for slope.
- `Sample(SphericalGeographicFieldType.MoistureNormalized, direction)` and `Sample(SphericalGeographicFieldType.TemperatureNormalized, direction)` for future placement rules.
- `Seed`, `Resolution` and `PlanetRadiusMeters` for reproducibility and scale-aware sampling.

`SphericalBiomeInspectionBootstrap` creates six cube-sphere face objects under `WORLD-002_GeneratedPlayableInspection`. It currently creates the planet surface, player, camera and lighting, but no environment instances.

The repository's production FBX inventory in the reviewed project archive contains these 10 models:

| Existing model | Initial placement role |
|---|---|
| `Vegetation/Grass_Short/SM_Grass_Short.fbx` | Plains ground vegetation |
| `Vegetation/Grass_Tall/SM_Grass_Tall.fbx` | Plains/Forest vegetation |
| `Vegetation/Wild_Grass/SM_Grass_Wild.fbx` | Plains/Forest vegetation |
| `Vegetation/Reed_Clump/SM_Reed_Clump.fbx` | Moist lowland placement, subject to water/shoreline rule |
| `Ground/Leaf_Litter/SM_Leaf_Litter.fbx` | Forest floor detail |
| `NaturalFeatures/Fallen_Log/SM_Fallen_Log.fbx` | Forest floor feature |
| `NaturalFeatures/Mushroom_Red/SM_Mushroom_Red.fbx` | Forest floor feature |
| `NaturalFeatures/Tree_Stump/SM_Tree_Stump.fbx` | Forest floor feature |
| `Rocks/Stone_Cluster_Small/SM_Stone_Cluster_Small.fbx` | Plains/Forest rocks |
| `Rocks/Stone_Cluster_Large/SM_Stone_Cluster_Large.fbx` | Sparse larger rocks |

No complete tree model was present in that FBX inventory. The first forest pass must therefore not promise a forest canopy; it will use the available ground vegetation and natural features. Add full trees in a later asset pass.

The actual prefab destination is expected to mirror the model hierarchy under `Assets/Project/Content/Prefabs/Environment`. Confirm the generated prefab paths in Unity before wiring prefab references.

## 3. Proposed implementation

### 3.1 Placement configuration

Create a serializable placement configuration (preferably a `ScriptableObject`) containing per-biome rules. Each rule references prefab assets and exposes:

- minimum and maximum spacing / density controls;
- spawn probability or target density;
- minimum and maximum uniform scale;
- random yaw range;
- minimum and maximum allowed slope;
- optional altitude and moisture ranges;
- optional exclusion radius around other placed instances;
- surface offset, to prevent z-fighting or partial burial.

Start with explicit values that can be tuned in the Inspector. Do not hard-code biome-specific values throughout the placement algorithm.

### 3.2 Candidate sampling

Use the shared cube-sphere geographic lattice as the source of candidate directions.

For each face and candidate cell:
1. Read its biome and water class.
2. Reject any cell that is not dry land.
3. Reject cells outside the configured slope / altitude / moisture constraints.
4. Use a deterministic hash based on world seed, face, cell coordinates and placement rule to decide whether the candidate is selected.
5. Apply a minimum-distance check to prevent obvious clumping.
6. Sample altitude and place the instance at `direction * (planetRadius + altitude + surfaceOffset)`.

Do not generate candidates independently per face using face-local random sequences. The deterministic hash must use stable cell identity so generation is repeatable and does not depend on iteration order.

### 3.3 Spherical orientation

For every accepted candidate:
- Use the radial direction as the local up vector.
- Build a tangent frame at that direction.
- Apply the randomized yaw around the radial up axis.
- Keep scale uniform by default.
- Parent generated instances under a dedicated environment root, separate from the terrain faces and player.

The implementation must not assume global `Vector3.up` is the surface normal.

### 3.4 First biome rules

The first pass covers only Plains and Forest.

**Plains**
- Grass short: common.
- Wild grass: medium frequency.
- Grass tall: uncommon.
- Small stone cluster: sparse.
- Large stone cluster: very sparse.
- Reeds: only near suitable moist lowland areas; never on ocean cells.

**Forest**
- Wild grass and tall grass: common ground vegetation.
- Leaf litter: common ground detail.
- Mushrooms: uncommon, with moisture preference.
- Fallen logs and tree stumps: uncommon.
- Small stone clusters: sparse.
- Large stone clusters: very sparse.

These are initial qualitative frequencies, not final balance values. Exact densities and constraints must be exposed in the configuration and tuned visually in Play Mode.

Desert and Mountains are deliberately deferred until their asset sets and placement rules are reviewed.

## 4. Work breakdown

| Sub-task | Work | Estimate | Output |
|---|---|---:|---|
| WORLD-011-A | Audit generated prefab paths, model pivots, scale and bounds; identify unsuitable pivots | 1–2 h | Asset readiness notes |
| WORLD-011-B | Define biome placement configuration and per-prefab constraints | 2 h | Configuration asset / data model |
| WORLD-011-C | Implement deterministic spherical candidate sampling and placement | 3–4 h | Runtime placement generator |
| WORLD-011-D | Integrate generator into the playable inspection bootstrap and cleanup lifecycle | 1–2 h | Environment root generated/cleared with inspection world |
| WORLD-011-E | Tune Plains and Forest density, spacing, scale and slope constraints | 2–3 h | Visually acceptable first pass |
| WORLD-011-F | Add tests and perform Unity validation | 2–3 h | Tests and validation report |

**Estimated total: 12–16 hours.**

If the pivot/bounds audit reveals that multiple prefabs need correction, split that work into a separate asset-fix task rather than silently expanding the placement implementation.

## 5. Files expected to be created or modified

Expected new runtime files (final names can be adjusted to repository conventions after implementation begins):

- `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementSettings.cs`
- `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementGenerator.cs`

Expected integration point:

- `Assets/Project/Runtime/World/Spherical/Inspection/SphericalBiomeInspectionBootstrap.cs`

Expected configuration asset location:

- `Assets/Project/Content/Configuration/World/` (confirm the existing configuration folder convention before creating the asset).

Potential tests:

- `Assets/Project/Tests/EditMode/World/Spherical/SphericalEnvironmentPlacementTests.cs`

Do not modify the terrain mesh generator, biome texture shader, or geographic field generator for this feature unless implementation demonstrates a concrete missing requirement.

## 6. Validation and acceptance criteria

### Determinism
- Same world seed and same settings produce identical candidate positions, prefab choices, rotations and scales.
- A different seed changes the distribution.
- Results do not depend on the order in which cube-sphere faces are processed.

### Geographic correctness
- No instance is placed on Ocean, LakeOrPond or River cells.
- Every instance belongs to the biome rule that selected it.
- Slope and any configured altitude/moisture constraints are respected.
- Instances sit on the terrain surface with no obvious floating or excessive burial.
- Local up follows the spherical surface normal, including across cube-face boundaries.

### Lifecycle and scene integration
- Environment objects are children of a dedicated generated environment root.
- Regenerating the inspection world does not duplicate instances.
- Clearing the inspection world removes all generated environment instances.
- Player, camera, lighting and terrain generation continue to work.

### Visual quality
- Plains and Forest are visually distinguishable at gameplay camera distance.
- Objects do not form obvious rows, face seams, dense clusters or repetitive patterns.
- Density remains appropriate on the 2 km Tiny World.
- The first pass remains usable without a complete tree model.

### Performance
- Record generation time and instance count for the Tiny World profile.
- Do not introduce an unbounded number of instances.
- Do not implement GPU instancing, pooling, chunk streaming or distance culling in this work package unless profiling shows that the first prototype cannot run acceptably. Track those as follow-up work.

## 7. Out of scope

- Desert and Mountains placement.
- Complete trees, canopy generation and forest composition.
- Procedural plant growth, wind animation or ecological simulation.
- Runtime spawning/despawning while travelling.
- Chunk streaming, LOD, GPU instancing and vegetation batching.
- Navigation, collision design for every asset, harvesting or gameplay interactions.
- New Blender asset creation, except fixes required by the initial pivot/bounds audit.

## 8. References and project files

Internal source files to inspect during implementation:

- `Assets/Project/Runtime/World/Spherical/Geography/SphericalGeographicFieldSet.cs`
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalGeographicFieldType.cs`
- `Assets/Project/Runtime/World/Spherical/Inspection/SphericalBiomeInspectionBootstrap.cs`
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalBiomeSurfaceMeshGenerator.cs`
- `Assets/Project/Runtime/World/Spherical/SphericalSurfaceQuery.cs`
- `Assets/Project/Runtime/World/Spherical/Studio/SphericalWorldProfile.cs`

Relevant work packages:

- `WORLD-002_SPHERICAL_GEOGRAPHIC_FIELDS.md`
- `WORLD-003_PLAYABLE_BIOME_INSPECTION.md`
- `WORLD-010A_BIOME_TEXTURE_RENDERING_AND_PLAINS_FOREST.md`

## 9. Decisions to confirm before implementation

1. **Placement density:** start with a visually moderate density and tune in Play Mode; exact per-km² targets are not yet established.
2. **Colliders:** do not add colliders to every vegetation prefab by default. Start with no environment colliders; add selective simple colliders only when gameplay requires them.
3. **Generation mode:** generate once when the inspection world is built, using the same seed as the geographic fields.
4. **Forest expectation:** first pass is forest floor / undergrowth only until full tree prefabs are available.

## 10. Commit policy

After each implemented and validated sub-task, prepare a commit message describing the validated result. If work stops before validation, prepare a checkpoint commit message that accurately states what is implemented and what remains unvalidated. Do not mark WORLD-011 complete until the acceptance criteria and Unity Play Mode validation have passed.
