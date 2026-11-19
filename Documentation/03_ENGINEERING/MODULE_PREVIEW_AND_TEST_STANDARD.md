# Module Preview and Test Standard

- **Status:** PROPOSED — owner approval required
- **Version:** 1.0
- **Purpose:** every module must be testable and, where visual, inspectable without relying on the full playable world.

## 1. Core rule

A module must have a reproducible way to answer:
1. Is its internal logic correct?
2. Is its output correct in isolation?
3. Does it obey its input/output contracts?
4. Does it integrate correctly with real dependencies?
5. Does it look and behave correctly in the game?

The full game scene is the final validation environment, not the only validation environment.

## 2. Required validation layers

| Layer | What it proves | Typical mechanism |
|---|---|---|
| Unit | Pure rules/calculations | EditMode NUnit |
| Component | One Unity component with controlled dependencies | EditMode fixture or small PlayMode fixture |
| Module preview | Visual output with deterministic test data | Dedicated preview scene/window |
| Contract | Inputs/outputs and failure semantics | Contract tests |
| Integration | Real collaborating modules | Integration fixture/PlayMode |
| Full game | Actual player-facing result | PlayMode/manual test |
| Performance | Cost at representative scale | Repeatable benchmark |

Choose only applicable layers, but explain each `N/A`.

## 3. Independent preview requirement

Every visual module must provide at least one preview route that does not require the main playable scene.

A preview must:
- create or load only the minimum dependencies;
- use a fixed seed and named fixture profile;
- expose the important parameters;
- show the generated result;
- show diagnostics and validation failures;
- support regeneration;
- support clear/reset without duplicates;
- record the Unity version and fixture configuration;
- not mutate production assets or the active world profile;
- be script-generated or self-contained; avoid repetitive manual setup.

### 3.1 Preview types

Use the simplest suitable option:

- **Pure data preview:** inspector/debug report, table or generated map.
- **Editor preview window:** for generation and asset-authoring tools.
- **Dedicated preview scene:** for camera, movement, rendering or spatial interaction.
- **Gizmo/overlay:** for positions, normals, bounds, candidates, paths and errors.
- **Automated image capture:** only when visual regression comparison is useful and stable.

Do not create a separate preview framework for every feature. Reuse a small set of project-level preview utilities where justified.

### 3.2 Preview contract

Each preview entry point must offer:
- `Generate` or `Refresh`;
- `Clear`;
- `Reset to Fixture`;
- `Validate`;
- `Show Diagnostics`;
- fixed seed/profile selection;
- output summary (counts, ranges, warnings, elapsed time);
- explicit indication of whether data is synthetic, fixture-based or from the real generator.

The preview must never silently use stale generated data.

## 4. World-specific preview standards

World modules must support small, fast fixtures before full planet generation.

Examples:
- topology: face grid, seam and direction visualization;
- geography: altitude/slope/moisture/water heatmaps and sampled values;
- biome classifier: colour-coded classification and confusion/boundary fixtures;
- surface query: normal/tangent gizmos and sample coordinates;
- terrain renderer: one-face and six-face preview;
- movement: small spherical test planet with a spawn marker and probe visualization;
- environment placement: candidate points, accepted/rejected reasons, prefab distribution, spacing and surface normal visualization;
- world profile: side-by-side profile preview and validation report.

A preview for a module must not depend on a fully populated production world.

## 5. Test naming and organization

Use:
- `<Feature>Tests.cs` for ordinary tests;
- `<Feature>RegressionTests.cs` for named regression suites;
- `<Feature>PlayModeTests.cs` for runtime tests;
- `<Feature>PerformanceTests.cs` for benchmarks.

Suggested layout:

```text
Assets/Project/Tests/
├── EditMode/
│   └── <Domain>/<Feature>/
├── PlayMode/
│   └── <Domain>/<Feature>/
└── Performance/
    └── <Domain>/<Feature>/
```

Keep one canonical folder layout. Do not maintain parallel test roots such as `07_Testing` under Documentation and `Assets/Project/Tests` as though both contain executable tests; documentation test plans and Unity test code must be clearly distinguished.

## 6. Required test plan fields

Every work package must specify:

- Inputs, valid ranges and units.
- Expected outputs and side effects.
- Invariants.
- Boundary cases.
- Invalid/missing input behaviour.
- Determinism and seed/version identity.
- Regression cases.
- Relevant test layers.
- Performance baseline/measurement method.
- Preview workflow and expected visible result.
- Manual QA steps and evidence.

## 7. Standard test categories

Mark each `PASS`, `FAIL`, `BLOCKED` or `N/A` with evidence.

1. Functional correctness.
2. Determinism/reproducibility.
3. Numerical validity and invariants.
4. Boundaries/topology.
5. Invalid input/failure behaviour.
6. Lifecycle/idempotency/cleanup.
7. Integration/contract.
8. Regression.
9. Performance/resource use.
10. Visual/UX inspection.
11. Content/asset references.

A test that has not been run is `NOT RUN`, never `PASS`.

## 8. Determinism

For procedural modules:
- seed must be explicit;
- configuration must be serializable or fully captured in the test;
- generator/version identity must be recorded when output compatibility matters;
- output equality must not depend on collection iteration order, frame rate or face processing order unless explicitly intended;
- avoid global `UnityEngine.Random` state for reproducible generation;
- tests must verify same-input repeatability and different-seed variation where relevant.

## 9. Integration failure diagnosis

When a module passes isolation but fails integration:
1. freeze the failing fixture/seed/configuration;
2. capture actual input values received by the module;
3. compare them to the module contract;
4. inspect adapter/composition wiring;
5. verify coordinate spaces, units, transforms and lifecycle;
6. check stale references, duplicate generation and execution order;
7. only change module logic if the evidence identifies a module defect.

Do not weaken module validation to make integration pass.

## 10. Visual evidence

For manual visual validation, record:
- scene or preview tool name;
- fixture/profile and seed;
- camera position/distance if relevant;
- expected observation;
- actual observation;
- screenshot or short recording when useful;
- Unity version;
- known visual limitations.

A screenshot alone does not prove functional correctness, determinism or performance.

## 11. Test report format

Each work package reports:

| Check | Status | Evidence |
|---|---|---|
| EditMode | PASS/FAIL/BLOCKED/NOT RUN | Test names and result |
| Module preview | PASS/FAIL/BLOCKED/NOT RUN | Fixture, seed, observation |
| Contract/integration | PASS/FAIL/BLOCKED/NOT RUN | Test and dependency versions |
| PlayMode | PASS/FAIL/BLOCKED/NOT RUN | Scene, steps and result |
| Regression | PASS/FAIL/BLOCKED/NOT RUN | Suite and result |
| Performance | PASS/FAIL/BLOCKED/NOT RUN/N/A | Metrics or reason |
| QA | PASS/FAIL/BLOCKED/NOT RUN | Reviewer and report |
| Owner acceptance | PENDING/ACCEPTED | Explicit decision |

## 12. Definition of module-ready

A module is `MODULE-READY` only if:
- its public contract is documented;
- its owner and dependencies are clear;
- automated tests cover its core rules;
- its preview works independently when visual;
- its diagnostics explain invalid output;
- integration consumers can use it without reaching into private implementation.

Module-ready is not the same as game-ready.
