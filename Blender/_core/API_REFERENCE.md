# `_core` API reference

| Module | API principale | Responsabilité |
|---|---|---|
| `paths` | `ProjectPaths.discover()` | Racine repo, sources, exports, destination Unity |
| `config` | `load_json()` | Configuration JSON |
| `logging` | `get_logger()` | Logs homogènes |
| `naming` | `slugify()`, `validate_identifier()` | Identifiants stables |
| `collections` | `get_or_create_collection()` | Organisation des objets |
| `scene` | `select_only()` | Gestion de sélection |
| `transforms` | `apply_transforms()` | Application/inspection des transforms |
| `geometry` | `mesh_stats()` | Statistiques de géométrie |
| `materials` | `get_or_create_material()` | Matériaux Blender |
| `uv` | `ensure_uv_layer()` | Présence des UV |
| `textures` | `load_image()`, `pack_image()` | Chargement/packing des images |
| `rigging` | `validate_armature_bindings()` | Contrôles rig basiques |
| `animation` | `actions_for_objects()` | Découverte des actions/clips |
| `lod` | `lod_name()`, `find_lods()` | Conventions LOD |
| `collision` | `collision_name()` | Convention des meshes de collision |
| `sockets` | `find_sockets()` | Détection des empties SOCKET_ |
| `validation` | `validate_objects()` | Contrôles génériques pré-export |
| `manifest` | `build_manifest()`, `write_manifest()` | Manifeste JSON atomique |
| `export` | `export_fbx()` | Export FBX validé et staging |
| `versioning` | `pipeline_versions()` | Versions Blender/core/schema |

`_core` ne contient volontairement aucun générateur d'asset spécifique.
