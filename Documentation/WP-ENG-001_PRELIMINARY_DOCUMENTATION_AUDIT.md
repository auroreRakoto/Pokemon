# WP-ENG-001 — Preliminary documentation audit

- **Audit status:** IN_PROGRESS (read-only investigation)
- **Source:** `Pokemon-clean.zip`, confirmed by the project owner as the latest supplied repository archive
- **Archive root:** `Pokemon-clean/`
- **Audit date:** 2026-11-19
- **Files changed in the Unity repository:** none
- **Unity validation/tests:** NOT RUN — this is a documentation audit and no Unity project was opened
- **Purpose:** identify authority conflicts, stale references, duplicate standards and decisions requiring owner review before any documentation migration

## 1. Executive summary

The archive contains a substantial existing documentation system. The new V2 documents are useful proposals, but they must not replace existing approved sources wholesale. The audit found several specific reconciliation items:

1. A confirmed combat decision is contradicted by the proposed V2 roadmap.
2. A confirmed data-directory decision is contradicted by both the proposed architecture baseline and an existing Unity architecture directory tree.
3. The proposed V2 roadmap changes the order and timing of server work compared with the existing roadmap; this must be reconciled while preserving DEC-007.
4. WORLD-011 approval/status is not represented consistently across current project instructions, the repository queue and WORLD-011 documents.
5. Two testing-standard files are byte-identical; two Copilot instruction entry points are also byte-identical.
6. The current queue identifies WORLD-004 integration as BLOCKED and globe traversal as NEXT, while the current ChatGPT project instruction says to begin WORLD-011-A.

No documents or source files were modified during this audit.

## 2. Authority sources inspected

- `Documentation/00_GOVERNANCE/DECISION_REGISTER.md`
- `Documentation/00_GOVERNANCE/PROJECT_CHARTER.md`
- `Documentation/00_GOVERNANCE/WORKING_AGREEMENTS.md`
- `Documentation/00_GOVERNANCE/GIT_WORKFLOW.md`
- `Documentation/01_ARCHITECTURE/SYSTEM_ARCHITECTURE.md`
- `Documentation/01_ARCHITECTURE/UNITY_ARCHITECTURE.md`
- `Documentation/01_ARCHITECTURE/MODULE_BOUNDARIES.md`
- `Documentation/01_ARCHITECTURE/INTERFACE_CONTRACTS.md`
- `Documentation/01_ARCHITECTURE/DATA_ARCHITECTURE.md`
- `Documentation/01_ARCHITECTURE/NETWORK_ARCHITECTURE.md`
- `Documentation/01_ARCHITECTURE/WORLD_ARCHITECTURE.md`
- `Documentation/01_ARCHITECTURE/SPHERICAL_WORLD_ARCHITECTURE_PROPOSAL.md`
- `Documentation/03_ENGINEERING/DEVELOPMENT_QUEUE.md`
- `Documentation/03_ENGINEERING/ROADMAP_AND_VERTICAL_SLICE.md`
- WORLD-011 work-package and audit documents under `Documentation/03_ENGINEERING/WORK_PACKAGES/`
- Testing standards and README under `Documentation/03_ENGINEERING/` and `Documentation/07_Testing/`
- `.github/copilot-instructions.md` and `Documentation/04_AI_INSTRUCTIONS/copilot-instructions.md`
- Proposed `DEVELOPMENT_CONSTITUTION.md`, `ARCHITECTURE_BASELINE_V2.md`, `MASTER_ROADMAP_V2.md`, `MODULE_PREVIEW_AND_TEST_STANDARD.md`, `EDITOR_TOOLING_AND_UI_STANDARD.md`, `DEPRECATION_AND_CLEANUP_POLICY.md`, and `WORK_PACKAGE_TEMPLATE_V2.md`

This is a preliminary reconciliation pass, not a claim that all 105 Markdown documents in the archive have been fully audited line by line.

## 3. Findings and reconciliation register

### ENG-001-F01 — Combat mode contradiction

- **Existing authority:** `DECISION_REGISTER.md`, DEC-006 — Combat: Real-time, status CONFIRMED.
- **Conflicting proposal:** `MASTER_ROADMAP_V2.md`, Phase 7 / §10, steps 2 and final paragraph — asks for an owner decision between turn-based and real-time combat.
- **Assessment:** direct contradiction. The proposed roadmap asks to reopen a decision already marked CONFIRMED.
- **Smallest compliant correction:** revise the proposed roadmap to treat real-time combat as fixed and keep only the detailed rules, timing model and combat contracts open.
- **Owner decision required:** no new combat-mode decision is needed unless the owner intends to supersede DEC-006. Approval is required before adopting the corrected roadmap.

### ENG-001-F02 — Runtime data directory contradiction

