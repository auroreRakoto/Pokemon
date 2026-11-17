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
| DEC-017 | Procedural world fields | Generate Perlin Noise base fields for height, temperature, vegetation variation, water potential and settlement variation; derive moisture from water distance, height and temperature | CONFIRMED | Moisture is not an independent noise map. Use an exact two-pass separable Euclidean Distance Transform on the water mask, then a configurable moisture/evaporation model. Keep settlement suitability as a mandatory, separate map for villages, towns and large cities. See WORLD_AND_BIOMES.md and TERRAIN_AND_RENDERING.md. Terrain renderer (Unity Terrain vs custom mesh) remains OPEN |
| DEC-018 | Settlement road/path network | Generate a separate deterministic road/path network connecting selected villages, towns and cities using a settlement graph and terrain-aware A*/Dijkstra least-cost routing | CONFIRMED | Do not use Perlin Noise or straight Euclidean lines as the route algorithm. Use traversal costs for slope, water/flood risk, biome/ground and obstacles; output route polylines and a rasterized path mask. See WORLD_AND_BIOMES.md and WORLD_ARCHITECTURE.md |
| DEC-020 | World-generation configuration location | Keep source JSON configuration under the single project-level `Assets/Project/Data` directory; runtime loading/contracts/projections belong under `Runtime/DataServices` | CONFIRMED | Do not create a second `Data` directory under `Runtime`. The loader must work in Editor and player builds without coupling the generation core to file access or `UnityEditor`. See UNITY_ARCHITECTURE.md |

## Decision change procedure

1. Create a change proposal.
2. Explain why the current decision is insufficient.
3. Identify affected domains and documents.
4. Compare options and trade-offs.
5. Obtain project-owner approval.
6. Add a new decision entry; supersede the old one.
7. Update architecture, contracts, instructions, and implementation plan.

| DEC-019 | World generation tooling and visualisation | One shared runtime-safe generation core callable from Unity Editor and Play Mode, with flat/debug/height/asset visualisation, scene terrain instantiation and independent map export | CONFIRMED | Editor workflow must instantiate/update a real terrain GameObject in the active scene from generated data; Play Mode uses the runtime-safe scene adapter. Editor UI/export stays in Editor-only assemblies. See TERRAIN_AND_RENDERING.md and WORLD_ARCHITECTURE.md. |
