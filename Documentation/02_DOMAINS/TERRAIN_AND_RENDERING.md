# Terrain, Textures and Rendering

## Rendering principles

- Stylized 3D is the target visual direction.
- Prefer coherent art direction over unnecessary material complexity.
- Keep biome logic separate from rendering implementation.
- Automate material, texture and terrain setup where possible.


## Approved visual direction

The target is a **stylized 3D world inspired primarily by modern Pokémon environments, with a manga influence**. It should read as Pokémon first, rather than as an abstract low-poly game. The intended finish is stylized, not photorealistic.

- Use a distinct, coherent colour palette for each biome; do not force one global green palette across the world.
- The terrain must have strong, readable landforms. Relief is a major visual and gameplay feature, not a minor surface variation.
- The world should remain visually compelling both close to the player and across distant vistas. The desired ambition is comparable to the sense of landscape scale and exploration in *Breath of the Wild*, while retaining the project's Pokémon-inspired stylization. This is a quality target, not a requirement to reproduce that game's technology or exact art direction.
- Use biome-aware materials, natural transitions, layered vegetation and environmental dressing to avoid a single flat colour plane.
- Water features include ponds and rivers in the initial world scope.
- Seasonal changes and a day/night cycle are required. Seasonal changes must represent genuine world-state variation, not merely cosmetic colour changes; visual, ecological and resource effects, calendar and transitions remain to be specified. Initial relief scope includes hills and mountains.
- Terrain and environment production must be script-driven. The selected tool/backend is secondary to reproducibility and integration into the game; normal production must not require manual terrain painting or manual placement.
- Use a Pokémon-like player camera presentation; exact follow distance, perspective/FOV, rotation and framing are still OPEN.
- Provide a repeatable Unity demonstration scene for visual validation.

Reference images and a detailed art bible are still desirable. No specific external game screenshot or exact palette has been approved as a direct reference.

## World shape and exploration target

The 2,000 m × 2,000 m dimensions currently used by generation are a **test configuration**, not the intended final world boundary. The long-term direction is a finite, globe-like world that can be explored throughout, without invisible boundary walls or mountains used as artificial map-edge barriers.

The approved product direction is a **true spherical globe/planet**, fully explorable with continuous travel and no invisible boundary walls. Its curvature must be **subtle from normal ground-level gameplay**: the local landscape should read as broad and almost planar, without an obviously curved ground or horizon. Curvature should become progressively perceptible when the player gains substantial altitude with a flying Pokémon, especially in distant aerial views. There must be no abrupt visual switch between ground and flight. This is a presentation requirement; the underlying world remains spherical at all times.

The recommended technical candidate is Cube-Sphere topology with seam-safe spherical world fields and hierarchical adaptive terrain tiles; see `../01_ARCHITECTURE/SPHERICAL_WORLD_ARCHITECTURE_PROPOSAL.md`. The proposal is not production-approved until fixed-resolution topology, camera/curvature and adaptive tile seam prototypes pass validation. Do not silently approximate the globe with a bounded planar map or simply bend the existing planar mesh. The planet radius is OPEN and must be selected through a scripted visual test comparing ground, low-flight, high-flight and aerial viewpoints. Natural mountains and cliffs may be genuine terrain obstacles; they must not be used as fake perimeter walls.

## Seasonal world variation

Seasons are a world-state feature, not just a material colour preset. The system should support meaningful seasonal changes in the world presentation and, as rules are defined by WORLD/ECO, ecological conditions and resource availability. Keep season state explicit and data-driven. Do not hard-code season changes into terrain shaders alone. The calendar, season durations, transitions, weather coupling and biome-specific effects remain OPEN.

## Terrain interaction and editability

The world is intended to be modifiable during play, but this does not imply arbitrary voxel terrain deformation. The current direction is to make selected, explicitly identified environmental objects or terrain features interactable/destructible (for example, particular rocks or ice blocks). The object catalogue, persistence, regeneration rules, collision updates and multiplayer replication requirements are OPEN. Keep these interactions separate from the base terrain representation until a concrete use case requires terrain deformation.

## Visual quality and acceptance direction

