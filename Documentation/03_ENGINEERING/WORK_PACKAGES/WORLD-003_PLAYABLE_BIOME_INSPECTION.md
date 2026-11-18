# WORLD-003 — Playable Biome Inspection

## Purpose

Provide a temporary Play Mode scene in which the developer can inspect the current WORLD-002 spherical terrain, ocean distribution and four biome classifications from player height. This is a validation scene, not the production world/session bootstrap or final player controller.

## Source of truth

- Uses the existing `SphericalPlanetStudioSettings` asset at `Assets/Project/Content/World/PlanetStudio/SphericalPlanetStudioSettings.asset`.
- Calls the runtime-safe `SphericalGeographicFieldGenerator.Generate(settings)` pipeline.
- Builds the six globe faces from that exact `SphericalGeographicFieldSet` with `SphericalPlanetStudioGenerator.GenerateGeographicGlobeFaces(..., SphericalPlanetStudioMap.Biomes)`.
- The player surface sampler reads `AltitudeMeters` from the same field set. It must not fall back to the older WORLD-001 height sampler, because that would make the player float or sink relative to the displayed WORLD-002 terrain.
- The inspection scene does not modify the Planet Studio preview or the existing World01/World02 scenes.

## Create and run

1. Open Unity and select `Tools > World > Create Playable Biome Inspection Scene`.
2. The tool loads the existing Planet Studio settings asset. If it does not exist, it creates one with all three terrain layers enabled and the current default settings.
3. The tool creates and saves `Assets/Project/Content/Scenes/Worlds/WORLD-002_BiomeInspection.unity` as a separate scene.
4. Enter Play Mode. The runtime bootstrap generates the geographic fields and biome globe, then places a temporary capsule on the lowest-slope plains cell found near a biome boundary.

The scene uses the same seed, radius, terrain layers, ocean target and biome thresholds as Planet Studio. Change those settings in Planet Studio, regenerate the playable scene by entering Play Mode again, and compare the result.

## Controls

- `WASD` / arrow keys: walk over the spherical surface.
- `Space`: temporary radial jump.
- Hold right mouse button and move mouse: orbit camera.
- Mouse wheel: zoom.
- `Page Up` / `Page Down`: camera debug altitude.
- `Home`: reset camera debug altitude.

## Scope boundaries

Included:
- exact WORLD-002 geographic field generation;
- globe mesh with biome vertex colours;
- low-slope plains spawn selection;
- field-aware spherical walking and simple jump;
- existing spherical third-person camera and runtime `SphericalWorldDemoOverlay` HUD, which displays the seed, planet radius and documented controls;
- no terrain mesh colliders; the temporary controller samples the generated geographic fields directly.

Not included:
- production character model, animation, input rebinding or full physics controller;
- production collision response, slope blocking, ledge handling or traversal abilities;
- chunk streaming / adaptive quadtree;
- Pokémon spawning, ecology simulation, combat, persistence or networking;
- vegetation, authored ground materials, shoreline assets or final lighting.

## Acceptance checks

### Manual Play Mode checks

1. The inspection scene opens without modifying the existing world scenes.
2. Entering Play Mode generates six globe faces and one temporary player/camera rig.
3. The Console reports the settings seed, face resolution, actual ocean coverage and spawn biome; spawn biome must be `Plains`.
4. The player capsule starts above the sampled surface, with no visible initial penetration or floating gap.
5. WASD/arrow movement follows the local tangent plane and keeps the player aligned to the local radial up direction while moving around the globe.
6. Space produces a short radial jump and the player returns to the sampled surface.
7. Right-mouse orbit, wheel zoom and camera altitude controls work.
8. The player can walk across at least one visible biome boundary; the terrain colours remain those of the generated field set.
9. Regenerating in a new Play Mode session with the same seed/settings gives the same spawn direction and biome map.
10. Exiting Play Mode removes generated runtime objects without changing the saved settings asset or source scenes.

### Automated coverage

The existing WORLD-002 EditMode suite remains responsible for field determinism, topology, ocean coverage and biome classification. This inspection scene adds a manual Play Mode integration check because its primary purpose is visual and input validation. Do not add the full high-resolution geographic generation to every fast EditMode test run.

## Known limitations

- Spawn selection searches for a plains cell with slope at most 18 degrees and prefers one whose immediate tangent neighbourhood contains another biome. If none exists, generation fails with an explicit message rather than silently spawning in ocean or another biome.
- The temporary controller is kinematic and samples the field set directly. The six planet-scale preview meshes intentionally have no `MeshCollider`: at the current face resolution, their triangle edges exceed Unity’s 500 m physics-stability guideline. This inspection scene performs no physics raycasts or collision queries against the terrain, so attaching those colliders would add warnings and cost without providing a used capability.
- Terrain collision must be implemented separately before production physics interaction: use spatially bounded/chunked collider meshes with sufficient tessellation so no triangle edge exceeds the documented 500 m guideline. Do not re-enable a single collider per full cube-sphere face as a shortcut.
- The current biome preview uses vertex colours and an unlit vertex-colour material. This is intentionally a diagnostic presentation; visual polish is a later step.
