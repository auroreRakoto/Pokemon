# ART-002 — First spherical forest trees

## Scope

This slice adds two deterministic Blender-generated static tree assets and a small runtime placement pass to the WORLD-002 playable biome inspection. It is a first visual integration, not the final scalable vegetation system.

## Assets

| Asset | Generator | Unity destination |
|---|---|---|
| Deciduous tree | `Blender/source/environment/tree_deciduous/generator.py` | `Assets/Project/Content/Models/Environment/Trees/SM_Tree_Deciduous.fbx` |
| Birch tree | `Blender/source/environment/tree_birch/generator.py` | `Assets/Project/Content/Models/Environment/Trees/SM_Tree_Birch.fbx` |

Both generators use the existing shared Blender `_core`, `project_modules.environment.generators.generate_asset`, static-mesh export profile, manifest contract and Blender-to-Unity sync. They export metres, with the object origin at the trunk base.

## Blender generation

1. Open `Blender/source/environment/generate_all.py` in Blender's Text Editor.
2. Keep `MODE = "PRODUCTION"`.
3. Set `RUN_BATCH = True`.
4. Keep `GENERATE_ALL = False`; the filtered list includes the two new trees and the existing environment assets. To generate only the trees, temporarily set `ASSET_FILTER = ["tree_deciduous", "tree_birch"]`.
5. Run the script. Confirm both FBX files and manifests are written under `Blender/exports/environment/`.
6. In Unity, use `Tools/Blender Sync/Rescan Now` (or resume the watcher) and confirm both models import to the exact destinations above.

Do not overwrite `project.json` or change the approved static-mesh profile as part of this task.

## Unity integration

On `WORLD-002_PlayableBiomeInspection`, in `Spherical Biome Inspection Bootstrap`:

1. Assign `SM_Tree_Deciduous` and `SM_Tree_Birch` (the imported FBX model assets) to `Forest Tree Prefabs`.
2. Set `Forest Tree Count` (initial default 140), minimum spacing (8 m), maximum slope (32°), and player exclusion radius (14 m) as desired.
3. Enter Play Mode. Trees are sampled deterministically from the same WORLD-002 geographic field set, placed only on dry-land Forest cells below the slope limit, aligned to the local radial up direction, and kept away from the player start.
4. Confirm both tree variants appear, sit on the spherical surface and remain within forest biome regions.

If no prefabs are assigned, the inspection world still runs and logs a warning. This avoids breaking existing scenes before the FBXs have been generated/imported.

## Known limitations

- The current placement pass creates individual GameObjects. It is intentionally limited to a configurable small count for prototype validation; it is not suitable for dense planet-wide forests on large worlds. A later chunked/instanced vegetation renderer is required for production scale.
- Tree collision is not generated in this slice.
- Blender and Unity are not available in the implementation container, so FBX generation, Unity import, play-mode rendering and C# compilation must be validated in the user's local applications.

## Validation checklist

- [ ] Both Blender generators run without errors.
- [ ] Both FBX files and manifests are created.
- [ ] Blender Sync imports both FBXs to their canonical destinations.
- [ ] Both prefabs are assigned in the inspection bootstrap.
- [ ] Play Mode places trees only on dry forest terrain.
- [ ] Trees align to the sphere and do not intersect the player spawn area.
- [ ] No console errors; placed count is reported.

## Commit guidance

Do not mark this slice complete until the local Blender and Unity validation checklist passes. If stopping before validation, use a commit message that describes the implemented-but-unvalidated tree generators and runtime placement, without claiming the feature is complete.
