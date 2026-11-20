- WORLD-002: added the first runtime geographic-field tranche (altitude, temperature, slope) and implementation progress report; hydrology/moisture remain gated.

- WORLD-002: added exhaustive scoped visual asset catalogue, Blender production batch registration set, and per-asset owner approval register.

- WORLD-002: added a mandatory owner-approved asset-list gate before visualisation generation and required registration/preflight in Blender's environment batch generator.

# Documentation Changelog

Record meaningful documentation changes. Keep entries concise and link to proposals/decision IDs.

## Unreleased
- MOD-001-C: added optional spherical biome classification, explicit absence APIs, biome-independent ground probing, independent geographic data preview and EditMode regression tests. Recorded DEC-049 as owner-confirmed.
- Added DEC-042 confirming hybrid vegetation presentation (biome-aware ground textures/materials plus deterministic 3D vegetation instances). Added the proposed WORLD-002 spherical geographic-fields plan, including a globe-correct hydrology/distance design gate, independent map outputs, dual-view inspection/export, biome and settlement suitability, and vegetation preview scope.
- Clarified the single source-data location at `Assets/Project/Data`; renamed the runtime data-services folder to `Runtime/DataServices` to avoid a second `Data` directory. Added DEC-020 and updated the canonical Unity layout.
- Added the confirmed two-milestone Git workflow, Conventional Commit format and mandatory agent attribution footer.
- Added DEC-016 to record the project-owner-approved Git workflow.
- Added the repository alignment plan and static audit findings for the supplied Unity snapshot.
- Updated specialist onboarding and AI instructions to require the canonical Git workflow.
- Initial Holy Grail documentation baseline created.
- Existing project decisions captured; unresolved technical choices explicitly marked OPEN or PROPOSED.


## 2026-11-17 — CLEANUP code ownership reorganization

- Reorganized Unity C# script locations to match module ownership and runtime/editor boundaries.
- Separated Pokemon definitions, individual needs, behaviour scheduling and population spawning.
- Separated Ecology resource definitions, instances, lifecycle systems, generation and world data.
- Moved UI-facing resource minimap and simulation speed controls to Runtime/Presentation.
- Moved cross-domain ecosystem telemetry to Runtime/Infrastructure/Telemetry.
- Grouped World biome/world definitions, generated data, generation, placement and presentation by responsibility.
- Grouped runtime Data contracts/loading/projections and Editor content/data/Blender tooling.
- Preserved every moved script's Unity `.meta` GUID. No gameplay logic or serialized field names were intentionally changed.
- Unity Editor compilation, scene/prefab import and Play Mode validation remain pending.


## 2026-11-17 — Runtime ownership and behaviour decomposition

- Split the PokemonAgent god object into needs, daily routine, foraging/wandering behaviours, mortality, reproduction, movement and stable identity responsibilities.
- Added narrow food, movement, reproduction and spawn-location contracts.
- Moved cross-domain scene wiring and world generation composition into `Runtime/Composition`.
- Separated World biome-region generation from Ecology field composition.
- Split authoritative FruitBush resource state from Presentation fruit visuals.
- Removed scene-wide Pokemon discovery from telemetry; the composition root supplies the population read model.

## 2026-11-17 — Dependency direction and Unity factory boundary

- Moved the food query/consume port to Ecology as `IEcologyFoodService`; Pokemon consumes the Ecology contract without Ecology depending back on Pokemon.
- Moved map renderers into Presentation and the habitat-aware spawn-location implementation into Ecology, leaving World domain code independent of Ecology/Presentation implementations.
- Added `IPokemonFactory` and `PokemonUnityFactory` so population rules no longer instantiate/destroy GameObjects or add concrete movement components.
- Added `PokemonPopulationProfile` assets to separate scenario population tuning from species biology.
- Added the full 59-file / 196-method ownership inventory and the implementation compliance report.

## 2026-11-17 — Settlement suitability and route network specification

