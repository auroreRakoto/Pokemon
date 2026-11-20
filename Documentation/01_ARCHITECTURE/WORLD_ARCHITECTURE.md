# World Architecture

## Confirmed world model

The world is session-based. A session contains a world instance or a selected/generated world configuration, players, Pokémon, resources, environmental conditions and runtime state.


## World scope and player modes

The current 2,000 m × 2,000 m generation dimensions are for testing only. The long-term target is a finite, globe-like world that is fully explorable, without invisible boundary walls or artificial mountain walls at the map edge. Whether this means a true spherical surface, a wrapped continuous world or another topology remains OPEN and requires an architecture proposal before implementation.

The game targets PC initially. Multiplayer is not a near-term priority, but this does not by itself supersede the current dedicated-server/server-authority architecture decisions.

Two gameplay modes are intended:
- **Exploration / living-world mode:** the player is part of an autonomous ecosystem; Pokémon and world systems continue to evolve independently of the player's immediate actions/presence.
- **Classic Pokémon mode:** a more traditional Pokémon experience, with toggles controlling selected autonomous simulation/world aspects.

Classic-mode toggles may disable or alter autonomous ecosystem simulation, ageing and other lifecycle systems, not only autonomous movement. The exact toggle list, dependencies, simulation progression, persistence and pause/offline behaviour remain OPEN. Prefer configuration/policy switches at system boundaries over scattered mode-specific conditionals.

## Initial biome and settlement scope

Initial biomes: plains, forest and desert. Villages are settlements, not biomes. An initial village may be represented by at least one house/building marker; the broader settlement layout can be expanded later. Village generation may combine procedural placement/layout with authored assets. Both natural trails and constructed roads are intended. Ponds and rivers are in scope. Seasonal changes and a day/night cycle are required. Seasons must cause genuine world-state variation, not merely a cosmetic colour swap; the exact calendar, duration, transitions, weather coupling and per-biome ecological/resource effects remain OPEN. Initial terrain relief includes hills and mountains.

## Traversal capabilities and habitats

World navigation must support different movement capabilities. A ground Pokémon may be blocked by slopes/cliffs/mountains that a flying Pokémon can cross. The world domain supplies terrain/barrier and habitat queries; the Pokémon movement/behaviour domain owns the interpretation of a species' movement capabilities and decisions. Pokémon must be able to query suitable shelters, resting places, nests and cover. The shared capability taxonomy and numerical traversal rules remain OPEN and require coordination between WORLD and POK.

## Population and platform target

Plan for an initial ecosystem target of approximately 2,000 Pokémon on PC. This is an unvalidated design target. Simulation frequency/LOD, off-screen updates, persistence and hardware performance budgets must be profiled before claiming the target is achieved.

## World layers

1. **World definition:** overall configuration, seed, dimensions, session rules.
2. **Spatial representation:** terrain/mesh, chunks or regions, coordinate system.
3. **Biome regions:** forest, meadow, mountain, wetlands and other defined environments.
4. **Environment objects:** trees, bushes, rocks, logs, grass, flowers, water features.
5. **Ecological resources:** berries, seeds, nectar, insects or other resources with lifecycle and quantity.
6. **Dynamic conditions:** time, weather, temperature, humidity and season where implemented.
7. **Simulation state:** resource depletion, traces, population, habitat occupancy and other changes.

## Spherical world technical direction

The approved product requirement is a true, fully explorable sphere whose curvature is subtle from ordinary ground-level play and perceptible from high flying viewpoints. The recommended technical candidate is documented in `SPHERICAL_WORLD_ARCHITECTURE_PROPOSAL.md`: Cube-Sphere topology, deterministic seam-safe spherical fields, hierarchical adaptive terrain tiles, local tangent frames and radial gravity. This is a proposed architecture, not yet an implementation approval.

The current `WorldGenerator`/`WorldTerrainBuilder` path is a planar X/Z test implementation. Do not attempt to make it spherical by simply bending its final mesh. A new spherical coordinate/address contract and a spherical terrain presentation path must be validated first. Existing ecological field concepts and generation rules should be reused only where their planar distance/neighbour assumptions have been adapted.