At minimum, a terrain presentation change should be assessed for:
- readable and attractive large-scale silhouettes from a distance;
- nearly planar visual impression from normal ground-level viewpoints, with curvature becoming perceptible gradually at high flying altitude;
- satisfying relief and transitions when viewed near the player;
- distinct biome palettes and clear biome boundaries/blending;
- no unexplained cracks, streaks, repetitive noise artefacts or large uniform colour patches;
- coherent rivers, ponds, shorelines and slopes;
- visual consistency between terrain materials, vegetation, rocks, water and settlements;
- acceptable frame time and memory use at the target PC configuration (numeric budgets remain to be defined);
- deterministic reproduction from the same seed and configuration.

## Terrain generation

The world terrain must be generated procedurally from multiple coordinated Perlin Noise base maps and derived spatial fields. A single noise map is not sufficient. Terrain shape, climate, water, vegetation, settlement suitability and route planning must be represented by distinct fields or derived products and combined through explicit rules.

### Noise map fields

All generated maps must use explicit, deterministic seeds and normalized values in the `[0, 1]` range unless a field's definition requires another documented range.

1. **Height map**
   - Defines terrain elevation and is the primary input for the terrain surface.
   - Use multi-octave Perlin Noise (fractal Brownian motion): combine several frequencies with decreasing amplitudes to obtain broad landforms and smaller surface variation.
   - Expose parameters such as seed, scale/frequency, octave count, persistence and lacunarity.
   - Apply optional shaping curves or elevation masks after noise sampling to control the distribution of plains, hills, valleys and mountains.

2. **Temperature map**
   - Represents local temperature independently from the height map.
   - Combine a broad-scale Perlin Noise field with large-scale world trends such as latitude, if the world coordinate model supports them.
   - Apply an altitude influence derived from the height map: higher elevations should generally be colder.
   - Keep the noise seed/offset and influence weights configurable.

3. **Derived moisture / humidity map**
   - Moisture is a derived environmental field, NOT an independent Perlin Noise map. It must be calculated from the water distribution, distance to the nearest water cell, terrain elevation (and optionally slope/drainage when available), and temperature.
   - First generate the water mask/map. Then compute a distance-to-water field with an Euclidean Distance Transform (EDT), and use that distance together with height and temperature to calculate moisture.
   - Use a configurable distance influence, for example `waterInfluence(d) = exp(-d / L)`, where `d` is world-space distance to the nearest water cell and `L` controls how far water influences nearby terrain. A smooth falloff such as `1 - smoothstep(0, R, d)` is also acceptable when a finite influence radius is preferred.
   - Derive a lowland/retention factor from normalized elevation (and slope/drainage if available). Low basins and gentle terrain may retain more moisture; steep slopes and exposed high ground generally retain less.
   - Apply a temperature-dependent evaporation/retention factor. Higher temperatures generally reduce soil-moisture retention unless compensated by strong water proximity. Keep this term configurable and bounded.
   - Example normalized model: `M = clamp((a * W(d) + b * Hretention) * E(T), 0, 1)`, where `W(d)` is water-distance influence, `Hretention` is the height/slope retention term, `E(T)` is the temperature-dependent retention factor, and `a`/`b` are configurable weights. Add a configurable background/rainfall baseline only if the world design requires moisture far from mapped water.
   - This field represents ecological/ground moisture availability for biome and vegetation logic. It is not a physically complete atmospheric relative-humidity or weather simulation.
   - Do not generate an unrelated Perlin pattern for moisture: nearby water must produce a coherent moisture gradient, modified by terrain and temperature.

4. **Vegetation map**
   - Represents vegetation suitability or potential density, not the placement of individual plants.
   - Generate a distinct Perlin Noise field to create local variation.
   - Combine it with temperature, moisture and elevation suitability. The resulting value should be reduced in conditions unsuitable for vegetation (for example, extreme cold, very low moisture or exposed high elevations).
   - Biome-specific rules may further modify vegetation potential.

5. **Water map**
   - A mandatory, independently inspectable field describing water presence/type and water-body classification.
   - Build it from a dedicated seeded Perlin Noise field combined with the height map and explicit hydrological rules. Noise alone must not place water arbitrarily on ridges or create disconnected river fragments.
   - Distinguish at least ocean/sea, lake/pond and river water where the world scale supports them. Lakes should respect low areas/basins; rivers should follow downhill flow and connect coherently to lakes or the sea where possible.
   - Store water presence/type separately from terrain height. Define configurable thresholds, depth/level rules and shoreline blending.
   - The water map is the source mask for the Euclidean Distance Transform used by the derived moisture calculation. Water proximity must also inform biome classification, vegetation suitability and settlement suitability.

