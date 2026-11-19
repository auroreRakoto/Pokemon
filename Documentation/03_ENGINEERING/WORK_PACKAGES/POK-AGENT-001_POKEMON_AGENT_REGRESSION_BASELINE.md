# POK-AGENT-001 — PokemonAgent regression baseline

- **Status:** PROPOSED
- **Owner module:** Pokémon (POK), with QA-owned evidence capture
- **Priority:** P2 prerequisite for ACT-006
- **Dependencies:** current owner project snapshot; owner-confirmed gameplay setup; Unity Test Framework already listed in `Packages/manifest.json`
- **Related action:** `Documentation/06_AUDIT/REPOSITORY_ACTION_REGISTER.md` — ACT-006
- **Target milestone:** establish a reproducible behavioural baseline before any `PokemonAgent` responsibility extraction

## 1. Goal

Capture the current observable behaviour of `PokemonAgent` in a repeatable regression baseline so later staged extractions can be compared against the existing implementation without intentionally changing gameplay rules.

This work package does **not** decompose `PokemonAgent`. It creates the verification gate required before that refactor can safely begin.

## 2. User/developer value

The project can separate responsibilities incrementally while protecting the currently working Pokémon loop, including the owner-reported successful berry finding and consumption by Chenipotte and Mystherbe.

## 3. Scope

### Included

- Audit the current `PokemonAgent` setup and its actual prefab/scene consumers in the current owner project.
- Record the exact Unity version, scene, prefab/definition setup, simulation setup and reproducible manual steps used for baseline capture.
- Add focused tests for pure existing logic where it can be tested without changing behaviour (initially `PokemonNeeds` and `PokemonScheduleCalculator`).
- Define and execute manual/PlayMode regression scenarios for:
  - initialization and initial wandering;
  - daily activity/sleep transitions and emergency wake on low satiety;
  - hunger threshold, food search, target loss and eating recovery;
  - one-fruit-per-consumption and cooldown behaviour;
  - starvation timer reset and death transition;
  - reproduction preparation, cancellation, retry and population-cap rejection;
  - telemetry fields/events consumed by `EcosystemDataCollector` and `EcosystemLogger`.
- Preserve evidence and classify each check `PASS`, `FAIL`, `BLOCKED` or `NOT RUN`.

### Explicitly excluded

- Any extraction or rewrite of `PokemonAgent` methods/classes.
- Any change to gameplay thresholds, timing, movement, feeding, reproduction, mortality or event names/order.
- Any change to `EntityId` / `GetEntityId()` or individual identity architecture.
- Any change to `FruitBushManager` versus `ResourceManager` ownership; that remains ACT-002.
- Any change to random-stream architecture; that remains ACT-007.
- Any networking, persistence, population-scheduling or performance redesign.
- Any prefab, scene, serialized field, `.meta` or GUID migration unless a separate approved plan is created.

## 4. Ownership and architecture

- **Owning module:** Pokémon.
- **Verification owner:** QA, with owner-provided Unity execution evidence.
- **Dependencies:** `PokemonAgent`, `PokemonNeeds`, `PokemonScheduleCalculator`, `PokemonDefinition`, `PokemonSpawnManager`, `FruitBushManager`, `FruitBush`, `EcosystemDataCollector`, `EcosystemLogger`.
- **Contracts consumed:** current public `PokemonAgent` properties and `Initialize(...)`; `FruitBushManager.FindNearestBush(...)` / `ConsumeFruit(...)`; `PokemonSpawnManager.TrySpawnOffspring(...)`; `PokemonAgent.OnEcosystemEvent`.
- **Contracts changed:** none.
- **Authoritative state:** remains in the existing `PokemonAgent` / `PokemonNeeds` implementation for this baseline.
- **Allowed dependencies:** current Unity runtime and existing Unity Test Framework only.
- **Forbidden changes:** no new framework, assembly boundary, global state, service locator or cross-domain contract.

## 5. Current-state audit (archive snapshot only)

Evidence inspected: `Pokemon-main (1).zip`, SHA-256 `df3061a74844331fa80120586aff9e4c7dcdb537a0c340ff98e7f1d1a7c0c93c`.

- `Assets/Project/Runtime/Pokemon/Behaviour/PokemonAgent.cs` is 862 lines and declares 18 methods.
- Responsibilities currently present include initialization, Unity update orchestration, simulation clock, daily routine/sleep, needs drain, food search/eating, movement/wandering, reproduction, starvation mortality, statistics and ecosystem event emission.
- `PokemonScheduleCalculator.cs` already extracts some schedule calculations.
- `PokemonNeeds.cs` contains satiety initialization, drain and restoration logic.
- `PokemonSpawnManager` initializes agents and provides `TrySpawnOffspring(...)`.
- `EcosystemDataCollector` reads public state/statistics from agents; `EcosystemLogger` subscribes to the static `OnEcosystemEvent` event.
- `Packages/manifest.json` includes `com.unity.test-framework` version `1.6.0`.
- The archive contains no executable test files under `Assets/Project/Tests` and no visible test assembly definitions.
- Static search in this archive found no scene/prefab/asset text reference to the `PokemonAgent.cs.meta` GUID. This is not proof that the live Unity project has no serialized component reference; inspect the current prefabs and Inspector before changing or moving the script.
- The archive is a snapshot, not the owner's live repository. Re-audit the latest owner-provided archive before implementation.

