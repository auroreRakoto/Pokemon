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

Each export should ideally include a manifest with asset ID, source path, export version, generator version, dependencies, units/scale, material references and intended Unity destination.
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

### Out of scope until validation is approved

No script may copy FBX files into Unity `Assets/`, edit/delete/create `.meta` files, or trigger Unity imports as part of generation. After the manual round-trip passes, define the FBX-to-Unity handoff separately (destination mapping, overwrite policy, `.meta`/GUID preservation, import settings, failure handling and ownership under TOOLS) before implementing it.
