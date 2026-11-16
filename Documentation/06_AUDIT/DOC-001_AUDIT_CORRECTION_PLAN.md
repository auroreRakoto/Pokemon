# DOC-001 --- Audit Correction Plan

-   **Work package:** DOC-001
-   **Owner:** DOC --- Documentation Architect
-   **Reviewer:** ARCH
-   **Status:** PLANNED
-   **Date:** 2026-11-16
-   **Source:** `ARCH-001_Agent_Handoff_Pack.zip`, especially
    `Agent_Packets/DOC-001_AUDIT_CORRECTIONS.md`
-   **Code changes:** NOT AUTHORIZED
-   **Canonical documentation owner:** DOC

## 1. Objective

Correct the initial static repository audit so it reflects the project
owner's clarification, withdraws the unsupported `EntityId` /
`GetEntityId()` compile-blocker inference, and distinguishes
owner-reported manual verification from independent QA evidence.

This plan is the Phase 1 planning milestone. It does not itself change
the audit report or authorize runtime-code changes.

## 2. Evidence and provenance

### 2.1 Owner-reported evidence

The project owner explicitly reports that:

-   The current Unity project compiles in the owner's Unity environment.
-   The project launches.
-   `EntityId` and `GetEntityId()` work in the current project context.
-   Chenipotte and Mystherbe successfully find and consume berries.
-   Grass instantiation is faulty, but is not a current priority.

These statements will be labelled **OWNER-REPORTED MANUAL
VERIFICATION**. They are not to be described as independently reproduced
QA results.

### 2.2 Static audit evidence

The initial audit was performed against a supplied ZIP snapshot without
Unity Editor execution, compilation, Play Mode testing or Git metadata.
Static inspection did not locate a declaration/provider for `EntityId` /
`GetEntityId()` in the archive. That absence was insufficient to
conclude that the Unity project could not compile in the owner's actual
Unity/project context.

The correction must explicitly explain this limitation rather than imply
that the original static search was a valid compile test.

## 3. Exact files to change

### A. `Documentation/06_AUDIT/REPOSITORY_AUDIT_REPORT.md`

1.  **Executive summary**
    -   Remove the statement that the snapshot has an unresolved
        apparent compile blocker.
    -   Add the owner-reported compile, launch and berry-feeding
        verification, clearly marked as owner-reported manual evidence.
    -   Retain the limitation that DOC did not independently run Unity
        or QA tests.
2.  **Findings table --- AUD-001**
    -   Preserve the finding ID for traceability.
    -   Change severity/status to `INFO / RESOLVED — OWNER-REPORTED`.
    -   State that the initial static inspection failed to identify the
        Unity-provided identity API/context.
    -   Withdraw the inferred compile-blocker conclusion and the P0
        identity-replacement recommendation.
    -   Record that the owner reports `EntityId` / `GetEntityId()` work
        in the current project context.
3.  **Findings table --- AUD-004**
    -   Remove the unsupported claim that the `IndividualID` accessor
        depends on unresolved symbols.
    -   Do not recommend replacing the current identity mechanism.
    -   Reassess the remaining concern separately: the absence of a
        complete individual persistence/data schema may still be
        relevant, but belongs with the documented data-model/persistence
        gaps (AUD-011/AUD-012), not a compile or identity-accessor
        blocker.
    -   If the remaining concern cannot be supported by the inspected
        source, mark it UNKNOWN or supersede it rather than retaining an
        unsupported claim.
4.  **Grass placement**
    -   Add an explicit note that grass instantiation is a known
        owner-reported issue, deferred by priority.
    -   Do not classify it as a migration blocker or elevate it above
        the agreed work packages.
5.  **Verification limitations and conclusion**
    -   State that the owner reports the project compiles, launches and
        supports berry consumption for Chenipotte and Mystherbe.
    -   State that independent QA reproduction remains pending.
    -   Keep the overall audit conclusion appropriately scoped to the
        supplied snapshot and unverified systems.

### B. `Documentation/06_AUDIT/REPOSITORY_ACTION_REGISTER.md`

1.  **ACT-001**
    -   Mark as `CLOSED — SUPERSEDED BY OWNER VERIFICATION` or
        equivalent.
    -   Explain that it was a compile-diagnostic task based on an
        incorrect static inference.
    -   Explicitly state that no identity rewrite is authorized from
        AUD-001.
2.  **ACT-003**
    -   Retain only if supported as a separate proposal for the
        individual runtime/persistence data model (e.g. life stage, sex
        if used, size, needs and lifecycle fields).
    -   Remove any implication that the current `IndividualID` accessor
        is broken.
    -   Keep this distinct from the withdrawn compile issue.
3.  **ACT-002, ACT-006 and ACT-007**
    -   Keep as proposed tasks, with no implementation authorization:
        -   resource ownership review;
        -   staged `PokemonAgent` decomposition;
        -   deterministic random strategy.
4.  **QA baseline regression**
    -   Add a QA-owned action to capture the current observable baseline
        before refactoring.
    -   Include successful berry finding/consumption by Chenipotte and
        Mystherbe as mandatory regression scenarios.
    -   Include relevant current needs, sleep, mortality, reproduction
        and simulation-clock behaviour where reproducible.
    -   Record current scene/setup and manual steps.
    -   Do not mark these scenarios QA-passed until QA actually runs and
        records them.
