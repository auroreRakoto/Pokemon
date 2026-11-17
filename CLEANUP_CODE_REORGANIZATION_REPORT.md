# CLEANUP — Code Ownership Reorganization Report

- **Work package:** CLEANUP-002 (executed directly at owner request)
- **Date:** 2026-11-17
- **Input:** `Pokemon-main (5).zip`
- **Agent:** CLEANUP
- **Result:** File ownership reorganization completed; Unity runtime validation pending.
- **Git:** No repository metadata was present in the supplied ZIP. No commit or push is claimed.

## 1. What changed

The Unity C# scripts were physically reorganized so each script is located under the architecture layer/domain responsible for its function. The paired Unity `.meta` file was moved with every moved script. Existing script GUIDs were preserved.

No gameplay method bodies, serialized field names, ScriptableObject values, or prefab/scene contents were intentionally changed.

### Pokémon

| Previous location | New location |
|---|---|
| `Runtime/Pokemon/Domain/Data/PokemonDefinition.cs` | `Runtime/Pokemon/Definitions/PokemonDefinition.cs` |
| `Runtime/Pokemon/Domain/Needs/PokemonNeeds.cs` | `Runtime/Pokemon/Individual/Needs/PokemonNeeds.cs` |
| `Runtime/Pokemon/Spawning/PokemonSpawnManager.cs` | `Runtime/Pokemon/Population/PokemonSpawnManager.cs` |
| `Runtime/Pokemon/Behaviour/PokemonScheduleCalculator.cs` | `Runtime/Pokemon/Behaviour/Scheduling/PokemonScheduleCalculator.cs` |

`PokemonAgent` remains in `Pokemon/Behaviour`. It still contains several behaviour/lifecycle/needs responsibilities in one class; moving it would not solve that design issue. Its staged functional decomposition remains open and must be handled as a behaviour-preserving code refactor with tests, not disguised as folder cleanup.

### Ecology

| Previous location | New location |
|---|---|
| `Runtime/Ecology/Resources/FruitBush.cs` | `Runtime/Ecology/Resources/Instances/FruitBush.cs` |
| `Runtime/Ecology/Resources/FruitType.cs` | `Runtime/Ecology/Resources/Definitions/FruitType.cs` |
| `Runtime/Ecology/Resources/FruitBushManager.cs` | `Runtime/Ecology/Resources/Systems/FruitBushManager.cs` |
| `Runtime/Ecology/Vegetation/ResourceManager.cs` | `Runtime/Ecology/Resources/Systems/ResourceManager.cs` |
| `Runtime/Ecology/Vegetation/BiomeEcologyGenerator.cs` | `Runtime/Ecology/Generation/BiomeEcologyGenerator.cs` |
| `Runtime/Ecology/Vegetation/WorldEcologyData.cs` | `Runtime/Ecology/World/WorldEcologyData.cs` |
| `Runtime/Ecology/Vegetation/WorldEcologyLayer.cs` | `Runtime/Ecology/World/WorldEcologyLayer.cs` |
| `Runtime/Ecology/Vegetation/MeadowData.cs` | `Runtime/Ecology/World/MeadowData.cs` |

Resource definitions, instances, and lifecycle managers are now grouped by role. `ResourceManager` and `FruitBushManager` remain distinct; this change does not resolve whether vegetation-grid food and fruit bushes are one or separate canonical resource models.

### World

| Previous location | New location |
|---|---|
| `Runtime/World/Biomes/BiomeDefinition.cs` | `Runtime/World/Biomes/Definitions/BiomeDefinition.cs` |
| `Runtime/World/Biomes/WolrdDefinition.cs` | `Runtime/World/Worlds/Definitions/WorldDefinition.cs` |
| `Runtime/World/Biomes/BiomeLayer.cs` | `Runtime/World/Biomes/Data/BiomeLayer.cs` |
| `Runtime/World/Biomes/BiomeMapData.cs` | `Runtime/World/Biomes/Data/BiomeMapData.cs` |
| `Runtime/World/Generation/WorldBiomeGenerator.cs` | `Runtime/World/Generation/Biomes/WorldBiomeGenerator.cs` |
| `Runtime/World/Generation/MeadowGenerator.cs` | `Runtime/World/Generation/Worlds/MeadowGenerator.cs` |
| `Runtime/World/Placement/GrassVegetationSpawner.cs` | `Runtime/World/Placement/Vegetation/GrassVegetationSpawner.cs` |
| `Runtime/World/Presentation/MapRenderer.cs` | `Runtime/World/Presentation/Maps/MapRenderer.cs` |
| `Runtime/World/Presentation/WorldBiomeMapRenderer.cs` | `Runtime/World/Presentation/Biomes/WorldBiomeMapRenderer.cs` |

`WorldDefinition` was also corrected from the misspelled filename `WolrdDefinition.cs`. Its `.meta` GUID was preserved.

