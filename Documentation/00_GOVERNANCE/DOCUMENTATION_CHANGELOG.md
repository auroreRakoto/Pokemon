# Documentation Changelog

Record meaningful documentation changes. Keep entries concise and link to proposals/decision IDs.

## Unreleased
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
