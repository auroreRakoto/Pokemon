# Spherical World Architecture Proposal

**Status: PROPOSED TECHNICAL ARCHITECTURE — requires prototype validation before implementation.**

## 1. Product requirements this architecture must satisfy

- The world is a true spherical globe, continuously explorable around the entire surface.
- The world must look almost flat from normal ground-level player and Pokémon viewpoints. Curvature must not be an obvious visual feature during ordinary ground exploration.
- Curvature should become perceptible from sufficiently high altitude, especially when riding/following a flying Pokémon and looking toward the distant horizon.
- This is a visual presentation requirement, not permission to fake a bounded planar world. The underlying world remains spherical and continuous at every altitude.
- Initial relief consists of hills and mountains.
- Terrain and environment generation remain deterministic and script-driven.
- The system must support PC, an initial ecosystem target around 2,000 Pokémon (subject to profiling), seasonal world changes, ponds/rivers, biome fields, and selected persistent environment-object modifications.

## Player camera, world scale and prototype boundary

The confirmed player camera direction is a Pokémon-style third-person camera, positioned at a distance with an adjustable viewing angle. Exact follow distance, pitch limits, rotation controls, and flying-camera behaviour remain open. Camera behaviour must be validated together with the ground-to-flight curvature requirement; do not tune the planet radius independently of the camera.

The long-term world target is a large planet with extensive regions and long-distance travel. The production radius, playable surface area, travel speeds, regional density and streaming budgets remain open. The 2 km × 2 km configuration is only a test setup and must not be treated as a scale target for the final planet.

The player is intended to both observe the autonomous ecosystem and participate as a Pokémon trainer. This is a confirmed gameplay direction, but trainer mechanics and their integration with ecosystem simulation are outside the first spherical-world prototype.

The first prototype is deliberately limited to a spherical planet, terrain, curvature presentation and player movement. It should validate the spatial foundation before ecosystem, trainer progression, combat or full procedural content is layered on top.

Performance is to be profiled against the project owner's own PC. No universal minimum PC specification is required at this stage. Record the actual machine configuration when profiling begins; do not claim compatibility or performance before measurements exist.

## 2. Recommended spatial topology: Cube-Sphere

Use a **Cube-Sphere** as the initial topology candidate: six square cube faces are mapped onto a sphere. Each face is represented by a regular parameter domain and subdivided into terrain tiles. Convert face coordinates to a normalized 3D direction before sampling world fields or constructing surface positions.

Reasons for this choice:
- avoids the severe area distortion and singular polar behaviour of a single latitude/longitude texture grid;
- gives six manageable face domains with explicit neighbour relationships;
- supports deterministic tile addressing and hierarchical subdivision;
- allows the same global spherical field to be sampled from every face, including face boundaries;
- is a practical fit for a custom mesh renderer and adaptive terrain streaming.

The mapping must be centralized in a tested spatial-coordinate module. No gameplay, ecology or rendering system should implement its own cube-face conversion. The exact cube-to-sphere mapping formula is an implementation detail to benchmark; use a mapping with acceptable area distortion and stable inverse/addressing operations.

## 3. World coordinates and terrain fields

Maintain a stable global spherical coordinate representation independent of render chunks. A world location should be expressible as a normalized direction from the planet centre plus a radial elevation/altitude value. Chunk-local vertex indices are presentation addresses only and must never become persistent Pokémon or resource identity.

For a surface direction `d` (normalized) and base radius `R`, terrain position is conceptually:

`P = (R + h(d)) * d`

where `h(d)` is the generated terrain elevation. Generate continuous base/derived fields by sampling deterministic 3D noise using the direction vector (or an equivalent seam-safe spherical domain), rather than evaluating unrelated 2D Perlin maps independently on each face. Existing conceptual fields remain: height, temperature, water, derived moisture, vegetation potential, biome, settlement suitability and traversal cost. Their algorithms must be adapted to spherical distances, neighbourhoods and drainage. Moisture remains derived from water proximity, terrain retention and temperature; it must not become an independent noise map.

