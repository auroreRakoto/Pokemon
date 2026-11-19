# Project File & Feature Index

**Purpose:** canonical repository navigation reference. Use it to find exact paths and the main responsibility of project folders and files.

## Snapshot and maintenance

- **Source:** `Pokemon-clean (1).zip`, archive root `Pokemon-clean/` (snapshot reviewed 2026-11-19).
- **Indexed:** 423 files (excluding Unity `.meta` sidecars and Python cache files) across 165 directories.
- This is a repository snapshot, not proof that every asset is loaded in the active scene or used at runtime.
- Update this index whenever paths, ownership or feature responsibilities change. Paths are relative to the project root.
- The repository contains both a legacy/general world-generation pipeline and the newer spherical-world pipeline; do not conflate them.

## Fast path lookup

| Need | Path |
|---|---|
| Spherical topology | `Assets/Project/Runtime/World/Spherical/CubeSphereTopology.cs` |
| Geographic fields (altitude, slope, moisture, temperature, water, biome) | `Assets/Project/Runtime/World/Spherical/Geography/` |
| Playable biome inspection | `Assets/Project/Runtime/World/Spherical/Inspection/SphericalBiomeInspectionBootstrap.cs` |
| Player and camera | `Assets/Project/Runtime/World/Spherical/Movement/` |
| Biome terrain meshes and shader | `Assets/Project/Runtime/World/Spherical/Presentation/` |
| Planet studio and profiles | `Assets/Project/Runtime/World/Spherical/Studio/` |
| Blender procedural generators | `Blender/source/` and `Blender/_core/` |
| Blender-to-Unity import tool | `Assets/Project/Editor/Tools/Blender/BlenderFbxSync.cs` |
| Environment models | `Assets/Project/Content/Models/Environment/` |
| Generated biome textures | `Assets/Project/Content/Textures/Environment/Biomes/Generated/` |
| World data and profiles | `Assets/Project/Data/`, `Data/`, `Assets/StreamingAssets/PokemonData/` |
| Tests | `Assets/Project/Tests/` |
| Architecture and work packages | `Documentation/` |

## 1. Runtime feature map

### 1.1 General / legacy world generation

| Path | Responsibility |
|---|---|
| `Assets/Project/Runtime/World/Generation/WorldGenerationConfig.cs` | Generation configuration model. |
| `Assets/Project/Runtime/World/Generation/WorldGenerationSettings.cs` | Runtime generation settings. |
| `Assets/Project/Runtime/World/Generation/WorldGenerationResult.cs` | Generated result data. |
| `Assets/Project/Runtime/World/Generation/WorldGenerator.cs` | Generation orchestration. |
| `Assets/Project/Runtime/World/Generation/WorldTerrainBuilder.cs` | Builds terrain from generation data. |
| `Assets/Project/Runtime/World/Generation/WorldVertexColor.shader` | Vertex-colour terrain shader. |
| `Assets/Project/Runtime/DataServices/Loading/WorldGenerationConfigLoader.cs` | Loads generation configuration. |

### 1.2 Spherical world

| Folder / file | Responsibility |
|---|---|
| `.../Spherical/CubeSphereTopology.cs` | Six-face cube-sphere topology and direction/face-coordinate mapping. |
| `.../Spherical/SphericalHeightField.cs` | Height-field data and queries used by spherical presentation/demo code. |
| `.../Spherical/SphericalSurfaceQuery.cs` | Surface and tangent-frame query helpers. |
| `.../Spherical/SphericalWorldSettings.cs` | General spherical-world settings. |
| `.../Spherical/Geography/` | Geographic field generation/storage, drainage, graph distances and neighbour graph. |
| `.../Spherical/Inspection/` | Playable inspection bootstrap, geographic player controller and demo overlay. |
| `.../Spherical/Movement/` | Ground probe, player motor/controller and third-person camera. |
| `.../Spherical/Presentation/` | Planet meshes/presenter, biome surface mesh, blend shader, material mapping, texture generator and demo presentation. |
| `.../Spherical/Studio/` | Planet studio generator, settings and world profiles. |

### 1.3 Runtime source catalog

