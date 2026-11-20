# World, Terrain and Biomes

## Scope

Owns spatial world generation, terrain representation, biome placement, environment objects, navigation-relevant spatial queries and world-side interaction locations.


## Confirmed initial world content and presentation scope

### Initial ocean and biome scope

The initial spherical-world generation targets **65% ocean coverage** by planet surface area. This is a configurable target, not a hard-coded elevation. The generator derives the sea-level threshold from the generated terrain elevation distribution so the requested ocean fraction can be approached consistently across seeds and resolutions. Coverage is area-weighted on the sphere; counting Cube-Sphere samples as if they represented identical surface areas is not an acceptable measurement. Inland lakes/ponds and rivers are separate water classes and do not count toward the ocean-coverage target.

The initial terrestrial biome set is:
- **Plains**
- **Forest**
- **Desert**
- **Mountains**

Mountains are classified primarily from elevation and slope constraints. Plains, forest and desert are distinguished primarily through temperature, derived moisture and terrain constraints. Water bodies are not terrestrial biomes: ocean, lake/pond and river remain water classifications. Coastlines/shore influence may be represented as a separate mask or transition layer, not as a fifth biome.

A **village is a settlement, not a biome**. It is a generated/placed built environment located within a suitable biome/region. In the initial implementation, a village must at least be identifiable by one house/building marker; the complete village layout and building catalogue can be developed separately. Settlement suitability remains a separate field from biome classification.

The world also includes lakes/ponds and rivers. Environment and vegetation placement are intended to be procedural. Village generation may use a hybrid approach: procedural site selection/layout with selected authored or generated building assets. The exact division is OPEN. Both natural trails and constructed roads are in scope; the current generation stage identifies their locations/classes, while final surface styling and construction details can follow.

### Traversal and habitat suitability

Traversal must be capability-aware rather than a single universal walkability mask. Different Pokémon have different movement capabilities: for example, a small ground Pokémon such as Chenipotte should not be able to climb an otherwise impassable mountain, while a flying Pokémon such as Roucoups can fly over terrain that blocks ground traversal. The final movement-capability taxonomy, slope/height thresholds, flight restrictions, stamina/energy costs and pathfinding interface are OPEN and must be coordinated with the Pokémon simulation/behaviour domain.

Pokémon must be able to identify and use suitable shelters/habitats. The world domain provides spatially queryable shelter/rest/nest/cover locations and suitability metadata; Pokémon behaviour decides which locations an individual chooses. Do not encode species-specific behavioural decisions inside terrain rendering.

### Dynamic world conditions

A day/night cycle and seasonal changes are required. Seasons must create genuine world-state variation, not only a cosmetic colour shift. They may affect biome appearance, vegetation, resource availability and ecological conditions once the relevant rules are specified. Their duration, calendar, transition model, weather coupling and per-biome effects remain OPEN. Keep time/season state as explicit world/session data rather than hard-coding it into terrain materials.

### Ecosystem scale

The initial design target is to support an ecosystem of approximately **2,000 Pokémon**. This is a planning target, not a proven performance result or a requirement that every individual use full-rate simulation continuously. Simulation scheduling, levels of detail, off-screen update rates, persistence and PC hardware baseline must be profiled and specified before the target is treated as validated. Pokémon are intended to continue living independently of the player's immediate presence in the exploration/living-world mode.

## Visual layers

1. Terrain base: grass, dirt, rock, sand, mud, snow, leaf litter and similar.
2. Model materials: bark, leaves, stone, Pokémon surfaces, berries and props.
3. Placed details: grass tufts, flowers, fallen leaves, stones, mushrooms, branches.
4. Interactive environment: berry bushes, water sources, shelters, nests, hollow logs.
5. Dynamic effects: puddles, wetness, wind response, footprints and temporary traces.

Do not confuse terrain texture painting with placement of actual 3D objects.

## Scene generation requirement

World generation must produce a real terrain surface in the Unity scene, not only procedural maps or an Editor-window preview. The Editor workflow must expose a `Generate & Apply to Scene` action (or an equivalent explicit Generate-then-Apply workflow) that creates or updates generator-owned terrain GameObject(s) in the active scene. The terrain surface is built from the generated height map and receives the relevant biome/material presentation; it must use the configured world dimensions and cell spacing and have an appropriate collider for gameplay. Re-generation updates/replaces the prior generated output rather than leaving duplicate terrain. The same application capability must be available in Play Mode through runtime-safe code. Keep this scene-instantiation step separate from the shared data-generation core and from the choice of Unity Terrain versus custom mesh. The current 2,000 m × 2,000 m size is a test configuration only. The long-term target is a finite, globe-like, fully explorable world without artificial invisible boundary walls; topology is OPEN and must be resolved before large-scale world architecture is locked.