- **Existing authority:** `DECISION_REGISTER.md`, DEC-020 — source JSON belongs in the single project-level `Assets/Project/Data`; runtime loading/contracts/projections belong under `Runtime/DataServices`; explicitly says not to create a second `Data` directory under `Runtime`.
- **Existing conflicting document:** `UNITY_ARCHITECTURE.md`, canonical Unity script layout, contains `Runtime/Data/Contracts`, `Runtime/Data/Loading`, and `Runtime/Data/Projections`.
- **New conflicting proposal:** `ARCHITECTURE_BASELINE_V2.md`, §8, contains `Runtime/Data/`.
- **Assessment:** direct documentation conflict with a confirmed decision. DEC-020 is the governing owner decision; the two architecture trees are stale/inconsistent with it.
- **Smallest compliant correction:** use `Runtime/DataServices/` for runtime loading/contracts/projections and retain `Assets/Project/Data/` as the single project-level source-data directory. Update both architecture trees and references after approval.
- **Owner decision required:** no change to DEC-020 is necessary if it remains intended; owner approval is needed for the documentation migration.

### ENG-001-F03 — Server roadmap sequencing needs owner reconciliation

- **Existing authority:** DEC-007 (dedicated server, CONFIRMED); `PROJECT_CHARTER.md` §3 and §6 (server authority; first meaningful vertical slice eventually includes server-authoritative state and persistence/session lifecycle); `NETWORK_ARCHITECTURE.md` §1–2.
- **Existing roadmap:** `ROADMAP_AND_VERTICAL_SLICE.md`, Phase 5 — Multiplayer/server foundation, including dedicated-server bootstrap, session lifecycle, authoritative state/client commands, replication and persistence prototype.
- **New proposal:** `MASTER_ROADMAP_V2.md`, §13 defers dedicated server/network replication as not prerequisites for P0/P1; Phase 3 explicitly excludes network replication and persistence for the first Pokémon encounter. The new roadmap does not provide a clearly corresponding dedicated-server foundation milestone.
- **Assessment:** deferring server work during the local spherical-world prototype is compatible with DEC-029 (PC initially; multiplayer not near-term) only if the dedicated-server/server-authoritative architecture remains preserved and the first meaningful vertical slice still meets the Charter. Replacing the old Phase 5 sequencing with no explicit server milestone is a roadmap change, not an automatic cancellation of DEC-007.
- **Smallest compliant correction:** explicitly distinguish the local technical prototype / first playable proof from the Charter's first meaningful vertical slice; retain a named future server-authority/session/persistence milestone and its dependencies. Do not describe dedicated-server architecture as cancelled or optional.
- **Owner decision required:** confirm whether the new sequencing is intended to move server foundation later than old Phase 5, and where the server-authoritative vertical-slice gate belongs.

### ENG-001-F04 — WORLD-011 status and queue conflict

- **Current owner instruction in this ChatGPT Project:** WORLD-011 is approved; start with WORLD-011-A; initial placement is Plains and Forest only.
- **Repository queue:** `DEVELOPMENT_QUEUE.md` (updated 2026-11-18) lists WORLD-004 integration as BLOCKED (CS7036), globe traversal as NEXT after P0, WORLD-005 as READY after P1, and small environment dressing as LATER. WORLD-011 is not listed in the queue.
- **Repository work-package status:** `WORLD-011_PROCEDURAL_ENVIRONMENT_PLACEMENT.md` says `PROPOSAL — ready for review`; `WORLD-011-A_ENVIRONMENT_ASSET_READINESS_AUDIT.md` says `PARTIAL`; `WORLD-011-A_CURRENT_ARCHITECTURE_AUDIT.md` is an audit report; `WORLD-011-B_JSON_CONFIGURATION.md` says implementation is in the source package but Unity compilation/EditMode execution are pending; `WORLD-011-B_PLACEMENT_CONFIGURATION.md` says implementation delivered but Unity validation and prefab assignment pending.
- **Assessment:** repository records do not reflect the current owner instruction that WORLD-011 is approved and WORLD-011-A is the starting task. In addition, the queue's WORLD-004 blocker and the owner's WORLD-011-A instruction establish different immediate priorities.
- **Current owner instruction:** start WORLD-011-A. This is the operative priority for this ChatGPT Project, even though the repository queue has not yet been synchronized.
- **Smallest compliant correction:** record the owner approval/status for WORLD-011 in the canonical work-package/decision/queue records; make WORLD-011-A the single `NEXT` item; retain WORLD-004 as `BLOCKED` with its CS7036 evidence rather than implying it is fixed. Do not claim the prefab-level audit is complete.
- **Owner decision required:** none for the immediate priority; the owner instruction already specifies WORLD-011-A. Approval is still needed before editing the repository documentation.

### ENG-001-F05 — Duplicate testing standard

- `Documentation/03_ENGINEERING/TESTING_AND_VALIDATION.md`
- `Documentation/07_Testing/TESTING_AND_VALIDATION.md`
- **Evidence:** SHA-256 hashes are identical.
- **Additional context:** `Documentation/07_Testing/README.md` calls `07_Testing` the canonical home for project-wide testing standards, plans, coverage and validation reports.
- **Smallest compliant correction:** keep `Documentation/07_Testing/TESTING_AND_VALIDATION.md` as the canonical policy; change the engineering-path copy to a short pointer only after checking and updating inbound links. Do not delete the duplicate in this audit.

### ENG-001-F06 — Duplicate Copilot instruction entry points