- `Assets/Project/Runtime/DataServices/Loading/WorldGenerationConfigLoader.cs` — Types: WorldGenerationConfigLoader.
- `Assets/Project/Runtime/World/Generation/WorldGenerationConfig.cs` — Types: WorldGenerationConfig.
- `Assets/Project/Runtime/World/Generation/WorldGenerationResult.cs` — Types: WorldGenerationResult.
- `Assets/Project/Runtime/World/Generation/WorldGenerationSettings.cs` — Types: WorldGenerationSettings, WorldBiome, WaterBodyType, SettlementKind, SettlementSite, RoadRoute.
- `Assets/Project/Runtime/World/Generation/WorldGenerator.cs` — Types: WorldGenerator.
- `Assets/Project/Runtime/World/Generation/WorldTerrainBuilder.cs` — Types: contains, WorldTerrainBuilder, GeneratedWorldSurface.
- `Assets/Project/Runtime/World/Generation/WorldVertexColor.shader` — Unity shader.
- `Assets/Project/Runtime/World/Spherical/CubeSphereTopology.cs` — Types: CubeSphereFace, CubeSphereTopology.
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalDistanceBenchmark.cs` — Types: SphericalDistanceBenchmarkResult, SphericalDistanceBenchmark.
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalDrainage.cs` — Types: SphericalDrainage, NodeMinHeap, Entry, SphericalDrainageResult.
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalGeographicFieldGenerator.cs` — Types: SphericalGeographicFieldGenerator, is, WeightedElevation, WeightedElevationComparer, IndexedMinHeap.
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalGeographicFieldSet.cs` — Types: SphericalGeographicFieldSet, IDs, boundaries.
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalGeographicFieldType.cs` — Types: SphericalGeographicFieldType, SphericalWaterClass, SphericalBiome.
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalGraphDistance.cs` — Types: SphericalGraphDistance, MinHeap, Entry.
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalNeighbourGraph.cs` — Types: SphericalNeighbourGraph, DirectionKey.
- `Assets/Project/Runtime/World/Spherical/Inspection/SphericalBiomeInspectionBootstrap.cs` — Types: SphericalBiomeInspectionBootstrap.
- `Assets/Project/Runtime/World/Spherical/Inspection/SphericalGeographicPlayerController.cs` — Types: SphericalGeographicPlayerController.
- `Assets/Project/Runtime/World/Spherical/Inspection/SphericalWorldDemoOverlay.cs` — Types: SphericalWorldDemoOverlay.
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalGroundProbe.cs` — Types: SphericalGroundProbe, Sample.
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalPlayerController.cs` — Types: SphericalPlayerController.
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalPlayerMotor.cs` — Types: SphericalPlayerMotor.
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalThirdPersonCamera.cs` — Types: SphericalThirdPersonCamera.
- `Assets/Project/Runtime/World/Spherical/Pokemon.Project.World.Spherical.asmdef` — Unity assembly definition.
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalBiomeBlend.shader` — Unity shader.
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalBiomeMaterialMapping.cs` — Types: SphericalBiomeMaterialMapping.
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalBiomeSurfaceMeshGenerator.cs` — Types: SphericalBiomeSurfaceMeshGenerator.
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalBiomeTextureGenerator.cs` — Types: SphericalBiomeTextureGenerator.
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalPlanetMeshGenerator.cs` — Types: SphericalPlanetMeshGenerator.
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalPlanetPresenter.cs` — Types: SphericalPlanetPresenter.
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalWorldDemoBootstrap.cs` — Types: SphericalWorldDemoBootstrap.
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalWorldDemoOverlay.cs` — Types: SphericalWorldDemoOverlay.
- `Assets/Project/Runtime/World/Spherical/SphericalHeightField.cs` — Types: SphericalHeightField.
- `Assets/Project/Runtime/World/Spherical/SphericalSurfaceQuery.cs` — Types: SphericalSurfaceQuery.
- `Assets/Project/Runtime/World/Spherical/SphericalWorldSettings.cs` — Types: SphericalWorldSettings.
- `Assets/Project/Runtime/World/Spherical/Studio/SphericalPlanetStudioGenerator.cs` — Types: SphericalPlanetStudioGenerator.
- `Assets/Project/Runtime/World/Spherical/Studio/SphericalPlanetStudioSettings.cs` — Types: SphericalPlanetStudioView, SphericalPlanetStudioPreviewStage, SphericalPlanetStudioMap, SphericalDrainagePreviewMode, SphericalDrainageInputMode, SphericalPlanetStudioSettings.
- `Assets/Project/Runtime/World/Spherical/Studio/SphericalWorldProfile.cs` — Types: SphericalWorldProfile.

## 2. Editor tooling

| Path | Responsibility |
|---|---|
| `Assets/Project/Editor/Tools/Blender/BlenderFbxSync.cs` | Unity Editor-side Blender FBX synchronization/import. |
| `Assets/Project/Editor/Tools/Blender/README.md` | Blender sync usage. |
| `Assets/Project/Editor/World/Spherical/SphericalPlanetGenerationStudioWindow.cs` | Spherical planet generation Editor window. |
| `Assets/Project/Editor/World/Spherical/SphericalBiomeInspectionSceneBuilder.cs` | Builds/configures playable biome inspection scene. |
| `Assets/Project/Editor/World/Spherical/SphericalDrainageStudioPreview.cs` | Drainage preview. |
| `Assets/Project/Editor/World/Spherical/SphericalWorldDemoSceneTool.cs` | Demo scene setup tool. |
| `Assets/Project/Editor/World/Spherical/Biomes/SphericalBiomeMaterialGenerator.cs` | Generates biome materials/textures. |
| `Assets/Project/Editor/WorldGeneration/WorldGenerationWindow.cs` | Legacy/general world generation Editor window. |
| `Assets/Project/Editor/WorldGeneration/WorldGenerationConfigBuildProcessor.cs` | Build-time generation-config processing. |

**Not confirmed in this archive:** the separately delivered `EnvironmentPrefabGeneratorWindow.cs` and the generated prefab output. Verify their live paths in Unity before recording them as installed.

## 3. Content and asset directories

### 3.1 Unity content

| Path | Role |
|---|---|
| `Assets/Project/Content/Scenes/Worlds/` | World-related Unity scenes. |
| `Assets/Project/Content/World/PlanetStudio/` | Planet Studio assets. |
| `Assets/Project/Content/Materials/Environment/` | Environment materials. |
| `Assets/Project/Content/Models/Environment/` | Imported environment models, organized by category. |
| `Assets/Project/Content/Textures/Environment/Biomes/Generated/` | Generated biome textures. |
| `Assets/Project/Data/WorldGeneration/` | Unity-side world-generation data assets. |
| `Assets/Project/Data/WorldProfiles/` | Unity-side world profiles. |
| `Assets/Scenes/` | Root Unity scenes. |
| `Assets/Settings/` | Unity settings assets. |
| `Assets/Resources/WorldGeneration/` | Resources-loaded generation assets. |
| `Assets/Generated/Worlds/` | Generated world assets. |
| `Assets/StreamingAssets/PokemonData/` | Runtime-readable JSON, schemas and map data. |

### 3.2 Environment model hierarchy in this snapshot

```text
Assets/Project/Content/Models/Environment/
├── Ground/Leaf_Litter/
├── NaturalFeatures/Fallen_Log/
├── NaturalFeatures/Mushroom_Red/
├── NaturalFeatures/Tree_Stump/
├── Rocks/Stone_Cluster_Large/
├── Rocks/Stone_Cluster_Small/
├── Vegetation/Grass_Short/
├── Vegetation/Grass_Tall/
├── Vegetation/Reed_Clump/
├── Vegetation/Wild_Grass/
└── _ART_TEST/ (test assets; exclude from production placement)
```

### 3.3 Blender pipeline

| Path | Role |
|---|---|
| `Blender/_core/` | Shared Python API: geometry, transforms, materials, UVs, textures, collision, LOD, rigging, animation, export, manifests, validation, naming, paths, logging and scene helpers. |
| `Blender/config/project.json` | Active Blender project configuration. |
| `Blender/config/project.example.json` | Example configuration. |
| `Blender/project_modules/environment/` | Environment generator aggregation/registration. |
| `Blender/project_modules/pokemon/` | Pokémon generator modules/geometry. |
| `Blender/source/environment/<asset>/generator.py` | Per-asset procedural environment generators. |
| `Blender/source/environment/generate_all.py` | Environment batch-generation entry point. |
| `Blender/source/pokemon/<species>/generator.py` | Per-species Pokémon generators. |
| `Blender/exports/environment/<asset>/` | Environment FBX exports and manifests. |
| `Blender/exports/pokemon/<species>/` | Pokémon FBX exports and manifests. |
| `Blender/old/` | Archived/legacy scripts and packages; not the active pipeline. |

### 3.4 Environment source/export mapping

| Blender source | Export FBX |
|---|---|
| `Blender/source/environment/fallen_log/` | `Blender/exports/environment/fallen_log/SM_Fallen_Log.fbx` |
| `Blender/source/environment/grass_short/` | `Blender/exports/environment/grass_short/SM_Grass_Short.fbx` |
| `Blender/source/environment/grass_tall/` | `Blender/exports/environment/grass_tall/SM_Grass_Tall.fbx` |
| `Blender/source/environment/leaf_litter/` | `Blender/exports/environment/leaf_litter/SM_Leaf_Litter.fbx` |
| `Blender/source/environment/mushroom_red/` | `Blender/exports/environment/mushroom_red/SM_Mushroom_Red.fbx` |
| `Blender/source/environment/reed_clump/` | `Blender/exports/environment/reed_clump/SM_Reed_Clump.fbx` |
| `Blender/source/environment/stone_cluster_large/` | `Blender/exports/environment/stone_cluster_large/SM_Stone_Cluster_Large.fbx` |
| `Blender/source/environment/stone_cluster_small/` | `Blender/exports/environment/stone_cluster_small/SM_Stone_Cluster_Small.fbx` |
| `Blender/source/environment/tree_stump/` | `Blender/exports/environment/tree_stump/SM_Tree_Stump.fbx` |
| `Blender/source/environment/wild_grass/` | `Blender/exports/environment/wild_grass/SM_Grass_Wild.fbx` |
| `Blender/source/environment/flower_blue/` and `flower_yellow/` | Generator sources exist; no production FBX export found in this snapshot. |

## 4. Data and schemas

| Path | Responsibility |
|---|---|
| `Data/biomes/` | Source biome definitions (forest, meadow). |
| `Data/generation/` | World-generation JSON configuration. |
| `Data/maps/world_meadow_01/` | Demo map layers: height, humidity, temperature, vegetation density; manifests and binary values. |
| `Data/pokemon/` | Source Pokémon species definitions. |
| `Data/schemas/` | JSON schemas for biome, map layer, Pokémon species and world data. |
| `Data/worlds/` | Source world definitions. |
| `Assets/StreamingAssets/PokemonData/` | Runtime copy of JSON source data, schemas and map layers. |

## 5. Tests

| Path | Coverage |
|---|---|
| `Assets/Project/Tests/EditMode/World/Spherical/` | Cube-sphere topology, geographic fields, drainage, neighbour graph, height field, biome rendering/textures, studio/profile and presentation. |
| `Assets/Project/Tests/EditMode/World/Spherical/Movement/` | Spherical player motor EditMode tests. |
| `Assets/Project/Tests/PlayMode/World/Spherical/` | Spherical planet studio PlayMode tests. |

## 6. Documentation map

| Folder | Contents |
|---|---|
| `Documentation/00_GOVERNANCE/` | Charter, working agreements, Git workflow, decisions, change process and document index. |
| `Documentation/01_ARCHITECTURE/` | System, Unity, world, data, network, module boundaries and interface contracts. |
| `Documentation/02_DOMAINS/` | Pokémon, ecology, world/biomes, combat, assets, terrain/rendering, audio/VFX, AI/API and persistence. |
| `Documentation/03_ENGINEERING/` | Coding, testing, performance, build, security, roadmap and work packages. |
| `Documentation/04_AI_INSTRUCTIONS/` | AI collaboration rules and domain-specific instructions. |
| `Documentation/05_TEMPLATES/` | Feature, audit, integration and chat templates. |
| `Documentation/06_AUDIT/` | Repository audits, action registers and code ownership inventory. |
| `Documentation/07_Testing/` | Testing strategy, coverage, migration and test plans. |

## 7. Full directory tree

```text
.github/
.github/instructions/
.vscode/
Assets/
Assets/Generated/
Assets/Generated/Worlds/
Assets/Project/
Assets/Project/Content/
Assets/Project/Content/Materials/
Assets/Project/Content/Materials/Environment/
Assets/Project/Content/Models/
Assets/Project/Content/Models/Environment/
Assets/Project/Content/Models/Environment/Ground/
Assets/Project/Content/Models/Environment/Ground/Leaf_Litter/
Assets/Project/Content/Models/Environment/NaturalFeatures/
Assets/Project/Content/Models/Environment/NaturalFeatures/Fallen_Log/
Assets/Project/Content/Models/Environment/NaturalFeatures/Mushroom_Red/
Assets/Project/Content/Models/Environment/NaturalFeatures/Tree_Stump/
Assets/Project/Content/Models/Environment/Rocks/
Assets/Project/Content/Models/Environment/Rocks/Stone_Cluster_Large/
Assets/Project/Content/Models/Environment/Rocks/Stone_Cluster_Small/
Assets/Project/Content/Models/Environment/Vegetation/
Assets/Project/Content/Models/Environment/Vegetation/Grass_Short/
Assets/Project/Content/Models/Environment/Vegetation/Grass_Tall/
Assets/Project/Content/Models/Environment/Vegetation/Reed_Clump/
Assets/Project/Content/Models/Environment/Vegetation/Wild_Grass/
Assets/Project/Content/Models/Environment/Vegetation/_ART_TEST/
Assets/Project/Content/Models/Environment/Vegetation/_ART_TEST/Grass_Short_Test/
Assets/Project/Content/Models/Environment/_ART_TEST/
Assets/Project/Content/Models/Environment/_ART_TEST/fallen_log/
Assets/Project/Content/Models/Environment/_ART_TEST/grass_short/
Assets/Project/Content/Models/Environment/_ART_TEST/grass_tall/
Assets/Project/Content/Models/Environment/_ART_TEST/leaf_litter/
Assets/Project/Content/Models/Environment/_ART_TEST/mushroom_red/
Assets/Project/Content/Models/Environment/_ART_TEST/reed_clump/
Assets/Project/Content/Models/Environment/_ART_TEST/stone_cluster_large/
Assets/Project/Content/Models/Environment/_ART_TEST/stone_cluster_small/
Assets/Project/Content/Models/Environment/_ART_TEST/tree_stump/
Assets/Project/Content/Models/Environment/_ART_TEST/wild_grass/
Assets/Project/Content/Scenes/
Assets/Project/Content/Scenes/Worlds/
Assets/Project/Content/Textures/
Assets/Project/Content/Textures/Environment/
Assets/Project/Content/Textures/Environment/Biomes/
Assets/Project/Content/Textures/Environment/Biomes/Generated/
Assets/Project/Content/World/
Assets/Project/Content/World/PlanetStudio/
Assets/Project/Data/
Assets/Project/Data/WorldGeneration/
Assets/Project/Data/WorldProfiles/
Assets/Project/Editor/
Assets/Project/Editor/Tools/
Assets/Project/Editor/Tools/Blender/
Assets/Project/Editor/World/
Assets/Project/Editor/World/Spherical/
Assets/Project/Editor/World/Spherical/Biomes/
Assets/Project/Editor/WorldGeneration/
Assets/Project/Runtime/
Assets/Project/Runtime/DataServices/
Assets/Project/Runtime/DataServices/Loading/
Assets/Project/Runtime/World/
Assets/Project/Runtime/World/Generation/
Assets/Project/Runtime/World/Spherical/
Assets/Project/Runtime/World/Spherical/Geography/
Assets/Project/Runtime/World/Spherical/Inspection/
Assets/Project/Runtime/World/Spherical/Movement/
Assets/Project/Runtime/World/Spherical/Presentation/
Assets/Project/Runtime/World/Spherical/Studio/
Assets/Project/Tests/
Assets/Project/Tests/EditMode/
Assets/Project/Tests/EditMode/World/
Assets/Project/Tests/EditMode/World/Spherical/
Assets/Project/Tests/EditMode/World/Spherical/Movement/
Assets/Project/Tests/PlayMode/
Assets/Project/Tests/PlayMode/World/
Assets/Project/Tests/PlayMode/World/Spherical/
Assets/Resources/
Assets/Resources/WorldGeneration/
Assets/Scenes/
Assets/Settings/
Assets/StreamingAssets/
Assets/StreamingAssets/PokemonData/
Assets/StreamingAssets/PokemonData/biomes/
Assets/StreamingAssets/PokemonData/generation/
Assets/StreamingAssets/PokemonData/maps/
Assets/StreamingAssets/PokemonData/maps/world_meadow_01/
Assets/StreamingAssets/PokemonData/maps/world_meadow_01/height/
Assets/StreamingAssets/PokemonData/maps/world_meadow_01/humidity/
Assets/StreamingAssets/PokemonData/maps/world_meadow_01/temperature/
Assets/StreamingAssets/PokemonData/maps/world_meadow_01/vegetation_density/
Assets/StreamingAssets/PokemonData/pokemon/
Assets/StreamingAssets/PokemonData/schemas/
Assets/StreamingAssets/PokemonData/worlds/
Assets/_Recovery/
Blender/
Blender/_core/
Blender/config/
Blender/exports/
Blender/exports/environment/
Blender/exports/environment/fallen_log/
Blender/exports/environment/grass_short/
Blender/exports/environment/grass_short_test/
Blender/exports/environment/grass_tall/
Blender/exports/environment/leaf_litter/
Blender/exports/environment/mushroom_red/
Blender/exports/environment/reed_clump/
Blender/exports/environment/stone_cluster_large/
Blender/exports/environment/stone_cluster_small/
Blender/exports/environment/tree_stump/
Blender/exports/environment/wild_grass/
Blender/exports/pokemon/
Blender/exports/pokemon/ecoraki/
Blender/exports/pokemon/nacoryn/
Blender/old/
Blender/project_modules/
Blender/project_modules/environment/
Blender/project_modules/pokemon/
Blender/source/
Blender/source/environment/
Blender/source/environment/fallen_log/
Blender/source/environment/flower_blue/
Blender/source/environment/flower_yellow/
Blender/source/environment/grass_short/
Blender/source/environment/grass_short_test/
Blender/source/environment/grass_tall/
Blender/source/environment/leaf_litter/
Blender/source/environment/mushroom_red/
Blender/source/environment/reed_clump/
Blender/source/environment/stone_cluster_large/
Blender/source/environment/stone_cluster_small/
Blender/source/environment/tree_stump/
Blender/source/environment/wild_grass/
Blender/source/pokemon/
Blender/source/pokemon/ecoraki/
Blender/source/pokemon/ferilume/
Blender/source/pokemon/nacoryn/
Data/
Data/biomes/
Data/generation/
Data/maps/
Data/maps/world_meadow_01/
Data/maps/world_meadow_01/height/
Data/maps/world_meadow_01/humidity/
Data/maps/world_meadow_01/temperature/
Data/maps/world_meadow_01/vegetation_density/
Data/pokemon/
Data/schemas/
Data/worlds/
Documentation/
Documentation/00_GOVERNANCE/
Documentation/01_ARCHITECTURE/
Documentation/02_DOMAINS/
Documentation/03_ENGINEERING/
Documentation/03_ENGINEERING/WORK_PACKAGES/
Documentation/04_AI_INSTRUCTIONS/
Documentation/04_AI_INSTRUCTIONS/instructions/
Documentation/05_TEMPLATES/
Documentation/05_TEMPLATES/CHAT_PACKS/
Documentation/06_AUDIT/
Documentation/07_Testing/
Documentation/07_Testing/Work_Packages/
Packages/
ProjectSettings/
ProjectSettings/BlenderAssetSync/
Tools/
```

## 8. Complete file path catalog

### Project root
- `.gitignore` — Project file.
- `.vsconfig` — Project file.
- `ARCH-REBOOT-001_IMPLEMENTATION_REPORT.md` — Documentation/instructions.
- `ART-001_BLENDER_PIPELINE_PLAN.md` — Documentation/instructions.
- `ART-ECORAKI-002_REPORT.md` — Documentation/instructions.
- `CLEANUP_CODE_REORGANIZATION_REPORT.md` — Documentation/instructions.
- `CLEANUP_EXECUTOR_001_FINAL_REPORT.md` — Documentation/instructions.
- `CODE_ARCHITECTURE_COMPLIANCE_REPORT.md` — Documentation/instructions.
- `NEXT_HANDOFF.md` — Documentation/instructions.
- `PROJECT_ALIGNMENT_PLAN.md` — Documentation/instructions.
- `README_INSTALLATION.md` — Documentation/instructions.
- `REBOOT-001_IMPLEMENTATION_PLAN.md` — Documentation/instructions.
- `WORLD-001_INITIAL_CODE_README.md` — Documentation/instructions.

### `.github/`
- `copilot-instructions.md` — Documentation/instructions.

### `.github/instructions/`
- `blender-python.instructions.md` — Documentation/instructions.
- `documentation.instructions.md` — Documentation/instructions.
- `simulation.instructions.md` — Documentation/instructions.
- `unity-csharp.instructions.md` — Documentation/instructions.

### `.vscode/`
- `extensions.json` — JSON data/configuration/schema.
- `settings.json` — JSON data/configuration/schema.

### `Assets/`
- `InputSystem_Actions.inputactions` — Project file.
- `Readme.asset` — Unity serialized asset.

### `Assets/Generated/Worlds/`
- `GeneratedWorldVertexColor.mat` — Unity material.
- `GeneratedWorld_481516_256x256 1.asset` — Unity serialized asset.
- `GeneratedWorld_481516_256x256 2.asset` — Unity serialized asset.
- `GeneratedWorld_481516_256x256 3.asset` — Unity serialized asset.
- `GeneratedWorld_481516_256x256 4.asset` — Unity serialized asset.
- `GeneratedWorld_481516_256x256.asset` — Unity serialized asset.
- `World_481516_256x256 1.asset` — Unity serialized asset.
- `World_481516_256x256.asset` — Unity serialized asset.
- `World_481516_623x623.asset` — Unity serialized asset.

### `Assets/Project/Content/`
- `SM_Fallen_Log.fbx` — FBX model.
- `SM_Mushroom_Red.fbx` — FBX model.

### `Assets/Project/Content/Materials/Environment/`
- `MAT_Ground_Desert.mat` — Unity material.
- `MAT_Ground_Forest.mat` — Unity material.
- `MAT_Ground_Mountains.mat` — Unity material.
- `MAT_Ground_Plains.mat` — Unity material.
- `MAT_Water_Surface.mat` — Unity material.

### `Assets/Project/Content/Models/Environment/Ground/Leaf_Litter/`
- `SM_Leaf_Litter.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/NaturalFeatures/Fallen_Log/`
- `SM_Fallen_Log.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/NaturalFeatures/Mushroom_Red/`
- `SM_Mushroom_Red.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/NaturalFeatures/Tree_Stump/`
- `SM_Tree_Stump.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/Rocks/Stone_Cluster_Large/`
- `SM_Stone_Cluster_Large.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/Rocks/Stone_Cluster_Small/`
- `SM_Stone_Cluster_Small.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/Vegetation/Grass_Short/`
- `SM_Grass_Short.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/Vegetation/Grass_Tall/`
- `SM_Grass_Tall.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/Vegetation/Reed_Clump/`
- `SM_Reed_Clump.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/Vegetation/Wild_Grass/`
- `SM_Grass_Wild.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/Vegetation/_ART_TEST/Grass_Short_Test/`
- `SM_Grass_Short_Test.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/_ART_TEST/fallen_log/`
- `SM_Fallen_Log.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/_ART_TEST/grass_short/`
- `SM_Grass_Short.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/_ART_TEST/grass_tall/`
- `SM_Grass_Tall.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/_ART_TEST/leaf_litter/`
- `SM_Leaf_Litter.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/_ART_TEST/mushroom_red/`
- `SM_Mushroom_Red.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/_ART_TEST/reed_clump/`
- `SM_Reed_Clump.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/_ART_TEST/stone_cluster_large/`
- `SM_Stone_Cluster_Large.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/_ART_TEST/stone_cluster_small/`
- `SM_Stone_Cluster_Small.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/_ART_TEST/tree_stump/`
- `SM_Tree_Stump.fbx` — FBX model.

### `Assets/Project/Content/Models/Environment/_ART_TEST/wild_grass/`
- `SM_Grass_Wild.fbx` — FBX model.

### `Assets/Project/Content/Scenes/Worlds/`
- `SphericalWorldDemo.unity` — Unity scene.
- `WORLD-002_BiomeInspection.unity` — Unity scene.
- `World01.unity` — Unity scene.
- `World02.unity` — Unity scene.

### `Assets/Project/Content/Textures/Environment/Biomes/Generated/`
- `TEX_Ground_Desert.png` — Project file.
- `TEX_Ground_Forest.png` — Project file.
- `TEX_Ground_Mountains.png` — Project file.
- `TEX_Ground_Plains.png` — Project file.

### `Assets/Project/Content/World/PlanetStudio/`
- `CFG_BiomeMaterialMapping.asset` — Unity serialized asset.
- `DrainageOverlayOpaque.shader` — Unity shader.
- `SphericalPlanetStudioSettings.asset` — Unity serialized asset.

### `Assets/Project/Data/WorldGeneration/`
- `world_generation.json` — JSON data/configuration/schema.

### `Assets/Project/Data/WorldProfiles/`
- `archipelago.json` — JSON data/configuration/schema.
- `arid.json` — JSON data/configuration/schema.
- `continental.json` — JSON data/configuration/schema.
- `rugged_highlands.json` — JSON data/configuration/schema.
- `temperate_balanced.json` — JSON data/configuration/schema.
- `tiny_world.json` — JSON data/configuration/schema.

### `Assets/Project/Editor/Tools/Blender/`
- `BlenderFbxSync.cs` — Types: BlenderFbxSync, FileReplacement, BlenderProjectConfig, BlenderPaths, BlenderRoundTrip, BlenderManifest, BlenderExport, BlenderUnity, BlenderManifestExtra, ManifestCandidate, FileStamp.
- `README.md` — Documentation/instructions.

### `Assets/Project/Editor/World/Spherical/`
- `SphericalBiomeInspectionSceneBuilder.cs` — Types: SphericalBiomeInspectionSceneBuilder.
- `SphericalDrainageStudioPreview.cs` — Types: SphericalDrainageOverlayDiagnostics, SphericalDrainageStudioPreview.
- `SphericalPlanetGenerationStudioWindow.cs` — Types: SphericalPlanetGenerationStudioWindow, field.
- `SphericalWorldDemoSceneTool.cs` — Types: SphericalWorldDemoSceneTool.

### `Assets/Project/Editor/World/Spherical/Biomes/`
- `SphericalBiomeMaterialGenerator.cs` — Types: SphericalBiomeMaterialGenerator, MaterialDefinition.

### `Assets/Project/Editor/WorldGeneration/`
- `WorldGenerationConfigBuildProcessor.cs` — Types: WorldGenerationConfigBuildProcessor.
- `WorldGenerationWindow.cs` — Types: WorldGenerationWindow, ExportNetwork, ExportMetadata.

### `Assets/Project/Runtime/DataServices/Loading/`
- `WorldGenerationConfigLoader.cs` — Types: WorldGenerationConfigLoader.

### `Assets/Project/Runtime/World/Generation/`
- `WorldGenerationConfig.cs` — Types: WorldGenerationConfig.
- `WorldGenerationResult.cs` — Types: WorldGenerationResult.
- `WorldGenerationSettings.cs` — Types: WorldGenerationSettings, WorldBiome, WaterBodyType, SettlementKind, SettlementSite, RoadRoute.
- `WorldGenerator.cs` — Types: WorldGenerator.
- `WorldTerrainBuilder.cs` — Types: contains, WorldTerrainBuilder, GeneratedWorldSurface.
- `WorldVertexColor.shader` — Unity shader.

### `Assets/Project/Runtime/World/Spherical/`
- `CubeSphereTopology.cs` — Types: CubeSphereFace, CubeSphereTopology.
- `Pokemon.Project.World.Spherical.asmdef` — Unity assembly definition.
- `SphericalHeightField.cs` — Types: SphericalHeightField.
- `SphericalSurfaceQuery.cs` — Types: SphericalSurfaceQuery.
- `SphericalWorldSettings.cs` — Types: SphericalWorldSettings.

### `Assets/Project/Runtime/World/Spherical/Geography/`
- `SphericalDistanceBenchmark.cs` — Types: SphericalDistanceBenchmarkResult, SphericalDistanceBenchmark.
- `SphericalDrainage.cs` — Types: SphericalDrainage, NodeMinHeap, Entry, SphericalDrainageResult.
- `SphericalGeographicFieldGenerator.cs` — Types: SphericalGeographicFieldGenerator, is, WeightedElevation, WeightedElevationComparer, IndexedMinHeap.
- `SphericalGeographicFieldSet.cs` — Types: SphericalGeographicFieldSet, IDs, boundaries.
- `SphericalGeographicFieldType.cs` — Types: SphericalGeographicFieldType, SphericalWaterClass, SphericalBiome.
- `SphericalGraphDistance.cs` — Types: SphericalGraphDistance, MinHeap, Entry.
- `SphericalNeighbourGraph.cs` — Types: SphericalNeighbourGraph, DirectionKey.

### `Assets/Project/Runtime/World/Spherical/Inspection/`
- `SphericalBiomeInspectionBootstrap.cs` — Types: SphericalBiomeInspectionBootstrap.
- `SphericalGeographicPlayerController.cs` — Types: SphericalGeographicPlayerController.
- `SphericalWorldDemoOverlay.cs` — Types: SphericalWorldDemoOverlay.

### `Assets/Project/Runtime/World/Spherical/Movement/`
- `SphericalGroundProbe.cs` — Types: SphericalGroundProbe, Sample.
- `SphericalPlayerController.cs` — Types: SphericalPlayerController.
- `SphericalPlayerMotor.cs` — Types: SphericalPlayerMotor.
- `SphericalThirdPersonCamera.cs` — Types: SphericalThirdPersonCamera.

### `Assets/Project/Runtime/World/Spherical/Presentation/`
- `SphericalBiomeBlend.shader` — Unity shader.
- `SphericalBiomeMaterialMapping.cs` — Types: SphericalBiomeMaterialMapping.
- `SphericalBiomeSurfaceMeshGenerator.cs` — Types: SphericalBiomeSurfaceMeshGenerator.
- `SphericalBiomeTextureGenerator.cs` — Types: SphericalBiomeTextureGenerator.
- `SphericalPlanetMeshGenerator.cs` — Types: SphericalPlanetMeshGenerator.
- `SphericalPlanetPresenter.cs` — Types: SphericalPlanetPresenter.
- `SphericalWorldDemoBootstrap.cs` — Types: SphericalWorldDemoBootstrap.
- `SphericalWorldDemoOverlay.cs` — Types: SphericalWorldDemoOverlay.

### `Assets/Project/Runtime/World/Spherical/Studio/`
- `SphericalPlanetStudioGenerator.cs` — Types: SphericalPlanetStudioGenerator.
- `SphericalPlanetStudioSettings.cs` — Types: SphericalPlanetStudioView, SphericalPlanetStudioPreviewStage, SphericalPlanetStudioMap, SphericalDrainagePreviewMode, SphericalDrainageInputMode, SphericalPlanetStudioSettings.
- `SphericalWorldProfile.cs` — Types: SphericalWorldProfile.

### `Assets/Project/Tests/EditMode/World/Spherical/`
- `CubeSphereTopologyRegressionTests.cs` — Types: CubeSphereTopologyRegressionTests.
- `CubeSphereTopologyTests.cs` — Types: CubeSphereTopologyTests.
- `Pokemon.Project.Tests.World.Spherical.EditMode.asmdef` — Unity assembly definition.
- `SphericalBiomeSurfaceRenderingTests.cs` — Types: SphericalBiomeSurfaceRenderingTests.
- `SphericalBiomeTextureGeneratorTests.cs` — Types: SphericalBiomeTextureGeneratorTests.
- `SphericalDrainageLogicIsolationTests.cs` — Types: SphericalDrainageLogicIsolationTests.
- `SphericalDrainageTests.cs` — Types: SphericalDrainageTests, WorkingSetPeakSampler.
- `SphericalGeographicFieldGeneratorTests.cs` — Types: SphericalGeographicFieldGeneratorTests.
- `SphericalHeightFieldTests.cs` — Types: SphericalHeightFieldTests.
- `SphericalNeighbourGraphTests.cs` — Types: SphericalNeighbourGraphTests.
- `SphericalPlanetStudioTests.cs` — Types: SphericalPlanetStudioTests.
- `SphericalPresentationTests.cs` — Types: SphericalPresentationTests.
- `SphericalWorldProfileTests.cs` — Types: SphericalWorldProfileTests.

### `Assets/Project/Tests/EditMode/World/Spherical/Movement/`
- `Pokemon.Project.Tests.World.Spherical.Movement.EditMode.asmdef` — Unity assembly definition.
- `SphericalPlayerMotorTests.cs` — Types: SphericalPlayerMotorTests.

### `Assets/Project/Tests/PlayMode/World/Spherical/`
- `Pokemon.Project.Tests.World.Spherical.PlayMode.asmdef` — Unity assembly definition.
- `SphericalPlanetStudioPlayModeTests.cs` — Types: SphericalPlanetStudioPlayModeTests.

### `Assets/Resources/WorldGeneration/`
- `world_generation.json` — JSON data/configuration/schema.

### `Assets/Scenes/`
- `SampleScene.unity` — Unity scene.

### `Assets/Settings/`
- `DefaultVolumeProfile.asset` — Unity serialized asset.
- `Mobile_RPAsset.asset` — Unity serialized asset.
- `Mobile_Renderer.asset` — Unity serialized asset.
- `PC_RPAsset.asset` — Unity serialized asset.
- `PC_Renderer.asset` — Unity serialized asset.
- `SampleSceneProfile.asset` — Unity serialized asset.
- `UniversalRenderPipelineGlobalSettings.asset` — Unity serialized asset.

### `Assets/StreamingAssets/PokemonData/biomes/`
- `forest.json` — JSON data/configuration/schema.
- `meadow.json` — JSON data/configuration/schema.

### `Assets/StreamingAssets/PokemonData/generation/`
- `world_generation.json` — JSON data/configuration/schema.

### `Assets/StreamingAssets/PokemonData/maps/world_meadow_01/height/`
- `manifest.json` — JSON data/configuration/schema.
- `values.f32` — Project file.

### `Assets/StreamingAssets/PokemonData/maps/world_meadow_01/humidity/`
- `manifest.json` — JSON data/configuration/schema.
- `values.f32` — Project file.

### `Assets/StreamingAssets/PokemonData/maps/world_meadow_01/temperature/`
- `manifest.json` — JSON data/configuration/schema.
- `values.f32` — Project file.

### `Assets/StreamingAssets/PokemonData/maps/world_meadow_01/vegetation_density/`
- `manifest.json` — JSON data/configuration/schema.
- `values.f32` — Project file.

### `Assets/StreamingAssets/PokemonData/pokemon/`
- `chenipotte.json` — JSON data/configuration/schema.
- `mystherbe.json` — JSON data/configuration/schema.

### `Assets/StreamingAssets/PokemonData/schemas/`
- `biome.schema.json` — JSON data/configuration/schema.
- `map_layer_manifest.schema.json` — JSON data/configuration/schema.
- `pokemon_species.schema.json` — JSON data/configuration/schema.
- `world.schema.json` — JSON data/configuration/schema.

### `Assets/StreamingAssets/PokemonData/worlds/`
- `world_meadow_01.json` — JSON data/configuration/schema.

### `Assets/_Recovery/`
- `0 (1).unity` — Unity scene.
- `0 (2).unity` — Unity scene.
- `0 (3).unity` — Unity scene.
- `0 (4).unity` — Unity scene.
- `0 (5).unity` — Unity scene.
- `0.unity` — Unity scene.

### `Blender/`
- `README.md` — Documentation/instructions.

### `Blender/_core/`
- `API_REFERENCE.md` — Documentation/instructions.
- `README.md` — Documentation/instructions.
- `__init__.py` — Python module.
- `animation.py` — Functions: actions_for_objects, action_metadata, set_scene_frame_range.
- `collections.py` — Functions: get_or_create_collection, move_objects_to_collection, collection_objects_recursive.
- `collision.py` — Functions: collision_name, find_collision_objects.
- `config.py` — Functions: load_json, load_project_config.
- `export.py` — Functions: static_mesh, skinned_mesh, animated_mesh, export_fbx.
- `geometry.py` — Functions: mesh_stats, mesh_has_faces, dimensions.
- `lod.py` — Functions: lod_name, find_lods.
- `logging.py` — Functions: get_logger.
- `manifest.py` — Functions: build_manifest, write_manifest.
- `materials.py` — Functions: get_or_create_material, assign_material.
- `naming.py` — Functions: slugify, validate_identifier, find_duplicate_names.
- `paths.py` — Functions: find_project_root, _is_inside, discover, blender, _configured_path….
- `result.py` — Functions: to_dict, errors, warnings, passed, add….
- `rigging.py` — Functions: find_armatures, find_skinned_meshes, validate_armature_bindings.
- `scene.py` — Functions: get_or_create_scene, set_active_scene, deselect_all, select_only.
- `sockets.py` — Functions: find_sockets, validate_socket_names.
- `textures.py` — Functions: load_image, pack_image, image_info.
- `transforms.py` — Functions: apply_transforms, transforms_are_identity.
- `uv.py` — Functions: ensure_uv_layer, validate_uvs.
- `validation.py` — Functions: validate_export_path, validate_objects.
- `versioning.py` — Functions: pipeline_versions.

### `Blender/config/`
- `project.example.json` — JSON data/configuration/schema.
- `project.json` — JSON data/configuration/schema.

### `Blender/exports/environment/fallen_log/`
- `SM_Fallen_Log.fbx` — FBX model.
- `SM_Fallen_Log.manifest.json` — JSON data/configuration/schema.

### `Blender/exports/environment/grass_short/`
- `SM_Grass_Short.fbx` — FBX model.
- `SM_Grass_Short.manifest.json` — JSON data/configuration/schema.

### `Blender/exports/environment/grass_short_test/`
- `SM_Grass_Short_Test.fbx` — FBX model.
- `SM_Grass_Short_Test.manifest.json` — JSON data/configuration/schema.

### `Blender/exports/environment/grass_tall/`
- `SM_Grass_Tall.fbx` — FBX model.
- `SM_Grass_Tall.manifest.json` — JSON data/configuration/schema.

### `Blender/exports/environment/leaf_litter/`
- `SM_Leaf_Litter.fbx` — FBX model.
- `SM_Leaf_Litter.manifest.json` — JSON data/configuration/schema.

### `Blender/exports/environment/mushroom_red/`
- `SM_Mushroom_Red.fbx` — FBX model.
- `SM_Mushroom_Red.manifest.json` — JSON data/configuration/schema.

### `Blender/exports/environment/reed_clump/`
- `SM_Reed_Clump.fbx` — FBX model.
- `SM_Reed_Clump.manifest.json` — JSON data/configuration/schema.

### `Blender/exports/environment/stone_cluster_large/`
- `SM_Stone_Cluster_Large.fbx` — FBX model.
- `SM_Stone_Cluster_Large.manifest.json` — JSON data/configuration/schema.

### `Blender/exports/environment/stone_cluster_small/`
- `SM_Stone_Cluster_Small.fbx` — FBX model.
- `SM_Stone_Cluster_Small.manifest.json` — JSON data/configuration/schema.

### `Blender/exports/environment/tree_stump/`
- `SM_Tree_Stump.fbx` — FBX model.
- `SM_Tree_Stump.manifest.json` — JSON data/configuration/schema.

### `Blender/exports/environment/wild_grass/`
- `SM_Grass_Wild.fbx` — FBX model.
- `SM_Grass_Wild.manifest.json` — JSON data/configuration/schema.

### `Blender/exports/pokemon/ecoraki/`
- `SK_Pokemon_Ecoraki_Base.fbx` — FBX model.
- `SK_Pokemon_Ecoraki_Base.manifest.json` — JSON data/configuration/schema.

### `Blender/exports/pokemon/nacoryn/`
- `SK_Pokemon_Nacoryn_Base.fbx` — FBX model.
- `SK_Pokemon_Nacoryn_Base.manifest.json` — JSON data/configuration/schema.

### `Blender/old/`
- `ART_Blender_Core_v0.1.0.zip` — Project file.
- `Chenipotte.fbx` — FBX model.
- `Chenipotte.py` — Functions: clear_scene, make_material, finish_obj, uv_sphere, cone….
- `Environment_Assets.py` — Functions: mat, begin, put, sphere, ico….
- `Mystherbe.fbx` — FBX model.
- `Mystherbe.py` — Functions: clear_scene, make_material, finish_obj, uv_sphere, cone….
- `Pokemon_Blender_Scripts.zip` — Project file.
- `Pokemon_Environment_Assets_Scripts.zip` — Project file.
- `Pokemon_Grass_Assets.py` — Functions: make_mat, begin_collection, link_current, make_blade, make_stem….
- `Pokemon_Grass_Assets_Scripts.zip` — Project file.
- `Roucool.fbx` — FBX model.
- `Roucool.py` — Functions: clear_scene, make_material, finish_obj, uv_sphere, cone….

### `Blender/project_modules/environment/`
- `__init__.py` — Python module.
- `generators.py` — Functions: _mat, _mesh_object, _append_tapered_blade, build_grass, build_flower….

### `Blender/project_modules/pokemon/`
- `__init__.py` — Python module.
- `ecoraki_geometry.py` — Functions: make_material, _mesh, ellipsoid, tube, sample….
- `procedural.py` — Functions: mat, _smooth_off, ico, uv_sphere, cone….

### `Blender/source/environment/`
- `generate_all.py` — Functions: _active_text_file_path, _find_blender_root, _is_test_generator, _discover_generators, _run_generator….

### `Blender/source/environment/fallen_log/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _find_blender_root, generate.

