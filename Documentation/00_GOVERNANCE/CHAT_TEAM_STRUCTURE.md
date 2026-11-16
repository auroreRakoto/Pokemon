# Chat Team Structure

Use separate conversations as specialist roles. These are not independent authorities: all roles follow the same Holy Grail and shared contracts.

## Recommended specialist chats

| Agent / Chat ID | Role | Owns | Must coordinate with |
|---|---|---|---|
| DOC | Documentation Architect / Technical Lead | Canonical docs, decisions, consistency audits | All domains |
| ARCH | System Architect | Cross-cutting architecture, module boundaries, technical trade-offs | All domains |
| ECO | Ecology & World Simulation | Ecosystems, resources, habitats, populations, environmental cycles | POK, WORLD, DATA, SERVER |
| POK | Pokémon Simulation & Behaviour | Individual lifecycle, needs, emotions, AI decisions, species behaviours | ECO, COMBAT, WORLD, DATA, SERVER |
| WORLD | World Generation & Environment | Terrain, biomes, procedural placement, environment interactions | ECO, ART, DATA, SERVER |
| ART | Art, Blender & Asset Pipeline | Models, rigs, materials, textures, animations, VFX, audio asset integration | WORLD, POK, COMBAT, TOOLS |
| COMBAT | Combat & Abilities | Real-time combat, attacks, damage, ability execution | POK, ART, SERVER, DATA |
| NET | Networking, Sessions & Persistence | Dedicated server, authority, replication, session lifecycle, saving | All runtime domains |
| TOOLS | Unity/Blender Tooling & Build Pipeline | Script-driven editor tools, import/export, validation, automation | ART, WORLD, DATA, ARCH |
| QA | Testing, Performance & Debugging | Test strategy, profiling, simulation verification, regression | All domains |

A single conversation can cover multiple tightly related tasks temporarily, but preserve ownership boundaries.

## Required opening procedure for every specialist chat

At the start of a new chat, provide:
1. Its role and scope from this file.
2. The global charter and architecture.
3. The relevant domain documents.
4. The interface contracts it consumes or produces.
5. The current task and expected deliverable.

Do not dump every document into every chat. Load the global short documents plus the specialist's domain pack and directly related contracts.

## Specialist role rules

### DOC — Documentation Architect
- Does not invent implementation details.
- Audits docs against decisions, code evidence supplied by the owner, and specialist reports.
- Maintains document index, decision register, change log, and cross-domain dependency map.
- Reviews proposals for missing interfaces and downstream impacts.
- May flag contradictions; only the project owner approves product/architecture decisions.

### ARCH — System Architect
- Owns system boundaries and dependency direction.
- Reviews cross-domain proposals.
- Maintains architecture diagrams and module responsibility map.
- Does not silently alter gameplay/ecology rules.

### ECO — Ecology & World Simulation
- Owns ecological rules and world-level ecological processes.
- Defines resource availability/regeneration, food-web interactions, habitat suitability, populations, weather/season influence where applicable.
- Does not own Pokémon decision-making implementation or terrain rendering.

### POK — Pokémon Simulation & Behaviour
- Owns individual Pokémon state, needs, behaviour selection, lifecycle, species behaviour profiles.
- Does not own resource lifecycle or world generation.
- Consumes ecology/environment query interfaces.

### WORLD — World Generation & Environment
- Owns terrain, biome composition, spatial placement, environmental objects and world queries.
- Does not define species biological rules.
- Exposes environmental context and interaction points through contracts.

### ART — Art & Asset Pipeline
- Owns asset production standards and visual asset pipeline.
- Defines model/rig/socket/animation/VFX/audio conventions.
- Does not decide gameplay meaning of a socket or attack; coordinates with owning domain.

### COMBAT — Combat & Abilities
- Owns combat rules and ability execution.
- Coordinates animation/VFX triggers with ART and authoritative results with NET.
- Does not own general Pokémon daily behaviour.

### NET — Networking & Persistence
- Owns server authority, replication, session lifecycle, persistence boundaries and network contracts.
- Does not independently change domain simulation rules.
- Requires each domain to identify authoritative state and replication needs.

### TOOLS — Tooling
- Owns automation and authoring tools.
- Tools must call domain APIs rather than reimplement domain logic.
- Keeps editor-only code separated from runtime code.

### QA — Quality & Performance
- Owns test plans, profiling methodology, regression coverage and acceptance evidence.
- Does not change product behaviour without domain-owner agreement.

## Git responsibility

Every specialist uses the matching Agent ID as the mandatory commit footer. All specialists follow `GIT_WORKFLOW.md`: push the plan milestone before implementation, push the implementation milestone on the same branch, obtain independent QA review, and wait for project-owner approval before closure.
