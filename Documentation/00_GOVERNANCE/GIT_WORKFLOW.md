# Git Workflow and Commit Convention

- **Status:** CONFIRMED project working agreement
- **Owner:** Project owner; maintained by DOC
- **Applies to:** all specialist agents and all project work packages

## 1. Purpose

Every work package must be traceable from its approved plan to its implementation and verification. Commit messages identify the change type, affected scope and responsible agent.

**Owner-operated repository rule:** the project owner runs Git in the local Unity repository. AI assistants prepare file deliverables and exact commands, but do not have standing access to the local working tree or remote and must never claim to have committed or pushed. The owner reviews the diff, executes the commands and shares the resulting output/hash when verification is needed.

This project adopts **Conventional Commits 1.0.0** as its commit-message structure, with a mandatory `Agent` footer. The two-milestone process below is an additional project rule; it is not part of the Conventional Commits specification itself.

## 2. Commit message format

```text
<type>(<scope>): <short imperative description>

Optional body explaining motivation, design or implementation details.

Agent: <AGENT_ID>
```

Rules:
- `type` is required and lowercase.
- `scope` is recommended and identifies the owning domain or subsystem.
- The description is short, specific and imperative where practical.
- The body is optional, separated from the subject by a blank line.
- The `Agent` footer is mandatory and uses the exact ID from Section 4.
- Use additional Conventional Commit footers only when useful (for example `BREAKING CHANGE: ...`).
- Do not put the agent ID in the subject; use the footer so the subject remains machine-readable and consistent.

### Accepted commit types

| Type | Use |
|---|---|
| `feat` | Add a user-visible or system capability |
| `fix` | Correct a defect |
| `docs` | Documentation-only change |
| `refactor` | Restructure code without intended behaviour change |
| `test` | Add or change tests |
| `perf` | Performance improvement |
| `build` | Build system or dependency changes |
| `ci` | Continuous integration changes |
| `chore` | Maintenance that does not fit a more specific type |
| `style` | Formatting-only changes without behaviour change |

Use `feat` and `fix` according to their actual meaning. Conventional Commits relates these types and explicit breaking changes to Semantic Versioning, but this convention does not by itself define the project's release/versioning policy.

### Examples

```text
docs(governance): define the two-phase development workflow

Document planning, implementation, QA and owner approval gates.

Agent: DOC
```

```text
feat(world): generate seeded biome placement

Implement the approved biome placement specification.

Work-Package: WORLD-004
Plan-Commit: <full-or-short-commit-hash>
Agent: WORLD
```

```text
fix(pokemon): preserve individual identity after reproduction

Correct the identity assignment path and add regression coverage.

Work-Package: POK-012
Agent: POK
```

## 3. Mandatory two-milestone workflow

Every feature or substantial work package has two mandatory milestone commits on the **same working branch**.

### Milestone A — plan/specification

Before implementation begins, the responsible agent must:

1. Read the relevant charter, decision register, domain documentation and contracts.
2. Inspect the current implementation, callers, dependencies and tests.
3. Describe the problem, expected result, included scope and exclusions.
4. Identify ownership, affected modules, dependencies and interfaces.
5. Define data, authority, lifecycle, persistence/network and performance implications where relevant.
6. Define tests and measurable acceptance criteria.
7. List exact documentation files expected to change.
8. Commit and **push** the plan to the working branch.

The plan may be a feature specification, RFC, task plan or a focused documentation plan, depending on the work. Use the canonical templates in `05_TEMPLATES/`.

Implementation for that work package must not start before the plan has been pushed. Repository inspection and investigation needed to prepare the plan are allowed.

### Milestone B — implementation

After implementation, the responsible agent must:

1. Implement the approved scope.
2. Record deviations from the plan and obtain approval for material changes before silently expanding scope.
3. Run the planned tests and relevant regression checks.
4. Update documentation affected by the actual implementation.
5. Provide test commands, results, manual verification evidence and known limitations.
6. Commit and **push** the implementation milestone to the same branch.
7. Reference the plan commit using a `Plan-Commit` footer when practical.

The implementation commit must not claim that a feature is complete if acceptance criteria have not been met.

## 4. Agent identifiers

| Specialist | Required ID |
|---|---|
| Documentation Architect | `DOC` |
| System Architect | `ARCH` |
| Ecology & World Simulation | `ECO` |
| Pokémon Simulation & Behaviour | `POK` |
| World Generation & Environment | `WORLD` |
| Art & Asset Pipeline | `ART` |
| Combat & Abilities | `COMBAT` |
| Networking & Persistence | `NET` |
| Unity/Blender Tooling | `TOOLS` |
| Quality Assurance | `QA` |

The `Agent` footer identifies the agent responsible for that commit. It must not falsely imply that an agent authored, reviewed or tested work it did not perform. If multiple agents make separate commits, each commit identifies its own responsible agent.

## 5. QA and project-owner approval

The project owner has confirmed this validation chain:

1. **Development agent:** performs and reports its own checks.
2. **QA agent:** independently reviews the acceptance criteria, test evidence and relevant regressions, then records PASS / FAIL / BLOCKED with findings.
3. **Project owner:** reviews the result and gives final approval or requests changes.

