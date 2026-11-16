# Project Alignment Plan --- Holy Grail Integration & Repository Audit

-   **Plan ID:** DOC-PLAN-001
-   **Owner:** DOC --- Documentation Architect
-   **Project:** Pokémon Ecology
-   **Status:** PLANNED
-   **Created:** 2026-11-16
-   **Target repository path:**
    `Documentation/00_GOVERNANCE/PROJECT_ALIGNMENT_PLAN.md`
-   **Implementation status:** NOT STARTED

## 1. Purpose

Establish a verified baseline between the current Unity repository and
the Pokémon Ecology Holy Grail documentation, then integrate and update
the documentation so it accurately describes the intended project
architecture and the repository's actual state.

This plan is the Phase 1 planning deliverable. It authorizes the audit
and documentation work described below; it does **not** claim that the
audit has been completed or that any feature is implemented.

## 2. Inputs currently available

1.  `Pokemon-main.zip` --- snapshot of the current Unity repository
    supplied for review.
2.  `Pokemon_Ecology_Holy_Grail_v1.zip` --- Holy Grail Documentation v1,
    containing governance, architecture, domain, engineering, AI
    instruction and template documents.
3.  Project-owner decisions made in this conversation:
    -   Two mandatory milestone commits per feature/work package, on the
        same branch.
    -   First commit records the plan/specification and is pushed before
        implementation begins.
    -   Second milestone commit records the implementation after it has
        been developed and checked.
    -   The responsible development agent and QA both participate in
        verification.
    -   The project owner gives final approval before the work is
        considered accepted/closed.

The supplied Unity archive is a repository snapshot, not a live Git
checkout. It does not provide Git history, branches, remotes or commit
metadata. No Blender project archive has yet been supplied.

## 3. Initial repository facts (inventory only)

The supplied Unity snapshot contains 26 C# source files under
`Assets/Project/Scripts/`, grouped into:

-   `Biomes/`
-   `Pokemon/AI/`
-   `Pokemon/Data/`
-   `Pokemon/Simulation/`
-   `Pokemon/Spawning/`
-   `Resources/`
-   `Simulation/`
-   `World/`

The snapshot includes Unity `Packages/` and `ProjectSettings/` content.
No Markdown documentation files were found in the supplied Unity
archive. These are inventory observations only; they do not establish
correctness, completeness, runtime behaviour or conformance.

## 4. Scope of work

### Included

1.  Inspect the supplied repository source and configuration.
2.  Map existing scripts and assets to Holy Grail domains and
    responsibilities.
3.  Review data ownership, domain boundaries, interfaces, runtime/editor
    separation and likely client/server authority implications where
    evidence exists.
4.  Identify implemented-looking, partial, missing, conflicting and
    unverifiable areas without overstating implementation status.
5.  Compare repository evidence against the Holy Grail's intended
    architecture and current decision register.
6.  Prepare a discrepancy register with severity, evidence, affected
    domain, recommended action and whether owner approval is required.
7.  Integrate the Holy Grail documentation into the repository,
    preserving its structure and terminology unless a justified change
    is approved.
8.  Add the project Git workflow rules described in Section 7.
9.  Update document index, changelog, working agreements and relevant AI
    instructions.
10. Run documentation consistency checks and produce a final audit
    report.

### Not included unless separately approved

-   Implementing or refactoring Unity gameplay/runtime code.
-   Changing game design, ecology rules, species facts or confirmed
    architectural decisions.
-   Selecting unresolved technologies (networking framework, persistence
    technology, terrain representation, AI provider, etc.) without a
    separate decision.
-   Claiming runtime functionality based only on source-code presence.
-   Creating or modifying Blender assets without the Blender project and
    an explicitly scoped task.
-   Pushing commits, changing branches or modifying the remote
    repository on the owner's behalf.

## 5. Audit method

For each relevant system:

1.  **Locate evidence:** identify source files, assets, configuration,
    tests and callers.
2.  **Understand behaviour:** inspect the complete relevant code paths
    and dependencies rather than judging by filenames alone.
3.  **Map ownership:** compare responsibilities with
    `MODULE_BOUNDARIES.md` and `SYSTEM_ARCHITECTURE.md`.
4.  **Check contracts and data:** compare models, identifiers, units,
    lifecycle, ownership and serialization with `DATA_ARCHITECTURE.md`
    and `INTERFACE_CONTRACTS.md`.
5.  **Check execution boundaries:** identify runtime/editor dependencies
    and any Unity-specific coupling.
6.  **Check verification:** locate tests and available
    reproducible/manual verification evidence.
7.  **Classify findings:** use the statuses below.
8.  **Recommend action:** documentation correction, code investigation,
    implementation task, architecture proposal or owner decision.