6. **Settlement suitability map**
   - A mandatory field identifying candidate locations for settlements. It is a suitability/score map, not a random placement map and not the final village/city layout.
   - Combine dedicated low-frequency seeded Perlin Noise variation with geographic constraints: slope and elevation from the height map, proximity to fresh water from the water map, flood risk, available buildable area, and access/connectivity where those systems exist.
   - Produce separate suitability outputs or classifications for **villages**, **towns** and **large cities**. Large-city candidates require larger contiguous buildable areas and stronger access conditions; they should be rarer than villages.
   - Enforce minimum spacing, settlement hierarchy and exclusion zones (for example, steep slopes, deep water and flood-prone areas). Use configurable thresholds and deterministic candidate selection.
   - This map identifies where settlements may be placed. Actual settlement generation (street layout, buildings, population and points of interest) is a later stage and must consume the selected candidate locations rather than being conflated with the suitability field.
   - Keep this map mandatory and independently inspectable; it must not be removed or merged into the road/path map.

7. **Road and path network map**
   - A mandatory derived map describing the planned connectivity corridors between selected settlements. It is distinct from the settlement suitability map: the settlement map selects *where places can exist*; the road/path map connects the *settlements that were actually selected*.
   - Do not generate roads by simply thresholding Perlin Noise or by drawing straight Euclidean lines between settlements. Use a graph plus terrain-aware least-cost pathfinding.
   - Represent selected villages, towns and cities as graph nodes. Choose graph edges according to settlement hierarchy, maximum travel distance, nearby connections and network policy (for example, large-city ↔ town and town ↔ village links). Avoid connecting every settlement to every other settlement.
   - Compute a traversability/cost field from normalized slope/height, water and flood masks, terrain/biome type, obstacles and optional existing paths. Assign configurable costs: gentle, buildable ground is cheaper; steep slopes, wetlands, dense obstacles and water crossings are more expensive or forbidden.
   - Find each route with A* (preferred for point-to-point routing with an admissible heuristic) or Dijkstra (when computing a complete least-cost field / multi-target network). Use accumulated traversal cost, not straight-line distance, as the route objective. The heuristic for A* must not overestimate the remaining cost.
   - Use 8-neighbour grid movement only if diagonal movement costs are corrected (for example, cardinal cost `1`, diagonal cost `sqrt(2)` multiplied by local traversal cost); prevent diagonal corner-cutting through blocked cells. A 4-neighbour grid is also valid.
   - Apply water-crossing rules explicitly: avoid deep water, route around lakes/sea, and permit river crossings only at designated ford/bridge candidate cells with a configurable crossing penalty. Bridges are a later construction/detail stage.
   - Store the resulting network as route polylines/edges with a rasterized road/path mask for rendering, biome interaction and distance queries. Smooth or simplify polylines only if the simplified route remains traversable and does not cut through forbidden cells.
   - Support route classes such as major roads (between large settlements/towns) and minor roads/trails (to villages). Width, surface and visual style are assigned from route class in a later presentation stage.
   - Use deterministic tie-breaking and stable node/edge ordering so the same world seed, selected settlements, generator version and settings reproduce the same network.

8. **Optional additional fields**
   - Further maps (for example, roughness, erosion or rock exposure) may be introduced when a concrete gameplay or visual need exists.
   - Each field must have a documented purpose, range, seed policy and combination rule. Avoid adding noise maps without a defined use.

### Combining the maps

- Keep base fields and derived products separate and inspectable. Height, temperature, water, derived moisture, vegetation, settlement suitability and the route network must remain individually inspectable.
- Combine the fields through explicit, configurable rules rather than collapsing them into one noise sample.
- Use height, temperature, derived moisture and water together to classify biome regions. Vegetation potential then influences the density and suitability of vegetation within those regions.
- Use the resulting biome and field values to drive terrain material weights and later environment-object placement.
- Define thresholds and blend ranges in configuration; avoid unexplained hard-coded constants.
- Normalize and validate generated values before they are consumed by biome, rendering or placement systems.
- Use distinct deterministic offsets/seeds per field so maps do not accidentally repeat the same pattern. The same world seed and generator version must still reproduce the same complete set of maps.