### `Blender/source/environment/flower_blue/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _find_blender_root, generate.

### `Blender/source/environment/flower_yellow/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _find_blender_root, generate.

### `Blender/source/environment/grass_short/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _find_blender_root, generate.

### `Blender/source/environment/grass_short_test/`
- `generator.py` — Functions: _find_blender_root, _remove_previous_test_collection, _make_grass_mesh, generate.

### `Blender/source/environment/grass_tall/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _find_blender_root, generate.

### `Blender/source/environment/leaf_litter/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _find_blender_root, generate.

### `Blender/source/environment/mushroom_red/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _find_blender_root, generate.

### `Blender/source/environment/reed_clump/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _find_blender_root, generate.

### `Blender/source/environment/stone_cluster_large/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _find_blender_root, generate.

### `Blender/source/environment/stone_cluster_small/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _find_blender_root, generate.

### `Blender/source/environment/tree_stump/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _find_blender_root, generate.

### `Blender/source/environment/wild_grass/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _find_blender_root, generate.

### `Blender/source/pokemon/ecoraki/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _current_text_file_path, find_blender_root, configured_profile, clear_owned_collection, generate.

### `Blender/source/pokemon/ferilume/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _current_text_file_path, _find_blender_root, generate.

### `Blender/source/pokemon/nacoryn/`
- `README.md` — Documentation/instructions.
- `generator.py` — Functions: _current_text_file_path, _find_blender_root, generate.