- `.github/copilot-instructions.md`
- `Documentation/04_AI_INSTRUCTIONS/copilot-instructions.md`
- **Evidence:** SHA-256 hashes are identical.
- **Smallest compliant correction:** retain `.github/copilot-instructions.md` as the active GitHub Copilot entry point; keep the Documentation copy as a clearly labelled mirror or pointer if the documentation index requires it. The scoped `.github/instructions/*.instructions.md` and Documentation scoped instructions need a separate content comparison before any consolidation.

### ENG-001-F07 — Architecture baseline omissions that should be linked, not replaced

- **Existing confirmed decisions:** DEC-040 layered non-destructive planet authoring; DEC-041 3D globe + 2D planisphere over the same data; DEC-042 hybrid biome ground texture + 3D vegetation; DEC-043 65% ocean coverage by spherical surface area; DEC-044 four initial terrestrial biomes; DEC-045 configurable planet radius/ocean range; DEC-046 versioned JSON world profiles; DEC-047 opaque URP playable terrain; DEC-048 shared opaque URP biome blend shader.
- **New proposal:** `ARCHITECTURE_BASELINE_V2.md`, §5 provides broad world submodule boundaries but does not explicitly map all these confirmed decisions to the world architecture contracts and work packages.
- **Assessment:** omission is not by itself a contradiction, but adoption must not erase or weaken these domain decisions.
- **Smallest compliant correction:** add references from the baseline/index to the relevant world architecture, terrain/rendering, Planet Studio amendments and approved work packages; do not duplicate their detailed rules in the baseline.

### ENG-001-F08 — Decision register formatting/order issue

- `DECISION_REGISTER.md` contains DEC-042–044 after the “Decision change procedure” heading, then DEC-048 appears before DEC-045–047 and includes literal escaped `\n` text around its table row.
- **Assessment:** formatting/navigation defect; decision content should be preserved.
- **Smallest compliant correction:** normalize the table ordering and escaped newline formatting without changing decision wording or status. Update changelog and index as required.

## 4. Confirmed alignments

- DEC-007 dedicated-server/server-authoritative direction is preserved by `SYSTEM_ARCHITECTURE.md`, `NETWORK_ARCHITECTURE.md`, `PROJECT_CHARTER.md` and the new architecture baseline's §10 caveat. The new baseline must not be read as cancelling it.
- DEC-019's shared runtime-safe generation core and Editor/Play Mode adapters align with the existing architecture separation principles.
- DEC-040/041's layered planet and dual-view requirements are present in `WORLD_ARCHITECTURE.md` and are confirmed in the decision register; they need explicit traceability from the new baseline.
- DEC-043/044 match the project-wide spherical-world direction: 65% ocean surface-area target and Plains/Forest/Desert/Mountains as terrestrial biomes.
- `ARCHITECTURE_BASELINE_V2.md`'s `Runtime/Composition` path is consistent with `UNITY_ARCHITECTURE.md` and `SYSTEM_ARCHITECTURE.md`.
- The proposed preview/test standard's separation between executable Unity tests (`Assets/Project/Tests`) and documentation test plans (`Documentation/07_Testing`) aligns with `Documentation/07_Testing/README.md`.

## 5. Proposed reconciliation order

1. Record the already-given WORLD-011 approval and set WORLD-011-A as the single `NEXT` item, while preserving WORLD-004 as `BLOCKED`.
2. Owner confirms the intended future milestone for dedicated-server foundation and the first server-authoritative vertical slice.
3. Correct the proposed roadmap's DEC-006 real-time combat contradiction.
4. Reconcile the `Runtime/Data` directory conflict against DEC-020.
5. Consolidate the duplicate testing standard and Copilot entry point without deleting history or breaking links.
6. Add traceability links for DEC-040–048 and normalize the decision register formatting.
7. Update `DOCUMENT_INDEX.md`, `PROJECT_FILE_AND_FEATURE_INDEX.md`, development queue, documentation changelog and ChatGPT reference pack only after the owner approves the actual migration.

## 6. Validation and handoff

| Check | Status | Evidence |
|---|---|---|
| Archive source inspected | PASS | `Pokemon-clean.zip` extracted and relevant governance/architecture/roadmap/WORLD-011 sources read |
| Duplicate testing file comparison | PASS | Identical SHA-256 hashes |
| Duplicate Copilot entry point comparison | PASS | Identical SHA-256 hashes |
| Unity compile / EditMode / PlayMode | NOT RUN | Documentation-only audit; no Unity Editor execution |
| Live generated prefab audit | BLOCKED | The archive's WORLD-011-A report states generated prefab outputs were absent; current prefab hierarchy/bounds/pivots need Unity-side asset evidence |
| Full 105-document reconciliation | NOT RUN | This is a preliminary pass; domain-specific and all linked-document references still need scoped review |

**Files changed in Unity repository:** none.

**Next work package:** WORLD-011-A remains the owner-designated next task. Its prefab-level checks remain blocked until the generated prefab assets are supplied or otherwise made inspectable; documentation migration remains pending owner approval and the server-roadmap decision above.

**Commit:** no repository commit was created. No Unity source files were modified.