## 6. Baseline scenarios and acceptance

| Scenario | Required observation | Evidence |
|---|---|---|
| Initialisation | Agent becomes alive, receives definition/managers, begins in the expected initial state | PlayMode or owner-run scene capture |
| Needs | Satiety clamps to 0–100, drains by configured rate and restores within bounds | EditMode tests |
| Schedule | Diurnal/nocturnal active periods and boundary hours match current calculator behaviour | EditMode tests |
| Sleep | Inactive period starts sleep; active period wakes; emergency hunger wakes during inactive period | PlayMode/manual |
| Foraging | Hunger threshold enters search; valid nearest bush is approached; missing target recovers | PlayMode/manual |
| Consumption | Chenipotte and Mystherbe can find and consume the configured berry; one consumption restores the configured amount and starts cooldown | Owner/QA PlayMode evidence |
| Mortality | Zero-satiety duration accumulates; satiety recovery resets timer; configured death threshold destroys/marks the agent as currently implemented | PlayMode/manual |
| Reproduction | Eligibility, preparation, ready event, blocked retry, success cooldown and population cap behave as currently implemented | PlayMode/manual |
| Telemetry | Existing state/statistic values and ecosystem events remain observable to current consumers | Integration/manual |

Acceptance criteria:
- [ ] Current Unity version, test scene/setup and required prefabs/definitions are recorded.
- [ ] Every scenario is marked PASS / FAIL / BLOCKED / NOT RUN with evidence; no unrun scenario is marked PASS.
- [ ] Chenipotte and Mystherbe berry-consumption regressions are explicitly recorded.
- [ ] Baseline captures current behaviour rather than imposing new desired behaviour.
- [ ] No production gameplay code, serialized data, prefab, scene, `.meta` or GUID is changed by this work package.
- [ ] QA review is recorded before ACT-006 extraction work is approved.

## 7. Implementation plan

| Step | Change | Expected files | Depends on |
|---|---|---|---|
| 1 | Re-audit latest owner archive and confirm actual component/prefab references | audit report only | Owner supplies current archive |
| 2 | Add pure EditMode tests for `PokemonNeeds` and `PokemonScheduleCalculator` where existing API permits | `Assets/Project/Tests/EditMode/Pokemon/` | Step 1 |
| 3 | Capture PlayMode/manual regression scenarios against current live setup | test files and baseline report | Step 1 |
| 4 | Run Unity tests and record evidence | baseline report | Steps 2–3 |
| 5 | QA reviews evidence and recommends the first isolated extraction candidate | QA section in report | Step 4 |

No production source changes are expected. Any required test seam that would alter runtime behaviour must be proposed separately before implementation.

## 8. Risks and rollback

- **Risk:** archive snapshot differs from the current project after owner commits. Mitigation: re-audit latest archive before implementation.
- **Risk:** Unity timing/randomness makes some scenarios non-deterministic. Mitigation: record scene, seed/setup and observed timing; do not introduce a new random/time abstraction in this package.
- **Risk:** test setup requires changing production APIs. Mitigation: mark BLOCKED and propose a separate bounded seam change rather than silently expanding scope.
- **Rollback:** remove only the new test files/report if they cause project issues; no production runtime changes are permitted.

## 9. Documentation impact

- Add baseline results to this work package or its companion audit report.
- Update `REPOSITORY_ACTION_REGISTER.md` only after evidence exists; ACT-006 remains PROPOSED until its own approved extraction plan.
- Do not modify architecture, interface contracts, decision register or roadmap in this package.

## 10. Status report

- **Implemented:** none.
- **Validated:** static archive inspection only.
- **Blocked:** live Unity prefab/scene verification and Unity test execution require owner environment.
- **Not run:** Unity compile, EditMode, PlayMode, prefab/scene validation, manual gameplay regression.
- **Next objective:** obtain owner approval and pushed plan milestone; then re-audit the latest archive and implement the baseline only.

## 11. Commit messages

Plan milestone:

`docs(pokemon): plan PokemonAgent regression baseline`

`Agent: POK`

Implementation milestone (only after the plan is approved and pushed):

`test(pokemon): capture PokemonAgent behaviour regression baseline`

`Agent: POK`