### `Data/biomes/`
- `forest.json` — JSON data/configuration/schema.
- `meadow.json` — JSON data/configuration/schema.

### `Data/generation/`
- `world_generation.json` — JSON data/configuration/schema.

### `Data/maps/world_meadow_01/height/`
- `manifest.json` — JSON data/configuration/schema.
- `values.f32` — Project file.

### `Data/maps/world_meadow_01/humidity/`
- `manifest.json` — JSON data/configuration/schema.
- `values.f32` — Project file.

### `Data/maps/world_meadow_01/temperature/`
- `manifest.json` — JSON data/configuration/schema.
- `values.f32` — Project file.

### `Data/maps/world_meadow_01/vegetation_density/`
- `manifest.json` — JSON data/configuration/schema.
- `values.f32` — Project file.

### `Data/pokemon/`
- `chenipotte.json` — JSON data/configuration/schema.
- `mystherbe.json` — JSON data/configuration/schema.

### `Data/schemas/`
- `biome.schema.json` — JSON data/configuration/schema.
- `map_layer_manifest.schema.json` — JSON data/configuration/schema.
- `pokemon_species.schema.json` — JSON data/configuration/schema.
- `world.schema.json` — JSON data/configuration/schema.

### `Data/worlds/`
- `world_meadow_01.json` — JSON data/configuration/schema.

