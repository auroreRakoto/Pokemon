# CLEAN-001 — Legacy Project Audit Report

- **Status:** AUDIT COMPLETE — read-only snapshot review
- **Work package:** CLEAN-001 — Legacy purge and project reconstruction
- **Audit date:** 2026-11-19
- **Audit basis:** six supplied archives: `Assets.zip`, `Packages.zip`, `ProjectSettings.zip`, `Data.zip`, `Documentation.zip`, `Tools.zip`
- **Repository revision:** UNKNOWN — no `.git` metadata, commit hash, branch or remote data supplied
- **Unity execution:** NOT RUN
- **Blender source audit:** BLOCKED — the Blender directory was intentionally not supplied
- **Changes made to the project:** NONE

## 1. Executive summary

The supplied Unity snapshot is no longer the older 2D ecology prototype described by some historical audit documents. Its current code is primarily a spherical-world prototype. The old Pokémon/ecology components survive mainly as serialized references in recovery/legacy scenes and as data samples; their corresponding runtime scripts are absent from the supplied `Assets` tree.

The strongest cleanup candidates are:

1. Six recovery scenes under `Assets/_Recovery/`, containing references to old missing components. Three of these scenes are byte-identical.
2. `Assets/Project/Content/Scenes/Worlds/World01.unity`, a legacy planar scene with four missing custom script components and no Build Settings entry.
3. Eight unreferenced generated mesh assets under `Assets/Generated/Worlds/`, totalling **121,747,201 bytes (116.1 MiB)**. No external GUID references to these mesh assets were found in the supplied Assets snapshot.
4. A duplicate planar world-generation JSON in `Assets/Resources/`, plus an unused `WorldGenerationConfig` Resources loader.
5. The isolated planar world-generation Editor pipeline. It has no current spherical-scene consumer, but an approved WORLD-001 work package explicitly says to keep it. Removing it therefore requires an explicit owner decision.

The spherical world, Planet Studio, spherical inspection scenes, world profiles, environment placement configuration, ten generated environment prefabs and their catalog are active project assets and must be preserved.

## 2. Archive inventory and limitations

| Archive | Supplied content |
|---|---|
| `Assets.zip` | 465 files: 173 non-`.meta` files and 292 `.meta` sidecars |
| `Packages.zip` | `manifest.json`, `packages-lock.json` |
| `ProjectSettings.zip` | 26 files, including Build Settings and Blender sync ownership ledger |
| `Data.zip` | 18 files: planar demo data, manifests, schemas and binary map layers |
| `Tools.zip` | `generate_demo_maps.py`, `validate_data.py` |
| `Documentation.zip` | 124 archive entries, including governance, architecture, work packages, test plans and audit records |

`Assets.zip` contains:
- 63 C# files: 38 Runtime, 10 Editor, 15 Tests;
- 11 Unity scenes;
- 10 environment prefabs;
- 23 FBX files;
- 23 JSON files;
- 4 PNG textures;
- 4 assembly definition files.

All 173 non-`.meta` files in the supplied Assets archive have a corresponding `.meta` sidecar. This is a static file-presence check, not proof that every GUID resolves in Unity.

The supplied archives do not contain:
- `.git` history or current working-tree state;
- the Blender source directory, `.blend` files, generators or export manifests;
- a Unity Editor log, compile report, Test Runner export or build report;
- the owner's live Unity/Blender session.

A ZIP is a snapshot, not a live repository connection.

## 3. Current implementation shape

### 3.1 Current runtime code

The supplied `Assets` tree contains spherical world code under:

`Assets/Project/Runtime/World/Spherical/`

This includes topology, height fields, geography, drainage/graph utilities, inspection bootstraps, movement, presentation, Planet Studio and environment placement.

The current `Assets` snapshot contains no runtime source folders or scripts for:
- Pokémon individual simulation;
- Ecology/resource simulation;
- Combat;
- Trainer/party/inventory gameplay.

This does not mean those product goals are cancelled. The approved long-term direction remains; it means these features are not implemented in this snapshot.

### 3.2 Current spherical scenes and environment content — PRESERVE

The following are current preview/inspection routes and must not be included in a legacy purge:

- `Assets/Project/Content/Scenes/Worlds/SphericalWorldDemo.unity`
- `Assets/Project/Content/Scenes/Worlds/WORLD-002_BiomeInspection.unity`
- `Assets/Project/Runtime/World/Spherical/`
- `Assets/Project/Editor/World/Spherical/`
- `Assets/Project/Data/WorldProfiles/`
- `Assets/Project/Content/World/PlanetStudio/`
- `Assets/Project/Content/Settings/World/EnvironmentPlacement/`
- `Assets/Project/Runtime/World/Spherical/Environment/`
- `Assets/Project/Editor/Tools/Prefabs/EnvironmentPrefabGeneratorWindow.cs`
- `Assets/Project/Editor/Tools/World/EnvironmentPlacementAssetGenerator.cs`
- `Assets/Project/Content/Prefabs/Environment/`
- `Assets/Project/Content/Models/Environment/` production model hierarchy
- `Assets/Project/Content/Materials/Environment/`
- `Assets/Project/Content/Textures/Environment/Biomes/`

The environment prefab catalog references ten prefabs. The placement configuration references its configuration JSON and prefab catalog. These assets are part of the current WORLD-011 path, not legacy debris.

### 3.3 Prefab evidence and remaining WORLD-011-A work

Ten environment prefabs are present in this archive. Each prefab YAML shows root local scale `(1, 1, 1)` and no collider component in the prefab file.

This does **not** establish:
- FBX mesh bounds;
- mesh pivot position;
- rendered dimensions;
- mesh hierarchy;
- ground-contact offset;
- actual Unity ModelImporter state;
- Blender source/export correctness.

Those checks still require Unity inspection and the relevant Blender source/manifests. Earlier WORLD-011-A audit documents that state no generated prefabs were present; that statement is contradicted by this newer supplied snapshot and should be updated after owner approval.

## 4. Legacy and cleanup findings

### CLEAN-F01 — Recovery scenes

Paths:
- `Assets/_Recovery/0.unity`
- `Assets/_Recovery/0 (1).unity`
- `Assets/_Recovery/0 (2).unity`
- `Assets/_Recovery/0 (3).unity`
- `Assets/_Recovery/0 (4).unity`
- `Assets/_Recovery/0 (5).unity`
- Include the matching `.meta` files and the folder `.meta` if the folder is removed.

Evidence:
- None is listed in `ProjectSettings/EditorBuildSettings.asset`.
- No references to these scene assets were found in the supplied scene/prefab/asset YAML.
- The scenes serialize old components such as `ResourceManager`, `EcosystemLogger`, `EcosystemDataCollector`, `FruitBushManager`, `SimulationTimeController`, `MeadowGenerator`, `MapRenderer`, `WorldBiomeGenerator`, `WorldBiomeMapRenderer`, `PokemonSpawnManager` and `GrassVegetationSpawner`.
- Those custom component scripts are absent from the current `Assets` snapshot.
- `0 (2).unity`, `0 (3).unity` and `0 (4).unity` are byte-identical.

Classification: `HISTORICAL` / `UNREFERENCED` / `NON_COMPLIANT` serialized residue.

Recommendation: archive outside the Unity project, then remove all six scenes and their `.meta` files after owner approval. If the owner wants a minimal recovery archive, retain one copy of each distinct scene snapshot outside `Assets`; do not keep duplicate recovery scenes in the active Unity project.

### CLEAN-F02 — Legacy planar scene

Path:
- `Assets/Project/Content/Scenes/Worlds/World01.unity`
- `Assets/Project/Content/Scenes/Worlds/World01.unity.meta`

Evidence:
- Not present in Build Settings.
- No external scene/prefab/asset references found in the supplied snapshot.
- Contains `GrassVegetationSpawner`, `WorldBiomeMapRenderer`, `ResourceManager` and `FruitBushManager` components whose scripts are absent from the current Assets snapshot.

Classification: `OBSOLETE` candidate / `UNREFERENCED` in the supplied snapshot.

Recommendation: remove after an external backup and owner approval. It is not the spherical inspection scene.

### CLEAN-F03 — Generated planar mesh outputs

