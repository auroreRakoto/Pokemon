# Blender FBX Sync (Unity Editor)

`BlenderFbxSync.cs` is an Editor-only service. It watches the repository-level `Blender/exports/` directory and imports FBX files paired with `.manifest.json` sidecars.

## Behaviour

- Scans every 3 seconds while the Unity Editor is idle.
- Requires the FBX and manifest to have identical file stamps over two consecutive scans before processing.
- Uses only the explicit `unity.destination` from the manifest; the path is relative to `Assets/Project/Content/Models/` (the full canonical Models path is also accepted for compatibility).
- Rejects paths outside Models, unsupported domains, unknown production categories, unsafe path segments, missing manifests/FBX files and duplicate destinations.
- Allows `extra.test_only=true` only when the destination contains `_ART_TEST`.
- Replaces existing FBX files via a staged copy. It never copies, creates, edits or deletes `.meta` files.
- Imports only the target asset through `AssetDatabase`, then applies the centralized `ModelImporter` profile.
- Stores the last successful source signature in `EditorPrefs` to avoid repeated imports of unchanged files.
- Does not transfer production exports until `round_trip.status` is `VALIDATED` and the matching `validated_profile` is set in `Blender/config/project.json`.

## Controls

Unity menu: `Tools > Blender Sync > Rescan Now`, `Pause`, `Resume`.

## Initial importer policy

- Materials: import standard FBX material data into the model prefab so material slots and basic diffuse colours can be preserved. This does not create project `.mat` assets or recreate URP Shader Graphs.
- Static meshes: no animation or rig import.
- Skinned meshes: Generic rig, no animation clips.
- Animated meshes: Generic rig and animation import.
- Normals: import; tangents: calculate with MikkTSpace.
- Cameras/lights: disabled; blend shapes: enabled; mesh compression: off; Read/Write: off; mesh optimization: on.

These are initial centralized defaults, not proof that a profile passed the round-trip. Validate each profile before enabling production transfer.
