> **HISTORICAL SNAPSHOT AUDIT — NOT CURRENT IMPLEMENTATION EVIDENCE.** The findings below describe an earlier archive containing `PokemonAgent.cs`. The latest owner-supplied `Pokemon-clean.zip` contains no Pokémon simulation Runtime source. Do not use this report to authorize code edits or restore the old subsystem. See `POK-AGENT-001_CURRENT_SOURCE_RECONCILIATION.md`.

# POK-AGENT-001 — Static audit of PokemonAgent

- **Status:** PROPOSED
- **Audit basis:** `Pokemon-main (1).zip` (owner archive snapshot)
- **SHA-256:** `df3061a74844331fa80120586aff9e4c7dcdb537a0c340ff98e7f1d1a7c0c93c`
- **Unity validation:** NOT RUN

## Findings

1. `Assets/Project/Runtime/Pokemon/Behaviour/PokemonAgent.cs` contains 862 lines and 18 declared methods in this snapshot. It is a multi-responsibility MonoBehaviour.
2. Responsibilities visible in the source include initialisation/update orchestration, simulation clock and daily schedule, satiety drain, sleep/wake, food search/eating, movement, reproduction, starvation mortality, statistics and event publication.
3. `PokemonScheduleCalculator.cs` already owns part of the schedule calculation; it depends on `PokemonAgent.ActivityPattern`, so that dependency must be considered before any further movement of types.
4. `PokemonNeeds.cs` owns a small serializable satiety value object, currently used by `PokemonAgent`.
5. `PokemonSpawnManager` constructs agents through `Initialize(...)` and is also called for offspring creation.
6. `EcosystemDataCollector` reads the agent's public statistics and current-state string. `EcosystemLogger` subscribes to the static `PokemonAgent.OnEcosystemEvent` event. Those consumers are compatibility constraints for a behaviour-preserving refactor.
7. `PokemonDefinition` supplies the current species tuning values used by the agent. No definition fields are changed by this audit.
8. `Packages/manifest.json` includes Unity Test Framework `1.6.0`, but no executable tests were found under `Assets/Project/Tests` in this snapshot.
9. Static search found no scene/prefab/asset text reference to the script GUID in `PokemonAgent.cs.meta`. This must be checked in the current Unity project; a text search of one archive is not sufficient to establish absence of serialized references.
10. Owner-reported current behaviour (from prior project communication): Unity project compiles/launches and Chenipotte/Mystherbe successfully find and consume berries. This is owner-reported manual verification, not independent QA evidence.

## Classification

`PokemonAgent.cs`: NON_COMPLIANT / multi-responsibility candidate for staged decomposition. It is active runtime code and must not be deleted.

`PokemonNeeds.cs`: currently a focused helper; retain unless a later approved design gives a concrete reason to change it.

`PokemonScheduleCalculator.cs`: existing extraction; retain and test before moving its public enum dependency.

## Required next step

Complete POK-AGENT-001 regression baseline first. Only after QA has recorded the baseline should a separate POK/ARCH work package select and extract one responsibility at a time. Do not perform a broad rewrite or rename serialized fields as part of cleanup.
