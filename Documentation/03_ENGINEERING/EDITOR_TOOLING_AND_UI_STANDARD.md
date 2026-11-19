# Editor Tooling and UI Standard

- **Status:** PROPOSED — owner approval required
- **Version:** 1.0
- **Purpose:** make all developer tools discoverable, consistent, safe and script-driven.

## 1. Two distinct UI categories

Do not mix these:

1. **Developer tools:** Unity Editor windows and menu commands used to generate, inspect, validate and debug content.
2. **Player UI:** runtime HUD, menus, inventory, party, map, settings and interaction screens.

They have different users and standards. A developer debug overlay must not become a player-facing menu by accident.

## 2. Canonical Editor menu root

Proposed menu convention:

```text
Tools
└── Pokemon
    ├── Project Dashboard
    ├── World
    │   ├── Planet Studio
    │   ├── Biome Preview
    │   ├── Environment Placement Preview
    │   └── Validate World
    ├── Content
    │   ├── Generate Environment Prefabs
    │   ├── Validate Prefabs
    │   └── Blender Sync
    ├── Testing
    │   ├── Open Test Runner
    │   ├── Run Project Validation
    │   └── Generate Test Report
    └── Diagnostics
        ├── Project Health
        ├── Asset Reference Audit
        └── Documentation / Feature Index
```

This is a target structure. Do not rename all current menu paths in one risky sweep. Migrate tools one at a time and retain compatibility aliases temporarily if needed.

## 3. Project Dashboard

The dashboard should become the discoverable entry point for project development.

Minimum sections:
- project/Unity version and repository status if available;
- active roadmap milestone and current work package;
- module status (IDEA/PROPOSED/IN_PROGRESS/VALIDATED/ACCEPTED);
- quick links to canonical documentation;
- validation shortcuts;
- current compile/test/asset validation summary;
- blocked items and missing prerequisites.

The dashboard must not duplicate authoritative data. Read from project configuration/documents where practical; otherwise label manually maintained status clearly.

## 4. Standard EditorWindow layout

Every substantial tool should use the same information hierarchy:

1. **Header:** tool name, owning module, purpose, status.
2. **Input:** source assets, configuration/profile, seed and target.
3. **Actions:** Generate/Refresh, Validate, Clear/Reset.
4. **Preview:** visible output or concise data visualization.
5. **Diagnostics:** errors, warnings, skipped items, counts and timing.
6. **Help:** short explanation and link/path to the owning documentation.

Use consistent labels and action placement. Destructive operations require confirmation and must report what will be changed.

## 5. Safe operation requirements

All generation tools must:
- validate prerequisites before writing;
- state exact output paths;
- avoid overwriting unrelated assets;
- be idempotent;
- support dry-run where file/asset replacement risk is meaningful;
- report created/updated/skipped/failed counts;
- catch and display actionable errors;
- avoid direct `.meta` file manipulation;
- preserve Unity GUIDs when replacing assets in place;
- never silently create duplicate scenes, prefabs or ScriptableObjects.

A failed operation must leave the previous valid output intact where practical.

## 6. Preview UX

Every visual preview should expose:
- fixture/profile;
- seed;
- resolution or sample size;
- regenerate;
- clear;
- validation;
- camera/view controls when relevant;
- diagnostic overlays toggle;
- summary statistics.

Do not expose dozens of raw implementation fields by default. Group settings into meaningful sections and provide sensible defaults.

## 7. Runtime debug overlay standard

Runtime diagnostics are allowed in development builds and should be disabled or hidden in release builds.

Common overlay:
- current world/profile/seed;
- player position and local surface normal;
- biome/water class;
- module generation status;
- object/instance counts;
- generation time;
- selected entity identity and state;
- warnings/errors.

Each domain may add a small domain-specific panel, but shared metrics should use common labels and units.

## 8. Player UI standard

Player-facing UI should be designed separately and introduced when the relevant gameplay milestone is reached.

Minimum rules:
- one consistent navigation model;
- clear input prompts;
- keyboard/controller support where required by the actual target;
- readable hierarchy and consistent spacing;
- modal dialogs have clear confirm/cancel behaviour;
- menus pause or do not pause gameplay intentionally and consistently;
- state changes come from domain/application commands and read models;
- UI never directly mutates domain state;
- debug information is not mixed with player information.

Do not build a complete menu system before the first playable loop requires it.

## 9. Naming and ownership

- Menu paths use `Tools/Pokemon/<Domain>/<Action>`.
- EditorWindow class names end in `Window` where they represent windows.
- Validators end in `Validator` or `Validation`.
- Preview builders end in `Preview` or `PreviewBuilder`.
- Place tools under `Assets/Project/Editor/Tools/<Domain>/`.
- Keep reusable Editor-only utilities separate from domain Runtime code.
- Add each tool to the project file/feature index and this menu catalog.

## 10. Owner-executable operational runbooks

A tool or content pipeline is not sufficiently documented by describing its intended behaviour. Its work package and handoff must tell the project owner exactly how to execute and verify it locally.

For Unity Editor tools, document:
- exact menu path and required open scene/project state;
- required source assets, profile/configuration and destination folder;
- ordered button/menu actions and expected output;
- whether the operation creates, updates, skips or deletes assets;
- how to inspect generated prefabs, materials, bounds, pivots, scale and references;
- how to clear/regenerate safely and how to recover from a failed run.

For Blender workflows, document:
- supported Blender version if known;
- `.blend` file and required collections/objects;
- exact object names, transforms, units, origin/pivot and export settings;
- ordered UI steps or a complete Blender Python script;
- output filenames and destination paths;
- import/reimport steps in Unity and expected asset validation.

For file-only transformations, provide a complete script or complete replacement files, exact input/output paths, invocation command, expected output and a non-destructive dry-run/backup approach where appropriate.

For every local operation, distinguish static checks performed on supplied files from Unity/Blender checks that the owner must run. Include the exact logs, screenshots, test reports or generated files to return as evidence. Never mark an owner-run operation as passed until its evidence is supplied and reviewed.

## 11. Tool acceptance checklist

- [ ] Correct owner/domain.
- [ ] Correct menu path.
- [ ] Clear purpose and help text.
- [ ] Inputs validated before generation.
- [ ] Idempotent output.
- [ ] No accidental overwrite or duplicate.
- [ ] Clear/reset lifecycle works.
- [ ] Diagnostics are actionable.
- [ ] Preview uses a fixed fixture/seed.
- [ ] Tool works without opening the full game scene.
- [ ] Editor-only dependencies remain isolated.
- [ ] Documentation/index updated.
- [ ] Owner-executable Unity/Blender runbook supplied.
- [ ] Git apply/review/commit/push instructions supplied where files change.
- [ ] Local-only checks and evidence to return are clearly identified.