### Finding statuses

-   **CONFORMANT:** available evidence supports conformance to the
    documented requirement.
-   **PARTIAL:** some requirements are supported, but gaps remain.
-   **CONTRADICTION:** repository evidence conflicts with an approved
    requirement or decision.
-   **MISSING:** an expected system, document, test or contract could
    not be found in the supplied snapshot.
-   **UNKNOWN:** available evidence is insufficient to reach a reliable
    conclusion.
-   **NOT APPLICABLE:** the requirement does not apply to the inspected
    component.

A source file existing is not, by itself, proof that its feature works.
Runtime claims require suitable test, execution or manual verification
evidence.

## 6. Planned deliverables

  -----------------------------------------------------------------------
  ID                      Deliverable             Expected content
  ----------------------- ----------------------- -----------------------
  D1                      Integrated              Holy Grail documents
                          documentation baseline  placed in a clear
                                                  repository
                                                  documentation structure

  D2                      `GIT_WORKFLOW.md`       Commit format, agent
                                                  attribution, branch and
                                                  two-phase workflow, QA
                                                  and owner approval

  D3                      Updated governance      Index, changelog,
                          documents               working agreements,
                                                  chat-team guidance and
                                                  AI instructions updated
                                                  consistently

  D4                      Repository audit report Scope, inspected
                                                  evidence, findings,
                                                  unknowns, risks and
                                                  limitations

  D5                      Discrepancy/action      Prioritized corrections
                          register                and implementation
                                                  tasks, with owners and
                                                  dependencies

  D6                      Documentation           Broken links, missing
                          consistency review      index entries,
                                                  contradictory statuses
                                                  and stale references
                                                  checked

  D7                      Follow-up work plan     Proposed next work
                                                  packages, each with
                                                  scope, dependencies and
                                                  acceptance criteria
  -----------------------------------------------------------------------

The audit report and discrepancy register will distinguish facts
observed in the supplied repository from design intent in the Holy Grail
and from recommendations made during the audit.

## 7. Git workflow to be documented

The following workflow is **CONFIRMED by the project owner** and will be
documented as a project-wide working agreement.

### 7.1 Two mandatory milestones on the same branch

**Milestone A --- Plan/specification commit**

Before implementation starts, the responsible agent must:

-   Read the relevant Holy Grail documents and inspect the current
    implementation.
-   Define the problem, expected behaviour, scope and exclusions.
-   Identify affected files/modules, ownership boundaries and
    dependencies.
-   Specify data/interface/authority/persistence implications where
    relevant.
-   Define tests and measurable acceptance criteria.
-   List documentation updates required.
-   Commit and push the plan on the feature's working branch.

No implementation for that work package starts before this plan has been
pushed, except repository inspection needed to prepare the plan.

**Milestone B --- Implementation commit**

After implementation, the responsible agent must:

-   Implement against the approved plan.
-   Record any deviations and obtain approval for material scope/design
    changes.
-   Run the planned tests and relevant regression checks.
-   Update documentation affected by the actual result.
-   Provide test commands, outcomes and remaining limitations.
-   Commit and push the implementation milestone on the **same branch**.

### 7.2 Verification and approval

-   The responsible development agent performs and reports its own
    checks.
-   QA independently reviews the acceptance criteria, tests and relevant
    regressions, and records a validation result.
-   The project owner gives final approval.
-   A feature is not considered accepted or closed before owner
    approval.
-   If QA finds a defect, corrective commits are allowed on the same
    branch. The two milestones are mandatory checkpoints, not a limit of
    two commits.
-   A failed QA result must remain visible; it must not be represented
    as passed.
-   If the plan must materially change, the agent records the change and
    obtains the required approval before proceeding.

### 7.3 Commit message format

Use Conventional Commits with a required agent-attribution footer:

``` text
<type>(<scope>): <imperative summary>

Optional explanation of the change.

Agent: <AGENT_ID>
```

Examples:

``` text
docs(governance): plan repository documentation alignment

Define the audit scope, deliverables and acceptance criteria.

Agent: DOC
```

``` text
feat(world): implement biome placement rules

Implement the approved biome placement specification.

Agent: WORLD
```

The agent IDs are:

  Agent                            ID
  -------------------------------- ----------
  Documentation Architect          `DOC`
  System Architect                 `ARCH`
  Ecology                          `ECO`
  Pokémon Simulation               `POK`
  World Generation & Environment   `WORLD`
  Art & Asset Pipeline             `ART`
  Combat & Abilities               `COMBAT`
  Networking & Persistence         `NET`
  Tooling                          `TOOLS`
  Quality Assurance                `QA`

