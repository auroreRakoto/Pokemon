# Art, Blender and Asset Pipeline

## Scope

Owns visual/audio asset production standards and reliable transfer from source creation to Unity.

## Source/export separation

- Blender source files and scripts are editable sources.
- Exports are generated outputs.
- Unity imported assets are runtime project content.
- Never edit an export as the only source of truth.
- Use explicit output directories; avoid writing to drive roots or arbitrary absolute paths.
- Export operations should be repeatable and report failures clearly.

## Suggested Blender structure

```text
Blender/
  _core/
  project_modules/
    pokemon/
    environment/
    collectibles/
    props/
  source/
    pokemon/<species>/
    environment/<category>/
    collectibles/<category>/
    props/<category>/
    materials/<category>/
  exports/
    pokemon/
    environment/
    collectibles/
    props/
  documentation/
```

`project_modules` contains package directories/modules, not unrelated root-level asset scripts. Individual asset generators belong under `source/<domain>/<asset>/`.

## Pokémon asset requirements

For each species, define as relevant:
- base mesh;
- material slots and texture references;
- armature/rig;
- skin weights;
- animation set;
- attachment sockets;
- colliders/interaction bounds metadata;
- LODs if needed;
- optional forms/dimorphism;
- audio/VFX references.

Not every species needs a bespoke rig if compatible rigs can be shared, but this must be validated.

## Size variants

Ordinary individual size variation uses one base mesh and a scale factor. Keep a stable gameplay root and scale `VisualRoot`. Ensure colliders, sockets, interaction points, navigation and camera framing are handled consistently.

## Environment asset requirements

Environment objects may expose interaction metadata:
- berry slots;
- shelter/rest points;
- nest points;
- perch points;
- hollow/log entrance;
- cover zones;
- resource harvest points.

Decorative objects do not need interaction sockets unless gameplay uses them.

## Texture/material standards

Define texture resolution, format, colour space, compression, tiling scale, naming, channel packing and shader compatibility per asset class. Do not assume every asset needs every PBR map. Stylized hand-painted/albedo-first workflows are valid.

## Naming

Use stable, readable identifiers. Suggested patterns:
- `SM_Pokemon_Chenipotte_Base`
- `MAT_Ground_Grass`
- `TEX_Ground_Grass_Albedo`
- `AN_Pokemon_Chenipotte_Idle`
- `VFX_Ability_<Name>_Impact`

Final naming convention must be consolidated with Unity import tooling before mass production.

## Validation

Before export/import, validate:
- object names and collection structure;
- transforms/origin/scale;
- normals and mesh integrity;
- material slots;
- rig and bone naming if applicable;
- socket naming;
- polygon/texture budgets;
- export path;
- manifest completeness.

## Manifest

Each export manifest must include at least: `asset_id`, `asset_domain`, `asset_category`, `source_path`, `export_version`, `generator_version`, `dependencies`, `units`, `scale`, `profile`, and explicit `unity.destination`. Pokémon exports must also include `species_id`; form and attachment exports include their corresponding stable IDs. Optional material references and asset-specific metadata may be added. The manifest schema is the contract between Blender and TOOLS; unknown or missing required fields are validation errors, not reasons to guess.
## Blender project configuration

The runtime configuration is `Blender/config/project.json` (the tracked template is `project.example.json`). It is the single source of truth for:

- source, export and Unity Models roots (project-relative paths only);
- scene units and scale convention;
- shared FBX export settings (unit application, scale mode, modifiers and smoothing);
- the `static_mesh`, `skinned_mesh` and `animated_mesh` profiles, including forward/up axes and animation flags;
- round-trip validation status.

Asset-specific parameters (for example blade count, dimensions, palette, seed, Pokémon species data or a particular Unity destination) stay in the asset generator. Do not duplicate global FBX settings in individual generators. A generator must load `project.json` and obtain its profile through `ExportProfile.*(config)`. Changes to global conventions must be made in the config and reflected in this document when they change the workflow.

## Blender → FBX → Unity round-trip validation

**Current status: PENDING.** There is not yet an automated FBX-to-Unity copy/import pipeline. The `unity.destination` value in a manifest is a planned destination only; it does not mean the file has been copied or imported. Do not implement automatic synchronization until the test below is completed and the import contract is approved.

### Goal

Confirm that the FBX profiles preserve orientation, scale, mesh integrity, materials and (for rigged assets) skeleton/animation data when a Blender asset is exported and imported into the project's exact Unity version. The current `-Z / Y` axis values are candidates, not approved settings.