5.  Ensure all actions remain proposals requiring their own plan
    milestone and owner approval before implementation.

### C. `Documentation/00_GOVERNANCE/DOCUMENTATION_CHANGELOG.md`

Append a dated correction entry. Do not rewrite or erase the existing
initial-audit history.

Proposed entry:

> 2026-11-16 --- Corrected AUD-001 following owner-reported Unity
> compile/launch verification and successful Chenipotte/Mystherbe berry
> consumption. Reclassified the earlier `EntityId` / `GetEntityId()`
> static inference as resolved by owner report, not independent QA
> evidence; deferred grass instantiation issue recorded; QA baseline
> regression action added.

### D. `Documentation/README.md`

Update the Repository audit paragraph to note that: - the initial static
audit has a subsequent owner-evidence correction; -
compile/launch/berry-feeding verification is owner-reported; -
independent QA verification remains pending.

### E. Other documents

-   Do **not** change `DECISION_REGISTER.md`: no new product or
    architecture decision is being approved by this correction.
-   Do **not** change `INTERFACE_CONTRACTS.md`: no approved
    eating-contract change has been proposed by ECO/POK.
-   Do **not** change architecture/domain documents merely to record
    this audit correction.
-   Update `DOCUMENT_INDEX.md` only if the actual file layout or audit
    links change; no layout change is planned.

## 4. Status changes

  ------------------------------------------------------------------------
  Item                    Before                  After
  ----------------------- ----------------------- ------------------------
  AUD-001                 CRITICAL / UNKNOWN      INFO / RESOLVED ---
                          pending Unity compile   OWNER-REPORTED

  ACT-001                 P0 compile diagnostic   CLOSED --- SUPERSEDED BY
                          and possible identity   OWNER VERIFICATION
                          contract task           

  AUD-004                 PARTIAL, incorrectly    Reassess as
                          coupled to unresolved   data-model/persistence
                          identity symbols        concern only; otherwise
                                                  supersede

  Grass instantiation     Mentioned as placement  Explicit known issue,
                          limitation              deferred and not a
                                                  migration blocker

  Berry feeding           Static source evidence  Owner-reported
                          only                    successful manual
                                                  behaviour; independent
                                                  QA pending

  QA baseline             No dedicated action     New QA-owned baseline
                                                  regression action
  ------------------------------------------------------------------------

## 5. Cross-document consistency

-   `AUD-001` status and evidence provenance must agree between the
    audit report and action register.
-   `AUD-004`, `AUD-011` and `AUD-012` must not duplicate or contradict
    one another.
-   The action register must not re-open identity replacement as a
    consequence of the withdrawn compile concern.
-   The changelog and README must describe owner-reported evidence
    accurately.
-   The Decision Register remains unchanged.
-   All future architecture proposals remain PROPOSED/OPEN until owner
    approval.

## 6. Unresolved points

1.  QA has not independently reproduced the owner's compile, launch and
    berry-feeding observations. This is explicitly pending, not a reason
    to keep the disproven static compile suspicion open.
2.  The exact scope of the individual runtime/persistence data model
    remains for POK/ARCH/DATA to assess; it must not be conflated with
    the existing `IndividualID` accessor.
3.  The grass-instantiation defect is acknowledged but intentionally
    deferred; no diagnosis or fix is part of DOC-001.

## 7. Acceptance criteria

-   [ ] AUD-001 no longer appears as an active compile blocker.
-   [ ] The report explains why the original static inference was
    insufficient.
-   [ ] Owner-reported evidence is clearly distinguished from QA
    evidence.
-   [ ] No replacement identity implementation is recommended from
    AUD-001.
-   [ ] AUD-004 is corrected or superseded without losing any separately
    supported data-model concern.
-   [ ] ACT-001 is closed/superseded.
-   [ ] ACT-002, ACT-006 and ACT-007 remain proposed.
-   [ ] A QA baseline regression action is added, including berry
    feeding by both species.
-   [ ] Grass instantiation is recorded as known and deferred, not a
    migration blocker.
-   [ ] Changelog history is preserved and a correction entry appended.
-   [ ] README audit summary is consistent.
-   [ ] Decision Register and unrelated contracts are not changed.
-   [ ] Markdown links and status references are checked.
-   [ ] ARCH review and project-owner approval are recorded before
    closure.

## 8. Git milestones

### Phase 1 --- This plan

``` text
docs(audit): plan correction of initial repository audit

Record owner-reported Unity verification and define the audit corrections.

Agent: DOC
```

Push this plan milestone before applying the documentation corrections.

### Phase 2 --- Corrected audit

``` text
docs(audit): correct repository audit using owner verification

Withdraw the unsupported identity compile blocker, distinguish owner
evidence from QA evidence, and update actions and audit history.

Agent: DOC
```

Use the Phase 2 message only after all acceptance criteria are met and
the corrected documents have been reviewed. QA review may occur later as
the owner has chosen to run QA manually; until QA evidence exists, the
documents must continue to label it as pending.

## 9. Change log

-   2026-11-16: DOC-001 correction plan created from the ARCH handoff
    pack.
