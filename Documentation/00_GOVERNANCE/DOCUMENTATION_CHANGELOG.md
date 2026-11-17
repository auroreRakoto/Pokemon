# Documentation Changelog

Record meaningful documentation changes. Keep entries concise and link to proposals/decision IDs.

## Unreleased
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