### `Documentation/`
- `DOCSET_METADATA.json` — JSON data/configuration/schema.
- `README.md` — Documentation/instructions.

### `Documentation/00_GOVERNANCE/`
- `CHANGE_PROPOSAL_TEMPLATE.md` — Documentation/instructions.
- `CHAT_TEAM_STRUCTURE.md` — Documentation/instructions.
- `DECISION_REGISTER.md` — Documentation/instructions.
- `DOCUMENTATION_CHANGELOG.md` — Documentation/instructions.
- `DOCUMENT_INDEX.md` — Documentation/instructions.
- `GIT_WORKFLOW.md` — Documentation/instructions.
- `NEW_CONCEPT_WORKFLOW.md` — Documentation/instructions.
- `PROJECT_ALIGNMENT_PLAN.md` — Documentation/instructions.
- `PROJECT_CHARTER.md` — Documentation/instructions.
- `WORKING_AGREEMENTS.md` — Documentation/instructions.
- `a.md` — Documentation/instructions.

### `Documentation/01_ARCHITECTURE/`
- `DATA_ARCHITECTURE.md` — Documentation/instructions.
- `INTERFACE_CONTRACTS.md` — Documentation/instructions.
- `MODULE_BOUNDARIES.md` — Documentation/instructions.
- `NETWORK_ARCHITECTURE.md` — Documentation/instructions.
- `SPHERICAL_WORLD_ARCHITECTURE_PROPOSAL.md` — Documentation/instructions.
- `SYSTEM_ARCHITECTURE.md` — Documentation/instructions.
- `UNITY_ARCHITECTURE.md` — Documentation/instructions.
- `WORLD_ARCHITECTURE.md` — Documentation/instructions.