The generation stages should remain separate and ordered:

1. Generate base noise fields.
2. Build and shape the height map.
3. Generate temperature and moisture maps, applying their documented world-scale influences.
4. Generate the water map and derive coherent water bodies/river flow using noise plus height and hydrology constraints.
5. Derive final moisture and vegetation potential using their noise fields and environmental suitability, including water proximity.
6. Classify biome regions from the combined fields.
7. Generate the settlement suitability map and select village, town and large-city candidate sites using geographic constraints.
8. Assign terrain materials and blending weights.
9. Place environment objects and ecological resources using biome and suitability data; generate settlement layouts in a separate stage.

## Water-distance calculation and moisture mathematics

### Euclidean Distance Transform (EDT)

Use an exact, separable Euclidean Distance Transform to calculate distance from every terrain cell to the nearest water cell.

- Input: a binary water-source mask (`0` for water/source cells, `+infinity` for non-source cells) or an equivalent feature map.
- First pass: compute the one-dimensional squared Euclidean distance transform along every row.
- Second pass: compute the one-dimensional squared Euclidean distance transform along every column, using the first-pass result.
- Take the square root only after the second pass to obtain Euclidean distance.
- For an exact linear-time 1D pass, use the lower-envelope-of-parabolas method (Felzenszwalb–Huttenlocher EDT) or an equivalent exact separable EDT implementation. Do not describe a simple 8-neighbour chamfer/two-scan approximation as exact Euclidean distance.
- Convert grid distance to world units using cell spacing: `dWorld = sqrt((dx * deltaX)^2 + (dz * deltaZ)^2)`. This matters when horizontal and vertical cell sizes differ.
- If the world is chunked, define whether the EDT is global or uses a documented border/halo region; otherwise chunk edges can create incorrect distance fields.

### Derived moisture model

Normalize all inputs to documented ranges. A recommended configurable model is:

```text
W(d)       = exp(-d / L)                         // water influence falloff
Hretention = clamp(1 - heightPenalty(h, slope), 0, 1)
E(T)       = clamp(1 - kEvap * max(0, T - Tref), Emin, 1)
M          = clamp((a * W(d) + b * Hretention + Mbase) * E(T), 0, 1)
```

- `d`: world-space Euclidean distance to the nearest water cell.
- `L`: water influence length scale.
- `h`: normalized elevation; `slope` may be included when available.
- `T`: normalized or physical temperature, consistently converted before use.
- `Tref`, `kEvap`, `Emin`, `a`, `b` and `Mbase`: versioned configuration parameters, not unexplained constants.
- `Mbase` is an optional background moisture contribution for rainfall/global climate. If no rainfall model exists, keep it small or zero and document the resulting dry-interior behaviour.
- Clamp the result to `[0, 1]` and validate monotonic expectations: all else equal, moisture decreases as distance from water increases; warmer temperature reduces retained ground moisture; lowland/basin retention is generally higher than exposed high terrain.

This is a gameplay/ecology moisture index. Do not label it physical relative humidity unless a separate atmospheric model is implemented. Temperature influences ground-moisture retention here as a simplified evaporation proxy; it is not a complete thermodynamic humidity equation.


## Generation execution and lifecycle

The procedural world generator must be callable explicitly in both Unity Editor and Play Mode. Editor generation and runtime generation must use the same generation core, settings model, seed derivation and ordered pipeline. Do not maintain two independent implementations.

### Generation entry points

Provide two user-facing entry points backed by one runtime-safe generation service:

1. **Editor entry point**
   - Provide a dedicated Unity Editor window or equivalent inspector tooling with a clear `Generate` action.
   - Allow the user to edit/select a generation configuration, seed, world dimensions, map resolution and output/preview mode.
   - Provide separate actions for `Generate`, `Regenerate`, `Export Maps`, and `Clear Preview`.
   - Show generation progress by stage, warnings, validation errors and the seed/configuration used.
   - Allow generation without entering Play Mode. The result must be inspectable in the Scene view and/or a dedicated preview area.
   - Editor-only asset creation, file dialogs and asset database operations must remain in Editor assemblies and must never be referenced by runtime code.

