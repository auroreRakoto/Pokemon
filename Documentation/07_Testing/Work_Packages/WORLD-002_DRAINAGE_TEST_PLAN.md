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