Latitude may contribute to climate/season rules, but it must be derived from the global spherical coordinate system and combined with seeded climate variation and altitude. Avoid treating cube-face rows as latitude.

## 4. Terrain tiles, adaptive LOD and seams

Use a hierarchical quadtree-like subdivision of the six cube faces. Tiles are refined according to camera/player distance, projected screen-space error and relevant gameplay needs. Nearby ground receives high geometric detail; distant terrain uses progressively coarser representations. Flying viewpoints can request a larger visible radius and appropriate LOD without loading the whole high-resolution surface.

The tile system must define:
- stable tile IDs from face, level and integer tile coordinates;
- deterministic generation from world seed, generator version and tile ID;
- neighbour lookup across same-face edges and all six face boundaries;
- crack-free edge stitching or a proven seam strategy between different LOD levels;
- consistent normals/tangents across tile borders;
- asynchronous generation/cancellation and memory budgets;
- collision meshes or simplified collision representations only where gameplay needs them;
- explicit ownership and cleanup when tiles stream out.

Do not implement adaptive LOD until the fixed-resolution Cube-Sphere prototype proves face continuity. Start with a deliberately small fixed grid per face, then add subdivision.

## 5. Subtle curvature: critical camera and scale requirement

The planet must be spherical mathematically, but its curvature should be visually subtle at ground level. The player camera should behave relative to the local tangent frame: local up follows the surface normal, and camera/player orientation is stabilized against the local tangent plane. The camera must not expose a conspicuous curved horizon merely because the world is spherical.

At normal ground gameplay height, the visible play area should read like a broad, nearly planar Pokémon landscape. Distant hills/mountains should dominate the horizon; the ground should not visibly roll away around the player. From higher altitude, the camera's horizon and the larger visible surface should gradually reveal the globe curvature. There must be no abrupt altitude threshold or visual switch.

Do not hard-code a planet radius from intuition. Determine the radius and camera/altitude relationship through a scripted visual test scene. Evaluate multiple candidate radii and camera settings at: (a) ground exploration height, (b) ordinary jump/low flight, (c) high flying-Pokémon altitude, and (d) a far aerial view. Record screenshots and measurable camera/geometry parameters. Choose the smallest radius that passes the ground-level visual acceptance test while still allowing curvature to be clearly perceived at the intended high-flight view.

The visual acceptance test must include a long, flat-ish vista as well as hills/mountains; otherwise relief can hide unwanted curvature. Curvature perception should emerge continuously with altitude and view distance.

## 6. Gravity, movement and navigation

The physical/world up direction at a surface location is the normalized radial direction from the planet centre. A true spherical movement model therefore needs radial gravity and a local tangent frame for grounded movement, camera orientation and surface attachment. Flying movement may leave the surface and use a 3D path/navigation representation.

WORLD provides terrain elevation, slope, obstacles, surface normal, water and habitat/spatial queries. POK owns species-specific movement capabilities and decisions. Ground traversal, climbing and flight must not be collapsed into one universal walkability mask. A small ground Pokémon can be blocked by steep terrain while a flying species can cross it.

Before integrating the full Pokémon population, validate a player/controller, a grounded test creature and a flying test creature across face boundaries and around the poles.

## 7. Hydrology, climate and biome implications

The existing flat-grid EDT and planar downhill assumptions cannot be copied unchanged. Spherical hydrology needs globally consistent neighbour relationships, distances and flow directions across tile/face boundaries. Rivers must not terminate or jump at cube-face seams. Ponds/lakes must fit basins. The exact global hydrology representation (global coarse analysis grid, hierarchical drainage graph, or another method) is an implementation decision for a dedicated prototype.

Temperature/climate may use latitude, altitude, seeded regional variation and seasonal state. Biome classification remains data-driven and independent of mesh rendering. Initial biomes remain plains, forest and desert; villages remain settlements rather than biomes.

## 8. Persistent modifications

Keep base terrain generation immutable and deterministic. Initially represent gameplay modifications as separate persistent world state keyed by stable global object/feature IDs (e.g. a breakable rock or ice block), not as direct arbitrary deformation of the planet mesh. A tile rebuild must reapply relevant modifications. The persistence format must not depend on transient tile IDs alone if a feature can move between LOD levels.

