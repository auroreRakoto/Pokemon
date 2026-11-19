# CLEAN-001 — Legacy Feature Inventory

- **Status:** PROPOSED — removal decisions pending owner approval
- **Snapshot:** supplied archives dated 2026-11-19
- **Purpose:** classify legacy candidates without deleting useful or approved work.

## Classification

| ID | Candidate | Classification | Evidence | Recommendation |
|---|---|---|---|---|
| CLEAN-F01 | `Assets/_Recovery/0*.unity` and `.meta` | HISTORICAL / UNREFERENCED | Six recovery scenes; old missing scripts; no Build Settings references; three byte-identical copies | Archive outside Unity, then remove all six |
| CLEAN-F02 | `Assets/Project/Content/Scenes/Worlds/World01.unity` and `.meta` | OBSOLETE candidate | Old planar scene; four missing custom scripts; not in Build Settings; no external references found | Remove after backup |
| CLEAN-F03 | Eight meshes under `Assets/Generated/Worlds/` and `.meta` | UNREFERENCED generated outputs | 116.1 MiB total; no external GUID references in supplied Assets YAML | Remove after Unity reference verification |
| CLEAN-F04 | `Assets/Resources/WorldGeneration/world_generation.json` and `WorldGenerationConfig.cs` | DUPLICATE / UNREFERENCED | JSON byte-identical to canonical `Assets/Project/Data` config; loader has no caller | Remove with planar pipeline or separately approved cleanup |
| CLEAN-F05 | `Assets/Project/Runtime/World/Generation/` planar generator and Editor tooling | OBSOLETE candidate / APPROVED-WP CONFLICT | Only current caller is planar Editor window; no spherical consumers; WORLD-001 §9 explicitly preserves it | Owner must decide whether to supersede WORLD-001 §9 |
| CLEAN-F06 | External `Data/` + `Tools/` demo | HISTORICAL / UNCONNECTED | 2D planar map generator and validator; external Data validator currently fails 12 metadata consistency checks | Archive or regenerate; preserve species/schema samples until POK/ECO data decision |
| CLEAN-F07 | `World02.unity` | GENERATED / REGENERABLE | 39.5 MB Planet Studio/drainage preview; editor tool has Generate/Clear operations | Preserve until regenerated and inspected in Unity |
| CLEAN-F08 | `com.unity.ai.navigation`, `timeline`, `visualscripting`, possibly `ugui` | UNKNOWN package cleanup candidates | No direct current C# API use; UGUI serialized components remain in recovery scenes | Review after scene purge and Package Manager dependency check |
| CLEAN-F09 | `ProjectSettings/BlenderAssetSync/ownership.json`, root-level FBX copies and `_ART_TEST` FBX | UNKNOWN | Current Unity sync code does not use ownership ledger; Blender sources/manifests not supplied | Preserve until Blender-side audit |

## Preserve — not legacy purge candidates

| Area | Paths | Reason |
|---|---|---|
| Spherical world runtime | `Assets/Project/Runtime/World/Spherical/` | Current world direction and preview/runtime contracts |
| Spherical Editor tools | `Assets/Project/Editor/World/Spherical/` | Planet Studio, diagnostics and reproducible preview tooling |
| Spherical preview scenes | `SphericalWorldDemo.unity`, `WORLD-002_BiomeInspection.unity` | Current independent preview routes |
| World profiles | `Assets/Project/Data/WorldProfiles/` | DEC-046 versioned spherical world profiles |
| Planet Studio settings | `Assets/Project/Content/World/PlanetStudio/` | Current spherical configuration/material mapping |
| Environment placement | `Assets/Project/Runtime/World/Spherical/Environment/` and `Assets/Project/Content/Settings/World/EnvironmentPlacement/` | Current WORLD-011 placement path |
| Environment prefabs/catalog | `Assets/Project/Content/Prefabs/Environment/`, `CAT_EnvironmentPrefabs.asset` | Ten prefabs referenced by the placement catalog |
| Production FBX and materials/textures | `Assets/Project/Content/Models/Environment/`, `Materials/Environment/`, `Textures/Environment/Biomes/` | Current art/placement inputs |
| Blender sync | `BlenderFbxSync.cs` and README | Active Editor-side import workflow; Blender source audit incomplete |
| Input System, URP, Test Framework | Packages and project settings | Used by current spherical code, rendering and test assemblies |
| DEC-007 | Dedicated-server direction | Confirmed decision; no cleanup may silently cancel it |

## Important owner decision

`WORLD-001_SPHERICAL_WORLD_FOUNDATION.md` §9 says the planar `WorldGenerator` and `WorldTerrainBuilder` must remain untouched and continue to serve the planar test workflow.

The new owner direction requests a broad purge of old functionality. Removing CLEAN-F05 therefore requires an explicit decision that the new cleanup direction supersedes WORLD-001 §9. Until then, the planar feature is a removal candidate, not an approved deletion.