### Test fixture

Create one disposable test scene/asset containing:

- a 1 m reference cube with clearly labelled local +X, +Y and +Z directions;
- an asymmetric mesh (not a sphere or cube alone), with a clearly identifiable front, top and right side;
- a ground plane and an origin marker;
- a simple material with a distinct colour;
- for the skinned/animated profile test, a two-bone armature and a short animation with visibly different start/end poses.

Apply/freeze object transforms as intended by the asset rules, keep the object origin at the documented pivot, and record the expected dimensions and facing direction before export. Do not use a production asset as the first test.

### Procedure

1. Record Blender version, Unity version (`6000.4.11f1`), profile name, configured `axis_forward` / `axis_up`, unit system and `scale_length` in the test report.
2. Generate the fixture and export it using the relevant profile from `Blender/config/project.json`. Keep the FBX and manifest together under `Blender/exports/round_trip_test/`; do not copy it automatically.
3. In Unity, import the FBX manually into a temporary test folder under `Assets/Project/Content/Models/_ART_TEST/`. Keep the default importer settings initially; record any setting that must be changed.
4. Place the imported object at world origin with rotation `(0,0,0)` and scale `(1,1,1)`. Compare its front/top/right directions with the Blender reference. Check for unexpected 90°/180° rotations, mirrored axes, inverted faces or changed handedness.
5. Compare measured dimensions against the Blender source. The 1 m reference must measure 1 Unity unit (1 m) unless the project explicitly adopts a different documented unit convention. Check both the FBX root transform and mesh child transforms; do not hide a scale error by compensating with a Unity prefab scale.
6. Inspect normals, visible faces, material slots and material assignment. Note unsupported shader/material features separately; FBX material transfer is not expected to recreate a Unity shader graph.
7. For the rigged profile, inspect bone hierarchy, bind pose, skin deformation and bone orientation. For the animated profile, play the imported clip and verify duration, frame rate, loop behaviour and start/end poses. Confirm whether NLA strips or Actions are the intended source.
8. If a setting fails, change only the relevant candidate setting in `project.json`, re-export the same fixture and repeat from a clean Unity test import. Do not change asset geometry to compensate for an exporter-axis or unit mismatch.
9. Repeat the successful test at least once after closing/reopening Blender and Unity to ensure the result is reproducible.
10. Remove the temporary Unity test asset only after recording results; retain the FBX, manifest, screenshots and report as test evidence.

### Acceptance criteria

A profile is validated only when all applicable checks pass:

- orientation is correct with no corrective root rotation;
- scale is correct with no compensating Unity scale;
- no mirrored geometry, inverted normals or missing faces;
- expected mesh objects and hierarchy are present;
- material slots are preserved sufficiently for the agreed material workflow;
- for rigged assets, hierarchy, bind pose and deformation are correct;
- for animated assets, the intended clip imports and plays correctly;
- a second export/import produces the same result.

Record the outcome per profile (`static_mesh`, `skinned_mesh`, `animated_mesh`), including exact Blender/Unity versions, tested axis pair, importer settings, screenshots, known limitations and pass/fail. Only then set `round_trip.status` to `VALIDATED` and `round_trip.validated_profile` to the tested profile in `project.json`. Validate each profile independently; success for static meshes does not validate rigged or animated meshes.

### FBX-to-Unity transfer contract

**Implementation owner: TOOLS (Unity Editor only).** Blender owns asset generation and FBX export. TOOLS owns discovery, validation, transfer, Unity import configuration and reporting. Runtime/domain modules must not depend on this editor workflow.

Automatic transfer is enabled only after the relevant FBX profile has passed the round-trip validation above. Until then, exports remain in `Blender/exports/` and are imported manually into `_ART_TEST`.

#### Canonical Unity asset architecture and destination mapping

The canonical Unity content root is `Assets/Project/Content/`. Content is separated by asset type first. The FBX synchronizer writes only under `Models/`; it must not decide or create destinations in the other content roots.