## Terrain rendering

Unity Terrain generated through scripts is a candidate initial solution. A custom mesh/chunk approach remains possible. Keep biome data independent of the rendering backend.

If Unity Terrain is selected:
- generate TerrainData through editor/runtime-appropriate APIs;
- create TerrainLayers via editor tooling where asset creation is required;
- generate alphamaps/splat weights by scripts;
- centralize texture references and import settings;
- validate layer count, texture tiling, dimensions and missing references.

If mesh terrain is selected:
- define chunk topology, normals, UVs, vertex colours/material blending and collision policy;
- support deterministic generation and LOD where needed.

## Procedural world fields — mandatory generation model

World generation must use multiple distinct Perlin Noise maps and combine them to create the terrain, climate and vegetation characteristics. A single Perlin Noise map reused for every property is not acceptable.

The generator must produce the following base noise fields and derived environmental fields. Not every output field is an independent Perlin Noise map:

| Field | Purpose | Generation and combination |
|---|---|---|
| Height | Terrain elevation and landform | Multi-octave Perlin Noise (fractal Brownian motion), with configurable scale, octaves, persistence and lacunarity. Optional shaping controls the proportions of plains, hills, valleys and mountains. |
| Temperature | Local thermal conditions | Its own Perlin Noise field, combined with broad-scale world trends (such as latitude, when available) and an altitude correction derived from height. Higher elevations are generally colder. |
| Moisture (derived) | Ecological ground-moisture availability | No independent Perlin Noise. Derive it from Euclidean distance to the nearest water cell, height/terrain retention and temperature-dependent evaporation. |
| Vegetation | Potential vegetation density/suitability | Its own Perlin Noise field, combined with temperature, moisture and elevation suitability. This is a suitability field, not the placement map for individual plants. |
| Water | Water presence, type and distribution | A dedicated seeded Perlin Noise field combined with height and hydrological constraints. Distinguish sea/ocean, lakes/ponds and rivers where supported. Lakes should occupy plausible low basins; rivers should follow downhill flow and connect coherently where possible. This field is the source mask for the distance transform. |
| Settlement suitability | Candidate locations for settlements | A low-frequency seeded Perlin Noise field combined with slope/elevation, fresh-water proximity, flood risk, contiguous buildable area and access/connectivity where available. Produce distinct suitability for villages, towns and large cities. This is not the final settlement layout and must remain a separate mandatory map. |
| Road/path network | Connectivity between selected settlements | A derived network, not a standalone Perlin Noise map. Build a settlement graph and route its edges over a terrain-aware traversal-cost field using A* or Dijkstra least-cost pathfinding. Store route polylines and a rasterized road/path mask. |

Additional fields such as roughness, erosion or rock exposure may be added when justified by a defined requirement.

### Field combination and biome classification

