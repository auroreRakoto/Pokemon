# WORLD-002 — Drainage topology and 512 performance validation

## Scope
This increment adds missing topology tests and a repeatable 512 drainage benchmark. It does not integrate river rendering into the Generator and does not declare hydrographic networks validated.

## Instrumented stage timings
`SphericalDrainageResult` now exposes:
- `PriorityFloodMilliseconds`: input initialization plus priority queue flood.
- `FlowDirectionMilliseconds`: downstream assignment.
- `AccumulationMilliseconds`: flow accumulation and river-mask extraction.

## Memory
The explicit benchmark samples the Unity Editor process working set every 5 ms during three drainage runs. The reported peak includes the Editor baseline and may miss sub-5-ms transients. It is not a precise per-stage or drainage-only allocation measurement.

## Run
1. Import the ZIP at project root.
2. Run all `SphericalDrainageTests` in EditMode.
3. Confirm topology tests pass.
4. Manually run `Benchmark_512_DrainageThreeRuns_ReportsStageTimesAndPeakWorkingSet` (Explicit / Performance).
5. Save the complete Test Runner log with Unity version, OS and PC details.

## Status
- Topology tests authored: pending Unity execution.
- Stage instrumentation authored: pending Unity compilation.
- Three-run 512 benchmark authored: pending owner execution.
- Hydrographic network acceptance: not yet granted.
