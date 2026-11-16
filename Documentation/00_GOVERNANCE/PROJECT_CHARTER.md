# Project Charter

## 1. Vision

Create a playable, stylized 3D Pokémon ecology game in which Pokémon are individuals living in a persistent-feeling ecosystem within session-based worlds. The world should feel inhabited: Pokémon seek food and water, rest, move through habitats, interact with resources and other individuals, reproduce, grow, age, and participate in ecological relationships.

The project is not merely a battle simulator or a static collection game. Ecological behaviour and the everyday life of Pokémon are central pillars. Combat exists and is real-time, but it is one system among several.

## 2. Confirmed project decisions

| Topic | Decision | Status |
|---|---|---|
| Engine | Unity | CONFIRMED |
| World/session model | Session-based world | CONFIRMED |
| Simulation | Individual Pokémon simulation | CONFIRMED |
| Ambition | Playable game | CONFIRMED |
| Visuals | Stylized 3D | CONFIRMED |
| Combat | Real-time | CONFIRMED |
| Hosting | Dedicated server | CONFIRMED |
| AI | API-based integration | CONFIRMED at high level; exact use OPEN |
| Team | One developer | CONFIRMED |
| Asset creation | Blender + scripts, imported into Unity | CONFIRMED workflow direction |
| Size variation | Runtime scaling of a base visual mesh | CONFIRMED design direction |
| Unity authoring | Prefer script-driven workflows; Inspector only for limited exceptions | CONFIRMED preference |

## 3. Design principles

1. **Ecology first-class:** needs, behaviours, resources, habitats, populations, and lifecycle are core systems.
2. **Individual identity:** each simulated Pokémon has individual data and state; species data is shared.
3. **Separation of concerns:** simulation, gameplay, visuals, audio, networking, persistence, and tools must not collapse into one monolith.
4. **Server authority:** the dedicated server owns authoritative game state and validates consequential actions.
5. **Data-driven content:** species, biomes, resources, abilities, and tuning should be represented as data where practical.
6. **Script-driven workflow:** automate repeatable setup, generation, validation, and import tasks.
7. **Reusable assets:** use shared materials, textures, rigs, animations, VFX, and generator utilities where appropriate.
8. **Incremental scope:** build a small vertical slice before expanding content.
9. **Observable simulation:** provide debug views, logs, metrics, and reproducible tests.
10. **Documented evolution:** new systems must update contracts and relevant documentation.

## 4. Non-goals and constraints

- Do not assume every Pokémon species requires a unique bespoke simulation implementation.
- Do not create a separate model for every individual size.
- Do not simulate every blade of grass or every decorative object as an ecological entity.
- Do not put all behaviour in a single `PokemonController`.
- Do not make the client authoritative over valuable or consequential world state.
- Do not add a third-party framework or service without documenting the reason and trade-offs.
- Do not invent Pokémon canon facts where the design needs a researched species-specific answer; mark unknowns and research them.

## 5. Product quality goals

- Coherent ecosystem behaviour over time.
- Stable and understandable individual Pokémon state.
- Clear separation between authoritative simulation and visual presentation.
- Procedural content that is reproducible from a seed and configuration.
- Assets that are validated before entering the game.
- A development workflow manageable by one developer.
- Architecture that permits additional species and biomes without rewriting core systems.

## 6. Definition of a vertical slice

The first meaningful vertical slice should demonstrate a small world containing a limited number of species and resources. At minimum it should eventually demonstrate:
- world/session startup;
- terrain and biome presentation;
- spawn of individual Pokémon;
- needs and behaviour loops;
- finding and consuming a resource;
- resting/sleeping;
- basic interaction with environment;
- server-authoritative state;
- persistence or a clearly scoped session lifecycle;
- basic player observation/interactions;
- debug tooling and tests.

Exact scope and acceptance thresholds remain OPEN until planned in the roadmap.
