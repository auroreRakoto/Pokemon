# WORLD-002 — Asset Readiness Checklist

**Status:** companion checklist to the owner-approved/pushed WORLD-002 plan.

This checklist reflects the supplied `Pokemon-WORLD-002-spherical-geographic-fields-plan-v1.zip` snapshot. Re-audit the active branch before asset creation.

## Existing imported models found

| Category | Asset | Intended preview use | Notes |
|---|---|---|---|
| Vegetation | `Grass_Short/SM_Grass_Short.fbx` | Short ground cover | Prefab/material still required |
| Vegetation | `Grass_Tall/SM_Grass_Tall.fbx` | Taller grass | Prefab/material still required |
| Vegetation | `Wild_Grass/SM_Grass_Wild.fbx` | Natural grass variation | Prefab/material still required |
| Vegetation | `Reed_Clump/SM_Reed_Clump.fbx` | Wet/water-edge vegetation | Prefab/material still required |
| NaturalFeatures | `Fallen_Log/SM_Fallen_Log.fbx` | Forest-floor prop | Not a plant |
| NaturalFeatures | `Tree_Stump/SM_Tree_Stump.fbx` | Forest-floor prop | Not a tree |
| NaturalFeatures | `Mushroom_Red/SM_Mushroom_Red.fbx` | Small natural feature | Prefab/material still required |
| Ground | `Leaf_Litter/SM_Leaf_Litter.fbx` | Ground detail prop | Not a terrain material |
| Rocks | `Stone_Cluster_Small`, `Stone_Cluster_Large` | Rock dressing | Prefabs/materials still required |

Blender generators exist for blue and yellow flowers, but production FBX files were not found in the inspected imported Environment model tree. `_ART_TEST` assets remain test-only.

## Missing or unconfirmed in the inspected snapshot

- At least two distinct tree models/prefabs for a representative forest canopy preview.
- Ground materials/textures for plains/grass, forest floor and desert/dry ground.
- Water presentation material.
- Unity prefabs for every selected vegetation instance.
- Biome-to-material mapping assets/configuration.
- Optional LOD/billboard representations for large-scale vegetation.

No environment `.mat`, texture-image or prefab files were found under the canonical `Content/Materials/Environment/`, `Content/Textures/Environment/` and `Content/Prefabs/Environment/` roots in the inspected snapshot. Search the active branch and other canonical locations before creating duplicates.

## Exhaustive catalogue

Use `WORLD-002_EXHAUSTIVE_VISUAL_ASSET_CATALOG.md` as the complete scoped inventory for WORLD-002's visual validation preview. The checklist below is an operational readiness checklist, not a substitute for that itemised catalogue.

## Mandatory approval gate

**Do not launch production visualisation generation until the project owner has reviewed and explicitly approved the complete asset list.** WORLD-002 plan approval alone is not asset-list approval. Record asset key/ID, role, intended biome/use, source generator, Unity destination, status, owner/action, exact path/GUID and approval status. Missing assets must have an explicit decision: Blender generation, reuse, or labelled temporary placeholder.

The approved Blender-generated assets must be registered in `Blender/source/environment/generate_all.py`: each needs a discoverable `source/environment/<asset_key>/generator.py`, an entry in `PRODUCTION_ASSETS`, and inclusion in `ASSET_FILTER` when filtered runs are used. The production batch must run a preflight against the approved inventory and fail closed on missing generators/routes, invalid destinations, unresolved status, or unapproved assets. `GENERATE_ALL=True` must never mean that every discovered generator is automatically approved. Unity prefab/material composition remains an explicit Unity step and must not be performed implicitly by FBX synchronization.

## Required order

1. Re-audit active branch and record exact asset paths/GUIDs.
2. Select the four existing vegetation candidates: short grass, tall grass, wild grass, reeds.
3. Source/create at least two tree models before claiming forest canopy preview is representative.
4. Prepare preview ground materials for plains, forest and desert/dry ground, plus water.
5. Compose Unity prefabs under `Assets/Project/Content/Prefabs/Environment/`; keep raw FBX under `Models/Environment/`.
6. Follow `ART_AND_ASSET_PIPELINE.md`; do not let FBX synchronization create materials/prefabs implicitly; do not promote `_ART_TEST` assets manually.
7. If production assets are missing, use visibly labelled temporary placeholders for validation and record the gap. Field generation and map export must not be blocked by missing art.

## Acceptance

- Every enabled vegetation instance references a stable prefab and is placed deterministically from the generated fields.
- A forest canopy preview uses at least two distinct tree prefabs, or is explicitly marked placeholder/incomplete.
- Plains, forest, desert/dry ground and water have distinguishable preview materials.
- Missing assets and required owners/actions are listed in the WORLD-002 implementation report.


## Blender batch registration requirements

Before a production batch is run, verify the approved inventory against:
- `Blender/source/environment/<asset_key>/generator.py`;
- `ASSET_FILTER` for any filtered run;
- `PRODUCTION_ASSETS` for the canonical category and Unity FBX destination;
- the generator's stable asset ID, manifest metadata and batch policy;
- the exported FBX/manifest and the Unity import/sync result.

The batch preflight must list approved, discovered, registered, missing and unapproved asset keys. It must stop production generation if any required approved asset is not fully registered, and it must skip unapproved discovered generators. Test assets remain isolated under `_ART_TEST`.

