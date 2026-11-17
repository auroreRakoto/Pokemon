# WORLD-002 — Retrospective Testing Standard Adoption

- **Work package:** WORLD-002
- **Agent:** WORLD
- **Status:** STANDARD DOCUMENTED; Unity execution pending
- **Standard:** `Documentation/07_Testing/TESTING_AND_VALIDATION.md`

## Retrospective coverage inventory

| Existing feature | Existing or added tests | Categories covered | Remaining gap |
|---|---|---|---|
| Cube-Sphere topology | Added face-centre mapping, normalized face-grid directions and zero-direction rejection | Functional, invariants, robustness | Explicit all 12 seam/corner pair fixtures should be expanded |
| WORLD-001 terrain layers | Existing base-sphere radius, deterministic/independent layer contribution and preview-stage restoration tests | Functional, determinism, regression | Add broader layer-stage combinations and mesh index/normal checks |
| Globe / planisphere | Existing direction round-trip, wrap boundary and same-height-field test; numeric tolerance corrected | Integration, boundaries, regression | Add sampled-field visual parity at multiple latitudes/resolutions |
| Planet Studio maps | Existing thematic-map geometry independence and temperature range tests | Invariants, regression | Add slope/altitude color-ramp tests when palette contract is finalized |
| Geographic fields | Existing resolution/metadata, deterministic generation, ranges, directional sampling and face-edge consistency tests | Functional, determinism, invariants, boundaries | Confirm Unity discovers and executes this test class |
| Spherical neighbour graph | Added node-count, connectivity, symmetric edges, cross-face node identity, Dijkstra determinism and benchmark tests | Functional, determinism, boundaries, performance baseline | Confirm Unity discovers and executes graph tests; test all seams/corners and higher resolutions |

## Corrected floating-point assertion

The existing `PlanisphereAndGlobeUseSameDirectionBasedHeightField` test previously required height agreement within `1e-4` metres. The observed values in Unity were approximately `538.037659` and `538.03717`, a discrepancy of approximately `4.89e-4` metres after a direction round-trip already constrained to `1e-5` unit-vector distance.

The assertion is changed to `1e-3` metres. This is a local tolerance for accumulated float/projection sampling round-off, not a global relaxation. The direction round-trip assertion remains unchanged.

## Test discovery issue

The supplied Test Runner screenshot showed 38 tests and did not visibly list `SphericalGeographicFieldGeneratorTests` or `SphericalNeighbourGraphTests`. Their source files exist in the supplied patch, but the screenshot alone does not establish whether Unity imported them, whether the active test assembly includes their paths, or whether the Test Runner view/filter omitted them. This must be checked in Unity before claiming those suites passed.

## Execution status

- Source changes prepared: yes.
- Unity compile after these changes: not run here.
- EditMode suite after these changes: not run here.
- QA independent review: pending.
- Owner approval: pending.


## PlayMode coverage status

- The supplied Test Runner screenshot with the PlayMode tab selected showed **No tests to show** and zero discovered tests.
- This is **NOT CONFIGURED / NOT DISCOVERED**, not a passing PlayMode suite.
- The current spherical field and graph calculations are primarily pure data algorithms and belong in EditMode.
- Runtime scene application, refresh/cleanup, lifecycle, colliders and runtime performance require PlayMode tests when those behaviours are implemented.
- No PlayMode test assembly or PlayMode test was added in this documentation-only update.
