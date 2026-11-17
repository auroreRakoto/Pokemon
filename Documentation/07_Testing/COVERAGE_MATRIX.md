# Testing Coverage Matrix — Existing Implemented Features

**Scope inspected:** supplied Unity project snapshot and testing documentation available for WORLD-001/WORLD-002.  
**Execution status:** the project owner reports that all currently available tests pass. Exact test counts and a fresh Test Runner export were not supplied with this documentation update; therefore this matrix records test presence separately from execution evidence.

Legend:
- **PRESENT** — a test currently exists in the inspected project snapshot.
- **GAP** — behaviour is implemented but no matching automated test was found in the inspected test files.
- **PLANNED** — a concrete test is specified below but has not been authored/verified as present.
- **MANUAL** — requires visual/editor workflow evidence in addition to automated tests.
- **N/A** — not implemented in the inspected slice.

## 1. Existing automated test inventory

| Test file | Current tests / behaviours covered | Test layer |
|---|---|---|
| `CubeSphereTopologyTests.cs` | Six-face direction grids; normalized finite directions; direction/face-coordinate round-trip; shared face-edge directions | EditMode |
| `CubeSphereTopologyRegressionTests.cs` | Face-centre mapping; finite normalized grid directions; zero-direction rejection | EditMode |
| `SphericalHeightFieldTests.cs` | Same-seed determinism; different-seed variation; configured elevation range | EditMode |
| `SphericalPlanetStudioTests.cs` | Disabled layers produce base sphere; layer contribution determinism/independence; globe/planisphere share direction-based height; preview stage restoration; thematic map does not alter geometry; planisphere wrap; temperature bounds; topology round-trip; invalid direction rejection; settings clamping | EditMode |
| `SphericalPresentationTests.cs` | Surface radius + elevation; tangent frame orthonormality; six generated face meshes with finite vertices/outward normals | EditMode |
| `SphericalGeographicFieldGeneratorTests.cs` | Field dimensions/metadata; deterministic fields; declared value ranges; finite directional sampling; shared face-edge altitude/temperature values | EditMode |
| `SphericalNeighbourGraphTests.cs` | Shared-edge/corner node merging; symmetric positive connected graph; cross-face seam identity; deterministic multi-source Dijkstra; graph-vs-great-circle benchmark report | EditMode |
| `SphericalPlanetStudioPlayModeTests.cs` | Six globe face meshes; planisphere mesh; runtime GameObject attachment over a frame; repeated generation and cleanup | PlayMode |

The test runner's green state is owner-reported. This inventory is based on test source files, not a newly exported Unity Test Runner XML report.

## 2. WORLD-001 — spherical world foundation

| Implemented component | Existing coverage | Missing automated coverage |
|---|---|---|
| `CubeSphereTopology` | Direction mapping, normalized face grids, round-trip, shared edges, zero vector rejection | All twelve face-pair seams explicitly; all eight cube corners and tie-break policy; invalid enum/coordinate behaviour |
| `SphericalHeightField` | Determinism, seed variation, configured range | Zero/non-finite direction behaviour; extreme settings; broad sample distribution and spatial continuity |
| `SphericalSurfaceQuery` | Surface position/radius relation; tangent frame orthonormality on representative cardinal/diagonal directions | Pole-neighbour continuity; tangent frame handedness/stability; invalid direction and settings behaviour |
| `SphericalPlanetMeshGenerator` | Six faces, expected counts, finite vertices and outward normals | Triangle index bounds/winding; UV range; seam vertex position equivalence; several resolutions/radii; degenerate settings |
| `SphericalPlanetPresenter` | No matching automated test found | Build twice without duplicates; material assignment; clear idempotency; generated-root ownership and resource cleanup |
| `SphericalPlayerController` | No matching automated test found | Initialization; movement tangent to local surface; movement across face/pole regions; speed behaviour; missing dependency handling |
| `SphericalThirdPersonCamera` | No matching automated test found | Orbit/angle/distance behaviour; camera movement basis; target changes; pole stability; missing camera/target handling |
| `SphericalWorldDemoBootstrap` | No matching automated test found | Generate/clear/re-generate; no duplicate scene roots; component references; teardown |
| `SphericalWorldDemoOverlay` | No matching automated test found | Runtime initialization, displayed seed/radius, camera input/readout integration, missing references |