2. **Play Mode / runtime entry point**
   - Expose a runtime-safe API/service that can be called by a scene composition root, debug UI or gameplay flow while the game is running.
   - It must accept the same generation request/configuration as the Editor entry point and execute the same core pipeline.
   - Support generating a new world or regenerating a preview while in Play Mode. Make it explicit whether the result is preview-only or applied to the active world.
   - Runtime generation must not depend on `UnityEditor`, `AssetDatabase`, editor windows or editor-only file APIs.
   - Runtime exports, if supported, must use a runtime-safe destination/API and must be optional; the Editor export workflow remains the primary way to save diagnostic images.

### Generation request and result contract

Represent a generation run with an explicit request object rather than relying on hidden inspector state or global random state.

The request must include at least:
- world seed;
- generator version;
- world dimensions in world units;
- map dimensions/resolution (width and height);
- cell spacing / world-to-grid mapping;
- generation settings/configuration reference or immutable snapshot;
- selected output mode(s);
- whether to build only data/maps, a terrain surface, or a full asset presentation;
- whether the result is preview-only or applied to the active world.

The result must expose:
- the request/settings snapshot and seed used;
- each generated base and derived map as separately addressable data;
- validation results and warnings;
- optional terrain/mesh output;
- selected settlement sites and settlement graph;
- route polylines/edges and rasterized route mask;
- timing/statistics per generation stage.

A generation run must be reproducible from its seed, generator version and complete settings snapshot. Do not use Unity's global random state as an implicit source of generation values. Derive stable per-map random streams/seeds from the world seed and stable field identifiers.

### Ordered generation stages and dependencies

Generation must be an explicit pipeline with named stages and clear input/output dependencies. A stage may not silently regenerate or mutate an upstream map.

1. **Request validation** — validate dimensions, resolution, cell spacing, configuration ranges and generator version.
2. **Base noise fields** — sample deterministic Perlin Noise / multi-octave fBm for height, temperature, vegetation variation, water potential and settlement variation.
3. **Height field** — shape the height map; derive slope/gradient and any required terrain masks.
4. **Hydrology** — derive ocean/sea, lakes/ponds and river network from water potential, height and flow constraints; output water type/mask.
5. **Water distance** — compute the exact two-pass separable Euclidean Distance Transform from the water mask.
6. **Moisture** — derive ground moisture from water distance, terrain retention and temperature-dependent evaporation.
7. **Vegetation suitability** — combine vegetation variation with moisture, temperature, elevation and biome constraints.
8. **Biome classification** — classify each cell using height, temperature, moisture and water context.
9. **Settlement suitability** — compute and retain a distinct candidate suitability map for villages, towns and cities; select actual sites using spacing, area and exclusion constraints.
10. **Settlement graph** — create a sparse, hierarchy-aware graph between selected settlements, not a complete all-to-all graph.
11. **Traversal cost** — compute terrain-aware movement/building cost and forbidden cells for route planning.
12. **Road/path routing** — route only selected graph edges using A*/Dijkstra least-cost pathfinding; output route polylines and raster mask.
13. **Presentation output** — optionally build a flat colour map, height visualisation, terrain surface and/or asset-based preview.
14. **Validation and result publication** — validate dependencies and publish an immutable/inspectable generation result only when required stages succeed.

The generator must support running only the stages required by a selected output, while respecting dependencies. For example, a route preview still requires height, water, settlement selection, traversal costs and route planning; it cannot skip those stages merely because only the route layer is being displayed.

### Visualisation modes

Provide selectable visualisation modes. They must consume the same generated map data; changing the visualisation mode must not change the generated world.

1. **Flat thematic map**
   - Display the world from above as a 2D raster/grid with a configurable colour palette.
   - At minimum distinguish water, beaches/coasts, lowlands, plains/green areas, forests/vegetation density, hills, mountains/cliffs, settlement candidates/selected settlements and roads/trails.
   - Provide a legend and stable colour definitions. Do not rely on colour alone for important distinctions; allow individual layers to be toggled.
   - Allow either a composite overview or one selected map/layer at a time.

2. **Single-map debug view**
   - Display any map independently in grayscale or a field-specific colour ramp.
   - Required selectable layers: height, temperature, water potential/type, distance-to-water, derived moisture, vegetation suitability, biome ID/weights, settlement suitability, traversal cost, forbidden terrain mask and road/path mask.
   - Show normalized values and the map's actual data range. Distinguish raw noise fields from derived fields.
   - Allow overlays such as grid coordinates, world coordinates, cell values and selected-site/route markers.

