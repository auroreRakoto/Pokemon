# ART-ECORAKI-002 — Ecoraki Geometry Rebuild

Status: IMPLEMENTED — Blender runtime and visual acceptance pending.

## What changed
- Replaced the rejected primitive-stack generator with a silhouette-led geometry builder.
- Added custom swept tube geometry for the continuous ringed tail.
- Added cupped pointed leaf meshes for collar/crown, shaped ears with inset inner surfaces, facial mask/eye layers, compact quadruped legs/paws/toes, and golden botanical markings.
- Loads `Blender/config/project.json`; static FBX profile is mapped from config because `_core.ExportProfile.static_mesh()` currently accepts no config argument.
- Uses canonical bootstrap from saved Blender Text datablock path; no workstation-specific paths.
- Exports FBX + manifest to `Blender/exports/pokemon/ecoraki/`; Unity destination is test-only `_ART_TEST`.
- Rig and Idle are deliberately deferred until the mesh is visually approved.

## Validation
- Python syntax compilation passed.
- Blender executable is not available in this authoring environment; no Blender execution, FBX inspection or Unity import is claimed.
- Need user-side validation of silhouette, scale, normals, material colors, mesh integrity and export.

## Contract gap
`_core.manifest.build_manifest` currently places extra Pokémon fields under `extra`; domain docs list `species_id`, `export_version`, `units`, `scale`, and `profile` as required manifest fields. Profile is top-level in current manifest; remaining metadata is in `extra`. Do not silently alter the shared schema in this asset task.
