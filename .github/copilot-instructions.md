# Copilot Instructions — Pokémon Ecology

This repository contains a solo-developed stylized 3D Pokémon ecology game using Unity and Blender. Read `Documentation/00_GOVERNANCE/PROJECT_CHARTER.md` and the relevant domain documentation before substantial changes.

## Confirmed baseline
- Unity; session-based world; individual Pokémon simulation.
- Playable-game ambition; stylized 3D; real-time combat.
- Dedicated server direction.
- API-based AI direction, exact use cases/provider still open.
- Prefer script-driven Unity workflows.
- Individual size variation normally scales a dedicated visual child using one base mesh.

## Mandatory rules
- Distinguish CONFIRMED, PROPOSED, OPEN and IMPLEMENTED. Never claim a proposal is implemented.
- Inspect existing code and tests before creating or changing systems.
- Respect module ownership and interface contracts.
- Separate species definitions from individual state.
- Separate needs, physiology, emotions, behaviour and presentation.
- Keep server-authoritative outcomes off the client.
- Keep Unity Editor APIs out of runtime assemblies.
- Prefer reusable, idempotent editor tooling over manual repetitive Inspector setup.
- Keep Blender source files separate from exports.
- Do not create a separate mesh for ordinary size variation.
- Do not add dependencies or architectural patterns without justification.
- Update documentation and tests when behaviour, data, interfaces, tooling or architecture changes.
- For cross-domain changes, use the RFC/change proposal workflow and identify affected domains.

## Before coding
1. Identify the owning domain.
2. Read its domain document and relevant contracts.
3. Inspect current implementation.
4. List affected systems and risks.
5. Implement the smallest coherent change.
6. Test and update docs.

## Git workflow
- Follow `Documentation/00_GOVERNANCE/GIT_WORKFLOW.md` for every substantial work package.
- Before implementation, inspect and commit/push the plan milestone on the working branch.
- After implementation and checks, commit/push the implementation milestone on that same branch.
- Every commit must use Conventional Commits and include `Agent: <AGENT_ID>` as a footer.
- The development agent reports checks; QA independently validates; the project owner gives final approval before closure.

## Documentation
The canonical index is `Documentation/00_GOVERNANCE/DOCUMENT_INDEX.md`. Do not duplicate large explanations across documents; update the canonical owner document and link to it.
