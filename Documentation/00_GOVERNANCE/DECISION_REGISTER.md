# Decision Register

This is the authoritative list of project decisions. Add entries; do not erase history. If a decision changes, mark the old entry superseded and add a new one.

| ID | Topic | Decision | Status | Notes |
|---|---|---|---|---|
| DEC-001 | Engine | Unity | CONFIRMED | Project baseline |
| DEC-002 | World model | Session-based | CONFIRMED | Session lifecycle details OPEN |
| DEC-003 | Simulation granularity | Individual Pokémon simulation | CONFIRMED | Scheduling/performance strategy OPEN |
| DEC-004 | Ambition | Playable game | CONFIRMED | Scope must remain solo-developer realistic |
| DEC-005 | Visual style | Stylized 3D | CONFIRMED | Detailed art bible OPEN |
| DEC-006 | Combat | Real-time | CONFIRMED | Detailed combat rules OPEN |
| DEC-007 | Hosting | Dedicated server | CONFIRMED | Networking stack/protocol OPEN |
| DEC-008 | AI | API-based | CONFIRMED at high level | Use cases, provider, fallback, privacy OPEN |
| DEC-009 | Team | One developer | CONFIRMED | AI assistants act as scoped collaborators |
| DEC-010 | Unity workflow | Script-driven by default | CONFIRMED | Inspector use limited |
| DEC-011 | Individual size | Scale base visual child at runtime | CONFIRMED | Root remains stable; collider/interaction scaling policy to define |
| DEC-012 | Blender pipeline | Reusable core + domain packages + source/export separation | PROPOSED | Existing pipeline needs repository verification |
| DEC-013 | Terrain technology | Unity Terrain initially considered | OPEN | Must be validated against world generation and chunk/session requirements |
| DEC-014 | Terrain rendering | Script-generated layers/materials and alphamaps if Unity Terrain selected | PROPOSED | Do not treat as final before DEC-013 |
| DEC-015 | Shared data | Shared contracts/models separated from client/server presentation | PROPOSED | Final assembly/package strategy OPEN |
| DEC-016 | Git workflow | Two mandatory milestone commits on the same branch: pushed plan before implementation, then implementation milestone; development agent + QA verify; project owner gives final approval | CONFIRMED | Conventional Commits with mandatory Agent footer; see GIT_WORKFLOW.md |

## Decision change procedure

1. Create a change proposal.
2. Explain why the current decision is insufficient.
3. Identify affected domains and documents.
4. Compare options and trade-offs.
5. Obtain project-owner approval.
6. Add a new decision entry; supersede the old one.
7. Update architecture, contracts, instructions, and implementation plan.
