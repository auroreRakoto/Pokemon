# Cross-Domain Interface Contracts

This document defines the information that must cross domain boundaries. Exact C# signatures are implementation decisions and should be recorded when finalized.

## Contract principles

- A contract describes meaning, not internal implementation.
- Use stable IDs, explicit units, bounded values and versionable schemas.
- Prefer commands for requested actions, queries for information, and events for facts that have occurred.
- Events describe completed facts; they should not be used as ambiguous requests.
- Include failure reasons where a command can be rejected.
- Never expose mutable internal collections across boundaries.

## Core concepts

### Species definition
Provides immutable or versioned species-level facts:
- species ID;
- display/localized name reference;
- type(s);
- base physical/statistical ranges;
- diet/food preferences;
- habitat preferences;
- lifecycle parameters;
- behaviour profile reference;
- visual asset reference;
- ability references;
- species-specific visual dimorphism/form rules where applicable.

### Pokémon individual
Represents one living or historical individual:
- unique individual ID;
- species ID;
- sex, if applicable;
- age/life stage;
- size factor and optional weight/body condition;
- health/physiology;
- needs;
- reproductive state;
- location and movement state;
- current behaviour/action;
- ownership/capture status if the game later needs it;
- version/revision metadata.

### Resource definition
Static description of a resource type:
- resource ID;
- category;
- edible/usable flags;
- nutrition/hydration or other effects;
- renewable status;
- growth/regeneration parameters;
- preferred consumers;
- biome/habitat constraints;
- visual reference.

### Resource instance
A particular patch/item in the world:
- instance ID;
- resource definition ID;
- location;
- current quantity/maturity/condition;
- lifecycle state;
- regeneration timer/state;
- revision.

### Biome definition and instance
Definition describes parameters and allowed content. Instance describes a generated spatial region, its seed, environmental values, and placed content.

## Suggested commands and queries

| Contract | Producer | Consumer | Meaning |
|---|---|---|---|
| `SpawnPokemonRequest` | Population/session | Pokemon domain | Request creation of an individual |
| `PokemonSpawned` | Pokemon domain | World, server, client view | Individual successfully created |
| `GetEnvironmentContext` | World | Pokemon, Ecology | Return local conditions and nearby relevant features |
| `FindResourceCandidates` | Ecology/World query | Behaviour | Return eligible nearby resources |
| `ConsumeResourceRequest` | Pokemon behaviour | Ecology | Request consumption; server validates |
| `ResourceConsumed` | Ecology | Pokemon, World, persistence | Resource quantity changed |
| `MoveIntent` | Client or behaviour | Movement/world | Request movement |
| `MovementValidated` | Movement/world | Pokemon, network | Accepted movement result |
| `CombatActionRequest` | Player/Pokemon | Combat | Request an attack/action |
| `CombatResolved` | Combat | Pokemon, client presentation, persistence | Authoritative result |
| `PokemonStateChanged` | Pokemon domain | Network/persistence/UI | State changed; payload scoped to consumer need |

## Interaction contract: eating

1. Behaviour identifies a candidate resource using query results.
2. Behaviour requests approach/movement.
3. Movement validates reachability and location.
4. Behaviour sends a consume request with individual ID, resource instance ID and requested amount.
5. Ecology validates existence, range, availability, compatibility and remaining quantity.
6. Ecology applies the resource change and emits `ResourceConsumed`.
7. Pokémon physiology/needs apply the defined effects.
8. Presentation reacts to events; it does not decide consumption.

## Interaction contract: sleeping/resting

- Behaviour chooses a valid rest action.
- World/habitat query returns suitable rest positions or shelter.
- Movement reaches the selected position.
- Pokémon state records resting/sleeping.
- Needs/physiology systems determine recovery.
- Presentation selects suitable animation.
- Server validates state and replicates relevant changes.

## Contract review requirement

Any change to IDs, units, ownership, event meaning, validation, serialization, authority or lifecycle requires review by all producer/consumer domains and a documentation update.