- Generate and retain each raw field separately so that it can be inspected, tested and reused.
- Normalize field values and document any alternate ranges.
- Use distinct deterministic seed offsets or streams for each field. A world seed must determine all fields reproducibly without making them share identical patterns.
- Generate temperature from its own field plus documented world-scale/altitude influences. Derive moisture only after water generation and distance calculation; do not generate an independent moisture Perlin map.
- Classify the four initial terrestrial biomes (plains, forest, desert, mountains) from shared height, temperature, moisture and slope fields. Mountains use elevation/slope as their primary criteria; plains/forest/desert use climate and terrain constraints. Water presence/type is classified separately; coast/shore influence is a separate mask or transition, not a terrestrial biome. Vegetation potential controls vegetation suitability/density within the resulting environment and can refine biome-specific vegetation profiles.
- Derive moisture from water proximity (Euclidean distance transform), terrain elevation/retention and temperature-dependent evaporation. The moisture field must correlate spatially with water: all else equal, cells farther from water must not become wetter merely because of an unrelated noise sample. Use the derived moisture for vegetation suitability, water-dependent biomes and resource/habitat placement. Keep generation dependencies ordered to avoid circular calculations.
- Generate settlement suitability only after terrain and water constraints are available. Reject or penalize steep, submerged and flood-prone locations; prefer suitable fresh-water access, contiguous buildable ground and connectivity.
- Produce separate candidate classes for villages, towns and large cities. Large cities must be rarer and require larger suitable areas and stronger access conditions than villages. Apply deterministic spacing and exclusion rules.
- The settlement suitability map is mandatory and only selects candidate sites; preserve it as a separate field. Streets, building footprints, buildings, population and points of interest belong to a later settlement-layout stage.
- The road/path network is a separate mandatory derived product connecting selected settlement nodes. It must not replace or be merged with settlement suitability.
- Build a settlement graph with deterministic node/edge selection and hierarchy-aware links (for example, major links between large towns/cities and minor links connecting villages to nearby towns). Do not connect every settlement to every other settlement.
- Construct a traversal-cost field from slope/elevation, water/flood masks, biome/ground type, obstacles and existing infrastructure. Gentle buildable terrain has lower cost; steep, wet, obstructed or hazardous terrain has higher cost or is blocked.
- Route graph edges using A* (with an admissible heuristic) or Dijkstra. Optimize accumulated terrain traversal cost, not straight-line Euclidean distance. If using 8-neighbour movement, diagonal steps must cost `sqrt(2)` times the local traversal cost and corner-cutting through blocked cells must be forbidden.
- Treat water crossings explicitly: avoid deep water, route around lakes/sea, and allow river crossings only at designated ford/bridge candidate cells with an added cost. Bridge construction/rendering is a later stage.
- Save route polylines/edges and a rasterized path mask. Any smoothing/simplification must preserve traversability. Use deterministic tie-breaking and stable ordering to keep the same seed/configuration reproducible.
- Route classes (major road, minor road, trail) determine later width, surface and visual presentation; they do not alter the settlement suitability field.
- Use configurable thresholds and blend ranges for biome and settlement suitability. Avoid unexplained hard-coded thresholds.
- Feed the same generated field data into terrain material assignment, vegetation placement, water-aware resource placement, habitat suitability and settlement site selection. Do not independently regenerate a contradictory version of a field in another system.

### Required generation order

1. Generate base Perlin Noise fields with deterministic per-field seeds/offsets (height, temperature, vegetation variation, water potential and settlement variation).
2. Shape the height map and build the terrain surface.
3. Generate the water map; derive plausible basins, shorelines and downhill river flow from water noise plus height/hydrology constraints.
4. Compute the exact Euclidean distance-to-water field using a two-pass separable EDT (row pass, then column pass; square-root after the second pass).
5. Derive moisture from distance-to-water, height/terrain retention and temperature-dependent evaporation.
6. Calculate vegetation potential from its noise field and environmental suitability, including derived moisture.
7. Classify the four terrestrial biomes (plains, forest, desert, mountains) from terrain, temperature, derived moisture and slope; keep ocean/lake/river and optional coastal influence masks separate.
8. Generate the settlement suitability map and select village, town and large-city candidate sites; retain this map as its own output.
9. Build a terrain traversal-cost field and a graph of selected settlements.
10. Connect the graph with A*/Dijkstra least-cost routes; output route polylines and a rasterized road/path map.
11. Assign terrain materials and blend weights.
12. Place environment objects and ecological resources; generate settlement layouts and route presentation in separate stages.
13. Validate field ranges, hydrology, distance field, moisture gradients, settlement suitability, route connectivity/traversability and deterministic reproducibility.

## Water distance and derived moisture

The moisture field is derived data, not a standalone noise map.

### Exact two-pass Euclidean Distance Transform

Use the water mask as the set of source cells and calculate the distance from every cell to its nearest water cell. The required approach is a separable exact Euclidean Distance Transform:

1. Apply a 1D squared-distance transform to each row.
2. Apply a 1D squared-distance transform to each column using the intermediate row results.
3. Take the square root after the second pass.
4. Convert grid coordinates to world-space distance using the actual X/Z cell spacing.

Use an exact 1D lower-envelope-of-parabolas algorithm (for example, Felzenszwalb–Huttenlocher) or an equivalent exact EDT. A two-scan chamfer/8-neighbour approximation is not interchangeable with exact Euclidean distance. For chunked worlds, define global processing or border/halo handling so distances remain correct across chunk boundaries.

### Moisture derivation

Use a configurable normalized ecological moisture model such as:

```text
waterInfluence = exp(-distanceToWater / waterInfluenceLength)
terrainRetention = clamp(1 - heightAndSlopePenalty, 0, 1)
temperatureRetention = clamp(1 - evaporationCoefficient
                                * max(0, temperature - referenceTemperature),
                             minimumRetention, 1)
moisture = clamp((waterWeight * waterInfluence
                  + terrainWeight * terrainRetention
                  + optionalBackgroundMoisture)
                 * temperatureRetention, 0, 1)
```