Paths:
- `Assets/Generated/Worlds/GeneratedWorld_481516_256x256.asset`
- `Assets/Generated/Worlds/GeneratedWorld_481516_256x256 1.asset`
- `Assets/Generated/Worlds/GeneratedWorld_481516_256x256 2.asset`
- `Assets/Generated/Worlds/GeneratedWorld_481516_256x256 3.asset`
- `Assets/Generated/Worlds/GeneratedWorld_481516_256x256 4.asset`
- `Assets/Generated/Worlds/World_481516_256x256.asset`
- `Assets/Generated/Worlds/World_481516_256x256 1.asset`
- `Assets/Generated/Worlds/World_481516_623x623.asset`
- Each asset's matching `.meta` file.

Evidence:
- Eight generated mesh assets total 121,747,201 bytes (116.1 MiB).
- No external GUID reference to any of the eight mesh assets was found in the supplied Assets YAML.
- Their names and location indicate generated world outputs. The legacy planar Editor tool also writes generated meshes under `Assets/Generated/Worlds/`.

Classification: `UNREFERENCED` generated-output candidates.

Recommendation: remove after owner approval and Unity-side reference verification. The `GeneratedWorldVertexColor.mat` file is not part of this eight-mesh list; retain it only if the planar Editor tool is retained.

### CLEAN-F04 — Duplicate Resources world-generation configuration

Paths:
- `Assets/Resources/WorldGeneration/world_generation.json`
- `Assets/Resources/WorldGeneration/world_generation.json.meta`
- `Assets/Project/Runtime/World/Generation/WorldGenerationConfig.cs`
- Matching `.meta` files.

Evidence:
- The Resources JSON is byte-identical to `Assets/Project/Data/WorldGeneration/world_generation.json`.
- `WorldGenerationConfig.Load()` uses `Resources.Load<TextAsset>`.
- No caller of `WorldGenerationConfig.Load()` was found in the supplied Assets source.
- The current planar Editor window instead uses `WorldGenerationConfigLoader.LoadForEditor()` to read `Assets/Project/Data/WorldGeneration/world_generation.json`.
- DEC-020 confirms `Assets/Project/Data` as the single project-level source location and `Runtime/DataServices` for runtime loading/contracts/projections.

Classification: `DUPLICATE` / `UNREFERENCED` / `NON_COMPLIANT` candidate.

Recommendation: remove the obsolete Resources loader and duplicate JSON as part of the planar pipeline retirement, or as a separately approved cleanup if the planar generator is retained.

### CLEAN-F05 — Isolated planar world-generation pipeline

Current files:
- `Assets/Project/Runtime/World/Generation/WorldGenerationConfig.cs`
- `Assets/Project/Runtime/World/Generation/WorldGenerationResult.cs`
- `Assets/Project/Runtime/World/Generation/WorldGenerationSettings.cs`
- `Assets/Project/Runtime/World/Generation/WorldGenerator.cs`
- `Assets/Project/Runtime/World/Generation/WorldTerrainBuilder.cs`
- `Assets/Project/Runtime/World/Generation/WorldVertexColor.shader`
- `Assets/Project/Runtime/DataServices/Loading/WorldGenerationConfigLoader.cs`
- `Assets/Project/Editor/WorldGeneration/WorldGenerationWindow.cs`
- `Assets/Project/Editor/WorldGeneration/WorldGenerationConfigBuildProcessor.cs`
- `Assets/Project/Data/WorldGeneration/world_generation.json`
- Associated `.meta` files and the generated planar output folder.

Evidence:
- `WorldGenerationWindow` is the only supplied C# caller of `WorldGenerator.Generate()`.
- The same Editor window uses `WorldTerrainBuilder`.
- The Editor window uses `WorldGenerationConfigLoader.LoadForEditor()`.
- `WorldGenerationConfigBuildProcessor` uses the loader's source path to copy the JSON into StreamingAssets during a build.
- No current spherical scene or spherical runtime class calls the planar generator.
- The planar pipeline has no matching automated tests in the supplied test tree; `COVERAGE_MATRIX.md` records gaps.
- The existing WORLD-001 work package explicitly says in §9: “The existing planar `WorldGenerator` and `WorldTerrainBuilder` remain untouched and continue to serve the existing planar test workflow.”

Classification: `OBSOLETE` candidate from the current product direction, but **owner decision required** because of the explicit WORLD-001 §9 preservation clause.

Recommendation: retire the complete planar feature as one coherent batch rather than deleting isolated files and leaving a broken Editor tool. This requires the owner to explicitly supersede WORLD-001 §9 and update the related test plan, coverage matrix, file index, queue and decision record.

