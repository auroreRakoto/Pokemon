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


## Blender generator bootstrap and `_core` discovery

Standalone Blender generators must run on any workstation after the repository is cloned or moved. They must not depend on a machine-specific absolute path, a manually configured Blender Python path, or the current working directory.

### Required user procedure (Blender Text Editor)

1. Keep the generator in its canonical repository location, for example `Blender/source/environment/grass_short_test/generator.py`. Do not run a copied version from `C:\`, Downloads, or a temporary folder.
2. In Blender, switch to the **Scripting** workspace.
3. In the Text Editor, choose **Text > Open**, then select the `generator.py` from the repository's `Blender/source/<domain>/<asset>/` folder. This is important: opening the file from disk gives Blender's Text datablock a real `filepath`.
4. If you have edited the script, use **Text > Save** (or `Alt+S`) before running it. Confirm that the text is saved to the canonical source folder, not an internal unsaved Text datablock.
5. Click **Run Script**. The bootstrap reads the saved Text Editor file path, walks upward through its parent directories, and locates the `Blender/` directory by checking for `_core/__init__.py`.
6. The bootstrap adds that discovered `Blender/` directory to `sys.path`, then and only then imports `_core`.
7. The generator can now call `ProjectPaths.discover(start=BLENDER_ROOT)` and load `Blender/config/project.json` as usual.

Do not run an unsaved internal Text block for a standalone generator. Do not rely on the `.blend` file location as the repository location: a `.blend` file may be stored outside the repository. Do not hardcode a path such as `C:/Users/...`, `C:/Downloads/Inputs`, or `/home/...`.

### Bootstrap implementation

Keep the generator's normal standard-library and Blender imports. In particular, imports used by the generator such as `math` and `traceback` must not be removed when adding the bootstrap. The following is a complete bootstrap pattern for a saved script opened in Blender's Text Editor; place it before every `_core` import:

```python
from pathlib import Path
import sys
import math
import traceback

import bpy
import bmesh


def _current_text_file_path() -> Path:
    """Return the saved path of the active Blender Text Editor script."""
    # Prefer the Text datablock shown in the active Text Editor.
    try:
        space = bpy.context.space_data
        text_block = getattr(space, "text", None)
        if text_block and text_block.filepath:
            path = Path(bpy.path.abspath(text_block.filepath))
            if path.is_file():
                return path
    except (AttributeError, TypeError, RuntimeError):
        pass

    # Fallback: search loaded external Text datablocks for a saved generator.
    candidates = []
    for text_block in bpy.data.texts:
        if not text_block.filepath:
            continue
        path = Path(bpy.path.abspath(text_block.filepath))
        if path.is_file() and path.name.lower() == "generator.py":
            candidates.append(path)

    # Do not guess if multiple generator.py files are open.
    unique_candidates = list(dict.fromkeys(path.resolve() for path in candidates))
    if len(unique_candidates) == 1:
        return unique_candidates[0]
    if len(unique_candidates) > 1:
        raise RuntimeError(
            "Several saved generator.py files are open. Activate the intended "
            "generator in the Text Editor and run it from there."
        )

    raise RuntimeError(
        "The active Blender Text block has no valid saved file path. "
        "Use Text > Open to open the generator from the repository, then save it."
    )


def find_blender_root(generator_path: Path) -> Path:
    """Walk upward from the saved generator until Blender/_core is found."""
    current = generator_path.resolve().parent
    for directory in (current, *current.parents):
        if (directory / "_core" / "__init__.py").is_file():
            return directory
        blender_dir = directory / "Blender"
        if (blender_dir / "_core" / "__init__.py").is_file():
            return blender_dir

    raise RuntimeError(
        "Cannot locate the project's Blender/_core directory. "
        f"Generator path checked: {generator_path}"
    )


GENERATOR_PATH = _current_text_file_path()
BLENDER_ROOT = find_blender_root(GENERATOR_PATH)
if str(BLENDER_ROOT) not in sys.path:
    sys.path.insert(0, str(BLENDER_ROOT))