```text
Assets/Project/Content/
├── Models/                         # FBX-imported meshes and model hierarchies
│   ├── Pokemon/
│   │   └── <SpeciesId>/             # One stable folder per Pokédex species ID
│   │       ├── SM_Pokemon_<SpeciesId>_Base.fbx
│   │       ├── Forms/<FormId>/      # Only genuine geometry/model variants
│   │       └── Attachments/<AttachmentId>/ # Reusable species-specific geometry
│   ├── Environment/
│   │   ├── Trees/
│   │   ├── Vegetation/
│   │   ├── Rocks/
│   │   ├── Ground/
│   │   ├── Water/
│   │   ├── Structures/
│   │   └── NaturalFeatures/         # Logs, stumps, cliffs, caves, etc.
│   ├── Collectibles/
│   │   ├── Berries/
│   │   ├── Food/
│   │   ├── Materials/
│   │   └── QuestItems/
│   ├── Props/
│   │   ├── Furniture/
│   │   ├── Containers/
│   │   ├── Tools/
│   │   ├── Decorations/
│   │   └── Interactive/
│   └── Shared/                      # Reusable models with no single domain owner
├── Materials/                       # Unity Material assets (.mat)
│   ├── Pokemon/<SpeciesId>/
│   ├── Environment/<CategoryId>/
│   ├── Collectibles/<CategoryId>/
│   ├── Props/<CategoryId>/
│   └── Shared/
├── Textures/                        # Source/imported texture files
│   ├── Pokemon/<SpeciesId>/
│   ├── Environment/<CategoryId>/
│   ├── Collectibles/<CategoryId>/
│   ├── Props/<CategoryId>/
│   └── Shared/
├── Animations/                      # Unity animation clips/controllers when stored as assets
│   └── Pokemon/<SpeciesId>/
├── Prefabs/                         # Composed Unity objects, not raw Blender exports
│   ├── Pokemon/<SpeciesId>/
│   ├── Environment/<CategoryId>/
│   ├── Collectibles/<CategoryId>/
│   ├── Props/<CategoryId>/
│   └── Shared/
├── VFX/                             # Visual-effect assets and graphs
│   ├── Pokemon/<SpeciesId>/
│   ├── Abilities/<AbilityId>/
│   ├── Environment/<CategoryId>/
│   └── Shared/
├── Audio/                           # Audio clips and authored audio assets
│   ├── Pokemon/<SpeciesId>/
│   ├── Environment/<CategoryId>/
│   ├── UI/
│   └── Shared/
├── UI/                              # UI sprites, icons and other UI-only content
├── Data/                            # Content data assets; not imported by the FBX tool
└── _ART_TEST/                       # Temporary manual round-trip fixtures only
```

This is the canonical target architecture. It describes where new content must go; it does not claim every directory already exists in the repository. Create a directory only when its first asset is added. Do not create empty placeholder folders solely to mirror this tree.

##### Model routing rules (authoritative for TOOLS)

Every FBX manifest must contain an explicit `unity.destination`, relative to `Assets/Project/Content/Models/`. The destination is the complete path including the `.fbx` filename. TOOLS must not infer any part of it from the source path, filename, asset ID or Blender package name.

| Asset kind | Required destination pattern | Example |
|---|---|---|
| Pokémon base model | `Pokemon/<SpeciesId>/SM_Pokemon_<SpeciesId>_Base.fbx` | `Pokemon/Chenipotte/SM_Pokemon_Chenipotte_Base.fbx` |
| Pokémon form model | `Pokemon/<SpeciesId>/Forms/<FormId>/SM_Pokemon_<SpeciesId>_<FormId>.fbx` | `Pokemon/Chenipotte/Forms/Alternate/SM_Pokemon_Chenipotte_Alternate.fbx` |
| Pokémon-specific attachment | `Pokemon/<SpeciesId>/Attachments/<AttachmentId>/<filename>.fbx` | `Pokemon/Chenipotte/Attachments/LeafCocoon/SM_Leaf_Cocoon.fbx` |
| Environment model | `Environment/<CategoryId>/<filename>.fbx` | `Environment/Trees/SM_Tree_Oak.fbx` |
| Collectible model | `Collectibles/<CategoryId>/<filename>.fbx` | `Collectibles/Berries/SM_Berry_Oran.fbx` |
| Prop model | `Props/<CategoryId>/<filename>.fbx` | `Props/Containers/SM_Wooden_Crate.fbx` |
| Shared reusable model | `Shared/<CategoryId>/<filename>.fbx` | `Shared/Markers/SM_Spawn_Marker.fbx` |

`SpeciesId`, `FormId`, `AttachmentId` and `CategoryId` are stable, filesystem-safe identifiers, not display names. Use the canonical IDs already defined by the relevant domain/data contract. Do not translate, localize or change their spelling for presentation.