This encodes three required relationships:
- water proximity increases local moisture, with a smooth configurable falloff;
- lowlands/basins and gentle terrain can retain more moisture than exposed high ground or steep slopes;
- higher temperatures increase the evaporation penalty and reduce retained ground moisture, all else equal.

The exact coefficients, reference temperature, influence length and optional background term belong in versioned generation settings. This is an ecological ground-moisture index, not a full atmospheric relative-humidity simulation. Do not use a separate moisture Perlin map that can contradict the water-distance gradient.

## Texture/material architecture

Separate:
- source texture files;
- imported Unity textures;
- material definitions;
- shader implementations;
- biome-to-material mappings;
- runtime terrain painting/assignment.

Stylized art does not require full PBR maps on every surface. Use albedo/base colour first; add normal, roughness/smoothness, AO, height or transparency only when visually justified.

## Biome definitions

May contain:
- climate ranges;
- terrain material palette and blend rules;
- vegetation profiles/density;
- rock/log/prop distribution;
- resource spawn profiles;
- habitat availability;
- suitable species and constraints;
- ambient audio/VFX;
- generation parameters.

## Procedural generation and determinism

Use explicit seeds and versioned generation settings. Given the same world seed, generator version and configuration, all base fields, derived fields and generation outputs must be reproducible. Avoid uncontrolled global random state.

Keep the stages separate: terrain shape, climate/vegetation fields, biome regions, material assignment, object placement, resource placement, habitat setup and validation.

## Generation controls, visualisation and map exports

World generation must be explicitly launchable from both Unity Editor and Play Mode through the same runtime-safe generation core.

- Editor tooling provides a dedicated generation window/inspector with seed, generator version, dimensions, resolution, settings and output mode, plus Generate/Regenerate/Export/Clear Preview actions.
- Play Mode exposes the same generation request through a runtime-safe service/API. It can generate a preview or apply the result to the active world; this choice must be explicit.
- The core generator must not reference UnityEditor. Editor windows, AssetDatabase operations and editor file dialogs belong in Editor-only assemblies.
- A generation request records seed, generator version, world size, map resolution, cell spacing, immutable settings snapshot, requested output modes and preview/apply policy. The result exposes every map, selected settlements, graph edges, route polylines/mask, validation and stage timings.
- Run named dependency stages in order: request validation → base noise → height/slope → water/hydrology → two-pass exact EDT → derived moisture → vegetation → biomes → settlement suitability/site selection → sparse settlement graph → traversal cost/barriers → A*/Dijkstra routing → selected visual output → validation/publication.
- Allow stage/output selection, but enforce dependencies. For example, road rendering still requires settlement selection, terrain costs and route planning.

Required visualisation modes:
1. **Flat thematic overview:** top-down 2D colour map for water, coast, green/plains, vegetation, hills/mountains/cliffs, settlements and routes, with legend and individually toggleable layers.
2. **Single-map debug view:** independent grayscale/colour-ramp display for every raw and derived map, including numeric range/value inspection.
3. **Height/3D view:** render height as a terrain surface with adjustable visual exaggeration, height/biome/slope colouring and optional wireframe/debug overlays.
4. **Asset preview:** place actual prefabs where available; otherwise use identifiable primitive/billboard placeholders for vegetation, rocks/cliffs, water, settlements, bridges and routes. Placeholder replacement must not require changing generation logic.
5. **Layered view:** toggle maps independently, adjust opacity/draw order, compare raw noise with derived maps and inspect seed/configuration.

Every map must be independently exportable at native resolution. Export raw numeric fields in precision-preserving formats (16-bit PNG or EXR where appropriate), categorical fields with documented integer/colour encodings, and colour previews as lossless PNG. Include a metadata sidecar with dimensions, world size, cell spacing, value range/encoding, seed, generator version and settings. Export height, temperature, water, distance-to-water, moisture, vegetation, biome, settlement suitability, traversal cost and route mask separately. The composite screenshot must never be the only export option.

## Spatial services

Expose queries such as:
- environment context at position;
- biome at position;
- nearby resource candidates;
- reachable interaction point;
- suitable shelter/rest location;
- ground/slope/water condition.

Other domains should not inspect private terrain-generation data structures directly.


### Optional classification in spherical World Core (MOD-001-C)

Biome classification is not required to generate authoritative spherical geography. World Core always provides altitude, temperature, slope, distance-to-water, moisture and water class. The biome classifier may be skipped as a derived output without changing those fields.

A field set explicitly reports classification availability. Biome consumers must use the `Try*` APIs when absence is valid, or retain the legacy throwing APIs when classification is a hard dependency. No consumer may treat `SphericalBiome.None` as Plains. Ground probing and movement queries do not require biome classification.