The `type` must reflect the actual change (`docs`, `feat`, `fix`,
`refactor`, `test`, `build`, `ci`, `chore`, etc.). The `scope` should
identify the affected domain or subsystem. The `Agent` footer identifies
the agent responsible for the commit; it must not imply that an agent
performed work it did not perform.

Branch naming and any additional footer fields will be proposed
separately if needed; they are not treated as confirmed by this plan.

## 8. Documentation integration targets

The Holy Grail v1 structure will be retained as the starting point. The
following areas are expected to be updated where the audit or Git
workflow requires it:

-   `00_GOVERNANCE/WORKING_AGREEMENTS.md`
-   `00_GOVERNANCE/CHAT_TEAM_STRUCTURE.md`
-   `00_GOVERNANCE/DOCUMENT_INDEX.md`
-   `00_GOVERNANCE/DOCUMENTATION_CHANGELOG.md`
-   New `00_GOVERNANCE/GIT_WORKFLOW.md`
-   `04_AI_INSTRUCTIONS/copilot-instructions.md`
-   `04_AI_INSTRUCTIONS/instructions/documentation.instructions.md`
-   Relevant specialist chat packs and templates, where the workflow
    needs to be explicit.

Other documents will only be changed when evidence shows they need
correction or clarification. Detailed explanations should have one
canonical home and be linked elsewhere.

## 9. Acceptance criteria

This work package is complete only when:

-   [ ] The Holy Grail documentation has been integrated into the
    repository without losing its canonical structure.
-   [ ] Every supplied project source file relevant to the audit has
    been inventoried and mapped to a domain or explicitly left
    unclassified with a reason.
-   [ ] Findings cite concrete repository evidence (paths and, where
    useful, symbols or line ranges).
-   [ ] No feature is labelled `IMPLEMENTED` solely because a class or
    file exists.
-   [ ] Confirmed decisions, proposals, open questions and verified
    implementation status remain distinct.
-   [ ] The Git workflow is documented consistently in its canonical
    file and relevant agent instructions.
-   [ ] The two-commit, same-branch process and QA/owner approval gates
    are unambiguous.
-   [ ] The document index and changelog are updated.
-   [ ] Internal Markdown links and referenced document paths are
    checked.
-   [ ] Contradictions and unknowns are recorded rather than silently
    resolved.
-   [ ] A final audit report and prioritized follow-up plan are
    delivered to the project owner.
-   [ ] The project owner has reviewed and approved the resulting
    documentation changes.

## 10. Risks and constraints

-   The supplied Unity archive is a snapshot and has no Git history or
    remote metadata.
-   The supplied archive does not include the repository's Markdown
    documentation.
-   No Blender project archive has been supplied; Blender pipeline
    conformance therefore remains UNKNOWN until its files are provided.
-   A source-only audit cannot prove runtime behaviour. Unity execution,
    tests or owner-provided verification may be required.
-   Network/server and persistence behaviour may not be verifiable if
    their implementation is not present.
-   Existing code/documentation discrepancies must be reported, not
    automatically "fixed" by changing either side.

## 11. Open questions

These are not blockers for beginning the repository and documentation
audit, but may block specific conclusions or later implementation work:

-   Which Unity Editor version is used by the working project, and does
    it match `ProjectVersion.txt` in the supplied snapshot?
-   Is there a separate Blender repository or workspace to inspect?
-   Are there tests or local development notes not included in the
    supplied archive?
-   Does the owner want a specific branch naming convention? No branch
    format is assumed in this plan.

## 12. Phase 1 / Phase 2 Git messages for this plan

**Phase 1 --- this plan**

``` text
docs(governance): plan Holy Grail integration and repository audit

Define audit scope, deliverables, Git workflow requirements and acceptance criteria.

Agent: DOC
```

**Phase 2 --- after the work is completed and verified**

``` text
docs(governance): integrate Holy Grail and publish repository audit

Integrate the documentation baseline, record verified repository findings,
update governance and provide the prioritized follow-up plan.

Agent: DOC
```

Use the Phase 2 message only after the deliverables and acceptance
criteria have actually been completed. QA's report and the project
owner's approval must be recorded before closure.

## 13. Change log

-   2026-11-16: Initial planning baseline created for Holy Grail
    integration, repository audit and project-wide Git workflow
    documentation.


## 14. Progress update — 2026-11-16

- Phase 1 planning artifact: owner reports pushed.
- Holy Grail v1 archive: received and inventoried (50 documentation files plus metadata).
- Unity project snapshot: received and statically inspected (217 files total; 26 C# scripts).
- Static audit: initial findings recorded in `../06_AUDIT/REPOSITORY_AUDIT_REPORT.md`.
- Documentation integration package: prepared for owner review; not yet committed or pushed.
- Unity Editor compilation/runtime verification and Blender pipeline audit remain outstanding.