The supported category IDs are exactly the folder names listed in the tree above. If an asset does not fit one of them, do not put it in the nearest-looking folder and do not invent a new category in a manifest. Propose and document a category addition first, then update the validator's allow-list. `Shared` is reserved for assets genuinely reused across domains; it is not a fallback for uncertain classification.

##### Destination validation

TOOLS validates all of the following before copying:

- `unity.destination` is present, relative, normalized and uses `/` separators;
- it contains no drive prefix, root prefix, `..`, empty segment or path traversal;
- its first segment is one of `Pokemon`, `Environment`, `Collectibles`, `Props`, `Shared`;
- its category/species/form/attachment segments follow the routing table and use stable IDs;
- its extension is `.fbx`;
- its resolved destination remains inside `Assets/Project/Content/Models/`;
- it is not under `_ART_TEST`, `Editor`, `Resources` or another reserved path;
- no other manifest claims the same destination with a different `asset_id`;
- the destination is not already claimed by a different asset identity in the synchronization registry.

For Pokémon, the manifest should also carry the canonical `species_id`; TOOLS checks that the destination's species segment matches it. For other domains, the manifest should carry `asset_domain` and `asset_category`; TOOLS checks that they match the path. These fields describe and validate the explicit destination; they do not replace it.

Non-model content has its own owner and import workflow. Materials belong in `Materials/`, texture files in `Textures/`, animation controllers/clips in `Animations/`, composed Unity objects in `Prefabs/`, and VFX/audio in their corresponding roots. The FBX synchronizer must not create or move these assets as a side effect of importing a model. ModelImporter material extraction/creation must remain disabled unless a separately approved material workflow explicitly requires it.

#### Discovery and trigger

TOOLS runs as an Editor-only synchronization service while the Unity project is open. It periodically scans the configured `Blender/exports/` tree for FBX files with their matching manifests. It must not run in player builds or introduce runtime dependencies.

A candidate is processed only when both files exist and the FBX is stable/complete (for example, unchanged size and modification time across consecutive scans). The tool records processed file identity/version so it does not repeatedly import unchanged exports. A changed FBX or manifest is eligible for another synchronization. The user does not need to press a button for normal transfers; an explicit rescan command may be provided for recovery and diagnostics.

#### Copy and replacement

- Copy the FBX to the manifest destination, creating missing directories.
- If the destination FBX already exists, replace its contents. Do not create numbered duplicates or silently choose another destination.
- Stage the new file in a temporary file beside the destination and replace the destination only after the copy succeeds. A failed copy must leave the previous destination untouched.
- Never copy a `.meta` file from Blender. Unity owns `.meta` files and generates them when needed.
- When replacing an existing FBX, leave its existing `.meta` file untouched so Unity retains the asset GUID and existing references.
- After a successful copy, request an import/update through `AssetDatabase` for that exact asset path. Do not call broad project-wide refreshes for every asset if a targeted import is sufficient.

#### Import settings

TOOLS applies import settings through Unity Editor APIs (`ModelImporter` and an Editor-only `AssetPostprocessor`/import configuration service), not through Blender and not by editing `.meta` text directly. The manifest's validated profile (`static_mesh`, `skinned_mesh`, or `animated_mesh`) selects a centralized import profile. The same profile must produce the same settings on every import/reimport.

The import profile must explicitly configure, where applicable:

- model scale/conversion and mesh import;
- normals and tangents;
- material import policy;
- animation import, clip settings and rig type for skinned/animated assets;
- camera/light import (disabled unless a documented asset requires them);
- mesh optimization/compression and read/write policy only when justified by project runtime needs.

Do not guess final numeric or enum values before the matching Blender-to-Unity round-trip profile has passed. Record the tested values in the import profile and this document. Avoid per-asset Inspector tweaks: exceptions must be explicit in the manifest or a reviewed central override, with a clear reason. Unity's `.meta` remains the persistence mechanism for importer settings; TOOLS must not manually create, delete or rewrite it.

FBX material import does not recreate Unity shader graphs. The default policy should preserve mesh material slots and use Unity-managed materials; material creation/assignment is a separate explicit content step, not an implicit side effect of FBX transfer.

#### Errors and reporting

Every candidate must end in a clear result logged to the Unity Console, including asset ID, source FBX, destination and outcome. Use `Debug.Log` for success, `Debug.LogWarning` for recoverable issues and `Debug.LogError` for failed transfers/imports. Include the exception or actionable reason; do not silently skip assets.

