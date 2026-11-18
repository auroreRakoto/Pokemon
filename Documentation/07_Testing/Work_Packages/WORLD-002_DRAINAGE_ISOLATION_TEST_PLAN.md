# WORLD-002 — Drainage logic and renderer isolation tests

## Purpose
Determine independently whether a missing drainage overlay is caused by hydrology data or Unity rendering. Do not infer a successful renderer from a successful drainage calculation, or vice versa.

## Test A — logic only
Use `Run Logic Only` in Planet Studio or run `SphericalDrainageLogicIsolationTests` in EditMode. This path calls the drainage algorithm and emits numerical diagnostics. It must not create a Mesh, GameObject, MeshRenderer or Material.

Required checks:
- Known polar outlet is the only outlet in the one-outlet synthetic cases.
- Every downstream chain terminates at the expected outlet.
- Outlet accumulation sum equals graph node count within 0.01 contributing cells.
- RiverMask is exactly the threshold predicate and excludes outlets.
- Raising the threshold never adds river candidates.
- Compare min, median, p95, max accumulation and candidate count in the log.

A flat one-outlet sphere is only a deterministic stress case. It is not a physically meaningful river network; large catchments converging on one outlet can produce broad accumulation.

## Test B — renderer only
Use `Renderer Only` in Planet Studio. This path must not call `SphericalDrainage.Build`, `SphericalDrainageStudioPreview.Generate`, or `BuildOverlayMesh`. It creates a display globe plus three thick, bright test ribbons at known spherical directions. The test mesh is double-sided and uses a diagnostic unlit material.

Required checks:
- Diagnostic mesh has vertices, triangles and non-empty bounds.
- Root and renderer are active.
- Three test ribbons are visible in Scene View.
- If not visible, inspect shader/material, transform hierarchy, bounds and camera clipping; hydrology is not involved.

## Test C — integration
Only after A and B pass, run `Generate Drainage Preview`. If A passes and B passes but C fails, inspect conversion from graph directions/elevations into overlay vertices and the preview root transform.

## Status
This patch adds the independent UI actions and automated logic tests. Unity compilation, EditMode execution and visual confirmation remain pending owner execution.


## Test C — drainage-to-renderer integration (implementation update)

The editor integration now consumes the real `SphericalDrainageResult` arrays and creates the overlay from graph directions and downstream links.

### Preview geometry budget
- The preview counts eligible segments before allocating mesh buffers.
- It deterministically samples the eligible sequence when it exceeds 150,000 segments.
- `EligibleSegments`, `RenderedSegments`, `SamplingStride`, and vertex count are written to the Unity Console.
- This cap is a Scene-view preview safeguard only; it does not modify drainage data or river masks.

### Visual settings
- Use `Terrain Heightfield` for terrain-derived drainage.
- Use `Final` preview stage and `Spherical` view for the first globe inspection.
- `Flat Base Sphere Diagnostic` remains an artificial one-outlet algorithm stress case and must not be interpreted as a natural river network.
- The overlay uses a deliberately exaggerated visual width/lift for inspection, not physical river dimensions.

### Acceptance criteria
1. `Terrain Heightfield` generates at least one water outlet for the selected sea level.
2. The Console reports non-zero `EligibleSegments` when the accumulation threshold selects river candidates.
3. The mesh has non-zero vertices and valid bounds.
4. The overlay is visible on the globe and follows the spherical surface.
5. `RenderedSegments` is at most 150,000; any reduction is reported through `SamplingStride`.
6. The overlay can be cleared and regenerated without duplicating drainage objects.
7. Changing overlay mode changes presentation only; drainage numerical results remain unchanged.

### Current evidence / status
- Test A (logic-only): user reports all EditMode tests pass; 60 total tests, 0 failures.
- Test B (renderer-only): user visually confirmed the three coloured spherical ribbons are visible.
- Test C (real drainage integration): implementation added; Unity visual confirmation is pending.
