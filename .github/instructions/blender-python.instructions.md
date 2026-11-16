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