A failure must not delete the source export or destroy a previously valid destination. A failed import is reported separately from a successful file copy. The tool must be able to retry on the next scan or explicit rescan without duplicating assets. A concise summary should report scanned, imported/updated, skipped-unchanged and failed counts.

#### TOOLS ownership boundary

TOOLS owns:

- scanning the external Blender export directory and pairing FBX files with manifests;
- manifest/schema/path validation and duplicate destination detection;
- safe copy, overwrite and retry behaviour;
- Unity `AssetDatabase` import triggering;
- deterministic `ModelImporter` settings based on the validated profile;
- Editor Console diagnostics and synchronization summary;
- optional Editor menu/status controls for pause, rescan and diagnostics.

TOOLS does not own:

- mesh generation, rigging, materials authoring or FBX export (Blender / Art);
- gameplay meaning or runtime asset selection (owning gameplay domains / Data);
- Unity runtime loading or player-build synchronization;
- editing `.meta` files directly or copying Blender-side `.meta` files;
- silently repairing invalid geometry or changing an asset's destination.

#### Acceptance criteria for the transfer tool

- a new valid export is automatically copied and imported to its explicit manifest destination;
- a changed export replaces the existing FBX without changing its GUID or breaking references;
- unchanged exports are not reimported on every scan;
- invalid/missing manifests, unsafe destinations, incomplete FBX files and copy/import failures produce actionable Console logs;
- a failed replacement leaves the previous valid destination intact;
- import settings are deterministic across first import and reimport;
- the tool is Editor-only and does not affect player builds.


### Implemented Editor synchronizer (initial version)

The first implementation lives at `Assets/Project/Editor/Tools/Blender/BlenderFbxSync.cs`. It is Editor-only and uses Unity's `EditorApplication.update` loop to scan the configured export directory every three seconds while the Editor is idle. A candidate must have an unchanged FBX and manifest file stamp over consecutive scans before it can be copied. This avoids importing a file while Blender is still writing it.

The synchronizer reads `Blender/config/project.json` (`paths.exports_root` and `paths.unity_models_root`) and the adjacent manifest. It uses `unity.destination` as the sole destination authority. For compatibility with existing test generators, it accepts either a path relative to Models or the full `Assets/Project/Content/Models/...` path; it normalizes both to a Models-relative path before validation. It never infers a production destination from the FBX filename.

Manifests marked `extra.test_only=true` may target a path containing `_ART_TEST`. Production destinations must follow the documented domain/category structure. Unknown production categories are rejected and logged; update this architecture before adding a new category. Duplicate destinations are rejected for every conflicting manifest.

The service replaces existing FBX files using a temporary sibling file and a backup/rollback strategy. It never transfers or edits `.meta` files. Unity creates and maintains `.meta` files; replacing the FBX at the same path is intended to preserve the existing GUID. The service then imports only the changed asset and applies the centralized `ModelImporter` profile. Unchanged exports are skipped using a signature stored in `EditorPrefs`.

The initial import policy imports standard FBX material data into the model prefab, preserves material slots and basic diffuse colours, disables camera/light import, imports normals, calculates MikkTSpace tangents, enables blend shapes, disables mesh compression and Read/Write, and enables mesh optimization. Static meshes do not import animation; skinned meshes use a Generic rig without clips; animated meshes use a Generic rig with animation import. These defaults must be checked against the project's actual assets. FBX import does not create reusable Unity `.mat` assets or recreate URP Shader Graphs.

The synchronizer logs success, unchanged, waiting and failure outcomes to the Unity Console. It exposes `Tools > Blender Sync > Rescan Now`, `Pause` and `Resume`. It does not run in player builds.

**Safety gate:** `extra.test_only=true` assets can be synchronized into `_ART_TEST` while the material round-trip is being debugged. Production exports remain blocked until `round_trip.status` is `VALIDATED` and `round_trip.validated_profile` matches the profile being imported. Set and validate profiles independently.

### Material colour troubleshooting

The shared Blender material helper must set both `Material.diffuse_color` and the Principled BSDF `Base Color`. Setting only the viewport diffuse colour is not sufficient evidence that the FBX contains the intended material colour. After updating the helper, regenerate a test asset, confirm its FBX imports with the expected basic diffuse colour, and only then mark the static material round-trip as passed. Unity's final URP material/shader authoring remains a separate content task.
