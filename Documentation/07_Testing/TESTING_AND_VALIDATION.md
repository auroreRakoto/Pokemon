# Testing and Validation

## Test layers

1. Unit tests: pure domain rules and calculations.
2. Integration tests: communication between modules.
3. PlayMode tests: Unity runtime behaviour.
4. Editor tests: asset/prefab/material/terrain generation tools.
5. Server tests: authority, commands, replication and session lifecycle.
6. Performance tests: population, world size, update rates, memory and bandwidth.
7. Content validation: meshes, rigs, textures, manifests and references.

## Essential early tests

- individual size factor is clamped and affects only intended visual/collider dimensions;
- species data and individual state remain separate;
- a Pokémon cannot consume an unavailable or out-of-range resource;
- two Pokémon cannot both consume the same final resource quantity;
- behaviour recovers from missing target/path;
- server rejects invalid client combat/resource requests;
- terrain/biome generation is reproducible for the same seed/configuration;
- editor generation is idempotent and does not create duplicate assets;
- missing content references produce actionable validation errors.

## Acceptance criteria

Every feature should define observable criteria before implementation. “Works” is not sufficient. Include expected inputs, outputs, edge cases and failure behaviour.

## Test data

Use small deterministic test worlds and fixed seeds. Provide fixtures for common species, resource patches and biome configurations. Do not rely only on a large random world to reproduce bugs.

## Regression

When fixing a bug, add a test that fails before the fix where practical. Record any known untestable/manual verification.


## 8. Common feature-validation contract

Every substantial feature and work package must include a **Testing & Validation** subsection in its owning module documentation. The subsection must link to this standard and contain feature-specific acceptance criteria. The shared categories are mandatory as applicable; a category may be marked `N/A` only with a short reason.

### 8.1 Standard test categories

| Category | Required question | Typical evidence |
|---|---|---|
| Functional correctness | Does the feature produce the specified result for valid inputs? | Unit/integration assertions |
| Determinism | Do identical inputs, seed and generator version produce identical outputs? | Repeated-run equality or stable hashes |
| Invariants and numerical validity | Are ranges, units, conservation rules and structural invariants respected? | Boundary/property tests |
| Boundaries and topology | Are seams, poles, wraparound, edges and degenerate cases handled? | Cross-boundary fixtures |
| Robustness and failure behaviour | Are invalid inputs rejected or handled predictably? | Exception/result assertions |
| Regression | Do previously accepted behaviours remain unchanged? | Existing tests plus regression tests |
| Integration | Does the feature interact correctly with its real callers and dependencies? | Integration/PlayMode/Editor tests |
| Performance and resources | Does it meet documented time, memory, allocation and scale expectations? | Reproducible benchmark |
| Content/tooling validation | Are generated assets, references and editor operations valid and repeatable? | Editor validation and asset audit |
| Manual visual/UX validation | Is the visible result understandable and correct in the target workflow? | Checklist, screenshot or recording |

Not every feature requires every test layer. The module owner must choose the relevant layers and explain omissions.

### 8.2 Required feature test plan

Every feature's module documentation must record:

- **Inputs:** valid ranges, units, defaults, seed/configuration and supported resolutions.
- **Expected outputs:** values, structures, side effects and ownership.
- **Invariants:** properties that must always hold.
- **Boundary cases:** minimum/maximum values, seams, poles, empty/full data, missing references and degenerate inputs as relevant.
- **Failure behaviour:** exception, validation error, fallback or explicit unsupported state.
- **Determinism:** whether deterministic output is required and what defines the input identity.
- **Performance envelope:** target hardware/context, representative sizes, timing and memory budgets. If not yet known, record a baseline rather than inventing a target.
- **Regression set:** previously accepted behaviours that must remain unchanged.
- **Manual QA:** exact scene/tool/workflow and expected observation.

### 8.3 Numeric tolerance policy