The planet radius is intentionally not fixed yet. A scripted curvature test must compare ground-level, low-flight, high-flight and aerial views. At ground level the play area should read as a broad, nearly planar Pokémon landscape; at high altitude the curved horizon should become progressively apparent.

## Terrain generation model

Terrain and environment generation must be based on multiple distinct Perlin Noise maps combined through explicit rules. The world generator provides base noise fields for height, temperature, vegetation variation, water potential and settlement variation, plus derived fields for water distribution, distance-to-water and ecological moisture. Moisture is not an independent Perlin Noise field.

- **Height:** multi-octave Perlin Noise defines elevation and landforms.
- **Temperature:** a separate Perlin Noise field, adjusted by broad world-scale trends where available and by elevation.
- **Moisture:** a derived field calculated from Euclidean distance to the nearest water cell, terrain height/retention (and slope when available), and temperature-dependent evaporation. Do not generate an independent moisture Perlin Noise map.
- **Vegetation potential:** a separate Perlin Noise field combined with temperature, moisture and elevation suitability; it describes potential density, not individual plant placement.
- **Water:** a dedicated seeded Perlin Noise field combined with height and hydrological constraints. It identifies sea/ocean, lakes/ponds and rivers where supported. Lakes should fit plausible basins and rivers should follow downhill flow rather than being arbitrary noise-shaped patches. Its water mask is the source for the distance-to-water calculation.
- **Settlement suitability:** a mandatory, independently retained low-frequency seeded Perlin Noise field combined with height/slope, water access, flood risk, contiguous buildable area and access/connectivity where available. It produces candidate suitability for villages, towns and large cities; it does not generate streets or buildings.
- **Road/path network:** a separate mandatory derived network connecting the settlements actually selected from the settlement-suitability map. It is built from a settlement graph and a terrain traversal-cost field using A* or Dijkstra least-cost pathfinding; it is not generated by Perlin Noise or straight-line links.

Keep base fields and derived fields independently inspectable. Compute an exact Euclidean Distance Transform from the water mask in two separable passes (rows, then columns; square-root after the second pass), then derive moisture from water distance, height/terrain retention and temperature-dependent evaporation. Use height, temperature, derived moisture and water to classify biome regions; use vegetation potential to guide vegetation profiles and density. Use settlement suitability to select settlement sites, with large cities rarer and requiring larger, better-connected buildable areas than villages. Enforce deterministic spacing and exclusion rules. Preserve the settlement-suitability map as a separate output. Build a sparse road/path network only after settlement selection: a small village normally connects to one suitable nearby town/city; towns connect selectively to regional centres; large cities form the primary backbone. Do not connect every settlement to every other settlement. Reject links with no valid path. Build routes by minimizing accumulated terrain traversal cost (slope, cliffs/ravines, water/flood risk, biome/ground, obstacles), using A* or Dijkstra. Hard barriers such as cliffs and impassable water are forbidden, not merely expensive. Output route polylines plus a rasterized road/path mask. Actual settlement layouts, road surfaces and bridge construction are later presentation/build stages. Each field must use deterministic seed derivation/offsets from the world seed. Identical seed, generator version and settings must reproduce the same fields and derived world data.

The generation pipeline is: base noise fields → terrain height/surface → water/hydrology → Euclidean distance-to-water (two-pass exact EDT) → derived moisture (water distance + height/retention + temperature/evaporation) → vegetation potential → biome classification → settlement suitability map and site selection → settlement graph → terrain traversal-cost field → A*/Dijkstra road/path network → terrain material assignment → environment/resource/settlement/road presentation → validation.

## Distance transform and moisture model

The water-to-moisture dependency is mandatory: moisture must correlate with the generated water distribution and must not be generated from an unrelated noise map.

Use an exact separable Euclidean Distance Transform (EDT) on the water mask:
- row pass: one-dimensional squared Euclidean distance transform for every row;
- column pass: one-dimensional squared Euclidean distance transform for every column using the intermediate result;
- final square root and conversion to world units using X/Z cell spacing.

An exact 1D lower-envelope-of-parabolas implementation (such as Felzenszwalb–Huttenlocher) is preferred. A two-pass chamfer/8-neighbour approximation must be identified as approximate, not exact Euclidean distance.

