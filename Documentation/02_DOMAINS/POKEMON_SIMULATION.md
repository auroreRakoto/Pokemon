# Pokémon Simulation and Behaviour

## Scope

Owns individual Pokémon identity/state, needs, physiology, emotion, behaviour selection, lifecycle and species behaviour profiles.

## Individual model

An individual is one entity with:
- unique ID;
- species ID;
- optional sex and species-specific sex traits;
- age/life stage;
- size factor and optional body condition/weight;
- health and physiological conditions;
- needs (hunger, thirst, fatigue, reproduction-related needs where appropriate);
- emotional variables (fear, curiosity, stress, aggression, confidence, etc. as chosen);
- current action/behaviour;
- location and movement state owned through the agreed movement/world interface;
- status effects and history where required.

## Independent dimensions

Do not represent every aspect using one mutually exclusive `CurrentState` enum. A Pokémon can be hungry, frightened and injured while walking. Separate:
- physiological conditions;
- needs;
- emotions;
- current action;
- locomotion state;
- temporary status effects.

Use explicit state machines only for processes that genuinely have mutually exclusive stages.

## Behaviour loop

A recommended conceptual loop:
1. gather relevant perception/context;
2. evaluate needs and priorities;
3. identify candidate goals;
4. score goals using urgency, opportunity, risk, personality/species profile and context;
5. select a goal/action;
6. request movement or interaction;
7. observe result/failure;
8. update state and reconsider.

Avoid tightly coupling decisions to execution. A decision can fail because a resource disappears, path becomes blocked, target moves, or server rejects an interaction.

## Everyday behaviours

Potential behaviour catalogue:
- idle/observe;
- explore;
- move between locations;
- forage/search food;
- eat;
- drink;
- rest/sleep;
- seek shelter;
- groom;
- flee/avoid;
- defend territory;
- socialize/communicate;
- court/reproduce;
- care for offspring;
- play/train where appropriate;
- fight or use abilities;
- investigate sounds/traces.

Species profiles determine which behaviours are available and how they are prioritized. Not every Pokémon needs every behaviour.

## Lifecycle

The design may cover birth/hatching, juvenile growth, maturity, reproduction, aging, death and post-death ecological consequences. Exact ages, durations, breeding rules and canon interpretation are OPEN per species/game design. Do not invent universal values.

## Size and sex

Ordinary size variation reuses one base mesh with a per-individual scale factor applied to a visual child. Sex does not automatically imply different size. Species-specific dimorphism or forms should be handled by data and optional visual variants only when needed.

## Species data

Species definitions should describe:
- diet and preferences;
- habitat preferences;
- activity patterns;
- movement capabilities;
- perception ranges;
- social/territorial tendencies;
- lifecycle parameters;
- available behaviours;
- combat capabilities;
- visual/audio references.

## Performance

The simulation is individual-based, but this does not imply every subsystem must update every frame. Scheduling, update rates, event-driven updates, distance/visibility relevance and offline/low-detail simulation are OPEN implementation topics requiring profiling.

## Dependencies

Consumes environment context and resource candidates from World/Ecology. Sends movement/interaction requests. Receives resource-consumed, damage, status and lifecycle events. Server owns authoritative state.

## Current repository implementation status — 2026-11-19 snapshot

**Status: NOT IMPLEMENTED IN THE CURRENT SPHERICAL PROJECT SNAPSHOT.**

The latest owner-supplied archive (`Pokemon-clean.zip`, SHA-256
`829ea7a6abec16886f0133825b376f91f2eece6092125b306fb3447aea3b7a57`)
contains no Pokémon simulation runtime source files.

Static inventory of `Assets/Project` found:
- 56 C# source files total: 33 Runtime, 8 Editor and 15 Tests.
- No `Assets/Project/Runtime/Pokemon/` directory.
- No declarations or usages of `PokemonAgent`, `PokemonNeeds`,
  `PokemonDefinition`, `PokemonScheduleCalculator`, `PokemonIndividualId`,
  `PokemonPopulationService`, `PokemonForagingBehaviour`,
  `PokemonDailyRoutine` or `PokemonReproductionSystem`.
- No Pokémon gameplay prefab/model/source file was identified by the filename
  inventory. `PokemonData.meta` and the spherical-world assembly/test assembly
  names are not Pokémon simulation implementations.
- `Assets/Project/Runtime/Infrastructure/Diagnostics/EcosystemLogger.cs`
  exists as an inert compatibility component. The archive has no matching
  `.meta` file for it and no active telemetry collector/snapshot implementation.

The sections above describe the **intended domain model and future ownership
boundaries**, not currently implemented Unity behaviour. Do not treat the
previous implementation description or the 2026-11-17 cleanup notes as proof
that those systems remain in the current repository.

### Reintroduction gate

Before implementing Pokémon simulation in the spherical world:
1. approve a bounded work package for the first visible Pokémon encounter;
2. select one species and approve a model or explicit temporary placeholder;
3. define an individual identity contract and a deterministic spherical spawn
   query using the World-owned surface/geographic data;
4. keep AI, needs, feeding, reproduction, capture, combat and population
   simulation out of the initial placement increment unless separately approved;
5. add tests against the actual new contracts and verify the independent spawn
   preview before scene integration.

Do not restore the archived `PokemonAgent` or the former Ecology/berry runtime
as a shortcut. Chenipotte, Mystherbe and berries remain deferred until a later
approved ecosystem work package.