### `Documentation/02_DOMAINS/`
- `AI_API_INTEGRATION.md` — Documentation/instructions.
- `ART_AND_ASSET_PIPELINE.md` — Documentation/instructions.
- `AUDIO_AND_VFX.md` — Documentation/instructions.
- `COMBAT_AND_ABILITIES.md` — Documentation/instructions.
- `ECOLOGY_SIMULATION.md` — Documentation/instructions.
- `PERSISTENCE_AND_SESSIONS.md` — Documentation/instructions.
- `POKEMON_SIMULATION.md` — Documentation/instructions.
- `TERRAIN_AND_RENDERING.md` — Documentation/instructions.
- `WORLD_AND_BIOMES.md` — Documentation/instructions.

### `Documentation/03_ENGINEERING/`
- `BUILD_AND_DEPLOYMENT.md` — Documentation/instructions.
- `CODING_STANDARDS.md` — Documentation/instructions.
- `DEVELOPMENT_QUEUE.md` — Documentation/instructions.
- `PERFORMANCE_GUIDELINES.md` — Documentation/instructions.
- `ROADMAP_AND_VERTICAL_SLICE.md` — Documentation/instructions.
- `SECURITY_AND_API_GUIDELINES.md` — Documentation/instructions.
- `TESTING_AND_VALIDATION.md` — Documentation/instructions.