Derive a normalized ecological moisture value from:
- distance to nearest water, using a configurable falloff such as `exp(-d / L)`;
- terrain height and, when available, slope/drainage, to estimate water retention;
- temperature, through a configurable evaporation/retention factor that generally reduces ground moisture at higher temperatures.

A representative formula is `M = clamp((a * exp(-d/L) + b * Hretention + Mbase) * E(T), 0, 1)`. All coefficients and thresholds must be configurable and versioned. This is ground/ecological moisture, not physical atmospheric relative humidity.

## Generation execution contract

The world generator must be callable explicitly in both Unity Editor and Play Mode. Both entry points use the same runtime-safe generation core; there must not be separate Editor and runtime implementations.

- **Editor:** a dedicated Editor window/inspector exposes the generation request and `Generate`, `Regenerate`, `Generate & Apply to Scene`, `Export Maps` and `Clear Preview` actions. It displays stage progress, warnings, validation and the exact seed/settings used. It works without entering Play Mode. `Generate & Apply to Scene` is a required workflow, not an optional future feature: it must create or update a real terrain GameObject in the currently open Unity scene from the generated height and presentation data. A preview shown only inside the Editor window does not satisfy this requirement.
- **Scene instantiation:** applying a generated world must create/update a clearly named root GameObject (for example, `GeneratedWorld`) and its terrain object(s), positioned and scaled from the world dimensions and cell spacing. The selected terrain backend (Unity Terrain or custom mesh; see the still-open terrain representation decision) must produce an actual visible surface in the scene, with an appropriate terrain collider for gameplay where applicable. Re-generating should update/replace the generator-owned output rather than accumulating duplicate terrain objects; use Unity Undo support for Editor-created scene changes. Keep generated data and scene presentation separate so the same result can be reapplied without rerunning the generation core.
- **Play Mode:** a runtime-safe generation service accepts the same request and can create a preview or instantiate/apply the generated terrain and other world presentation to the active session. The preview/apply decision is explicit.
- The shared core must not reference `UnityEditor`, `AssetDatabase` or Editor-only file APIs. Editor-specific UI, asset creation and file export are isolated in Editor assemblies.
- The request includes seed, generator version, world dimensions, map resolution, cell spacing, a complete settings snapshot, requested output modes and preview/apply policy.
- The result exposes every map independently, selected settlement sites, sparse graph edges, route polylines, raster route mask, validation and per-stage timing.
- All generation uses deterministic per-field seed derivation; do not rely on hidden inspector state or Unity global random state.

Generation stage order: validate request → generate base noise fields → height/slope → water/hydrology → exact two-pass Euclidean distance transform → derived moisture → vegetation → biome classification → settlement suitability and site selection → sparse settlement graph → traversal-cost/barrier field → A*/Dijkstra route generation → visual output → validation/publication.

The generator may skip unrequested presentation stages, but may not skip data dependencies. A road preview must still generate/obtain the selected settlement sites, graph, traversal costs and routes. Scene application is a presentation stage that consumes the generated result; it must not change the underlying maps or rerun their algorithms. The current 2,000 m × 2,000 m world size is a test configuration only. The long-term target is a finite, fully explorable globe-like world; its topology remains OPEN.

## Visualisation and export contract

Support independent visualisation modes over the same generated data:
- flat top-down thematic colour map with legend (water, coast, green areas, hills/mountains/cliffs, settlement classes and roads);
- individual raw/derived map debug views with value ranges and selectable grayscale/colour ramps;
- height-based 3D terrain view with adjustable visual exaggeration and optional slope/biome/wireframe overlays;
- asset presentation with final prefabs or clearly identifiable primitive/billboard placeholders;
- layered view with per-map visibility and opacity controls.

Changing visualisation mode must never alter the generated map values. Placeholder assets are presentation-only and can be replaced without changing generation contracts.

Every map is independently exportable at native resolution. Preserve continuous numeric precision (16-bit PNG or EXR where appropriate), encode categorical maps unambiguously, and optionally export lossless colour previews. Include metadata: seed, generator version, map name, dimensions, world dimensions, cell spacing, value range/encoding and settings. The Editor exporter is separate from the runtime core. Required independent outputs include height, temperature, water, distance-to-water, moisture, vegetation, biome, settlement suitability, traversal cost and road/path mask.