# Shared project imports are deliberately after path discovery.
import _core
from _core.paths import ProjectPaths
from _core.collections import get_or_create_collection, move_objects_to_collection
from _core.materials import get_or_create_material, assign_material
from _core.export import ExportProfile, export_fbx
from _core.config import load_project_config
```

For an asset generator, retain all imports actually used by that file. For example, the `grass_short_test` generator uses `math` for geometry and `traceback` in its top-level exception handler. The bootstrap is not a replacement for the generator's imports; it only discovers the repository and prepares `sys.path`.

### Diagnostic procedure when discovery fails

If the bootstrap reports that it cannot locate `_core`:

1. In Blender's Text Editor, inspect the opened text and confirm it is the repository's saved `generator.py`.
2. Save it, then verify that the file is physically under `.../Blender/source/<domain>/<asset>/generator.py`.
3. Confirm that the same repository contains `Blender/_core/__init__.py`.
4. Close duplicate or stale `generator.py` Text datablocks, reopen the intended file with **Text > Open**, and run it again.
5. Do not fix the issue by adding a user-specific path. If the expected `_core/__init__.py` is absent, the repository checkout is incomplete or the wrong folder was opened.

### Rules for new and existing generators

- Put the bootstrap before **all** `_core` imports.
- Preserve all standard-library and Blender imports required by the generator (`math`, `traceback`, `bpy`, `bmesh`, etc.).
- Use the same discovery helper in every standalone generator, or use a shared launcher that establishes `sys.path` before loading generators.
- Never use the `.blend` file directory as the sole discovery source.
- Do not add the repository root or a user-specific folder permanently to Blender's global Python configuration.
- If `_core` cannot be found, stop with a clear error; do not silently fall back to another checkout.
- Keep `project_modules/` package imports separate: the bootstrap makes `Blender/` importable, after which package imports can use the documented package structure.

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

For Pokémon, the manifest should also carry the canonical `species_id`; TOOLS checks that the destination's species segment matches it. For non-Pokémon domains, the manifest must carry `asset_domain` and `asset_category`; for environment assets these are `Environment` and the canonical first-level category (`Vegetation`, `Ground`, `Rocks`, `Trees`, `Water`, `Structures`, or `NaturalFeatures`). TOOLS checks that they match the path. These fields describe and validate the explicit destination; they do not replace it.

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

A failure must not delete the source export or destroy a previously valid destination. A failed import is reported separately from a successful file copy. The tool must be able to retry on the next scan or explicit rescan without duplicating assets.

At the end of **every completed scan**, TOOLS writes exactly one concise summary line to the Unity Console, including when zero assets are found. The report distinguishes:

- `manifests found`: number of `*.manifest.json` files discovered in the export tree;
- `assets scanned`: number of manifests successfully parsed and validated into FBX candidates (each candidate identifies one asset to inspect/process);
- `invalid manifests`: discovered manifests rejected during parsing or validation;
- `imported/updated`: assets copied and imported during this scan;
- `unchanged`: assets already synchronized at the current source version;
- `waiting`: assets whose FBX/manifest have not yet been stable for two scans, or whose profile is awaiting validation;
- `failed`: candidates that failed due to duplicate destinations, copy errors or import errors.

Use a stable, searchable prefix such as `[Blender Sync] Scan report:`. Example:

`[Blender Sync] Scan report: 4 manifest(s) found, 3 asset(s) scanned, 1 invalid manifest(s), 1 imported/updated, 1 unchanged, 1 waiting, 0 failed.`

The report is a scan-level diagnostic, not a success claim for every discovered file. Invalid manifests are counted separately and are not included in `assets scanned`. A scan that cannot access the configured export directory or load its configuration must emit a clear warning/error explaining why no scan report could be completed.

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

## Environment batch generation (`generate_all.py`)

The canonical batch launcher is `Blender/source/environment/generate_all.py`. It is the supported entry point for generating multiple environment assets from Blender's Text Editor.

### User procedure

1. Open Blender and switch to the **Scripting** workspace.
2. In the Text Editor, use **Text > Open** and select `Blender/source/environment/generate_all.py` from the current repository checkout.
3. Save the file if you changed its configuration. Do not run an unsaved copy or a copy from Downloads/a temporary directory.
4. Review the configuration block at the top of the script:
   - `GENERATE_ALL = True` runs every discovered production generator.
   - `GENERATE_ALL = False` runs only the asset folder names listed in `ASSET_FILTER`.
   - `INCLUDE_TEST_ASSETS = False` excludes test generators by default. Set it to `True` only when deliberately running test assets such as `grass_short_test`.
5. Click **Run Script**. The launcher discovers its own saved Text Editor path, walks upward to find `Blender/_core/__init__.py`, adds the discovered `Blender/` directory to `sys.path`, and imports `_core` only after discovery.
6. Read the per-asset output and the final batch report in Blender's **System Console** (Windows: **Window > Toggle System Console**). A failed asset is reported, but does not prevent later generators from running.
7. Review the final counts and failure list before using the generated FBXs in Unity. The Unity synchronizer remains responsible for transferring/importing the FBXs; `generate_all.py` only generates them in `Blender/exports/`.

### Test assets versus production assets (promotion workflow)

`_ART_TEST` is reserved for temporary validation fixtures and prototypes. It is not a general destination for assets generated by a batch, and it must never become the default destination for production content. The batch launcher selects which generators to execute; it does **not** rewrite their destinations or promote test assets automatically. Each generator remains responsible for declaring its explicit `unity.destination` in its export call/manifest.

Use this workflow when an asset has completed its initial validation and is ready to become game content:

1. Keep a dedicated test generator (for example, `grass_short_test`) and its manifest destination under `Environment/_ART_TEST/...` only when the asset is still a test fixture. Set `extra.test_only: true`.
2. For the production asset generator (for example, `grass_short`), set `extra.test_only: false` or omit the flag (the production default), and change `unity.destination` to the canonical production category path. Do not copy the `_ART_TEST` path into a production generator.
3. Use the category mapping in the canonical model routing table. For example, short/tall grass, reeds and wild grass belong under `Environment/Vegetation/`; logs and stumps belong under `Environment/NaturalFeatures/`; rocks belong under `Environment/Rocks/`; trees belong under `Environment/Trees/`.

For the current procedural environment generator set, use these explicit production destinations (the asset-specific folder and FBX filename are part of the destination):

| Generator folder | `asset_category` | Production destination relative to `Models/` |
|---|---|---|
| `fallen_log` | `NaturalFeatures` | `Environment/NaturalFeatures/Fallen_Log/SM_Fallen_Log.fbx` |
| `flower_blue` | `Vegetation` | `Environment/Vegetation/Flower_Blue/SM_Flower_Blue.fbx` |
| `flower_yellow` | `Vegetation` | `Environment/Vegetation/Flower_Yellow/SM_Flower_Yellow.fbx` |
| `grass_short` | `Vegetation` | `Environment/Vegetation/Grass_Short/SM_Grass_Short.fbx` |
| `grass_tall` | `Vegetation` | `Environment/Vegetation/Grass_Tall/SM_Grass_Tall.fbx` |
| `leaf_litter` | `Ground` | `Environment/Ground/Leaf_Litter/SM_Leaf_Litter.fbx` |
| `mushroom_red` | `NaturalFeatures` | `Environment/NaturalFeatures/Mushroom_Red/SM_Mushroom_Red.fbx` |
| `reed_clump` | `Vegetation` | `Environment/Vegetation/Reed_Clump/SM_Reed_Clump.fbx` |
| `stone_cluster_large` | `Rocks` | `Environment/Rocks/Stone_Cluster_Large/SM_Stone_Cluster_Large.fbx` |
| `stone_cluster_small` | `Rocks` | `Environment/Rocks/Stone_Cluster_Small/SM_Stone_Cluster_Small.fbx` |
| `tree_stump` | `NaturalFeatures` | `Environment/NaturalFeatures/Tree_Stump/SM_Tree_Stump.fbx` |
| `wild_grass` | `Vegetation` | `Environment/Vegetation/Wild_Grass/SM_Grass_Wild.fbx` |

The `Flowers`, `Mushrooms`, and asset-specific folders above are organizational subfolders beneath the documented canonical category; they do not introduce new top-level category IDs. `asset_domain` must be `Environment`. `asset_category` must match the first category folder immediately below `Environment` (for example, `Vegetation`, `Ground`, `Rocks`, or `NaturalFeatures`).
4. Keep the destination explicit in the generator/manifest, including the asset-specific folder and `.fbx` filename. Example: `Environment/Vegetation/Grass_Short/SM_Grass_Short.fbx`. The path is relative to `Assets/Project/Content/Models/`.
5. Run the production generator or select it in `generate_all.py`. `GENERATE_ALL`, `ASSET_FILTER` and `INCLUDE_TEST_ASSETS` control generator selection only; they do not change `test_only` or the destination. `INCLUDE_TEST_ASSETS = False` excludes test-named generator folders, but does not detect or repair a production generator whose manifest still points to `_ART_TEST`.
6. Inspect the generated manifest before synchronization. A production manifest must not contain `_ART_TEST` in `unity.destination` and must not set `extra.test_only` to `true`. A test manifest must use an `_ART_TEST` destination and set `extra.test_only: true`. These two conditions must agree.
7. Run the Unity synchronizer and confirm the asset appears under its production category. Existing test copies are not automatically moved or deleted; clean them up manually only after confirming that no scene, prefab or other asset references them.

**Important:** enabling a generator in the production batch does not make its output a production asset. Production status comes from the generator's manifest metadata and canonical destination. TOOLS must reject contradictory combinations (for example, `_ART_TEST` with `test_only: false`, or a production path with `test_only: true`) rather than silently rerouting the file.

### Selection and test-asset convention

- Each generator is discovered from `Blender/source/environment/<asset_folder>/generator.py`.
- A folder is considered a test generator when its name ends in `_test`, starts with `test_`, or is one of `test`, `tests`, `_test`, `_tests`.
- Test generators are excluded unless `INCLUDE_TEST_ASSETS = True`.
- `ASSET_FILTER` entries are exact source folder names, not FBX names or asset IDs.
- If a requested filter entry has no matching generator, the launcher prints a warning rather than silently pretending it ran.
- Keep the test naming convention consistent; do not place production generators in folders marked as tests.

### Required batch report

Every run prints one final report, including when no generator was selected or all selected generators failed:

- `Discovered`: all `generator.py` files found under `source/environment/`.
- `Selected`: generators remaining after the all/filter selection and test exclusion.
- `Executed`: generators actually started.
- `Succeeded`: generators whose script completed without raising an exception.
- `Failed`: generators that raised an exception, with their names and error summaries.
- `Skipped`: discovered generators not executed, split into filter exclusions and test exclusions.

The launcher also prints the success and failure asset names. It must continue after an individual failure and must never report a failed generator as successful. A successful batch report means the Python generator scripts completed; it does not by itself prove that the FBX round-trip or Unity import is valid.

### Launcher responsibilities and limits

- `generate_all.py` owns discovery, selection, isolated script execution, and batch reporting.
- Individual `source/environment/<asset>/generator.py` files own their asset parameters, geometry/material generation, validation request, and FBX export.
- `_core` owns shared path/config/export/manifest behavior.
- Unity TOOLS owns the subsequent FBX transfer and Unity import.
- The launcher must not copy files into `Assets/`, create/edit `.meta` files, or bypass manifest validation.
- The launcher must use the same saved-Text-Editor-path bootstrap convention as standalone generators and must preserve required imports such as `traceback`.