## 3. WORLD-002 — geographic fields and Planet Studio

| Implemented component | Existing coverage | Missing automated coverage |
|---|---|---|
| `SphericalPlanetStudioGenerator` | Layer composition; preview stages; globe/planisphere direction conversion; temperature bounds; geometry independent from thematic map | Temperature continuity across all seams/poles; mesh topology at several resolutions; invalid settings; stable deterministic output hashes |
| `SphericalGeographicFieldSet` | Resolution/metadata and directional sampling via generator tests | Out-of-range face coordinates; invalid enum; zero/non-finite direction; storage/index bounds; metadata mismatch |
| `SphericalGeographicFieldGenerator` | Altitude, temperature and slope fields; deterministic generation; ranges; finite sampling; edge consistency | Slope reference comparison on analytic surfaces; resolution convergence; pole and all-seam continuity; no mutation of source settings |
| `SphericalNeighbourGraph` | Node count/merging, connectivity, symmetric positive edges, seam identity | Every seam and corner; degree distribution; invalid resolution/radius; uniqueness tolerance at supported maximum resolution |
| `SphericalGraphDistance` | Multi-source determinism and zero at sources | Known-graph exact distances; disconnected graph behaviour; invalid source IDs; empty source list; monotonicity along shortest paths |
| `SphericalDistanceBenchmark` | Reports graph and great-circle reference error | Deterministic benchmark fixtures; measured scaling across resolutions; memory/allocation profile; approved accuracy threshold |
| Hydrology, rivers, moisture, biome/settlement fields | Not implemented in this spherical WORLD-002 slice | N/A until implementation; do not mark these criteria as passed |

## 4. Existing planar world-generation pipeline

The inspected project snapshot also contains a separate planar `World/Generation` pipeline. It must be tested independently from the spherical WORLD-002 implementation. In particular, planar distance-transform behaviour must not be treated as evidence that spherical nearest-water distance has been solved.

| Implemented component | Required test coverage | Current test status in inspected snapshot |
|---|---|---|
| `WorldGenerationSettings` | Valid settings; resolution/dimensions; threshold ordering; invalid values | GAP |
| `WorldGenerator.Generate` | Determinism; array dimensions; finite/ranged fields; water/biome classification; river routing; moisture/distance consistency; settlements/routes; edge handling | GAP |
| `WorldGenerationResult` | Array lengths; index mapping; cell-to-world coordinates; bounds | GAP |
| `WorldTerrainBuilder.BuildMesh` | Vertex/triangle counts; indices; normals; UVs; colors; vertical exaggeration; 16/32-bit index format | GAP |
| `WorldTerrainBuilder.CreateSceneObject` | MeshFilter/Renderer/Collider wiring; parent; material ownership; invalid shader behaviour | GAP |
| `WorldTerrainBuilder.ApplyToScene` | Reuse target; replace old mesh; collider add/remove/update; preserve external material; no duplicate generated component | GAP |
| `GeneratedWorldSurface` | Replaced mesh/material cleanup; OnDestroy cleanup; no destruction of externally owned material | GAP |
| `WorldGenerationConfig.Load` / loader | Missing config; malformed config; valid config; defaults and validation | GAP |
| `WorldGenerationWindow` / build processor | Editor generation, repeatability, asset/scene side effects, build validation | MANUAL + GAP |

## 5. Acceptance and closure status

- Current EditMode and PlayMode suites: **owner-reported PASS**.
- Exact test count and machine-readable result export: **not recorded in this update**.
- Automated coverage of all implemented behaviours: **NO**.
- Highest-priority missing tests: spherical movement/camera and presenter/bootstrap PlayMode; planar generator/terrain builder EditMode and PlayMode; spherical graph edge/corner and Dijkstra known-distance tests.
- Hydrology/biome/settlement spherical tests: **not applicable yet**, because those stages are not implemented in the inspected spherical slice.


## 512-resolution benchmark update

A manual Explicit EditMode stress test has been added for the CSR-backed spherical graph at resolution 512. It is authored but not yet executed; see `Work_Packages/WORLD-002_512_SPHERICAL_GRAPH_BENCHMARK.md`.
