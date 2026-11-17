---
applyTo: "**/*.py"
---
# Blender Python Instructions

- Reuse the shared Blender `_core` and package modules.
- For standalone generators run from Blender's Text Editor, give the user the exact procedure: keep the script in `Blender/source/<domain>/<asset>/`, use **Text > Open** to open that saved file, save it, then click **Run Script**. Discover the active Text datablock's saved `filepath`, walk upward until `Blender/_core/__init__.py` is found, add that `Blender/` directory to `sys.path`, and only then import `_core`. Do not rely on `__file__`, the current working directory, or the `.blend` file location alone; never hardcode a machine-specific absolute path. Preserve all generator imports such as `math` and `traceback`. If the saved script path or `_core` cannot be found, fail clearly rather than using another checkout.
- `project_modules` contains package/folder names and reusable modules, not asset-specific root scripts.
- Keep asset-specific generators under `source/<domain>/<asset>/`.
- Keep source and export paths separate.
- Never write to a drive root or assume a user-specific absolute path.
- Centralize configuration and output paths.
- Validate collections, transforms, materials, geometry and export destinations.
- Make generators repeatable and report errors clearly.
- Load shared paths and FBX settings from `Blender/config/project.json`; never hardcode global axis, scale or exporter settings in an asset generator.
- Keep asset-specific geometry parameters, seeds and intended Unity destinations in the relevant `source/<domain>/<asset>/generator.py`.
- Use `ExportProfile.static_mesh(config)`, `skinned_mesh(config)` or `animated_mesh(config)` for configured FBX profiles.
- Treat FBX axis values as unvalidated until the documented Blender → FBX → Unity round-trip passes for that profile.
- Do not implement FBX copying/import or `.meta` handling before the round-trip and handoff contract are approved.
- Do not commit Python cache artifacts (`__pycache__/`, `*.py[cod]`).

## Environment batch launcher requirements

When modifying or creating `Blender/source/environment/generate_all.py`:

- Treat it as the canonical batch entry point for environment generators.
- Discover `Blender/` from the saved active Text Editor script path and locate `_core/__init__.py` before importing `_core`; never hardcode a workstation path or rely on the current working directory.
- Preserve all required imports, including `traceback` for per-generator exception reporting.
- Support `MODE` (`PRODUCTION`, `TEST`, `CUSTOM`), `GENERATE_ALL`, `ASSET_FILTER`, and `INCLUDE_TEST_ASSETS` as documented in `ART_AND_ASSET_PIPELINE.md`.
- Exclude test-named generators in `PRODUCTION` mode regardless of `INCLUDE_TEST_ASSETS`; in `TEST`/`CUSTOM`, include them only when explicitly enabled.
- Execute each generator in an isolated namespace, continue after individual failures, and print a final report with discovered, selected, executed, succeeded, failed, and skipped counts plus asset names.
- Keep generation/export in Blender. Do not copy FBXs into Unity or modify `.meta` files from the batch launcher.
- Keep the user procedure and report definitions synchronized with `ART_AND_ASSET_PIPELINE.md`.

## Test and production destination rules

- `_ART_TEST` is only for temporary fixtures/prototypes; never leave a production generator targeting `_ART_TEST`.
- The batch launcher injects `ART_BATCH_POLICY` into each isolated generator namespace. Individual generators must pass it to `resolve_batch_policy(...)` from `project_modules.environment.generators`.
- `PRODUCTION` must override individual destinations/status using the central `PRODUCTION_ASSETS` table and force `test_only=False`; unknown assets must fail before export.
- `TEST` must force an `Environment/<asset_category>/_ART_TEST/<asset_key>/<object_name>.fbx` destination and `test_only=True`.
- `CUSTOM` preserves declared generator values but validates destination safety and consistency between `_ART_TEST` and `test_only`.
- Standalone generator execution has no batch context and uses its declared fallback values. Do not mutate generator source files or globals to apply batch policy.
- Do not delete old test imports automatically.
- Choose the production category from the documented routing table (for example, grass/reeds/wild grass -> `Environment/Vegetation`; logs/stumps -> `Environment/NaturalFeatures`; rocks -> `Environment/Rocks`; trees -> `Environment/Trees`).


## Production and test destination rules

- Production environment generators must use an explicit canonical `unity.destination` under `Assets/Project/Content/Models/Environment/<CategoryId>/...`; never retain `_ART_TEST` after promotion.
- Production calls to `generate_asset(...)` must pass `test_only=False` and the explicit `asset_category`. The shared helper defaults `test_only` to `False`; test fixtures must opt in with `test_only=True`.
- Every environment FBX manifest must include `asset_domain: "Environment"` and the canonical first-level `asset_category` matching its destination.
- Keep dedicated test generators (for example `grass_short_test`) under `_ART_TEST` with `test_only=True`; do not include them in normal batch runs.
- Keep the central production route table in `generate_all.py` synchronized with the canonical routing table in `ART_AND_ASSET_PIPELINE.md`.
