# WORLD-011-A — Environment Asset Readiness Audit

- **Status:** PARTIAL — source/import pipeline audited; generated prefab assets still require live-project verification.
- **Audit date:** 2026-11-19
- **Source snapshot:** `Pokemon-clean (1).zip`, archive root `Pokemon-clean/`.
- **Scope:** production environment FBX inventory, Blender export manifests, Unity FBX import policy, and readiness evidence for spherical placement.

## 1. Executive summary

The repository snapshot contains 10 production environment FBX models and 1 test-only FBX. Their production manifests route them into the expected Unity `Models/Environment` hierarchy. The Unity FBX sync tool and its README define a controlled manifest-driven import pipeline and centralized ModelImporter defaults.

However, the archive contains **no `.prefab` files** under the environment content hierarchy, and it does not contain the separately delivered `EnvironmentPrefabGeneratorWindow.cs`. The user's later Unity prefab generation is therefore not represented in this snapshot. Prefab paths, root hierarchy, mesh bounds, pivot offsets, rendered dimensions, and collider configuration cannot be confirmed from this archive.

No placement runtime code should be implemented against assumed prefab paths until the current generated prefab folder is inspected.

## 2. Production asset inventory

| Category | Asset | Unity FBX path | Status |
|---|---|---|---|
| Ground | Leaf litter | `Assets/Project/Content/Models/Environment/Ground/Leaf_Litter/SM_Leaf_Litter.fbx` | Production FBX present |
| NaturalFeatures | Fallen log | `Assets/Project/Content/Models/Environment/NaturalFeatures/Fallen_Log/SM_Fallen_Log.fbx` | Production FBX present |
| NaturalFeatures | Red mushroom | `Assets/Project/Content/Models/Environment/NaturalFeatures/Mushroom_Red/SM_Mushroom_Red.fbx` | Production FBX present |
| NaturalFeatures | Tree stump | `Assets/Project/Content/Models/Environment/NaturalFeatures/Tree_Stump/SM_Tree_Stump.fbx` | Production FBX present |
| Rocks | Large stone cluster | `Assets/Project/Content/Models/Environment/Rocks/Stone_Cluster_Large/SM_Stone_Cluster_Large.fbx` | Production FBX present |
| Rocks | Small stone cluster | `Assets/Project/Content/Models/Environment/Rocks/Stone_Cluster_Small/SM_Stone_Cluster_Small.fbx` | Production FBX present |
| Vegetation | Short grass | `Assets/Project/Content/Models/Environment/Vegetation/Grass_Short/SM_Grass_Short.fbx` | Production FBX present |
| Vegetation | Tall grass | `Assets/Project/Content/Models/Environment/Vegetation/Grass_Tall/SM_Grass_Tall.fbx` | Production FBX present |
| Vegetation | Reed clump | `Assets/Project/Content/Models/Environment/Vegetation/Reed_Clump/SM_Reed_Clump.fbx` | Production FBX present |
| Vegetation | Wild grass | `Assets/Project/Content/Models/Environment/Vegetation/Wild_Grass/SM_Grass_Wild.fbx` | Production FBX present |

Test-only asset:
- `Assets/Project/Content/Models/Environment/Vegetation/_ART_TEST/Grass_Short_Test/SM_Grass_Short_Test.fbx`

The archive also contains duplicated historical/test copies under `_ART_TEST` and a few root-level legacy FBX files. They must not be treated as additional production assets.

## 3. Blender source and export mapping

Production environment source generators exist under `Blender/source/environment/<asset>/generator.py`. Production exports and manifests exist under `Blender/exports/environment/<asset>/`.

The batch entry point is `Blender/source/environment/generate_all.py`. Its current settings:
- `MODE = "PRODUCTION"`
- `RUN_BATCH = False` (safe/paused by default)
- `GENERATE_ALL = False`
- `INCLUDE_TEST_ASSETS = False`
- `ASSET_FILTER` lists the 12 production generator names, including `flower_blue` and `flower_yellow`.
- `PRODUCTION_ASSETS` defines canonical categories and destinations.

The source contains generator entries for `flower_blue` and `flower_yellow`, but this archive has no corresponding production FBX/manifests or Unity model imports. They are not part of the current 10-model production inventory.

Production manifests were found for all 10 listed production FBX assets. The manifest destinations match the Unity FBX paths shown in §2. The test-only grass manifest routes under `_ART_TEST` and sets `extra.test_only=true`.

## 4. Unity FBX import pipeline

### 4.1 Sync tool

- `Assets/Project/Editor/Tools/Blender/BlenderFbxSync.cs`
- `Assets/Project/Editor/Tools/Blender/README.md`
- Configuration: `Blender/config/project.json`