A work package is not accepted or closed until QA has reported PASS and the project owner has approved it. A build succeeding is not sufficient proof by itself.

QA does not need to create a third milestone commit merely to record a review. QA must use `Agent: QA` on any commit it actually authors. A QA report may be included in the work package or the designated audit/integration report.

## 6. Corrections and changed plans

- The two milestones are required checkpoints, not a limit of two commits.
- QA failures and owner-requested corrections may produce additional commits on the same branch.
- Each additional commit follows the same Conventional Commit format and identifies its actual authoring agent.
- A material change to scope, behaviour, data ownership, interfaces, authority, persistence or architecture requires an updated plan/RFC and the appropriate owner approval.
- Do not rewrite a pushed plan to hide a deviation; preserve the decision trail.
- A failed or blocked QA result must remain visible and must not be described as passed.


## 6A. Session checkpoints and commit-message handoff

In addition to the two mandatory work-package milestones, every independently implemented and validated feature or implementation increment must receive its own commit message. The same applies when a work session is ending before the current task is complete: prepare a checkpoint commit message that describes the actual state, even if the work is partial or blocked.

The development assistant must proactively provide the exact commit message at either point; the project owner must not need to ask for it. The assistant must not claim that a commit was created, committed or pushed unless that action was actually performed and confirmed.

### When a feature is implemented and validated

- Prepare a normal Conventional Commit message using the format in Section 2.
- State the relevant work-package ID and reference the plan commit when available.
- Mention the meaningful validation performed in the body; never imply tests passed if they were not run.
- Keep the commit scoped to the feature/increment that was validated.

Example:

```text
feat(world): enable spherical player traversal

Implement and validate ground movement, radial gravity and camera follow
on the approved spherical-world scene.

Work-Package: WORLD-004
Plan-Commit: <hash>
Agent: WORLD
```

### When work stops before validation or completion

- Prepare a checkpoint message with a truthful partial-work description.
- Use only an accepted Conventional Commit type from Section 2; do not introduce `wip` as a new type without owner approval. Make the incomplete checkpoint explicit in the subject or body.
- Explicitly record what is implemented, what is not validated, the current blocker, and the next action.
- A checkpoint commit is not the implementation milestone and does not mean the feature is accepted, QA-passed or complete.

Example:

```text
fix(world): checkpoint player bootstrap integration

Partial checkpoint: the controller call site has been updated, but Unity
compilation and Play Mode validation have not yet been run.
Next: compile the project, inspect all Initialize call sites and resolve
any remaining errors.

Work-Package: WORLD-004
Agent: WORLD
```

If no source files changed during the session, do not invent a code commit; report that there is nothing to commit and provide the next-step handoff. Documentation-only changes use `docs(...)`.

## 7. Branch and push rules

All commands below are instructions for the project owner to execute in a terminal opened at the Unity repository root. They are not commands the assistant claims to have run.

Before applying a delivered file package:

```bash
git status --short --branch
```

Review each changed file and preserve unrelated local edits. After copying the delivered files into the stated project-relative paths, review:

```bash
git diff --check
git status --short
git diff -- <path/to/changed/file>
```

Stage only the intended paths, then commit and push the current working branch:

```bash
git add <path/to/file1> <path/to/file2>
git diff --cached --check
git diff --cached
git commit -m "<subject>" -m "Agent: <ID>"
git push
```

Replace placeholders with the exact paths and commit message supplied for the work package. Do not use broad staging such as `git add .` unless the owner has reviewed and intentionally wants every working-tree change included. If the push is rejected, stop and inspect the Git output; do not force-push or rewrite shared history without explicit owner approval.

The owner should return the commit hash and relevant command output when the assistant needs to update documentation status or prepare the next milestone. A proposed command is not evidence that it was executed successfully.


- The plan and implementation milestones for a work package must be on the same working branch.
- Push Milestone A before implementation starts.
- Push Milestone B after implementation checks are complete.
- This project has not yet selected a mandatory branch naming convention; do not invent one.
- Pull Requests are not required by this confirmed workflow. The project owner may introduce them later through an explicit decision.

## 8. Work package traceability

For substantial work, include a stable work-package identifier in the plan and, where useful, as a `Work-Package` footer in commits. The implementation commit should reference the plan commit with `Plan-Commit: <hash>` when practical. These extra footers are project conventions, not requirements of Conventional Commits.

## 9. Definition of done

A work package is done only when:
- the pushed plan exists;
- implementation matches the approved plan or approved amendments;
- acceptance tests and regression checks have been run;
- QA reports PASS;
- documentation and contracts reflect the result;
- known limitations are recorded;
- the project owner has approved closure.

## 10. References

- [Conventional Commits 1.0.0 specification](https://www.conventionalcommits.org/en/v1.0.0/)
- `WORKING_AGREEMENTS.md`
- `CHAT_TEAM_STRUCTURE.md`
- `05_TEMPLATES/FEATURE_SPEC_TEMPLATE.md`
- `05_TEMPLATES/INTEGRATION_REPORT_TEMPLATE.md`