### CLEAN-F06 — External Data and Tools demo pipeline

Paths supplied as separate archives:
- `Data/`
- `Tools/generate_demo_maps.py`
- `Tools/validate_data.py`

Evidence:
- The Python generator creates 2D X/Z float32 map layers for `world_meadow_01`.
- The data uses `meadow` and `forest` biome definitions and two Pokémon species examples.
- The `Assets/StreamingAssets/PokemonData/` copy contains 16 of the 18 Data files byte-identically.
- The two differing files are `generation/world_generation.json` and `worlds/world_meadow_01.json`.
- The external `Data` snapshot currently describes a 256×256, 2,000 m world, while its four map manifests describe 64×64, 128 m layers at origin (-64, -64).
- Running the supplied validator against the external `Data` snapshot returned 12 dimension/origin/world-size mismatch errors.
- Running the same validator against `Assets/StreamingAssets/PokemonData` returned PASS.
- No current C# source reference to `PokemonData` was found in the supplied Assets tree.

Classification: `HISTORICAL` / `UNCONNECTED` demo data pipeline, with useful Pokémon/species schema samples that may be reusable later.

Recommendation: do not delete all Pokémon/biome JSON blindly. Decide whether to archive the old planar world/map demo separately while preserving species/schema examples for future POK/ECO work. If retained as source data, regenerate its maps and run the validator before calling it valid.

### CLEAN-F07 — Large spherical Planet Studio preview scene

Path:
- `Assets/Project/Content/Scenes/Worlds/World02.unity`
- Matching `.meta` file.

Evidence:
- File size: 39,506,308 bytes.
- Contains `WORLD-001_PlanetStudioPreview` and WORLD-002 drainage overlay objects.
- No custom MonoBehaviour component was found in the scene YAML.
- It is not in Build Settings.
- `SphericalPlanetGenerationStudioWindow` provides `Generate / Refresh Preview` and `Clear Preview` operations in the active scene.

Classification: `GENERATED` / `REGENERABLE` candidate, not proven obsolete.

Recommendation: preserve until the Planet Studio preview has been regenerated in Unity and visually checked. Then decide whether to keep the scene as a saved fixture or replace it with a smaller reproducible preview scene.

### CLEAN-F08 — Package candidates

The supplied C# scan found no direct code use of:
- `com.unity.ai.navigation` APIs;
- `com.unity.timeline` APIs;
- `com.unity.visualscripting` APIs;
- UGUI APIs in current C# scripts;
- a multiplayer runtime API/framework.

UGUI components do appear in the old recovery scenes. These package candidates require a full serialized asset/scene and Package Manager dependency review before removal. `com.unity.inputsystem`, URP and Unity Test Framework are used by the current spherical code/project/tests and must be retained.

`com.unity.multiplayer.center` is not a networking implementation. Its presence or removal must not be used to infer a change to confirmed DEC-007 dedicated-server direction.

### CLEAN-F09 — Blender sync state

The Unity-side `BlenderFbxSync.cs` and README are present. The tool is paused by default and reads `Blender/config/project.json` plus manifest files under the Blender exports directory. The Blender source/config/manifests were not supplied.

`ProjectSettings/BlenderAssetSync/ownership.json` is present, but the current `BlenderFbxSync.cs` does not reference `ownership.json`. Two ledger entries have imported hashes matching root-level FBX copies rather than the listed `_ART_TEST` destinations. This may be stale ledger state or a migration artifact; the missing Blender source prevents a safe conclusion.

Recommendation: preserve the Blender sync tool, `_ART_TEST` assets, production FBX files, prefabs and ledger until the owner supplies the small relevant Blender config/manifests or explicitly decides to retire the pipeline.

## 5. Documentation discrepancies

