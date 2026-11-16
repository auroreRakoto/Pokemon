# New Concept / New Feature Integration Workflow

Use this whenever a task introduces a concept not covered by the current documentation.

## Phase 1 — Identify the concept

Write a short concept note:
- What is the concept?
- Why is it needed?
- Is it gameplay, ecology, data, rendering, networking, tooling, production, or cross-cutting?
- Is it a new feature, a new implementation technique, or a new external dependency?
- What user-visible or simulation-visible behaviour changes?

Do not start by creating a class. First identify the responsibility.

## Phase 2 — Search existing knowledge

Check:
- `README.md` and document index;
- `PROJECT_CHARTER.md`;
- `DECISION_REGISTER.md`;
- `SYSTEM_ARCHITECTURE.md`;
- relevant domain docs;
- `INTERFACE_CONTRACTS.md`;
- current code and tests, if implementation exists.

Ask: is this truly new, or is it an extension of an existing system?

## Phase 3 — Classify impact

Mark each as YES/NO/UNKNOWN:
- changes individual Pokémon state?
- changes species data?
- changes resources or habitats?
- changes world generation or terrain?
- changes visuals, assets, animations, VFX, audio?
- changes client/server authority or replication?
- changes persistence or save schema?
- changes public interfaces or shared data models?
- adds a package, API, service, or runtime dependency?
- changes performance/memory/network budget?
- changes user/player experience or game rules?

UNKNOWN means investigate before implementation.

## Phase 4 — Proposal

For cross-domain or architectural features, create a proposal using `CHANGE_PROPOSAL_TEMPLATE.md`.

A proposal must include:
1. Problem and motivation.
2. User-facing/gameplay result.
3. Domain owner.
4. Proposed responsibility and boundaries.
5. Data model changes.
6. Interfaces/events/queries required.
7. Authoritative owner (client/server/editor/tool).
8. Persistence and network implications.
9. Visual/audio implications.
10. Performance risks.
11. Tests and acceptance criteria.
12. Documentation files to update.
13. Alternatives and rejected options.
14. Open questions.

## Phase 5 — Review dependencies

Ask each impacted specialist to review only its affected boundary. DOC/ARCH coordinate the review.

No implementation should proceed on an unresolved critical interface, authority, save compatibility, or data ownership question.

## Phase 6 — Approve and implement

- Project owner approves the design and any decision changes.
- Add a decision-register entry if an architectural/product decision is made.
- Define or update contracts before parallel implementation.
- Implement in small increments.
- Add tests and validation.
- Keep changes within the approved scope.

## Phase 7 — Documentation update

Update, as applicable:
- domain concept document;
- architecture and module map;
- data schema;
- interface contracts;
- testing strategy;
- performance budgets;
- content/asset guidelines;
- Copilot instructions if a durable coding rule is introduced;
- decision register;
- changelog.

Do not copy the same detailed explanation into many files. Put detail in one canonical document and link to it elsewhere.

## Phase 8 — Integration and closure

A feature can be marked complete only when:
- implementation is verified;
- relevant tests pass;
- cross-domain contracts match;
- data migration/persistence is considered;
- server authority is correct;
- documentation is updated;
- limitations and deferred work are recorded.

## Documentation update checklist

- [ ] Does the document describe current intended behaviour?
- [ ] Are confirmed decisions separated from proposals?
- [ ] Are data fields and ownership clear?
- [ ] Are inputs, outputs, events and failure cases documented?
- [ ] Are dependencies and affected domains listed?
- [ ] Are client/server responsibilities clear?
- [ ] Are performance implications considered?
- [ ] Are tests and acceptance criteria defined?
- [ ] Are links and document index updated?
- [ ] Is the changelog updated?
