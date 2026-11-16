# Working Agreements

## 1. Truth and uncertainty

- Never claim a feature is implemented unless the repository proves it.
- Label assumptions and proposals explicitly.
- If two documents conflict, do not silently choose one. Record the conflict and ask the documentation owner to resolve it.
- Prefer the newest approved decision-register entry over an older informal discussion.
- A code implementation that conflicts with approved documentation is a discrepancy to investigate, not automatic permission to rewrite the design.

## 2. Code changes

Before changing code:
1. Locate the current implementation.
2. Read relevant domain docs and interface contracts.
3. Inspect callers, dependencies, tests, and serialization/network implications.
4. State the intended scope.
5. Avoid unrelated refactors.
6. Add or update tests.
7. Update documentation when behaviour, data, contracts, or architecture changes.

Never assume the developer has modified files unless they explicitly say so or the repository state confirms it.

## 3. AI assistant behaviour

AI assistants must:
- work only within the assigned chat/domain scope unless a cross-domain task is explicitly agreed;
- consult the relevant documentation before proposing architecture;
- identify impacted domains before changing shared contracts;
- avoid inventing file paths, APIs, package versions, or implementation status;
- ask one focused question when a missing decision blocks safe progress;
- provide complete files when requested, except where the project owner specifies a different editing style;
- explain any undocumented concept introduced by a task and propose where it belongs in the documentation.

## 4. Script-driven Unity rule

Prefer code and repeatable editor tooling for:
- asset registration and validation;
- material and terrain-layer creation;
- prefab setup;
- scene/bootstrap setup;
- procedural world generation;
- data import and validation.

Inspector assignments are acceptable for a small number of stable references or values that genuinely benefit from manual tuning. Avoid workflows requiring repetitive manual setup.

## 5. Change discipline

A feature is not complete merely because it compiles. Completion requires:
- implementation;
- tests or documented manual verification;
- updated documentation;
- updated contracts if cross-domain;
- known limitations recorded;
- no unresolved critical integration issue.

## 6. Git workflow and agent attribution

- All agents must follow [`GIT_WORKFLOW.md`](GIT_WORKFLOW.md).
- Every commit must use Conventional Commits format and include the mandatory `Agent: <ID>` footer.
- Every substantial feature/work package requires a pushed plan/specification milestone before implementation, followed by an implementation milestone on the same branch.
- Development-agent checks and independent QA review are required; the project owner gives final approval before closure.
- Corrective commits are allowed; the two milestones are checkpoints, not a two-commit limit.