The sync tool:
- Scans `Blender/exports/` for `*.manifest.json`.
- Reads `asset_id`, `export`, and `unity` manifest sections.
- Accepts FBX only and validates source/destination paths.
- Rejects duplicate destinations, missing FBX files, invalid categories and unsafe paths.
- Requires stable FBX + manifest file stamps across consecutive scans.
- Requires production round-trip validation in `Blender/config/project.json` before transferring production exports.
- Imports only the target FBX and applies a centralized ModelImporter profile.
- Does not copy or mutate `.meta` files.

Unity menu controls: `Tools > Blender Sync > Rescan Now`, `Pause`, `Resume`.

### 4.2 Import defaults documented by the sync README

- FBX material data is imported into the model prefab; this does not create reusable project `.mat` assets or URP Shader Graph materials.
- Static meshes: no rig/animation import.
- Skinned meshes: Generic rig, no animation clips.
- Animated meshes: Generic rig and animation import.
- Normals: Import.
- Tangents: Calculate with MikkTSpace.
- Cameras/lights: disabled.
- Blend shapes: enabled.
- Mesh compression: off.
- Read/Write: off.
- Mesh optimization: on.

These are documented centralized defaults, not a substitute for checking the actual importer settings and model contents in the current Unity Editor.

### 4.3 FBX importer metadata in the snapshot

All 10 production FBX `.meta` files show:
- `globalScale: 1`
- `useFileScale: 1`
- `meshCompression: 0`
- `bakeAxisConversion: 0`
- `isReadable: 0`

This confirms the serialized importer settings in the archive only. It does **not** reveal the actual mesh dimensions, local pivot location, orientation, mesh hierarchy, or bounds.

## 5. Prefab generation status

The separately delivered prefab generator was intended to be installed at:
`Assets/Project/Editor/Tools/Prefabs/EnvironmentPrefabGeneratorWindow.cs`

Its menu entry:
`Tools > Assets > Generate Environment Prefabs`

Expected output root:
`Assets/Project/Content/Prefabs/Environment/`

**Neither the generator script nor any `.prefab` assets were present in the inspected project archive.** The user has since confirmed successful prefab generation, so this is a snapshot gap, not evidence that the current working project lacks prefabs.

The exact destination hierarchy and generated prefab names must be verified from the current Unity project. Do not infer that the generator output has been installed at the intended path or that every model successfully generated a prefab.

## 6. Readiness matrix

| Check | Result | Evidence / next action |
|---|---|---|
| Production FBX inventory | PASS | 10 production FBX files found |
| Production manifests | PASS | 10 matching production manifests found |
| Test-only separation | PASS | Test FBX and manifest route under `_ART_TEST` |
| Canonical category paths | PASS | Manifest destinations match Unity model hierarchy |
| Unity import pipeline documented | PASS | `BlenderFbxSync.cs` and README inspected |
| Serialized importer scale settings | PASS | All 10 production FBX metas use scale 1 / file scale |
| Blender source generators | PARTIAL | 10 production models mapped; flower generators have no production exports in this snapshot |
| Generated prefab output paths | BLOCKED | Prefabs absent from snapshot; current project archive needed |
| Prefab root/child hierarchy | BLOCKED | Must inspect generated prefabs in Unity/current project |
| Mesh pivot and bounds | BLOCKED | Need actual imported mesh data/Unity Editor or Blender inspection |
| Physical dimensions | BLOCKED | Need evaluated mesh bounds in Blender/Unity; FBX file size is not a dimension |
| Ground contact/orientation | BLOCKED | Requires visual/model-space inspection |
| Material slots and shader compatibility | PARTIAL | Import policy documented; actual current prefab material assignments require Unity inspection |
| Collider policy | BLOCKED | Generated prefabs absent; initial WORLD-011 recommendation is no blanket vegetation colliders |

## 7. Required next input to finish WORLD-011-A

Provide a fresh ZIP of the current Unity project containing:
- `Assets/Project/Content/Prefabs/Environment/`
- `Assets/Project/Content/Models/Environment/`
- the `.meta` files for these assets
- `Assets/Project/Editor/Tools/Prefabs/EnvironmentPrefabGeneratorWindow.cs` and its `.meta`, if installed
- `Blender/` source/export folders if they have changed since the inspected archive

A reduced ZIP containing those folders is sufficient; the whole Unity project is not necessary.

With that snapshot, finish the prefab-level audit:
1. Verify every production FBX maps to exactly one intended prefab.
2. Inspect prefab root and child transforms.
3. Inspect mesh bounds, pivot offsets, and local axes.
4. Compare asset dimensions and scale consistency.
5. Check material slots and missing references.
6. Check colliders and ensure vegetation does not receive unnecessary colliders.
7. Produce a correction list before WORLD-011-B/C implementation.

## 8. Audit conclusion

WORLD-011-A is **partially complete**: the source asset inventory, manifests, Blender generator entry point, and Unity import pipeline have been audited. The actual generated prefab audit remains open because those assets are absent from the available snapshot.

No placement implementation has been started. This avoids binding the future generator to unverified prefab paths or assumptions about pivot and scale.
