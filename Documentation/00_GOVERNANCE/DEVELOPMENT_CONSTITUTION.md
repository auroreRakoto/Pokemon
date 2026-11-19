# Development Constitution

- **Status:** PROPOSED — owner approval required
- **Version:** 1.0
- **Date:** 2026-11-19
- **Scope:** all code, assets, tools, tests, documentation and AI-assisted work in this repository.

## 1. Mission

Build a Pokémon-inspired 3D world that supports two complementary experiences:

1. observing a living, autonomous ecosystem;
2. exploring and interacting with that world as a trainer.

The project is developed on the owner's PC. Performance decisions must be measured on the actual development machine. Do not invent a broad minimum-hardware target.

The current development priority is the explorable spherical world. The first playable milestone comes before broad ecosystem, combat, multiplayer or content expansion.

## 2. Authority hierarchy

When instructions disagree, use this order:

1. Explicit project-owner decision recorded in `DECISION_REGISTER.md`.
2. This constitution for process and governance.
3. `ARCHITECTURE_BASELINE_V2.md` for architecture and dependency rules.
4. `MASTER_ROADMAP_V2.md` for sequencing and release scope.
5. The owning domain document for domain behaviour and data.
6. An approved work-package specification for its bounded implementation.
7. Implementation and tests as evidence of actual behaviour.

Code does not automatically overrule documentation. Old documents, old chat messages, generated reports and comments do not overrule a newer approved decision.

If two approved sources conflict, stop the affected work, record the conflict and request a decision. Never silently reconcile it.

## 3. Status vocabulary

Every feature, document and work package must use one of these states:

- `IDEA`: unreviewed possibility.
- `PROPOSED`: scoped proposal awaiting owner approval.
- `APPROVED`: scope and design approved; implementation may begin after the plan milestone.
- `IN_PROGRESS`: implementation underway.
- `BLOCKED`: a named dependency or defect prevents progress.
- `IMPLEMENTED`: source changes exist; validation is not necessarily complete.
- `VALIDATED`: required automated and manual evidence passed.
- `ACCEPTED`: QA review passed and project owner approved.
- `DEPRECATED`: replacement exists; migration/removal remains.
- `REMOVED`: deleted after dependency and reference checks.

Never use `DONE` as a substitute for these states.

## 4. Non-negotiable development rules

### 4.1 One work package, one outcome

- One work package must deliver one independently describable capability or one bounded technical correction.
- Split work if it has multiple independent outputs, different validation environments, or more than one major architectural dependency.
- Do not add opportunistic refactors, unrelated bug fixes or future-proofing to an implementation.
- Record out-of-scope discoveries in the queue as separate work.

### 4.2 Specification before implementation

No substantial implementation starts until its work package is `APPROVED` and its plan milestone has been committed/pushed on the working branch.

The specification must identify:
- purpose and user-visible result;
- owner module;
- dependencies and exact contracts used;
- files expected to change;
- data inputs/outputs and units;
- deterministic inputs and seed policy;
- independent preview method;
- automated tests;
- integration tests;
- manual visual checks;
- performance measurements;
- failure cases;
- acceptance criteria;
- exclusions and rollback/removal plan.

### 4.3 Inspect supplied sources; do not assume repository access

The project owner operates the local Unity Git repository. An AI assistant does not have standing access to that repository, its working tree, remote, Unity Editor, Blender session or local machine.

Before proposing a change, the assistant must:
1. read the relevant project documents available in the ChatGPT Project;
2. identify which sources are snapshots and ask the owner for current files when implementation state matters;
3. request the exact scripts, `.meta` files, assets, scenes, prefabs, tests, logs or a focused archive needed for the task;
4. inspect every supplied caller, consumer, test and serialization reference relevant to the change;
5. state what could not be inspected because it was not supplied;
6. prepare changes against the supplied files only;
7. return complete replacement files or a clearly scoped patch/package for the owner to apply locally.

A supplied ZIP is a snapshot, not a live connection. Never claim to have inspected the current working tree, branch, remote, uncommitted changes or post-upload repository state unless the owner supplies fresh evidence. Never claim to have edited, committed, pushed or pulled the owner's repository. Never infer an API or namespace from a filename or folder name.

The assistant may inspect and validate the files actually supplied in the conversation or project workspace. The owner remains responsible for applying the deliverables to the local repository and confirming the resulting state.

### 4.4 Module-first implementation

Implement and validate the smallest useful module in isolation before wiring it into the full game.

The required sequence is:
1. pure logic and data contracts;
2. module-level EditMode tests;
3. module preview or sandbox;
4. module validation;
5. adapter/integration;
6. integration tests;
7. full-game visual and PlayMode validation;
8. QA and owner acceptance.

A known integration failure must not be hidden by changing the module's internal rules without evidence.

### 4.5 No manual repetitive setup

Prefer code-driven, repeatable and idempotent tooling for:
- creating or updating ScriptableObjects;
- prefab generation and validation;
- material and shader setup;
- scene/bootstrap creation;
- test fixtures and preview scenes;
- asset import and registration;
- batch diagnostics.

Manual Inspector setup is allowed for a small number of stable references or values intentionally designed for human tuning. Document why it is manual.

### 4.6 No silent architecture expansion

A work package may not:
- create a new cross-domain dependency;
- move ownership of data or behaviour;
- introduce a service locator/global mutable state;
- add a package or framework;
- change public interfaces or serialized schemas;
- change world-generation semantics;
- add a second implementation of an existing responsibility;

without an approved change proposal and updated architecture/decision records.

### 4.7 No premature optimization

Measure first. Record baseline time, memory, allocations, object count and target test scale where relevant. Optimize only against a demonstrated bottleneck. Do not introduce pooling, streaming, ECS, jobs, GPU instancing or complex LOD solely because they may be useful later.

