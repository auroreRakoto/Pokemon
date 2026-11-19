# STD-003 — Python Code Compliance Refactor

- **Status:** PROPOSED
- **Owner module:** Tools / Art
- **Priority:** P1
- **Dependencies:** STD-001 plan committed and pushed; owner approval of this work package; active-vs-legacy script inventory
- **Target milestone:** project-authored Python source conforms to Pokémon v2 Code Standard 1.0

## 1. Goal

Bring project-authored Python source into compliance with the same three mandatory rules while preserving Blender workflows, generated assets and data outputs.

## 2. Scope

### Included
- Audit all 45 Python files in the supplied snapshot.
- Identify active, legacy, experimental and generated scripts before editing.
- Refactor active project-authored files exceeding 10 functions or 50 logical code lines.
- Add concise descriptions above every declared function.
- Preserve script entry points, output paths, Blender scene assumptions and `_core` reuse.

### Explicitly excluded
- Deleting `Blender/old` or other legacy files.
- Rewriting asset generators or changing generated geometry/materials.
- Changing export formats, manifests or output locations.
- Adding third-party Python dependencies.
- Modifying Unity C# source or serialized Unity assets.

## 3. Initial static findings

- `Blender/_core/paths.py` — 12 functions.
- `Blender/old/Environment_Assets.py` — 17 functions; legacy status must be confirmed before any edit.
- `Blender/project_modules/environment/generators.py` — 12 functions.
- Functions exceeding 50 scanned code-bearing lines were flagged in:
  - `Blender/_core/export.py`;
  - `Blender/old/Pokemon_Grass_Assets.py`;
  - `Blender/source/environment/grass_short_test/generator.py`;
  - `Tools/generate_demo_maps.py`;
  - `Tools/validate_data.py`.

The scan is approximate; recalculate logical code lines using the adopted standard before declaring a violation.

## 4. Validation plan

- Parse all Python source with the Python standard library.
- Run `py_compile` on supported active scripts.
- Run available unit/integration checks if present.
- For Blender scripts, owner runs the relevant generator in the target Blender version and checks output collections, object transforms, manifests and export results.
- Compare generated outputs and file paths against the pre-refactor baseline.
- Record legacy files as `N/A` only with a reason and explicit disposition; do not silently delete or exempt them.

## 5. Acceptance criteria

- [ ] No active project-authored Python file exceeds 10 declared functions, unless an owner-approved exception is recorded.
- [ ] No active project-authored Python function exceeds 50 logical code lines, unless an owner-approved exception is recorded.
- [ ] Every function has a concise description.
- [ ] Python syntax checks pass for all supported active scripts.
- [ ] Blender generation/export checks are run by the owner and reported exactly.
- [ ] Legacy scripts remain intact unless a separate approved cleanup work package authorizes removal.
