# Source Reference Index — Pokémon v2

- **Status:** PROPOSED — owner approval required
- **Purpose:** identify which project files contain rules and how they relate. This index does not silently supersede existing approved project decisions.

## A. New proposed governing sources in this package

| Source | What it governs | Authority after approval |
|---|---|---|
| `DEVELOPMENT_CONSTITUTION.md` | Process, statuses, gates, definition of done, Git/communication rules | Project-wide process authority |
| `ARCHITECTURE_BASELINE_V2.md` | Layers, ownership, dependency direction, module boundaries | Architecture baseline; must be reconciled with existing approved architecture decisions |
| `MASTER_ROADMAP_V2.md` | Product milestones, phase order and exit criteria | Roadmap authority; does not reset approved in-progress work |
| `MODULE_PREVIEW_AND_TEST_STANDARD.md` | Independent preview, test layers, deterministic fixtures, evidence | Validation standard |
| `EDITOR_TOOLING_AND_UI_STANDARD.md` | Developer menus, EditorWindow UX, diagnostics and player UI separation | Editor tooling standard |
| `DEPRECATION_AND_CLEANUP_POLICY.md` | Audit, migration, removal and Unity GUID/meta safety | Cleanup authority |
| `WORK_PACKAGE_TEMPLATE_V2.md` | Required feature specification | Work package template |
| `PROJECT_INSTRUCTIONS_COPY_PASTE.md` | Instructions to paste into ChatGPT Project settings | AI collaboration instructions, not a replacement for technical source docs |

## B. Existing repository sources that must be preserved and reconciled

These files were present in the supplied `Pokemon-clean.zip` snapshot. They remain important source references until an explicit migration is approved.

### Governance
- `Documentation/00_GOVERNANCE/DECISION_REGISTER.md` — owner decisions; highest project-specific authority.
- `Documentation/00_GOVERNANCE/GIT_WORKFLOW.md` — branch, milestone, push and commit conventions.
- `Documentation/00_GOVERNANCE/DOCUMENT_INDEX.md` — current documentation navigation.

### Architecture
- `Documentation/01_ARCHITECTURE/SYSTEM_ARCHITECTURE.md`
- `Documentation/01_ARCHITECTURE/UNITY_ARCHITECTURE.md`
- `Documentation/01_ARCHITECTURE/MODULE_BOUNDARIES.md`
- `Documentation/01_ARCHITECTURE/INTERFACE_CONTRACTS.md`
- `Documentation/01_ARCHITECTURE/DATA_ARCHITECTURE.md`
- `Documentation/01_ARCHITECTURE/WORLD_ARCHITECTURE.md`
- `Documentation/01_ARCHITECTURE/SPHERICAL_WORLD_ARCHITECTURE_PROPOSAL.md`
- `Documentation/01_ARCHITECTURE/NETWORK_ARCHITECTURE.md`
- `Documentation/01_ARCHITECTURE/PROJECT_FILE_AND_FEATURE_INDEX.md`

### Engineering and tests
- `Documentation/03_ENGINEERING/DEVELOPMENT_QUEUE.md` — immediate actionable queue, not the full roadmap.
- `Documentation/03_ENGINEERING/ROADMAP_AND_VERTICAL_SLICE.md` — existing roadmap to reconcile with the proposed master roadmap.
- `Documentation/03_ENGINEERING/TESTING_AND_VALIDATION.md`
- `Documentation/07_Testing/TESTING_AND_VALIDATION.md`
- `Documentation/07_Testing/COVERAGE_MATRIX.md`
- `Documentation/07_Testing/README.md`
- Relevant feature-specific test plans under `Documentation/07_Testing/Work_Packages/`.

### AI instructions
- `.github/copilot-instructions.md`
- `.github/instructions/*.instructions.md`
- `Documentation/04_AI_INSTRUCTIONS/copilot-instructions.md`
- `Documentation/04_AI_INSTRUCTIONS/instructions/*.instructions.md`

## C. Known reconciliation tasks

1. Compare the two Copilot instruction entry points. Select one canonical source; make the other a short pointer if appropriate.
2. Compare `Documentation/03_ENGINEERING/TESTING_AND_VALIDATION.md` and `Documentation/07_Testing/TESTING_AND_VALIDATION.md`. In the supplied snapshot their content is duplicated. Keep one canonical policy and make the other a pointer or remove it only after references are migrated.
3. Reconcile `ROADMAP_AND_VERTICAL_SLICE.md` with `MASTER_ROADMAP_V2.md`. Preserve unique decisions and historical evidence; do not leave both active with contradictory phase order.
4. Reconcile the new architecture baseline with existing `MODULE_BOUNDARIES`, `INTERFACE_CONTRACTS`, `SYSTEM_ARCHITECTURE`, `WORLD_ARCHITECTURE`, `NETWORK_ARCHITECTURE` and the spherical-world proposal. Do not silently cancel existing approved server/network decisions or turn proposals into approved designs.
5. Keep `DEVELOPMENT_QUEUE.md` short and current. The project context identifies WORLD-011 as approved and WORLD-011-A as its next audit step; verify repository state before changing the queue.
6. Update `DOCUMENT_INDEX.md`, `PROJECT_FILE_AND_FEATURE_INDEX.md`, decision register and changelog only after approval and actual migration.

## D. Rule for future references

When a work package cites a rule, cite the exact source path and section. Do not write "as per architecture" without identifying the contract. If a source is superseded, mark it historical and link to the replacement.