## Terrain representation

Unity Terrain generated by scripts is a reasonable initial candidate, but not yet a final decision. A custom mesh/chunk system may be more suitable depending on world shape, streaming, runtime edits, networking and target scale. Regardless of the backend selected, the Editor workflow must instantiate the generated terrain into the active Unity scene; a map preview or exported heightmap alone is insufficient. The current 2 × 2 km scene-application setup is a test configuration only. Keep the backend and scene adapter replaceable so the eventual finite globe-like world can be supported after its topology is selected.

The multi-map Perlin Noise specification is independent of the terrain renderer. Do not couple biome rules or field generation to a specific terrain representation. Biome data should describe ecological and visual properties; a terrain adapter translates these into Unity Terrain layers or mesh materials.

## Biome composition

A biome definition may include:
- ground material distribution;
- height/slope preferences;
- vegetation and object spawn profiles;
- resource spawn profiles;
- climate ranges;
- habitat availability;
- species suitability;
- visual palette and ambient presentation.

A biome is not just a texture or a painted colour on a map.

## Determinism

Procedural generation should accept explicit seeds and configuration. Given the same generator version, seed and configuration, generation should be reproducible where practical. Record generator version if saved worlds depend on it.

## Ecological vs decorative content

Not every visual object needs simulation. Classify content:
- decorative static;
- interactive but non-renewable;
- renewable resource;
- habitat/cover;
- dynamic environmental feature;
- trace/temporary effect.

Only simulate the detail required by gameplay/ecology.


## World-generation configuration source

The authoritative world-generation JSON configuration is stored at:

`Assets/Project/Data/WorldGeneration/world_generation.json`

This is the single project-level `Data` directory. Runtime code that loads and validates the JSON belongs under `Assets/Project/Runtime/DataServices/Loading`; do not add a second `Runtime/Data` folder. The world-generation core receives a validated settings model and must not read files directly or depend on `UnityEditor`. The selected loading mechanism must support both Editor generation and player builds.

## Confirmed player and world direction

The player camera follows a Pokémon-style third-person presentation, with a distant follow view and adjustable viewing angle. Exact camera controls and parameters remain open and must be validated against the spherical curvature presentation test.

The long-term world target is a large planet with extensive regions and long-distance travel. The final radius and playable surface area remain open; the existing 2 km × 2 km configuration is only a test setup.

The player can both observe the autonomous ecosystem and participate as a Pokémon trainer. The first spherical-world prototype is narrower: validate spherical topology, terrain, ground-to-flight curvature presentation and player movement before implementing the full ecosystem or trainer gameplay.

Performance should be profiled on the project owner's own PC. A universal minimum hardware specification is not a project requirement at this stage.

## Layered planet authoring and dual-view inspection

The first spherical-world prototype is an inspectable generation workspace, not a one-shot terrain generator. It starts with a smooth base sphere and composes three non-destructive terrain layers in order: macro relief, regional relief and fine details. Each layer has independent settings, deterministic seed derivation, visibility/enable state and inspectable contribution. Users can return to any earlier stage without losing later-layer settings.

The same generated data must be available in two presentation views: a 3D spherical globe and a 2D planisphere. These are views of one spherical dataset, not separate world generators. Switching views must not alter or regenerate source fields. The planisphere projection and its distortion/wrap behaviour must be explicit and tested; equirectangular is only the initial candidate.

Independent thematic maps (altitude, temperature, derived moisture, slope, water, biomes, settlement suitability, etc.) are selected for inspection without modifying terrain geometry or source values. See `WORLD-001_LAYERED_PLANET_STUDIO_AMENDMENT.md` for the detailed prototype workflow and acceptance criteria.


## Spherical geographic-field pipeline (WORLD-002 plan)

The next planned spherical-world work package is `WORLD-002_SPHERICAL_GEOGRAPHIC_FIELDS.md`. It extends the WORLD-001 Planet Studio with shared, deterministic fields for temperature, water/hydrology, slope, spherical distance-to-water, derived moisture, vegetation suitability, biome classification and settlement suitability. These are separate outputs with explicit dependencies; downstream presentation and simulation must consume the same fields rather than regenerating contradictory local versions.

