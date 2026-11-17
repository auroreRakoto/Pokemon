# WORLD-002 — 512-resolution spherical graph benchmark

## Objective

Validate whether the spherical neighbour graph and multi-source Dijkstra prototype can operate at 512 samples per Cube-Sphere face on the project owner's PC.

## Implementation changes in this patch

- Replaced per-node adjacency lists and the edge dictionary with compressed sparse row (CSR) arrays.
- Replaced edge dictionary de-duplication with packed undirected edge keys, sorting and in-place compaction.
- Added indexed neighbour accessors so Dijkstra does not allocate interface wrappers in its hot loop.
- Retained `GetNeighbours` as a compatibility accessor for tests and diagnostic tools.
- Added a deterministic, evenly distributed node sample for graph-vs-great-circle benchmark comparisons.
- Added estimated persistent graph storage reporting.
- Added an explicit 512-resolution stress test so the heavy run is opt-in from Unity Test Runner.

## Expected topology counts at resolution 512

- Unique nodes: `6 × 512² + 2 = 1,572,866`.
- Undirected graph edges: expected `12 × 512² = 3,145,728` for the current face-grid adjacency topology.
- Face samples stored in the face-to-node lookup: `6 × 513² = 1,578,294`.

These are structural expectations; the test must confirm the actual edge count.

## How to run

1. Import the patch at the Unity project root.
2. Wait for compilation.
3. Open Test Runner → EditMode.
4. Find `SphericalNeighbourGraphTests`.
5. Run `Benchmark_Resolution512_BuildsAndRunsDijkstra` manually. It is marked Explicit/Performance and is not part of routine test runs.
6. Copy the complete `512 benchmark:` line from the Test Runner output.

## Metrics recorded

- Node and edge count.
- Graph construction time.
- Dijkstra execution time for three deterministic source nodes.
- Estimated persistent graph storage (bytes and MiB).

## Important limitations

- `EstimatedStorageBytes` measures the graph's persistent managed array payload, not temporary build allocations, dictionary capacity, CLR object overhead, Unity editor overhead or peak process memory.
- This benchmark does not yet record peak process memory. Use the Unity Profiler/ProfilerRecorder or OS process metrics during the explicit run if peak memory is required.
- The distance benchmark uses a graph-path approximation and great-circle references on a deterministic subset. Its sample count and distribution should be selected to balance runtime and reference quality.
- A single run is not a performance acceptance. Repeat on the target PC and record machine/Unity version.
- No accuracy or performance threshold is approved yet. Do not mark spherical hydrology validated from this benchmark alone.

## Status

- CSR implementation authored.
- 512 explicit benchmark authored.
- Unity compilation/execution: pending owner run.
- Hydrology acceptance: pending benchmark review.
