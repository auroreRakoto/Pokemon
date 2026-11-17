# WORLD-001 — Layered Planet Studio Implementation Progress 01

- **Work package:** WORLD-001
- **Amendment:** Layered generation and dual-view inspection
- **Status:** IMPLEMENTATION SLICE PREPARED — Unity validation pending
- **Agent:** WORLD
- **Plan milestone:** Project owner confirmed approved and pushed amendment on 2026-11-17

## Implemented in this slice

### Runtime-safe generation core
- Added `SphericalPlanetStudioSettings` as a ScriptableObject configuration asset.
- Added `SphericalPlanetStudioGenerator` under the runtime spherical assembly; it has no UnityEditor dependency.
- A smooth base sphere is produced when all three relief layers are disabled.
- Added a preview-stage selector (Base Sphere, Layer 1, Layer 2, Layer 3, Final) so earlier cumulative stages can be restored without changing later layer settings.
- Layer 1 (macro relief), Layer 2 (regional relief) and Layer 3 (fine detail) have separate enabled states, amplitudes and frequencies.
- Layer contributions are sampled independently from direction on the unit sphere and composed additively. The same seed and settings reproduce the same output.
- Layer 2/3 frequency is capped relative to the globe face sampling resolution to reduce obvious undersampling in this prototype.

### Dual presentation views
- Spherical view generates six Cube-Sphere face meshes.
- Planisphere view uses an equirectangular mapping and samples the same direction-based height functions.
- Added forward and inverse planisphere coordinate conversion helpers.
- Projection changes are presentation-only; no separate planar noise field is used.

### Thematic map previews
- Implemented Terrain, Altitude and Temperature colour visualisation.
- Temperature is a prototype normalized field influenced by latitude, seeded Perlin variation and positive elevation cooling.
- Moisture, Water, Biomes, Villages and Slope are selectable in the enum but blocked from generation with an explicit unavailable message. No placeholder data is presented as a real map.

### Editor tooling
- Added `Tools > World > Open Planet Generation Studio`.
- The window creates/loads a persistent settings asset at `Assets/Project/Content/World/PlanetStudio/SphericalPlanetStudioSettings.asset`.
- Generate/Refresh Preview creates a scene-owned preview root with the selected view and map.
- Clear Preview removes generated preview objects and owned meshes/material.

## Files added

- `Assets/Project/Runtime/World/Spherical/Studio/SphericalPlanetStudioSettings.cs`
- `Assets/Project/Runtime/World/Spherical/Studio/SphericalPlanetStudioGenerator.cs`
- `Assets/Project/Editor/World/Spherical/SphericalPlanetGenerationStudioWindow.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalPlanetStudioTests.cs`
- Corresponding Unity `.meta` files and the Studio folder `.meta`.

## Automated checks added (not yet executed in Unity)

- All layers disabled produce vertices at the configured base radius.
- Layer contributions are deterministic and can be hidden/restored without losing their settings.
- Planisphere direction-coordinate round trip preserves the sampled spherical field; the left/right wrap boundary maps to the same direction.
- Temperature values remain in [0, 1].

## Known limitations / not yet implemented

- The current Perlin implementation uses a direction-weighted three-plane blend as a seam-continuous prototype sampler. It requires visual seam and gradient QA; it is not yet the final production noise implementation.
- Surface normals in the Cube-Sphere preview are radial approximations; slope-aware normals remain a follow-up.
- Equirectangular planisphere has expected polar distortion and a duplicated wrap boundary; projection comparison and seam QA remain open.
- The planisphere is a mesh preview, not yet a dedicated zoom/pan map viewport with legend and export.
- Temperature is an initial normalized inspection field, not a climate simulation.
- Moisture, hydrology, slope map, biome classification and settlement suitability/location maps are not implemented and remain unavailable.
- No caching/invalidation system or independent field export is included yet.
- Existing runtime demo/bootstrap is preserved and has not been replaced.
- Unity Editor compilation, NUnit execution, scene rendering, and manual seam inspection were not available in this environment and must be performed by the project owner/QA.

## Manual validation steps

1. Open `Tools > World > Open Planet Generation Studio`.
2. Confirm the settings asset is created and persisted.
3. Set all three layers disabled; generate a spherical preview and verify a smooth sphere.
4. Enable only Layer 1; regenerate and verify broad landforms without sharp radial spikes.
5. Enable Layer 2, then Layer 3; verify progressively finer relief.
6. Disable Layer 2/3 again; verify Layer 1 output returns unchanged.
7. Switch to Planisphere and verify the same landform arrangement appears in equirectangular form.
8. Inspect Terrain, Altitude and Temperature maps. Confirm unavailable maps are blocked.
9. Check all six cube-face seams and planisphere left/right wrap, especially near both poles.
10. Run the `Pokemon.Project.Tests.World.Spherical.EditMode` test assembly.

## QA / acceptance

- **Development checks:** source-level review only; Unity compilation and tests not executed here.
- **QA status:** BLOCKED pending Unity import, compilation, EditMode tests and visual inspection.
- **Project-owner approval:** pending validation evidence.