- Reaffirmed the settlement-suitability map as a mandatory, separate output for village, town and large-city candidate locations.
- Added a distinct mandatory road/path network map derived after settlement selection.
- Specified settlement graph construction and terrain-aware A*/Dijkstra least-cost routing, including slope, water crossings, biome/ground costs, deterministic routing and route-mask outputs (DEC-018).

## 2026-11-17 — World generation tooling and visual inspection

- Specified a shared generation request/result contract and one runtime-safe generation core callable from both Unity Editor and Play Mode.
- Added flat thematic, single-map debug, 3D height and asset/placeholder visualisation modes.
- Required independent native-resolution export of every map with precision-aware formats and metadata sidecars.
- Formalized sparse settlement connectivity: villages normally connect to one suitable town/city; routes avoid or reject cliffs, ravines and impassable terrain rather than forcing all settlements into an all-to-all network (DEC-019).


## 2026-11-17 — Explicit terrain instantiation in Unity scenes

- Clarified that procedural generation is launched from a dedicated Unity Editor tool and is also callable in Play Mode through the same runtime-safe core.
- Made actual terrain creation in the active Unity scene a required outcome, not merely a 2D/3D Editor preview or exported map.
- Added the `Generate & Apply to Scene` workflow, generator-owned scene root/update behaviour, backend-specific terrain collider requirement and Undo support.
- Recorded the initial configurable world target as 2,000 m × 2,000 m; kept Unity Terrain versus custom mesh as an open renderer decision (DEC-019).

## 2026-11-17 — WORLD-001 layered planet studio and dual views

- Recorded DEC-040: the first spherical prototype is a non-destructive, inspectable stack consisting of a smooth base sphere plus macro, regional and fine-detail terrain layers.
- Recorded DEC-041: the same generated spherical data must be inspectable in both a 3D spherical view and a 2D planisphere view.
- Added a dedicated WORLD-001 scope amendment defining independent thematic maps, presentation-only map visualisation, layer controls, projection behaviour, exclusions and acceptance criteria.
- Preserved the original WORLD-001 plan and approval history; the expanded scope requires its own approved and pushed plan milestone before implementation.


## 2026-11-17 — WORLD-001 Planet Studio first implementation slice

- Recorded project-owner approval and pushed-plan status for the layered Planet Studio amendment.
- Added a runtime-safe shared generation core and ScriptableObject settings for a smooth base sphere plus independently enabled macro, regional and fine-detail relief layers.
- Added a Unity Editor Planet Generation Studio window to configure seed, radius, sampling resolutions, layer amplitudes/frequencies, cumulative preview stage, view and map selection.
- Added spherical Cube-Sphere preview and an equirectangular planisphere preview sampling the same direction-based field functions.
- Implemented terrain, altitude and temperature visualisation. Moisture, water, biome, village and slope maps remain explicitly unavailable until their generation stages exist.
- Added EditMode tests for smooth base-sphere radius, deterministic non-destructive layer recomposition, planisphere coordinate round trips and bounded temperature values.
- Unity Editor compilation and actual visual QA remain pending; the current implementation slice is not marked accepted.


## 2026-11-17 — WORLD-002 asset readiness amendment

- Recorded owner-confirmed approval/push status for WORLD-002.
- Added an explicit asset inventory based on the supplied project snapshot, distinguishing imported environment models, Blender source generators, test-only assets, missing production assets, and Unity prefab/material requirements.
- Identified existing grass, wild-grass and reed models as candidates for the first vegetation preview; identified tree models and biome ground materials/textures as gaps in the inspected snapshot.
- Added asset preparation tasks, canonical content-root/pipeline constraints, and minimum asset acceptance criteria.
- Clarified that field generation must continue with labelled debug presentation when production art assets are unavailable; no placeholder is to be represented as final art.

## 2026-11-18 — WORLD-004 plan