### `Documentation/03_ENGINEERING/WORK_PACKAGES/`
- `WORLD-001_IMPLEMENTATION_PROGRESS_01.md` — Documentation/instructions.
- `WORLD-001_IMPLEMENTATION_REPORT.md` — Documentation/instructions.
- `WORLD-001_LAYERED_PLANET_STUDIO_AMENDMENT.md` — Documentation/instructions.
- `WORLD-001_LAYERED_PLANET_STUDIO_PROGRESS_01.md` — Documentation/instructions.
- `WORLD-001_SPHERICAL_WORLD_FOUNDATION.md` — Documentation/instructions.
- `WORLD-002_ASSET_READINESS_CHECKLIST.md` — Documentation/instructions.
- `WORLD-002_EXHAUSTIVE_VISUAL_ASSET_CATALOG.md` — Documentation/instructions.
- `WORLD-002_IMPLEMENTATION_PROGRESS_01.md` — Documentation/instructions.
- `WORLD-002_IMPLEMENTATION_PROGRESS_02.md` — Documentation/instructions.
- `WORLD-002_IMPLEMENTATION_PROGRESS_03_TESTING_STANDARD.md` — Documentation/instructions.
- `WORLD-002_IMPLEMENTATION_PROGRESS_04_PLAYMODE_TESTS.md` — Documentation/instructions.
- `WORLD-002_IMPLEMENTATION_PROGRESS_05_DEPRECATED_API_POLICY.md` — Documentation/instructions.
- `WORLD-002_OCEAN_COVERAGE_AND_FOUR_BIOMES_AMENDMENT.md` — Documentation/instructions.
- `WORLD-002_SPHERICAL_GEOGRAPHIC_FIELDS.md` — Documentation/instructions.
- `WORLD-003_PLAYABLE_BIOME_INSPECTION.md` — Documentation/instructions.
- `WORLD-004_PLAYER_CONTROLLER_AND_SPHERICAL_LOCOMOTION.md` — Documentation/instructions.
- `WORLD-007_CONFIGURABLE_PLANET_SIZE_AND_OCEAN_RANGE.md` — Documentation/instructions.
- `WORLD-008_JSON_WORLD_PROFILES.md` — Documentation/instructions.
- `WORLD-009A_OPAQUE_BIOME_INSPECTION_TERRAIN.md` — Documentation/instructions.
- `WORLD-010A_BIOME_TEXTURE_RENDERING_AND_PLAINS_FOREST.md` — Documentation/instructions.
- `WORLD-010_DIAGNOSTIC_TEXTURE_PIPELINE_CHECK.md` — Documentation/instructions.

### `Documentation/04_AI_INSTRUCTIONS/`
- `copilot-instructions.md` — Documentation/instructions.

