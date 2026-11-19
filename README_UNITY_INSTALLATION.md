# Unity repository documentation update — ENG-001

**Status: PROPOSED.** This package is intended for the Unity Git repository, not for ChatGPT Project uploads.

## Contents

This package adds proposed governance and engineering standards under their project-relative `Documentation/` paths:

- `00_GOVERNANCE/DEVELOPMENT_CONSTITUTION.md`
- `00_GOVERNANCE/SOURCE_REFERENCE_INDEX.md`
- `00_GOVERNANCE/DEPRECATION_AND_CLEANUP_POLICY.md`
- `01_ARCHITECTURE/ARCHITECTURE_BASELINE_V2.md`
- `03_ENGINEERING/MASTER_ROADMAP_V2.md`
- `03_ENGINEERING/MODULE_PREVIEW_AND_TEST_STANDARD.md`
- `03_ENGINEERING/EDITOR_TOOLING_AND_UI_STANDARD.md`
- `05_TEMPLATES/WORK_PACKAGE_TEMPLATE_V2.md`

## Important installation rule

Do not blindly overwrite existing files. In particular, compare these proposed documents with the current `DECISION_REGISTER.md`, `GIT_WORKFLOW.md`, `PROJECT_CHARTER.md`, `WORKING_AGREEMENTS.md`, all existing architecture/domain contracts, `ROADMAP_AND_VERTICAL_SLICE.md`, `DEVELOPMENT_QUEUE.md`, and existing work packages. Preserve approved decisions and unique existing content. The new files remain `PROPOSED` until the owner approves them.

This package does not include code, assets, scenes, prefabs or Unity project files. It does not authorize cleanup/deletion. It does not mark ENG-001 complete.

## Required next task

Create/approve `WP-ENG-001 — Project Engineering Standardization`: audit and reconcile the existing documentation, choose canonical sources, update indexes and decision records only after approval, then plan the controlled cleanup. Keep WORLD-011 and WORLD-011-A visible in the active queue; verify the repository state before changing their status.
