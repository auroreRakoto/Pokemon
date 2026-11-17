# Repository Audit — Action Register

- **Source:** `REPOSITORY_AUDIT_REPORT.md`
- **Status:** PROPOSED follow-up actions; not implementation authorization

| Priority | Action ID | Related finding | Owner | Proposed action | Approval / dependency |
|---|---|---|---|---|---|
| P0 | ACT-001 | AUD-001 | POK + DOC | Open Unity and capture the exact compiler diagnostic for `EntityId` / `GetEntityId`; define the correct stable identity contract if missing. | Requires Unity compile evidence; contract change requires ARCH/NET review if shared. |
| P1 | ACT-002 | AUD-006 | ECO + POK + ARCH | Resolve whether the vegetation grid and fruit bushes are separate resource classes or competing food systems; define ownership and consumption contracts. | Owner approval before implementation. |
| P1 | ACT-003 | AUD-004, AUD-011 | POK + DATA/ARCH | Specify stable individual identity and the individual runtime data model (age/life stage, sex if used, size, needs, lifecycle, persistence fields). | Cross-domain proposal; owner approval. |
| P1 | ACT-004 | AUD-002 | QA | Add deterministic tests for needs bounds, feeding, resource regeneration, reproduction and mortality transitions. | Requires compilable project and agreed rule expectations. |
| P1 | ACT-005 | AUD-003, AUD-012 | NET + ARCH | Prepare the server-authority/networking implementation proposal and identify required domain contracts. | Networking stack and hosting decisions remain OPEN. |
| P2 | ACT-006 | AUD-005 | POK + ARCH | Propose a staged separation of PokemonAgent responsibilities without changing behaviour. | Preserve current behaviour; tests should precede refactor. |
| P2 | ACT-007 | AUD-007 | WORLD + ECO | Define a world/session seeded random-stream strategy covering all procedural placement. | Decide determinism scope and generator version policy. |
| P2 | ACT-008 | AUD-008 | WORLD + TOOLS | Confirm intended startup scene and build-scene configuration. | Owner confirmation if intended scene is unclear. |
| P2 | ACT-009 | AUD-009, AUD-010 | WORLD + ARCH | Document current raster prototype boundaries and future terrain/biome integration requirements. | Terrain representation remains OPEN. |
| P2 | ACT-010 | AUD-013 | TOOLS + WORLD | Verify `WorldDefinition` ScriptableObject import and consider a safe filename correction. | Preserve `.meta` GUID and verify assets in Unity. |
| P3 | ACT-011 | AUD-014 | ART | Supply Blender project/workspace for pipeline audit. | Requires project files. |
| P3 | ACT-012 | AUD-015 | ARCH + TOOLS | Reassess assembly definitions after module dependencies stabilize. | No blind assembly creation. |
| P3 | ACT-013 | AUD-012 | COMBAT / NET / POK | Create separate scoped specifications for combat, persistence/session lifecycle and API AI when their roadmap milestones are reached. | Each requires its own plan milestone and owner approval. |

No action in this table authorizes immediate code changes. Each substantial action must follow `00_GOVERNANCE/GIT_WORKFLOW.md` and the relevant change-proposal process.


## 2026-11-17 — Implementation status update

| Action ID | Updated status | Evidence / remaining work |
|---|---|---|
| ACT-001 | SOURCE FIXED; UNITY VERIFY OPEN | Missing `EntityId`/`GetEntityId()` reference replaced by `PokemonIndividualId`; Unity compile still required. |
| ACT-002 | PARTIALLY RESOLVED | Continuous vegetation biomass and discrete fruit bushes documented as separate resource models. Vegetation consumption by Pokemon remains future work. |
| ACT-003 | PARTIALLY IMPLEMENTED | Runtime stable individual ID added. Full age/life-stage/sex/size/persistence schema remains open. |
| ACT-004 | OPEN | No automated C# test assembly added; Unity compilation is prerequisite. Data validation script passes. |
| ACT-005 | OPEN | Networking/server authority remains unimplemented. |
| ACT-006 | IMPLEMENTED AT SOURCE LEVEL | `PokemonAgent` decomposed into cohesive individual systems and action behaviours; Unity compile/Play Mode verification remains open. |
| ACT-007 | PARTIALLY IMPLEMENTED | World generation, initial spawn placement, fruit-bush placement and grass selection are seed-driven; runtime wandering/schedule randomness is not replay-deterministic. |
| ACT-008 | OPEN | Meadow scene references repaired; Build Settings still selects SampleScene. Owner must confirm intended startup scene. |
| ACT-010 | SOURCE FIXED; UNITY VERIFY OPEN | `WorldDefinition.cs` naming corrected and GUID retained. |

The original proposal table remains historical. This update records implementation evidence and does not claim Unity verification.
