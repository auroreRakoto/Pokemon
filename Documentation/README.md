# Pokémon Ecology — Holy Grail Documentation

**Purpose:** the canonical project handbook for architecture, implementation, collaboration, and documentation maintenance.

## How to use this repository

1. Read `00_GOVERNANCE/PROJECT_CHARTER.md`.
2. Read `00_GOVERNANCE/DECISION_REGISTER.md` and `01_ARCHITECTURE/SYSTEM_ARCHITECTURE.md`.
3. Open only the domain documents relevant to the task.
4. Before changing code, inspect the current implementation and its tests.
5. If a task changes behaviour, data, interfaces, architecture, tooling, or constraints, update the relevant documentation in the same change.
6. Use `00_GOVERNANCE/CHANGE_PROPOSAL_TEMPLATE.md` for cross-domain or architectural changes.
7. Follow `00_GOVERNANCE/GIT_WORKFLOW.md` for every substantial work package and every commit.

## Status vocabulary

- **CONFIRMED** — explicitly selected by the project owner.
- **PROPOSED** — recommended design, not yet approved.
- **OPEN** — decision needed before implementation or before the relevant milestone.
- **IMPLEMENTED** — exists in the repository and has been verified.
- **DEPRECATED** — retained for historical context; do not use for new work.

A decision can be CONFIRMED without being IMPLEMENTED. Never confuse the two.

## Current project baseline

- Engine: Unity.
- World: session-based.
- Pokémon simulation: individual-based.
- Target ambition: playable game.
- Visual direction: stylized 3D.
- Combat: real-time.
- Hosting: dedicated server.
- AI: API-based component/integration (exact responsibilities and provider remain OPEN).
- Team: one human developer, assisted by separate AI conversations/tools.
- Current early species discussed: Chenipotte, Mystherbe, Roucool.
- Preferred workflow: script-driven; minimize manual Inspector setup.
- Blender is used for procedural/source asset creation and exports to Unity.
- Individual size variation should normally reuse one base mesh and scale a dedicated visual child, not create a separate mesh per size.

## AI instruction deployment

The maintained instruction sources are in `04_AI_INSTRUCTIONS/`. The integration archive also places deployable copies at the repository root under `.github/` so GitHub Copilot can discover them. When changing these rules, update the maintained source and keep the `.github/` copies synchronized.

## Repository audit

The initial static audit of the supplied Unity snapshot is recorded in `06_AUDIT/REPOSITORY_AUDIT_REPORT.md`. It is evidence from a source archive, not a Unity Editor compile or runtime test. The report distinguishes observed repository facts from unknowns and recommendations.

## Important caveat

This handbook describes the intended architecture and rules. It is not proof that a system already exists. Verify the repository before assuming implementation.