Floating-point assertions must use a justified tolerance; exact equality is appropriate only for intentionally discrete or bitwise-deterministic values.

Use both absolute and relative tolerance when values can vary substantially in magnitude:

`allowedError = max(absoluteTolerance, relativeTolerance × max(abs(expected), abs(actual)))`

Guidelines:
- Choose tolerance from the algorithm's units, precision and accumulated operations, not merely to make a failing test pass.
- Keep geometric direction/normalization tolerances separate from world-space distance/height tolerances.
- State the unit beside every physical tolerance (for example metres, degrees or seconds).
- Test the tolerance boundary where practical.
- Do not widen a shared tolerance globally to fix one feature.
- When a failure is caused by expected floating-point round-off, document the measured discrepancy and why the new bound remains below the feature's meaningful error threshold.

### 8.4 Test naming and evidence

Use names that express the invariant or behaviour, for example:
- `Generate_IsDeterministicForSameSeedAndSettings`
- `CubeFaceSeam_UsesOneCanonicalSample`
- `InvalidResolution_IsRejected`
- `Consume_WhenResourceIsEmpty_DoesNotChangeState`

For each implementation milestone, report:
1. exact test assembly and test command/Unity Test Runner mode;
2. pass / fail / skipped counts;
3. failing test names and relevant assertion values;
4. benchmark resolution, input size, seed, hardware context and measured time/memory;
5. manual QA performed and evidence;
6. tests not run and why.

A test authored is not a test executed. A compile success is not a test pass. A benchmark result without its configuration is not reproducible evidence.

### 8.5 Module documentation template

Copy and adapt this subsection into each module/work-package document:

```markdown
## Testing & Validation

- Standard: `Documentation/03_ENGINEERING/TESTING_AND_VALIDATION.md`
- Test assembly/layer:
- Deterministic fixture / seed:
- Supported input range and units:

| Category | Acceptance criterion | Automated/manual evidence | Status |
|---|---|---|---|
| Functional | | | TODO |
| Determinism | | | TODO |
| Invariants | | | TODO |
| Boundaries | | | TODO |
| Robustness | | | TODO |
| Regression | | | TODO |
| Integration | | | TODO |
| Performance | | | TODO |
| Visual/content QA | | | TODO |

### Results
- Automated: NOT RUN
- Manual: NOT RUN
- Performance baseline: NOT MEASURED
- QA review: PENDING
- Owner approval: PENDING
```

### 8.6 Retrospective adoption

When this standard is introduced to an existing module:
- inventory existing tests and map them to the categories;
- add missing high-value regression tests for already implemented behaviours;
- identify untested behaviours and record them as gaps rather than claiming coverage;
- preserve historical failures and explain corrections;
- do not mark a module compliant until the relevant tests have actually run and QA has reviewed the evidence.


## 9. Unity EditMode and PlayMode test strategy

Every Unity feature must identify which behaviours belong in EditMode, which require PlayMode, and which need both. A feature is not considered fully tested merely because its EditMode tests pass.

### 9.1 EditMode — isolated logic and editor tooling

Use EditMode for deterministic tests that do not require entering a running scene:
- pure algorithms, data transformations, topology and numerical calculations;
- validation of settings, serialization contracts and invalid inputs;
- deterministic generation from fixed seeds/configuration;
- mesh/data construction where objects can be inspected without runtime lifecycle;
- EditorWindow/editor tooling behaviour, AssetDatabase operations and Undo where practical;
- import/configuration validation for generated assets.

EditMode tests should avoid depending on scene timing, frame updates, physics simulation or runtime object lifecycle unless the test explicitly drives those systems.

### 9.2 PlayMode — runtime behaviour and scene integration

Use PlayMode whenever correctness depends on Unity's runtime lifecycle or a running scene. Relevant tests include:

| Area | Required PlayMode checks when applicable |
|---|---|
| Runtime initialization | Components initialize in the expected order; dependencies are available; no unexpected exceptions occur |
| Scene integration | Runtime systems work when loaded into a representative scene; required references resolve |
| Lifecycle | Enable/disable, Start/Update, scene unload/reload and destruction do not leave stale state or duplicate systems |
| Runtime generation | Generation can be invoked at runtime; output is attached to the intended scene root and can be refreshed/cleared safely |
| Physics/collisions | Colliders are present and queries/interactions return expected results |
| Time-dependent behaviour | Coroutines, timers, animation, day/night or simulation ticks progress correctly |
| Inter-system integration | Modules communicate through their runtime contracts without missing references or order-dependent failures |
| Performance/runtime resources | Runtime generation, allocations, GameObject counts and cleanup are measured in representative scenes |
| Multiplayer/authority | When applicable, runtime authority and client/server behaviour are verified in actual PlayMode/network tests |

A PlayMode test should create or load a minimal representative scene, exercise the public runtime entry point, yield frames only when necessary, assert the observable result, and clean up created objects. Avoid tests that merely call the same pure method already covered by EditMode without adding runtime integration value.

### 9.3 Choosing the test mode

- **EditMode only:** isolated deterministic logic, validation, mathematical/data algorithms, and editor-only tooling.
- **PlayMode only:** behaviours that exist only during runtime lifecycle or scene execution.
- **Both:** features with pure core logic plus runtime composition, scene objects, physics, lifecycle, or inter-module integration.
- **Manual QA in addition:** visual appearance, UX, content quality and workflows that cannot be reliably asserted automatically.

### 9.4 PlayMode test assembly setup

PlayMode tests must live in a dedicated test assembly definition that:
- references the runtime assembly or assemblies under test;
- references Unity Test Framework test assemblies (`TestAssemblies` optional Unity reference);
- is not restricted to the Editor platform;
- is placed in a clear test folder such as `Assets/Project/Tests/PlayMode/`;
- does not introduce runtime dependencies on NUnit or Unity Test Framework.

Keep EditMode test assemblies Editor-only. Do not move runtime code into test assemblies to make tests compile.

If the Test Runner reports "No tests to show" in PlayMode, that means no discoverable PlayMode test assembly/tests are currently available for that selection; it does not mean PlayMode tests passed.

### 9.5 Minimum evidence for a feature

For each substantial feature, record separate results:
- EditMode: discovered / passed / failed / skipped.
- PlayMode: discovered / passed / failed / skipped / not applicable, with reason.
- Manual QA: performed / not performed.
- Performance: measured / not measured, including scene and machine context.

A blank PlayMode tab or zero tests must be reported as **NOT CONFIGURED** or **NOT DISCOVERED**, not PASS. A feature that requires runtime integration cannot be accepted on EditMode evidence alone.


## 10. Deprecated API policy

**Do not introduce deprecated/obsolete APIs in new or modified code.** This rule applies to Runtime, Editor, tooling, tests, examples and build scripts.

- Prefer the current supported API for the project's pinned Unity/editor/package versions.
- Treat compiler obsolete/deprecation warnings (for example `CS0618`) as issues to resolve before considering the change complete.
- Do not use a deprecated API merely because it still compiles or because an older project file already uses it.
- When replacing an API, check the replacement's semantics and minimum supported Unity/package version; do not perform a blind name substitution.
- If a deprecated API is temporarily unavoidable for compatibility, isolate it behind a small adapter, document the reason and affected version range, and create a tracked removal/migration task. This requires explicit project-owner approval.
- Do not suppress obsolete warnings globally or locally just to obtain a clean Console.
- During code review, search changed files for obsolete API usage and inspect compiler warnings. Report any pre-existing warnings separately from warnings introduced by the change.

**Acceptance criterion:** newly authored or modified code compiles without new obsolete/deprecation warnings. Existing deprecated usages discovered during work must be recorded as technical debt, not silently copied into new code.