3. **Height / 3D terrain view**
   - Convert the height map into a visible 3D surface using the selected terrain backend (Unity Terrain or custom mesh, which remains a separate architecture decision).
   - Support height exaggeration for inspection without modifying source height data.
   - Allow colour-by-height, colour-by-biome and slope visualisation.
   - Provide controls for vertical scale, surface visibility and wireframe/debug overlays where practical.

4. **Asset presentation preview**
   - Place available environment prefabs/assets on top of the generated terrain using the same placement data.
   - If final assets do not exist, use clearly identifiable placeholder prefabs (simple coloured cubes, low-poly primitives, capsules, billboard markers or proxy buildings).
   - Use distinct placeholder categories for trees/vegetation, rocks/cliffs, water features, villages, towns/cities, bridges and route markers.
   - Placeholder mode must not block or alter procedural map generation. Replacing a placeholder with a final prefab must not require changing the map-generation algorithm or its data contracts.

5. **Layered inspection**
   - Support enabling/disabling each visual layer independently and changing draw order/opacity where applicable.
   - Allow comparison of raw Perlin fields and derived maps side by side or through a layer selector.
   - Include a reset-to-default view and a way to inspect the exact seed/configuration used for the displayed result.

### Map image export

Every generated map must be exportable independently, not only as part of a screenshot of the composite preview.

- Export at the map's native grid resolution, without UI, legend or scene gizmos baked into the data image.
- Export a separate optional presentation image with legend/labels for reports and debugging.
- Use lossless formats. Use 16-bit PNG or EXR for height and other continuous numeric fields when preserving precision matters; 8-bit PNG is acceptable for colour previews and categorical masks when the mapping is documented.
- Export categorical maps (water type, biome ID, settlement class) with a documented integer/colour encoding so they can be re-imported without ambiguity. A colourised preview is not a substitute for the raw categorical data.
- Export a metadata sidecar containing map name, dimensions, world dimensions, cell spacing, value range/encoding, seed, generator version and relevant settings.
- Use stable filenames, for example `World_<seed>_<mapName>_<generatorVersion>`.
- At minimum support separate export for height, temperature, water, distance-to-water, moisture, vegetation, biome, settlement suitability, traversal cost and road/path mask.
- Export must never silently resample a map or discard precision. If a conversion is requested, record it in metadata.
- Keep Editor file writing and AssetDatabase import in Editor-only code. The core generator returns data; a separate exporter serializes it.

## Terrain representation

## Generated terrain in the Unity scene

The Editor generator must do more than calculate maps or display a 2D preview: it must be able to instantiate and generate the actual terrain surface in the currently open Unity scene. Provide an explicit `Generate & Apply to Scene` action (or an equivalent clearly exposed Generate-then-Apply workflow).

- Create or update a generator-owned root GameObject (for example, `GeneratedWorld`) and its terrain GameObject(s).
- Build the visible terrain surface from the generated height data using the selected backend: Unity Terrain or a custom mesh. Keep this backend decision independent of the procedural map-generation algorithms.
- Apply the generated world dimensions and X/Z cell spacing correctly; the current test size is 2,000 m × 2,000 m, configurable through generation settings; this is not the final world extent.
- Apply the generated terrain visual data (biome/material or vertex-colour representation) without modifying the source maps.
- Add the appropriate terrain collision representation for gameplay (TerrainCollider or MeshCollider, depending on the backend).
- Re-generation must update or replace the previous generator-owned scene output instead of creating duplicate terrain roots. Editor-created changes must support Unity Undo.
- Keep scene application as a presentation/adapter step consuming `WorldGenerationResult`; it must not contain a second implementation of the generation algorithms.
- Retain the Editor preview and map-export features as complementary tools. A preview window, texture, or exported heightmap by itself does not count as terrain instantiation.

Play Mode must expose the same scene-application capability through runtime-safe code, without referencing `UnityEditor`.

### Unity Terrain
Advantages: built-in heightmap terrain, terrain layers, alphamaps, detail/tree integration and familiar tooling.

