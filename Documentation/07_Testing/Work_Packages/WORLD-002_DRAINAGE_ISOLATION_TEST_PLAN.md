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


## Test C — strict visual integration acceptance (mandatory)

A non-empty overlay mesh is **not** sufficient to pass integration. `EligibleSegments > 0`,
`RenderedSegments > 0`, a valid mesh bounds, or a successful `MeshRenderer` creation prove
only that geometry was generated; they do not prove that the geometry is aligned with, or
visible on, the displayed globe.

### Coordinate and surface contract

The drainage overlay and the globe preview must use the same local coordinate frame:

- `SphericalNeighbourGraph.GetDirection(node)` is the canonical unit direction.
- The preview root and its mesh children must have identity local position, rotation and scale
  unless a documented transform is deliberately applied to both terrain and overlay.
- The terrain radius at a direction is `planetRadius + SamplePreviewHeight(direction, settings, stage)`.
- `FilledElevationMeters` is a routing surface used by Priority-Flood only. It must never be
  used to position the rendered overlay.
- A diagnostic radial clearance is allowed only in the explicit renderer-isolation step.
  It must not be mistaken for a production offset or accepted as evidence of surface alignment.
- Globe ribbons must be built along the spherical arc between adjacent graph directions;
  a straight chord through the sphere is invalid.
- Ribbon winding/material must render the outward-facing side under the project's active
  render pipeline and back-face culling policy.

### Required automated / logged diagnostics

The integration preview must report, separately:

- eligible and rendered segment counts;
- number of eligible segment midpoints on the camera-facing hemisphere;
- number of rendered segment midpoints on the camera-facing hemisphere;
- number of rendered vertices whose radial distance is below the sampled terrain radius;
- minimum, median and maximum radial clearance above the sampled terrain;
- preview-root world position/rotation/scale and overlay-child local position/rotation/scale.

The camera-facing hemisphere is determined in the preview root's local frame using the Scene
View camera position transformed into that frame. A segment midpoint is camera-facing when
`dot(midpointDirection, cameraDirection) > 0`. Do not infer front/back visibility from node ID
ranges or cube-face ordering.

### Manual visual procedure

1. Generate the normal terrain preview (`Preview Stage = Final`, `View = Spherical`).
2. Select `Drainage Input = Terrain Heightfield`, resolution `512`, and a sea level that produces
   both water outlets and land nodes.
3. Generate the drainage preview with the normal production clearance, not the exaggerated
   renderer diagnostic.
4. Frame the entire globe, then inspect at least three camera orientations separated by roughly
   90 degrees around the globe. Do not rotate the globe object to perform this check; orbit the
   Scene View camera.
5. Inspect the near hemisphere at a closer zoom. At least some complete drainage paths must be
   visible across the face of the globe, not only as fragments at the silhouette.
6. Clear the drainage overlay and regenerate it. The result must not duplicate renderers or
   change the numerical drainage result.

### Pass / fail criteria

Test C passes only when all of the following hold:

- A (logic) and B (renderer-only) pass independently.
- The overlay is generated from the actual `DownstreamNodeIds`, `FlowAccumulation` and `RiverMask`
  of the selected drainage input.
- At least 100 eligible segments are located on the camera-facing hemisphere in the test view;
  if fewer exist, the test fixture/view is invalid and must be diagnosed rather than counted as
  a renderer pass.
- At least 90% of those camera-facing eligible segments are represented by rendered geometry
  (after deterministic sampling, if any).
- At least 90% of the rendered near-hemisphere segment midpoints are visibly outside the terrain
  surface, with no systematic inward placement.
- Continuous visible paths can be followed over the near hemisphere; isolated dots only at the
  globe silhouette are an automatic FAIL.
- The overlay follows the spherical terrain without long straight chords, detached floating
  arcs, large filled patches, or visible face-seam discontinuities.
- The ordinary production clearance is the smallest stable offset that prevents z-fighting at
  the display mesh resolution. The 2 km forced-clearance diagnostic is not an acceptable final
  setting.
- Clearing and regenerating leaves exactly one drainage overlay child and does not alter the
  underlying drainage arrays.

### Failure isolation

- A fails: investigate drainage inputs, outlet mask, Priority-Flood, downstream links and
  accumulation. Do not adjust rendering.
- A passes and B fails: investigate shader, material, mesh winding, bounds, renderer state and
  Scene View visibility. Do not adjust hydrology.
- A and B pass, but the near-hemisphere count is zero or unexpectedly low: investigate the
  direction frame / camera-frame conversion and graph-to-preview orientation before changing
  width, lift or threshold.
- Near-hemisphere candidates exist but vertices are inside the terrain: investigate the
  sampled terrain surface, preview stage, root transform and child transform.
- Vertices are outside but only silhouette fragments are visible: investigate ribbon winding,
  material culling, depth testing, mesh bounds and camera-facing geometry.
- Only the 2 km diagnostic makes the overlay appear: integration remains FAIL; do not ship that
  offset as a workaround.

### Current evidence — 2026-11-18

Owner-provided Unity screenshot after the forced-clearance diagnostic:

- Input: `Terrain Heightfield`
- Preview stage: `Final`
- View: `Spherical`
- Drainage resolution: `512`
- Sea level: `0 m`
- Accumulation threshold: `50`
- Water nodes: `794,333`
- River candidates / eligible segments / rendered segments: `88,875 / 88,875 / 88,875`
- Sampling stride: `1`
- Overlay vertices: `355,500`
- The screenshot shows only sparse blue fragments around the globe silhouette; no continuous
  drainage paths are visible across the near hemisphere.

**Status: Test C FAIL — visual integration not accepted.** The counts prove that candidate
geometry is produced, but the current screenshot does not prove correct near-side placement or
visibility. Test A and Test B remain independently passed according to owner-reported results.


## Renderer correction — opaque depth-writing material (2026-11-18)

The previous overlay material selected `Sprites/Default` first. That shader is transparent and does not write depth. Since the terrain consists of six large preview renderers, whole-renderer transparent sorting can draw the drainage overlay before terrain faces and allow terrain to overdraw it. This is a plausible explanation for a dense overlay mesh whose visible fragments remain near the globe silhouette; it must be confirmed in Unity.

The overlay now uses `Hidden/Pokemon/DrainageOverlayOpaque`: an opaque vertex-colour shader with Geometry render queue, `ZWrite On`, `ZTest LEqual`, `Cull Off`, and blending disabled. The 2 km forced-clearance diagnostic is removed; the preview returns to a 300 m minimum / 0.6% radius clearance.

The preview log adds camera-facing eligible/rendered segment counts, vertices below sampled terrain, min/median/max radial clearance, and root/child transforms. These measurements support failure isolation; they do not by themselves constitute visual acceptance. Test C remains FAIL until continuous drainage paths are visibly verified on the near hemisphere.
