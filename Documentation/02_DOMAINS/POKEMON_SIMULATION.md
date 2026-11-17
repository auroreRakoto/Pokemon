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


## Current Unity implementation ownership

- `PokemonAgent` is a thin Unity composition adapter and update coordinator, not the owner of all behaviour rules. It depends on the Ecology-owned food service abstraction, not `FruitBushManager` directly.
- `PokemonNeeds` owns satiety state; `PokemonDailyRoutine`, `PokemonMortalitySystem` and `PokemonReproductionSystem` own their respective individual processes.
- `PokemonForagingBehaviour` owns the seek/eat action flow; `PokemonWanderBehaviour` owns exploration destination selection.
- `PokemonIndividualId` provides a stable per-instance identifier. `PokemonSimulationEvent` carries completed facts to consumers.
- Movement is expressed through `IWorldMovement`; the World `PokemonMovementController` is the only adapter that writes the Transform.
- Population rules belong to `PokemonPopulationService`; prefab instantiation and movement-component wiring are delegated through `IPokemonFactory` to `PokemonUnityFactory` in Composition. Scene setup and concrete dependency wiring also belong to Composition.

The current vertical slice implements wandering, fruit foraging, day/night rest, starvation mortality and a simple reproduction request. Species food configuration is explicitly fruit-based (`eatsFruit` plus `preferredFood`); continuous vegetation biomass is not yet consumed by Pokémon. Other behaviours in the catalogue remain planned and must not be inferred as implemented.

Population tuning is held by `PokemonPopulationProfile` assets, separate from `PokemonDefinition`: initial population and maximum population are scenario settings, not species biology.