### Supporting runtime layers

| Previous location | New location |
|---|---|
| `Runtime/Ecology/Presentation/ResourceMinimap.cs` | `Runtime/Presentation/Ecology/ResourceMinimap.cs` |
| `Runtime/Simulation/Time/SimulationTimeController.cs` | `Runtime/Presentation/Simulation/SimulationTimeController.cs` |
| `Runtime/Ecology/Telemetry/EcosystemDataCollector.cs` | `Runtime/Infrastructure/Telemetry/EcosystemDataCollector.cs` |
| `Runtime/Ecology/Telemetry/EcosystemSnapshot.cs` | `Runtime/Infrastructure/Telemetry/EcosystemSnapshot.cs` |
| `Runtime/Infrastructure/Diagnostics/EcosystemLogger.cs` | `Runtime/Infrastructure/Telemetry/EcosystemLogger.cs` |
| `Runtime/Infrastructure/ProjectContentCatalog.cs` | `Runtime/Infrastructure/Content/ProjectContentCatalog.cs` |
| `Runtime/Data/WorldDataRuntimeLoader.cs` | `Runtime/Data/Loading/WorldDataRuntimeLoader.cs` |
| `Runtime/Data/WorldDataModels.cs` | `Runtime/Data/Contracts/WorldDataModels.cs` |
| `Runtime/Data/GeneratedDefinitionAssets.cs` | `Runtime/Data/Projections/GeneratedDefinitionAssets.cs` |

The minimap and simulation speed controller are presentation-facing Unity components, not domain simulation rules. Ecosystem collection/logging/snapshots span multiple domains and are now treated as infrastructure telemetry.

### Editor tools

| Previous location | New location |
|---|---|
| `Editor/ContentCatalogBuilder.cs` | `Editor/Content/ContentCatalogBuilder.cs` |
| `Editor/DataPipeline/PokemonDataSyncMenu.cs` | `Editor/Data/Pipeline/PokemonDataSyncMenu.cs` |
| `Editor/DataPipeline/JsonToScriptableObjectProjection.cs` | `Editor/Data/Pipeline/JsonToScriptableObjectProjection.cs` |
| `Editor/Tools/AssetSync/BlenderEnvironmentSync.cs` | `Editor/Tools/Blender/BlenderEnvironmentSync.cs` |

All four Editor scripts remain under `Assets/Project/Editor`; no Editor API was moved into Runtime.

## 2. Cleanup and documentation

- Removed 40 generated Python cache/bytecode artifacts (`__pycache__`, `.pyc`/`.pyo`).
- Updated `Documentation/01_ARCHITECTURE/UNITY_ARCHITECTURE.md` with the canonical script tree and placement rules.
- Updated `Documentation/01_ARCHITECTURE/MODULE_BOUNDARIES.md` to define Data, Infrastructure, Presentation and Editor as supporting layers.
- Updated `Documentation/00_GOVERNANCE/DOCUMENTATION_CHANGELOG.md`.
- Appended this implementation addendum to `Documentation/06_AUDIT/REPOSITORY_AUDIT_REPORT.md`, preserving the original audit as historical evidence.

## 3. Verification performed

- 36 C# scripts inventoried after reorganization.
- 34 scripts moved; 2 already had appropriate locations.
- All 34 moved scripts had their `.meta` files moved alongside them.
- All 34 moved scripts retained their existing GUID metadata.
- Source file SHA-256 checks confirmed moved `.cs` file contents were unchanged during the move.
- No old source paths remain for the 34 moved scripts.
- Generated Python cache artifacts removed.
- No Unity project execution was available in this environment.

## 4. Not verified

- Unity compilation and script import.
- Scene and prefab component resolution in Unity Editor.
- Play Mode behaviour and regression testing.
- Blender generator execution or FBX reimport.
- Build output.

The source snapshot's earlier audit reports a potentially unresolved `EntityId` / `GetEntityId()` reference in `PokemonAgent`. This reorganization did not invent an identity implementation; Unity compilation must confirm whether it remains a blocker.

## 5. Remaining code-level architecture work

1. Decompose `PokemonAgent` into cohesive behaviour/physiology/lifecycle components or services while preserving behaviour. This is not solved by moving the file.
2. Resolve the ownership and interaction contract between vegetation-grid resources and discrete fruit bushes.
3. Resolve the stable individual identity contract before adding an ad-hoc `EntityId`.
4. Revisit assembly definitions only after dependency boundaries stabilize.

## 6. Final status

**Folder and script ownership reorganization: completed.**

**Architecture-wide code decomposition: not complete.** No claim is made that every responsibility inside `PokemonAgent` has already been extracted into separate classes.

**Unity validation: pending.** The project must be opened in the target Unity Editor before declaring the refactor fully validated.
