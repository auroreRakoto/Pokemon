# WORLD-002 — Implementation Progress 02: Spherical Neighbour Graph Prototype

- **Work package:** WORLD-002
- **Agent:** WORLD
- **Status:** IMPLEMENTED LOCALLY — Unity validation pending
- **Scope:** canonical six-face lattice graph, great-circle edge lengths, multi-source Dijkstra prototype, small-scale great-circle reference comparison.

## Implemented

1. `SphericalNeighbourGraph` builds one canonical node per shared Cube-Sphere lattice direction. Face-edge and cube-corner samples are merged, and cardinal face-grid adjacency is combined into a bidirectional graph.
2. Edge weights are great-circle arc lengths in metres (`central angle × planet radius`).
3. `SphericalGraphDistance` computes multi-source shortest graph distances with a binary min-heap.
4. `SphericalDistanceBenchmark` compares graph distances against a direct nearest-source great-circle reference on a configurable subset. The reference is intentionally O(compared nodes × source count) and is for small prototype cases only.
5. EditMode tests cover unique node count (`6N² + 2`), graph connectivity, bidirectional positive edges, a cross-face shared edge, deterministic multi-source distances and benchmark output.

## Interpretation and limitations

- Dijkstra on this graph returns shortest paths constrained to the sampled graph edges. It is **not** the exact continuous spherical geodesic distance to an arbitrary water region.
- The benchmark reports mean and maximum absolute error against the nearest-source great-circle reference; no acceptable error bound or production resolution has been approved yet.
- The graph currently uses four-neighbour (cardinal) adjacency per cube face. Diagonal edges are intentionally not added in this prototype.
- Direction deduplication uses quantized normalized coordinates at 1e-6. Tests must confirm this merges all shared boundaries/corners without merging distinct samples at supported resolutions.
- This implementation does not classify water, calculate rivers, or derive moisture. The hydrology design gate remains open until benchmark evidence is reviewed.

## Validation status

- Source-level review completed.
- Unity compilation, EditMode test execution and timing on the project owner's PC: **not yet run**.
- No Git commit or push was performed from this environment.
- Next decision: run tests and benchmark at several resolutions, inspect seam/pole degrees and error scaling, then choose or revise the graph/distance method before implementing water and moisture.

## Manual Unity checks

1. Run `SphericalNeighbourGraphTests` in the EditMode Test Runner.
2. Record node/edge counts and benchmark errors at resolutions 4, 8, 16 and 32 (start small; reference comparison cost grows with source count).
3. Confirm graph connectivity and shared node IDs across all 12 cube edges and 8 corners.
4. Do not treat the prototype as production hydrology until error/performance evidence is reviewed.