## 9. Proposed module boundaries

- **Spherical coordinates / topology:** cube-face mapping, direction conversion, global position, neighbour/address queries.
- **World data generation:** deterministic spherical fields and derived environmental data; no Mesh/Renderer/UnityEditor dependencies.
- **Spherical hydrology and climate:** global water/drainage, water-distance and climate fields with seam-safe queries.
- **Terrain tile/LOD service:** tile selection, generation scheduling, stitching and streaming.
- **Terrain presentation adapter:** meshes, materials, normals, colliders and visual biome blending.
- **World query/navigation services:** surface, slope, traversal, habitat and water queries.
- **World state/persistence:** object/feature modifications and seasonal state.

The current `WorldGenerator` and `WorldTerrainBuilder` are planar: the generator stores width/height grids and the builder constructs an X/Z plane from them. Treat them as the existing test implementation, not as a spherical architecture to extend by adding a curvature transform. Preserve reusable field rules where valid, but introduce a spherical data/address contract and a dedicated terrain presentation path.

## 10. Implementation phases and acceptance criteria

1. **Coordinate/topology prototype:** six Cube-Sphere faces, direction mapping, global coordinate round trips and cross-face neighbour tests. No terrain art required.
2. **Fixed-resolution spherical mesh:** generate one continuous sphere and a simple deterministic height field. Validate no cracks, flipped faces, discontinuous normals or face-specific seams.
3. **Curvature/camera test:** scripted scene with ground camera, low flight, high flight and aerial views; compare candidate radii. Ground curvature must not be conspicuous; high flight must reveal the globe.
4. **Adaptive tile prototype:** quadtree LOD, streaming, edge stitching and stable tile IDs. Validate no cracks/popping at face and LOD boundaries.
5. **Spherical field/hydrology prototype:** height, temperature, water, moisture, biome and river continuity across all face boundaries and polar regions.
6. **Movement prototype:** radial gravity, grounded controller, flying controller and traversal queries across the whole sphere.
7. **World integration:** persistent feature state, seasons, settlement/path generation, ecosystem queries and final stylized rendering.

Every phase must be reproducible from a seed/configuration and have a dedicated automated or scripted validation scene. Do not claim production readiness from a visual-only sphere demo.

## 11. Open technical decisions

- Planet radius and final world scale: choose only after the curvature/camera test.
- Cube-to-sphere mapping variant and tile resolution: benchmark.
- Global hydrology/drainage data structure: prototype before selection.
- Exact camera follow distance, FOV, ground altitude and flying altitude: art/gameplay validation.
- Tile memory/frame-time budgets and target PC hardware: define before performance acceptance.
- Save format and stable IDs for persistent world features: coordinate with DATA/PERSISTENCE.
- Whether the dedicated-server architecture needs to host world generation or only authoritative world state: retain current authority decision and resolve in NET/ARCH; multiplayer is not an initial priority.

## 12. Initial implementation evidence (WORLD-001)

The first WORLD-001 code slice uses the normalized-cube projection as the initial Cube-Sphere mapping candidate: each face point is formed from its canonical face basis and normalized onto the unit sphere. The six face meshes sample one deterministic 3D-domain height field from the resulting unit direction, so face-specific 2D noise maps are not used. This confirms the initial topology/field implementation direction only; it does not yet validate mesh seams in Unity, terrain normals, camera curvature, player movement or production LOD.

The initial mesh generator assigns radial placeholder normals to keep face-edge normals consistent by construction. Slope-aware normals must be implemented and tested before the terrain presentation is considered visually complete.

## 13. Approval boundary

This proposal recommends Cube-Sphere + seam-safe spherical world fields + hierarchical adaptive terrain tiles + radial gravity. The user's product requirements (true globe, full exploration, subtle ground-level curvature and visible high-altitude curvature) are confirmed. The technical architecture remains a proposal until the topology, curvature/camera and tile-seam prototypes pass their acceptance criteria. No production scripts or configuration should be migrated before this review.