Risks/constraints: terrain-specific limits, layer/shader constraints, runtime editing/streaming complexity and possible mismatch with custom topology.

### Custom mesh terrain
Advantages: full control over topology, chunks, vertex colours, mesh generation and custom data.

Risks/constraints: must implement more infrastructure: LOD, collision, normals, UVs, chunk seams, painting and tooling.

The terrain representation decision remains OPEN. The multi-map Perlin Noise generation specification above is independent of whether the generated height data is rendered with Unity Terrain or a custom mesh. Start with a small technical prototype and compare representations based on project needs.

## Texture maps

- Base Color/Albedo: visible colour.
- Normal: lighting detail.
- Roughness/Smoothness: surface response.
- AO: local occlusion.
- Height: parallax/displacement or blending where supported.
- Alpha: cutout/transparency where required.

Use only maps that materially improve the stylized result.

## Procedural texture distribution

Potential inputs:
- biome ID/weights;
- height;
- slope;
- temperature;
- moisture;
- vegetation potential;
- distance to paths/water/objects;
- authored masks;
- seeded noise.

Texture weights should be normalized and validated. Avoid hard transitions unless intentionally stylized.

## Runtime and editor responsibilities

Editor tooling creates reusable assets (materials, TerrainLayers, configuration assets) and validates imports. Runtime code applies/generates terrain state supported by the chosen representation. Keep UnityEditor APIs out of builds.

## Dynamic ground

Potential effects: wetness, mud, snow, footprints, flattened grass, leaf litter, resource traces. Prefer decals, overlays, instanced details or material parameters over modifying huge terrain textures for every small event. Choose implementation based on persistence, multiplayer visibility and performance.

## Layered spherical terrain authoring and dual-view inspection

For the first spherical-world prototype, terrain generation must be incremental and non-destructive:

- **Base sphere:** perfectly smooth reference sphere with no elevation displacement.
- **Layer 1 — macro relief:** large-scale landforms (plains, plateaus, basins, valleys and mountain masses).
- **Layer 2 — regional relief:** hills, ridges and secondary valleys.
- **Layer 3 — fine details:** small-scale variations constrained by actual mesh/map sampling resolution and anti-aliasing limits.

Each layer has independent settings, deterministic seed derivation, a separately inspectable contribution and an enable/disable state. Disabling a layer or selecting an earlier preview stage must not erase settings or mutate source data. Composition rules must be explicit and deterministic. Regenerating a layer must invalidate only that layer and its dependent composed outputs, not unrelated fields.

The generation studio must offer both a **spherical 3D view** and a **2D planisphere view** over the same spherical data. The planisphere is not a separate planar generation path. Its projection, inverse mapping, seam/wrap behaviour and distortion must be documented. Equirectangular may be used as the first prototype candidate, but is not locked as the final projection. Switching views is presentation-only and must not trigger regeneration.

A thematic-map selector must allow inspection of independent fields such as altitude, temperature, derived moisture, slope, water, biome and settlement suitability. The selected map changes only the visualisation (colour ramp/overlay); it must not change terrain geometry or map values. Raw and derived fields must be distinguishable, with range/legend and generation metadata. Do not display a map as available until its underlying generation stage exists.

The complete studio scope and measurable acceptance criteria are defined in `03_ENGINEERING/WORK_PACKAGES/WORLD-001_LAYERED_PLANET_STUDIO_AMENDMENT.md`.


## Initial stylized biome ground textures (WORLD-002)

The approved first texture direction is modern Pokémon-inspired stylization: soft painted-looking colour variation, clear readable forms and restrained surface contrast. Avoid photorealistic scan textures and avoid fine noise that overwhelms the terrain silhouette.

The first implementation uses deterministic tileable albedo-detail textures generated by `SphericalBiomeTextureGenerator`, then sampled through world-space triplanar projection in `SphericalBiomeBlend.shader`. The shared geographic biome weights remain the sole authority for biome identity and transition placement. The renderer must not infer or regenerate biome classification from texture colour/noise.

Initial repeat size is 18 world metres. This is a starting tuning value to validate at player height, medium distance and aerial distance. The first pass does not include normal/height/AO maps, shoreline foam, water waves or seasonal texture variants. See `WORLD-002_EXHAUSTIVE_VISUAL_ASSET_CATALOG.md` for paths and acceptance checks.
