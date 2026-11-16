# Module Boundaries and Ownership

| Module | Owns | Does not own | Main collaborators |
|---|---|---|---|
| Pokemon | Individual identity, species profile use, lifecycle, needs, behaviour | Resource regeneration, terrain rendering, transport | Ecology, World, Combat, Server |
| Ecology | Resource lifecycle, food-web rules, habitat suitability, population ecology | Animation, player UI, mesh generation | Pokemon, World, Data |
| World | Sessions' spatial world, terrain, biome placement, environment queries | Species biology, combat resolution | Ecology, Art, Server |
| Combat | Attack execution, damage/effects, combat state | Daily needs, resource growth | Pokemon, Art, Server |
| Art | Model/rig/material/animation/VFX/audio content standards | Gameplay authority and ecological rules | All visual consumers |
| Networking | Transport, replication, authority, session connectivity | Domain-specific rules | All runtime domains |
| Persistence | Save/load lifecycle and serialization | Deciding gameplay rules | Server, Data, all domains |
| Tools | Editor automation, import/export, validation tooling | Reimplementing domain logic | Art, World, Data |
| QA | Tests, profiling, acceptance evidence | Unapproved feature design | All domains |

## Dependency rules

- Domain modules communicate through narrow interfaces, events, commands, or queries.
- Avoid cyclic dependencies. If A needs B and B needs A, introduce a contract or coordinator owned by an appropriate layer.
- Do not let UI call deep domain internals.
- Do not let rendering code mutate authoritative simulation state.
- Do not let editor tooling become a required runtime dependency.
- Shared contracts must remain small and stable.

## Suggested assembly boundaries

These are conceptual; actual `.asmdef` layout is an implementation task:
- `Pokemon.Shared`
- `Pokemon.Domain`
- `Pokemon.World`
- `Pokemon.Ecology`
- `Pokemon.Combat`
- `Pokemon.Client`
- `Pokemon.Server`
- `Pokemon.Editor`
- `Pokemon.Tests`

Do not create all assemblies blindly. Introduce them when dependencies and compile times justify them.
