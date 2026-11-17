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
| Visuals | Stylized 3D, Pokémon-inspired with manga influence; closer to Pokémon than to a strongly abstract low-poly style | CONFIRMED |
| Combat | Real-time | CONFIRMED |
| Hosting | Dedicated server | CONFIRMED |
| AI | API-based integration | CONFIRMED at high level; exact use OPEN |
| Team | One developer | CONFIRMED |
| Asset creation | Blender + scripts, imported into Unity | CONFIRMED workflow direction |
| Size variation | Runtime scaling of a base visual mesh | CONFIRMED design direction |
| Unity authoring | Prefer script-driven workflows; Inspector only for limited exceptions | CONFIRMED preference |
| Target platform | PC initially | CONFIRMED |
| Multiplayer priority | Not a priority for the initial development focus; do not remove the dedicated-server decision without a separate review | CONFIRMED direction |
| World scale target | Current 2 km × 2 km map is a test scale; long-term target is a finite, fully explorable globe/world without invisible boundary walls | CONFIRMED vision; technical representation OPEN |
| Initial biome scope | Plains, forest and desert; villages are settlements, not biomes | CONFIRMED initial scope |
| Population target | Design and profile the ecosystem around up to approximately 2,000 Pokémon as an initial target | CONFIRMED target; performance validation required |
| Game modes | Two modes: autonomous ecosystem exploration and a more classic Pokémon mode; selected simulation aspects are controlled by toggles | CONFIRMED direction; exact toggle list OPEN |

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
11. **Script-driven world authoring:** terrain, biome presentation, environment placement and scene setup must be reproducible through scripts; do not depend on manual scene painting or hand placement for normal world production.
12. **Integrated feature quality:** a feature is considered complete only when it is ready to integrate into the game, has a reproducible visual/demo scene where applicable, and has explicit acceptance criteria.

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

## 7. Modes and ecosystem simulation direction

The game is intended to support two modes:
- **Exploration / living-world mode:** the player is part of a world whose ecosystem continues to evolve independently of the player. Pokémon live their lives and the world simulation continues according to the selected rules.
- **Classic Pokémon mode:** a more traditional Pokémon experience, using toggles to disable or alter selected autonomous-world and simulation aspects.

The classic mode is expected to disable or alter more than autonomous movement: toggles may also disable ageing and other ecosystem/lifecycle systems. The exact toggle catalogue, dependencies between toggles, ownership of simulation time while paused/offline, and persistence behaviour remain OPEN and must be specified before implementing mode-specific branching throughout systems. The target is PC initially. Multiplayer is not a near-term priority; existing server-authority decisions must not be silently removed or contradicted.

## 8. Development completion standard

Features should be delivered at integration quality, not as isolated throwaway prototypes. Each substantial visual/system feature should have a reproducible demonstration scene or equivalent test harness, documented configuration, and measurable acceptance criteria. Normal authoring and generation workflows must be script-driven; manual authoring should be limited to explicitly approved exceptions.


## 9. Globe and seasonal world direction

The long-term world target is a **true spherical globe/planet** that the player can explore continuously and completely, returning to previously visited locations without invisible boundary walls. This supersedes the earlier ambiguity between a sphere and a wrapped planar world as the approved product direction; the technical implementation and topology details still require an architecture proposal. The current 2 km × 2 km world remains a test configuration.

The initial relief scope includes hills and mountains. More complex features such as snow-capped peaks, deep ravines and cave networks are not required for the first terrain pass unless separately approved.

Season changes must be real world-state variation, not only a cosmetic colour shift. They should be capable of affecting visual presentation and, once specified by the relevant domains, ecological conditions and resources. Exact season lengths, transition rules, weather coupling and per-biome effects remain OPEN.