1. `Documentation/06_AUDIT/REPOSITORY_AUDIT_REPORT.md` is based on `Pokemon-main.zip` from 2026-11-16 and describes an older 2D ecology codebase. Mark it `HISTORICAL`; do not use its old compile findings as current implementation evidence.
2. `Documentation/06_AUDIT/CODE_FUNCTION_OWNERSHIP_INVENTORY.md` references many old `Runtime/Ecology`, `Runtime/Pokemon` and `Runtime/Data` files absent from the supplied Assets snapshot. Rebuild it from the current snapshot or mark it historical.
3. `Documentation/03_ENGINEERING/TESTING_AND_VALIDATION.md` and `Documentation/07_Testing/TESTING_AND_VALIDATION.md` are byte-identical. Keep one canonical standard and make the other a pointer only after references are checked.
4. `Documentation/03_ENGINEERING/DEVELOPMENT_QUEUE.md` still says P0 is a CS7036 call from `SphericalWorldDemoBootstrap` to `SphericalPlayerController.Initialize`. In the supplied source snapshot, `SphericalWorldDemoBootstrap` no longer makes that call; `SphericalBiomeInspectionBootstrap` calls the current five-argument `Initialize` signature. This is a static source comparison only; Unity compilation was NOT RUN.
5. The queue omits the owner-approved WORLD-011-A sequence. The current owner instruction now prioritizes CLEAN-001 audit/purge before returning to WORLD-011-A; update the queue after the removal scope is approved.
6. `Documentation/01_ARCHITECTURE/UNITY_ARCHITECTURE.md` shows a `Runtime/Data` tree, while confirmed DEC-020 requires `Runtime/DataServices`. The supplied current source uses `Runtime/DataServices`.
7. `Documentation/03_ENGINEERING/MASTER_ROADMAP_V2.md` §7 still asks for a turn-based/real-time combat decision, while DEC-006 records real-time combat as CONFIRMED.
8. The new Constitution, Source Reference Index, Architecture Baseline, Master Roadmap, Module Preview/Test Standard and Deprecation/Cleanup Policy are marked `PROPOSED — owner approval required`. The project instructions refer to an approved Constitution. Reconcile the status metadata; do not silently treat proposed architecture/cleanup documents as approved.
9. `WORLD-011-A_ENVIRONMENT_ASSET_READINESS_AUDIT.md` says generated prefabs are absent. The current Assets archive contains the prefab generator and ten generated environment prefabs. Update the audit after Unity validation.

## 6. Validation performed

| Check | Result | Evidence |
|---|---|---|
| ZIP integrity | PASS | All six supplied ZIP archives passed `ZipFile.testzip()` |
| Assets `.meta` sidecar presence | PASS | All 173 non-`.meta` Assets files have a matching `.meta` |
| Static C# reference scan | PASS with findings | Planar generator calls are confined to its Editor tool/build processor; old scene components have no matching scripts in Assets |
| Generated mesh GUID reference scan | PASS with findings | No external GUID reference found for the eight generated mesh assets in supplied Assets YAML |
| External `Data` validator | FAIL | 12 dimension/origin/world-size mismatches between world definition and four layer manifests |
| StreamingAssets `PokemonData` validator | PASS | Supplied `Tools/validate_data.py` reports valid definitions/references/map grids |
| Unity compile/import | NOT RUN | No Unity Editor execution available |
| EditMode tests | NOT RUN | No Test Runner execution/export supplied |
| PlayMode tests | NOT RUN | No Unity Play Mode execution |
| Build | NOT RUN | No Unity build report |
| Blender round-trip | BLOCKED | Blender source/config/manifests omitted |

## 7. Recommended cleanup sequence

1. Owner approves the CLEAN-001 plan milestone and the explicit decision on WORLD-001 §9.
2. Batch 1: archive/remove `_Recovery` scenes and `World01.unity`; remove only after confirming the archive backup exists.
3. Batch 2: remove the eight unreferenced generated planar mesh assets and sidecars; keep the generated material only if the planar tool remains.
4. Batch 3: if approved, remove the planar generation pipeline as one coherent feature, including Editor window, runtime generator/build helper, duplicate Resources loader/config, canonical planar config and associated sidecars.
5. Batch 4: decide the fate of the external `Data/Tools` planar demo and `Assets/StreamingAssets/PokemonData`; preserve reusable schemas/species data unless explicitly retired.
6. Batch 5: review unused packages after old scenes are removed and serialized references are rechecked.
7. Repair/revalidate the spherical base: Unity import/compile, EditMode, PlayMode, independent previews, scene references and target build.
8. Update documentation, queue, indexes and decision records only after the owner approves the actual removal scope.

No project file was deleted or modified during this audit.
