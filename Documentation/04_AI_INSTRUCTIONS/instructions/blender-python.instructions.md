---
applyTo: "**/*.py"
---
# Blender Python Instructions

- Reuse the shared Blender `_core` and package modules.
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