### `Documentation/04_AI_INSTRUCTIONS/instructions/`
- `blender-python.instructions.md` — Documentation/instructions.
- `documentation.instructions.md` — Documentation/instructions.
- `simulation.instructions.md` — Documentation/instructions.
- `unity-csharp.instructions.md` — Documentation/instructions.

### `Documentation/05_TEMPLATES/`
- `DOCUMENTATION_AUDIT_TEMPLATE.md` — Documentation/instructions.
- `DOMAIN_CHAT_STARTER.md` — Documentation/instructions.
- `FEATURE_SPEC_TEMPLATE.md` — Documentation/instructions.
- `INTEGRATION_REPORT_TEMPLATE.md` — Documentation/instructions.

### `Documentation/05_TEMPLATES/CHAT_PACKS/`
- `ARCHITECT_CHAT.md` — Documentation/instructions.
- `ART_CHAT.md` — Documentation/instructions.
- `COMBAT_CHAT.md` — Documentation/instructions.
- `DOC_CHAT.md` — Documentation/instructions.
- `ECOLOGY_CHAT.md` — Documentation/instructions.
- `NETWORK_CHAT.md` — Documentation/instructions.
- `POKEMON_CHAT.md` — Documentation/instructions.
- `QA_CHAT.md` — Documentation/instructions.
- `TOOLS_CHAT.md` — Documentation/instructions.
- `WORLD_CHAT.md` — Documentation/instructions.

### `Documentation/06_AUDIT/`
- `CODE_FUNCTION_OWNERSHIP_INVENTORY.md` — Documentation/instructions.
- `DOC-001_AUDIT_CORRECTION_PLAN.md` — Documentation/instructions.
- `REPOSITORY_ACTION_REGISTER.md` — Documentation/instructions.
- `REPOSITORY_AUDIT_REPORT.md` — Documentation/instructions.

### `Documentation/07_Testing/`
- `COVERAGE_MATRIX.md` — Documentation/instructions.
- `MIGRATION.md` — Documentation/instructions.
- `README.md` — Documentation/instructions.
- `TESTING_AND_VALIDATION.md` — Documentation/instructions.

### `Documentation/07_Testing/Work_Packages/`
- `PLANAR_WORLD_GENERATION_TEST_PLAN.md` — Documentation/instructions.
- `PLAYMODE_RUNTIME_TEST_PLAN.md` — Documentation/instructions.
- `WORLD-001_SPHERICAL_WORLD_FOUNDATION_TEST_PLAN.md` — Documentation/instructions.
- `WORLD-002_512_SPHERICAL_GRAPH_BENCHMARK.md` — Documentation/instructions.
- `WORLD-002_DRAINAGE_512_BENCHMARK.md` — Documentation/instructions.
- `WORLD-002_DRAINAGE_ISOLATION_TEST_PLAN.md` — Documentation/instructions.
- `WORLD-002_DRAINAGE_TEST_PLAN.md` — Documentation/instructions.
- `WORLD-002_DRAINAGE_VISUAL_INTEGRATION.md` — Documentation/instructions.
- `WORLD-002_IMPLEMENTATION_PROGRESS_03_TESTING_STANDARD.md` — Documentation/instructions.
- `WORLD-002_IMPLEMENTATION_PROGRESS_04_PLAYMODE_TESTS.md` — Documentation/instructions.
- `WORLD-002_IMPLEMENTATION_PROGRESS_05_DEPRECATED_API_POLICY.md` — Documentation/instructions.
- `WORLD-002_OCEAN_BIOME_TEST_PLAN.md` — Documentation/instructions.
- `WORLD-002_SPHERICAL_FIELDS_TEST_PLAN.md` — Documentation/instructions.
- `WORLD-004_PLAYER_MOVEMENT_TEST_PLAN.md` — Documentation/instructions.
- `WORLD-007_PLANET_SIZE_AND_OCEAN_RANGE_TEST_PLAN.md` — Documentation/instructions.

### `Packages/`
- `manifest.json` — JSON data/configuration/schema.
- `packages-lock.json` — JSON data/configuration/schema.

### `ProjectSettings/`
- `AudioManager.asset` — Unity serialized asset.
- `ClusterInputManager.asset` — Unity serialized asset.
- `DynamicsManager.asset` — Unity serialized asset.
- `EditorBuildSettings.asset` — Unity serialized asset.
- `EditorSettings.asset` — Unity serialized asset.
- `GraphicsSettings.asset` — Unity serialized asset.
- `InputManager.asset` — Unity serialized asset.
- `MemorySettings.asset` — Unity serialized asset.
- `MultiplayerManager.asset` — Unity serialized asset.
- `NavMeshAreas.asset` — Unity serialized asset.
- `PackageManagerSettings.asset` — Unity serialized asset.
- `Physics2DSettings.asset` — Unity serialized asset.
- `PresetManager.asset` — Unity serialized asset.
- `ProjectSettings.asset` — Unity serialized asset.
- `ProjectVersion.txt` — Project file.
- `QualitySettings.asset` — Unity serialized asset.
- `SceneTemplateSettings.json` — JSON data/configuration/schema.
- `ShaderGraphSettings.asset` — Unity serialized asset.
- `TagManager.asset` — Unity serialized asset.
- `TimeManager.asset` — Unity serialized asset.
- `URPProjectSettings.asset` — Unity serialized asset.
- `UnityConnectSettings.asset` — Unity serialized asset.
- `VFXManager.asset` — Unity serialized asset.
- `VersionControlSettings.asset` — Unity serialized asset.
- `XRSettings.asset` — Unity serialized asset.

### `ProjectSettings/BlenderAssetSync/`
- `ownership.json` — JSON data/configuration/schema.

### `Tools/`
- `generate_demo_maps.py` — Functions: clamp, generate, main.
- `validate_data.py` — Functions: __init__, error, read_json, files, check_version_id….

## 9. Snapshot gaps and cautions

- `Assets/Project/Content/Prefabs/Environment/` was not present in this archive. The user has since generated prefabs, but their exact live paths must be verified before indexing them.
- The separate `EnvironmentPrefabGeneratorWindow.cs` deliverable is not present in this archive. Do not infer its installed path from the ZIP's intended extraction path.
- `Blender/exports/` and `Assets/Project/Content/Models/Environment/` are different pipeline stages: Blender source/export versus Unity-imported models.
- `Documentation/06_AUDIT/CODE_FUNCTION_OWNERSHIP_INVENTORY.md` is a specialized method-level inventory, not a complete path index.
- This index reflects the supplied archive. Reconcile against the current working tree when it is provided.

## 10. Maintenance checklist

- [ ] Update exact paths when files/folders are added, moved, renamed or removed.
- [ ] Update feature ownership when responsibilities change.
- [ ] Update architecture docs when data flow, dependencies or lifecycle change.
- [ ] Add new documents to `Documentation/00_GOVERNANCE/DOCUMENT_INDEX.md`.
- [ ] Distinguish source assets, imported assets and generated runtime assets.
- [ ] Record Unity compilation/Play Mode validation separately; file presence does not prove runtime correctness.

## WORLD-011 environment placement configuration (added 2026-11-19)

| Need | Path | Responsibility |
|---|---|---|
| Placement rules (JSON source of truth) | `Assets/Project/Content/Settings/World/EnvironmentPlacement/CFG_SphericalEnvironmentPlacement.json` | Plains/Forest rules and tunable numeric parameters; schema v1 |
| Unity prefab ID catalog | `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPrefabCatalog.cs` | Stable logical ID → prefab asset reference |
| JSON parsing and validation | `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementJson.cs` | Parse, validate and resolve enabled JSON rules |
| Config asset linking JSON + catalog | `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementConfig.cs` | Inspector references to the JSON TextAsset and prefab catalog |
| EditMode tests | `Assets/Project/Tests/EditMode/World/Spherical/Environment/SphericalEnvironmentPlacementSettingsTests.cs` | JSON loading and validation tests |

The prefab catalog is populated in Unity because generated prefab assets are not present in the inspected source snapshot. Do not hardcode inferred prefab paths into the JSON.
