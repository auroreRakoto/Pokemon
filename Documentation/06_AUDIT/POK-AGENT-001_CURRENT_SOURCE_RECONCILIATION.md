# POK-AGENT-001 — Current source reconciliation

- **Status:** BLOCKED — target implementation absent from latest project snapshot
- **Owner module:** Pokémon (POK)
- **Evidence source:** owner-supplied `Pokemon-clean.zip`
- **Archive SHA-256:** `829ea7a6abec16886f0133825b376f91f2eece6092125b306fb3447aea3b7a57`
- **Audit date:** 2026-11-19

## 1. Result

The latest supplied project is the spherical-world project, not the earlier archive
on which the original `PokemonAgent` cleanup plan was based.

Static inspection found:
- 56 C# files under `Assets/Project`: 33 Runtime, 8 Editor, 15 Tests.
- No `Assets/Project/Runtime/Pokemon/` directory.
- No C# declaration or use of `PokemonAgent`, `PokemonNeeds`,
  `PokemonDefinition`, `PokemonScheduleCalculator`, `PokemonIndividualId`,
  `PokemonPopulationService`, `PokemonForagingBehaviour`,
  `PokemonDailyRoutine` or `PokemonReproductionSystem`.
- No `EcosystemDataCollector` or `EcosystemSnapshot` implementation.
- `EcosystemLogger.cs` remains as an inert compatibility MonoBehaviour, but
  the archive contains no `EcosystemLogger.cs.meta`. Serialized scene/prefab
  references therefore require a separate Unity/GUID audit before this
  component is renamed, removed or relied upon.

The owner previously supplied a Unity Console screenshot after cleanup showing
0 errors and 0 warnings. This is owner-provided compile-console evidence only.
Unity tests, PlayMode, scene startup and serialized reference checks were not
run by this audit.

## 2. Consequence for the pushed POK-AGENT-001 plan

The original plan targeted:
- `PokemonAgent`;
- `PokemonNeeds`;
- `PokemonScheduleCalculator`;
- `PokemonDefinition`;
- `PokemonSpawnManager`;
- `FruitBushManager` / `FruitBush`;
- ecosystem telemetry consumers.

Those types are absent from the latest archive. The earlier baseline test files
were consequently invalid for this project version and the owner removed them.
Do not restore those files or write new tests against those absent types.

The original pushed plan is retained as historical planning evidence; this
reconciliation does not rewrite its history or claim that its implementation
milestone was completed.

## 3. Product and architecture status

The Pokémon domain document remains the intended future domain design. It is
not evidence that the subsystem currently exists in Runtime.

The immediate project queue remains focused on spherical World traversal and
biome rendering. The first Pokémon encounter is still an optional proposed
future feature, requiring a new bounded work package against the current
World surface/geographic contracts.

Chenipotte, Mystherbe, berries, feeding, needs, reproduction, mortality and
autonomous ecosystem behaviour remain deferred. They are not treated as
regressions in the current spherical project.

## 4. Recommended next work

Do not continue the old PokemonAgent cleanup: there is no current target file.

When the owner wants to reintroduce a Pokémon:
1. create a new first-encounter work package;
2. select one species and approve its model or explicit placeholder;
3. define an individual identity contract;
4. define deterministic spawn eligibility/location against the World-owned
   spherical fields and surface query;
5. provide an isolated spawn preview;
6. keep needs, AI, feeding, reproduction, capture, combat and population
   simulation out of scope unless separately approved.

## 5. Validation

| Check | Status | Evidence |
|---|---|---|
| Latest archive extracted and inspected | PASS | `Pokemon-clean.zip` |
| C# source inventory | PASS | 56 files listed in companion inventory |
| Former Pokémon Runtime declarations/usages | ABSENT | Static C# search |
| Current Unity Console | OWNER-REPORTED PASS AT SCREENSHOT TIME | 0 errors / 0 warnings screenshot |
| Unity compile by assistant | NOT RUN | No Unity execution environment |
| EditMode / PlayMode tests | NOT RUN | Not executed in this audit |
| Scene/prefab GUID audit for EcosystemLogger | NOT RUN | Requires Unity serialized reference inspection |
| Pokémon behaviour regression baseline | BLOCKED | Target implementation absent |

## 6. Status

- POK-AGENT-001: **BLOCKED / superseded for the current source snapshot**.
- No Pokémon Runtime source was changed or restored.
- No Unity asset, scene, prefab, `.meta` or GUID was changed.
- Next Pokémon work should be a new first-encounter feature, not a refactor of
  the absent archived `PokemonAgent`.
