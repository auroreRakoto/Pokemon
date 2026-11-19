# WORLD-011-A — Current Architecture Audit

**Audit basis:** `Pokemon-clean (1).zip`, project root `Pokemon-clean/`, inspected 2026-11-19.

## Confirmed from source

- `SphericalBiomeInspectionBootstrap` is the runtime composition root for the playable inspection scene.
- It calls `SphericalGeographicFieldGenerator.Generate(settings)` once and stores the resulting `SphericalGeographicFieldSet` in `_fields`.
- The same `_fields` instance is consumed by the visible biome surface generator and the player controller.
- `SphericalGeographicFieldSet` stores altitude, temperature, slope, distance-to-water, moisture, water class, and biome arrays over six cube-sphere faces.
- Continuous fields use bilinear direction-based sampling; categorical biome/water values use nearest-cell sampling.
- `SphericalBiomeSurfaceMeshGenerator` creates six face meshes, stores biome weights in vertex RGBA, ocean blend weight in UV2.x, and uses radial normals.
- The inspection bootstrap owns generated scene objects under `WORLD-002_GeneratedPlayableInspection` and clears that root during regeneration/destruction.
- The planet inspection surface intentionally has no planet-scale MeshCollider.
- `SphericalSurfaceQuery.GetTangentFrame` supplies a radial-up tangent frame.

## Not confirmed in the inspected ZIP

- The separate `EnvironmentPrefabGeneratorWindow.cs` editor utility was not present in this ZIP.
- The ZIP did not include the user's post-generation Unity prefab output. Exact prefab paths, pivots, scales, bounds, hierarchy, and materials therefore cannot be audited from this archive.
- A complete tree model was not present in the inspected production FBX inventory.

## Consequence

WORLD-011-A remains necessary. The architecture section in `WORLD-011_PROCEDURAL_ENVIRONMENT_PLACEMENT.md` documents the verified current runtime structure and explicitly labels the proposed WORLD-011 components as not yet implemented. The actual generated prefab assets must be inspected from the current Unity project before implementation proceeds.
