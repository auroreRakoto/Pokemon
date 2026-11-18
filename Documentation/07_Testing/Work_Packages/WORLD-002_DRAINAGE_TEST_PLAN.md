# WORLD-002 — Spherical drainage topology and 512 benchmark test plan

## Automated topology coverage

Run `SphericalDrainageTests` in EditMode. Required scenarios:

| Test | Coverage |
|---|---|
| `CubeSphereCorners_AreDeduplicatedIntoEightCanonicalNodes` | Eight shared cube corners |
| `AllSixFaceBoundaries_AreConnectedThroughCanonicalSeamNodes` | Every edge sample on all six faces |
| `NorthAndSouthPoles_AreRepresentedByFiniteConnectedNodes` | Both poles and adjacency |
| `DrainageFromNegativeXFace_ReachesOutletOnPositiveXFace` | Cross-face downstream path |
| `FlatPlateau_DrainsDeterministicallyWithoutCycles` | Flat filled surface |
| `MultipleOutlets_ProduceDeterministicAcyclicDrainage` | Multiple terminal outlets |
| `DryWorldFallbackOutlet_IsExcludedFromRiverMask` | Synthetic outlet policy |
| Existing depression/determinism/conservation/invalid-input tests | Core invariants |

### Numeric tolerances
- Pole direction: `1e-5` absolute.
- Synthetic filled elevation: `1e-4 m` absolute.
- Small graph outlet accumulation conservation: `0.01` contributing cells.
- Flood-rank ordering and node identity: exact integer equality.

## Explicit 512 benchmark

Run `Benchmark_512_DrainageThreeRuns_ReportsStageTimesAndPeakWorkingSet` manually on the target PC.

The test:
- Builds one 512 graph and one deterministic synthetic elevation/water input.
- Executes drainage exactly three times with identical inputs.
- Reports Priority-Flood, flow-direction and accumulation timings separately for each run.
- Reports median and maximum timing per stage.
- Samples Unity Editor process working set every 5 ms and reports sampled peak bytes/MiB.
- Verifies deterministic downstream node arrays across runs.

Memory caveat: the sampled working set is process-wide and includes Unity Editor baseline. A 5 ms sampler may miss shorter transient peaks; it is not an exact allocation profiler.

## Acceptance
- All EditMode topology tests pass.
- All three 512 runs complete.
- Downstream arrays are deterministic across runs.
- Stage timings and sampled peak working set are present in Test Runner output.
- No crash or out-of-memory condition occurs.
- No absolute performance threshold is imposed until the owner's PC baseline is reviewed.

Passing these tests and benchmark validates the drainage prototype's topology invariants and execution cost only. It does not validate river-network visual quality or hydrological realism.


## Visual integration gate (separate from topology and benchmark)

Passing the topology suite and the three-run 512 benchmark validates graph connectivity,
acyclic routing, accumulation invariants and execution cost. It does **not** validate the
rendered hydrographic network.

Visual acceptance is tracked separately in
`WORLD-002_DRAINAGE_ISOLATION_TEST_PLAN.md`, Test C. In particular:

- a non-empty overlay mesh is not a visual pass;
- the camera-facing hemisphere must contain eligible and rendered drainage segments;
- the overlay must be positioned outside the displayed terrain in the same local coordinate
  frame, using the original sampled terrain elevation rather than Priority-Flood filled elevation;
- continuous paths must be visible across the near hemisphere, not merely fragments at the
  silhouette;
- an exaggerated 2 km clearance is diagnostic only and cannot be accepted as final placement.

Current status (2026-11-18): topology and 512 benchmark were owner-reported passing; Test A and
Test B were owner-reported passing; Test C remains **FAIL / not accepted** based on the latest
Unity screenshot. Do not mark the hydrographic network visually validated until Test C passes.


## Implementation note — opaque overlay depth behaviour (2026-11-18)

Do not use `Sprites/Default` as the production drainage overlay material: its transparent sorting and disabled depth writes can hide an otherwise valid overlay behind the six large terrain-face renderers. Use `Hidden/Pokemon/DrainageOverlayOpaque`, which preserves vertex colours and uses opaque depth-writing, double-sided rendering. This is a rendering-only correction; it does not alter Priority-Flood, flow directions, accumulation, water masks, river candidates, or graph topology. The 2 km forced-clearance experiment is removed.