- Added the WORLD-004 player controller and spherical locomotion plan, separating production movement from the temporary WORLD-003 inspection controller.
- Clarified field-based ground contact, radial gravity, water/slope policy, camera reuse, collision limitations, test coverage and acceptance criteria in the world architecture.
- Status is plan-ready; implementation remains gated on project-owner approval and the required plan milestone push.


## 2026-11-18 — WORLD-004 first implementation increment

- Added spherical field-aware ground probing and a world-space player motor/controller.
- Integrated the controller into the temporary WORLD-003 playable inspection bootstrap.
- Kept planet-scale preview meshes free of MeshColliders.
- Added focused EditMode motor tests and documented the current approximation boundaries and Unity validation sequence.
- Status remains implementation-in-progress until Unity compilation, tests and manual Play Mode checks are reported passing.
\n\n## 2026-11-18 — WORLD-010-A/B biome texturing implementation checkpoint\n\n- Integrated the existing opaque URP biome blend shader and world-space triplanar mapping into the playable spherical inspection bootstrap.\n- Switched playable mesh generation to normalized geographic biome weights and the dedicated ocean mask.\n- Added an Editor command to assign the existing biome material mapping to an open inspection scene without recreating it.\n- Added distinct deterministic Plains grass-striation and Forest moss/leaf-litter texture patterns; kept Desert and Mountains on their existing detail patterns for WORLD-010-C.\n- Preserved the solid-colour background correction and documented the 18 m physical texture repeat scale.\n- Added focused texture regression tests. Unity compilation, EditMode execution and visual validation remain pending.\n

## 2026-11-18 — WORLD-010 biome texture visibility correction

- Confirmed the mapping asset is assigned; the legacy single `Biome Material` slot is not used by the blended renderer and may remain empty.
- Replaced the pale Plains tint with a grass-green palette and strengthened the procedural grass blade/tuft and forest floor patterns.
- Reduced physical texture repeat size from 18 m to 8 m and added a Plains contrast regression test.
- Unity material regeneration and Play Mode visual validation remain pending.


## 2026-11-18 — WORLD-010 production texture restoration

- Removed the temporary checkerboard diagnostic shader and texture generator from the production path; restored the normal opaque biome blend shader.
- Restored the stronger procedural Plains/Forest texture patterns and green Plains material tint.
- Confirmed the diagnostic checkerboard was visible when the separate `WORLD-001_PlanetStudioPreview` sphere was disabled, identifying the preview as the surface that had masked the playable textured planet.
- Updated the playable inspection bootstrap to disable that exact legacy preview root for the duration of Play Mode.
- Unity validation of the restored production textures remains pending; no feature-complete commit should be made yet.


## 2026-11-19 — WORLD-010 Plains grass readability pass

- Replaced subtle Plains noise marks with deterministic, tapered grass-blade strokes and wrapped tile-edge rasterization.
- Brightened the Plains material tint to improve the green read in gameplay.
- Added a focused Plains texture variation regression test.
- Unity compilation, generated texture refresh and visual validation remain pending; do not mark WORLD-010-A/B complete yet.


## 2026-11-19 — WORLD-010 Forest texture and selectable spawn biome

- Added tileable Forest floor details with moss/ground-cover patches and warm leaf-litter flecks.
- Exposed the desired spawn biome in the playable inspection bootstrap Inspector (Plains default; Forest, Desert and Mountains selectable).
- Spawn selection now searches dry land in the requested biome, prefers low-slope interior cells and falls back to the flattest available cell when necessary.
- Added a Forest texture detail regression test. Unity compilation, EditMode execution and visual validation remain pending.


## 2026-11-19 — MOD-001-A profile module configuration

- Added versioned module options to spherical world profiles.
- Added an Editor window for configuring terrain presentation, biome classification/presentation, environment assets, player and future Pokémon capabilities.
- Added compatibility defaults for profiles created before the module section existed.
- Added EditMode tests for defaults, JSON round-trip and unsupported/missing module configuration.
- Runtime activation/deactivation is explicitly deferred to MOD-001-B and later increments.
