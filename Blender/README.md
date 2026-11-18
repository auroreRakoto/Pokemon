# ART Pokémon starters — v0.2.0

Updated against the current repository Blender instructions and ART asset pipeline contract.

## Contents
- `project_modules/pokemon/procedural.py`: shared procedural mesh, rig, Idle and config-driven export helpers.
- `source/pokemon/ferilume/generator.py`
- `source/pokemon/nacoryn/generator.py`
- `source/pokemon/ecoraki/generator.py`

## Required run procedure
1. Merge this `Blender/` subtree into the repository. Do not replace `_core` or `config`.
2. In Blender Text Editor use **Text > Open** to open the saved generator directly from `Blender/source/pokemon/<species>/generator.py`.
3. Save it, then click **Run Script**. Unsaved internal text blocks are not supported.
4. The bootstrap locates `_core` from the active Text datablock's saved path, then adds the Blender root to `sys.path`.
5. Each generator loads `Blender/config/project.json`. FBX profile axes, scale and animation flags are not hardcoded in species scripts.

## Exports and round-trip gate
Exports go to `Blender/exports/pokemon/<species>/`. Because the supplied `project.json` only validates `static_mesh`, the starter animated exports are routed to `Assets/Project/Content/Models/Pokemon/_ART_TEST/<Species>/...` in their manifests. They are not copied there by Blender. TOOLS must manually import and validate the animated profile; production destinations must not be enabled until `round_trip.validated_profile` is `animated_mesh`.

## Manifest
The generator supplies `asset_domain=Pokemon`, `asset_category=BaseSpecies`, and species ID, export version, units and scale metadata in `extra`. The current `_core.manifest.build_manifest` places species-specific extras under `extra`; it does not expose `species_id`, `export_version`, `units` or `scale` as top-level fields. This is recorded as a core/manifest contract gap for ART/TOOLS follow-up.

## Known compatibility note
The checked-in `_core.ExportProfile.animated_mesh()` takes no config parameter, despite the domain instruction's desired `ExportProfile.*(config)` interface. This package uses a small adapter to instantiate `ExportProfile` from the validated `project.json` values without hardcoding those settings in species generators. A future `_core` revision should align the API with the documentation.