### 4.8 Full-file and patch discipline

- Deliver complete replacement files by default for ordinary project files, unless a narrowly scoped patch is safer and explicitly explained.
- For the VR HackWand project only, provide changed functions/lines.
- Include all required companion files, including `.meta` files when they are part of the supplied change and their GUID implications are understood.
- Identify exact destination paths relative to the Unity project root.
- Never assume the owner applied a file unless they explicitly confirm it or supply the resulting file.
- Do not overwrite unknown local changes: ask for the current file first.

### 4.9 Owner-operated Unity, Blender and Git workflow

The assistant prepares artifacts and precise operating instructions; the owner executes them in the local tools. Every deliverable that requires local action must include an ordered runbook with:
- prerequisites and exact files to copy/import;
- destination paths relative to the Unity project root or Blender project;
- Unity menu/Inspector/scene steps, or Blender UI steps, with object names and settings where known;
- scripts or command-line commands where repeatable automation is appropriate;
- expected visible result and diagnostics;
- validation steps and evidence to send back (logs, screenshots, test output, generated files);
- rollback or recovery instructions for any destructive or serialized-data operation.

Prefer idempotent Editor scripts, Blender Python scripts and file-based transformations over repetitive manual setup when the supplied context supports them. Do not invent menu paths, scene names, object names, API calls, Blender versions or command arguments. If those details depend on unavailable project state, label them as assumptions or request the relevant files.

Git instructions must be copy/paste-ready but owner-executed. State the expected branch and working-tree assumptions; provide `git status`, diff review, staging, commit and push commands only when the paths and intended changes are known. Never run or claim to run Git against the owner's repository, and never claim a commit/push succeeded. The owner reports the command output or supplies the resulting commit hash.

For Unity/Blender validation, distinguish:
- checks performed on supplied files outside Unity/Blender;
- checks that require the owner to run Unity or Blender locally;
- checks blocked by missing assets, versions, plugins or project context.

Do not mark local Editor, PlayMode, Blender or target-machine checks as passed based only on static inspection.

## 5. Mandatory validation gates

A feature cannot advance past a gate with an unexplained failure.

| Gate | Required evidence |
|---|---|
| G0 — Scope | Approved work package, owner module, dependencies, acceptance criteria |
| G1 — Static integrity | Correct ownership, namespaces, references, no forbidden dependency |
| G2 — Isolated logic | EditMode tests and deterministic fixture where applicable |
| G3 — Isolated preview | Preview/sandbox result, reproducible inputs, diagnostics |
| G4 — Integration | Contract tests and integration test in intended composition root |
| G5 — Runtime | PlayMode/manual validation in the real target scene |
| G6 — Quality | Performance baseline, regression checks, documentation updated |
| G7 — Acceptance | Independent QA report and explicit owner approval |

A gate may be `N/A` only with a written reason in the work package. `BLOCKED` is not `PASS`.

## 6. Definition of Done

A work package is `ACCEPTED` only when:
- implementation matches the approved specification;
- all applicable tests pass;
- independent module preview passes;
- integration and target-scene checks pass;
- no new compiler errors or unresolved critical warnings are introduced;
- diagnostics and failure cases are understandable;
- performance measurements are recorded where relevant;
- documentation, feature index and decision records are updated;
- obsolete code/docs are either migrated or explicitly tracked;
- QA has reviewed the evidence;
- the project owner has approved the result;
- the exact commit message is prepared.

A feature that compiles but has not been run in Unity is `IMPLEMENTED`, not `VALIDATED`.

## 7. Git and checkpoint rules

Follow `GIT_WORKFLOW.md`.

- Use the same working branch for plan and implementation milestones.
- Plan/specification milestone must be pushed before implementation.
- Implementation milestone follows source changes and available checks.
- QA corrections use a separate corrective commit.
- After each validated feature, provide the exact commit message.
- If work stops early, provide a checkpoint commit message describing exactly what is implemented, what remains, which tests were not run, and the next objective.
- Every commit uses Conventional Commits and the mandatory `Agent: <ID>` footer.
- Corrective commits are allowed; milestones are checkpoints, not a two-commit limit.

## 8. Cleanup is controlled migration

Non-compliant code is not automatically deleted. First classify it as:
- duplicate;
- obsolete;
- wrong ownership;
- dead/unreferenced;
- valid but not yet compliant;
- required compatibility layer;
- uncertain.

Before removal, identify references, serialized assets, scene usage, tests, public APIs, `.meta` GUIDs and documentation links. Replace or migrate first, validate, then remove. See `DEPRECATION_AND_CLEANUP_POLICY.md`.

## 9. Communication contract for AI agents

At the start of a work package, report:
- work package ID and status;
- intended outcome;
- owning module;
- dependencies;
- exact scope and exclusions;
- planned tests and preview.

During work:
- distinguish verified facts from assumptions;
- report blockers immediately;
- do not claim tests that were not run;
- do not change scope without approval;
- treat all Unity, Blender and Git operations in the owner's environment as owner-executed steps;
- provide exact files and runbooks instead of claiming direct project edits.

At handoff, report:
- files created/modified/deleted;
- implementation status;
- tests run and exact results;
- tests not run and why;
- manual Unity and Blender steps still required, in execution order;
- Git commands for owner review, commit and push where applicable;
- known limitations;
- documentation changes;
- next work package;
- commit message.

## 10. Change control

Changes to this constitution, architecture baseline, release gates or roadmap require:
1. a change proposal explaining why;
2. impact analysis;
3. explicit owner approval;
4. update to the decision register and changelog;
5. migration of affected documents and work packages.

The goal is stability, not bureaucracy: small local implementation details may evolve inside the approved boundaries without changing the constitution.