The confirmed vegetation presentation is hybrid: biome-aware ground materials/textures plus deterministic 3D vegetation instances driven by vegetation suitability and environmental exclusion masks (DEC-042). The planisphere remains a presentation projection of the same spherical dataset. The planar EDT specified in the terrain domain must not be applied naïvely to the globe; WORLD-002 contains a mandatory spherical distance/hydrology design gate. The work package is currently a proposed plan and must not be treated as implemented or approved until the project-owner milestone gate is satisfied.


## Spherical player movement foundation (WORLD-004)

The first reusable player movement implementation is owned by the World domain under `Assets/Project/Runtime/World/Spherical/Movement/`. WORLD-003 remains a temporary Play Mode inspection/bootstrap fixture and must not own production locomotion rules.

The player motor uses world-space velocity and radial gravity directed toward the planet centre. Ground height, slope and categorical water state come from the same `SphericalGeographicFieldSet` used to render the WORLD-002 planet; do not fall back to the older WORLD-001 height sampler. Ground contact is field-aware because the six full-face preview meshes intentionally have no `MeshCollider` (their current triangles are too large for stable physics queries). The first implementation supports terrain grounding, falling, landing, walking and jumping, but not arbitrary collision with trees/rocks/buildings or dynamic terrain.

Movement is tangent to the local spherical surface and camera-relative, with a transported tangent basis to avoid orientation discontinuities across cube-sphere face seams and near poles. Character orientation follows the sampled terrain normal with radial-up fallback. Ocean/lake/river cells are not solid ground; swimming and water traversal are later features. Steep-slope blocking is configurable and validated against the WORLD-002 slope field.

The movement motor must not poll input directly. A narrow Unity Input System adapter supplies movement/jump intent; the existing spherical third-person camera remains the camera implementation unless a focused extension is required. World-owned movement remains the only owner of player Transform mutation, consistent with the `IWorldMovement` boundary.

See `Documentation/03_ENGINEERING/WORK_PACKAGES/WORLD-004_PLAYER_CONTROLLER_AND_SPHERICAL_LOCOMOTION.md` for the implementation plan, defaults, tests and acceptance criteria.


## Player movement on the spherical surface (WORLD-004)

The World domain owns player world-position mutation. `SphericalPlayerMotor` stores world-space velocity and applies gravity toward the explicit planet centre. `SphericalGroundProbe` queries the same `SphericalGeographicFieldSet` that drives WORLD-002 terrain/biome rendering; it does not depend on the six coarse preview meshes or their colliders. `SphericalPlayerController` is the current PC Input System adapter and consumes the existing `SphericalThirdPersonCamera` tangent movement basis.

This first slice supports field-based ground contact, radial falling, grounded jump, camera-relative walking, a slope threshold and blocking grounded entry into water. It does not yet provide swept capsule collision, step climbing, prop collision, swimming, traversal abilities, animation, multiplayer prediction or server-authoritative player state. Those remain separate follow-up work packages.


## Optional biome classification (MOD-001-C)

The spherical `SphericalGeographicFieldSet` is the authoritative World Core snapshot. Its topology, altitude, temperature, slope, distance-to-water, moisture and water-class fields are generated regardless of whether biome classification is requested.

`SphericalGeographicFieldGenerator.Generate(settings)` retains the historical behaviour and includes biome classification. `Generate(settings, includeBiomeClassification)` allows a caller to request World Core without allocating biome face arrays or classifying biome nodes. The generated field set exposes `HasBiomeClassification`, `TryGetBiome` and `TrySampleBiome` so consumers can declare whether biome data is available.

Legacy `GetBiome` and `SampleBiome` remain available for existing biome-dependent consumers, but throw `InvalidOperationException` when classification is absent. They never substitute `Plains` or another default. The ground probe is a World Core consumer and therefore remains operational without biome classification; its result exposes `HasBiomeClassification` and uses `SphericalBiome.None` only as the explicit absent-value paired with that flag.

Biome presentation, biome-dependent environment placement and the inspection bootstrap remain biome-dependent until their respective MOD-001-D/E/F composition work packages connect the activation report and optional providers. This change does not claim that the full world-creation pipeline can yet run without biomes.
