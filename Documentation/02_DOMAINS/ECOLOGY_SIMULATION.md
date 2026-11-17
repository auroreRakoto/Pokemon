# Ecology Simulation

## Scope

Owns ecological relationships and world-level biological processes: resources, regeneration, food-web interactions, habitat suitability, population dynamics and environmental cycles.

## Resource categories

Potential resources include:
- plant food: berries, fruits, seeds, nuts, leaves, roots, flowers, pollen, nectar, mushrooms;
- animal food: insects and other prey where supported by game design;
- water: puddles, streams, ponds, dew, rainwater;
- construction/utility materials: sticks, fibres, bark, moss, feathers, silk, shells and similar;
- special resources: rare items, fossils or evolution-related objects if included.

Eggs are reproductive entities and should not casually be treated as generic food resources.

## Resource definition vs instance

Definition: type, category, effects, renewability, growth/regeneration rules, preferred consumers, habitat/biome constraints, visual reference.

Instance: location, quantity, maturity, condition, lifecycle state, regeneration progress, unique ID.

## Resource lifecycle

Possible lifecycle states (adapt to resource type):
- absent;
- growing/developing;
- immature;
- mature/available;
- depleted/consumed;
- regenerating;
- permanently removed.

Not every resource needs every state. Define transitions and triggers explicitly.

## Food web

Document relationships as explicit data/rules:
- producer/resource;
- consumer species/groups;
- predation or competition relationships;
- resource availability constraints;
- seasonal or habitat dependencies.

Do not assume a complete canonical food chain exists for every Pokémon. Game ecology may need carefully labeled design extrapolation.

## Habitat

Habitat features may provide:
- shelter from weather/predators;
- rest/sleep locations;
- nesting/reproduction sites;
- feeding locations;
- perches, burrows, hollows, leaf litter or cover.

A visual tree/log/rock becomes an ecological habitat only if it exposes suitable interaction/habitat data.

## Population dynamics

Potential factors:
- births and deaths;
- resource carrying capacity;
- habitat availability;
- predation and competition;
- disease/parasites if implemented;
- migration/dispersal;
- environmental conditions;
- player impact.

Avoid adding complex population equations before the first vertical slice. Begin with transparent, testable rules.

## Environmental cycles

Day/night, weather, seasons, temperature, humidity, rainfall and decomposition can influence resource growth, activity and habitat suitability. Their exact scope and simulation frequency are OPEN.

## Visual traces

Footprints, eaten leaves, broken branches, silk, droppings, flattened grass, abandoned nests and similar traces can communicate simulation history. Most should be lightweight, temporary or aggregated rather than individually simulated forever.


## Current Unity resource implementations

- `FruitBush` is an authoritative resource instance: fruit quantity, production timer and consumption counters only.
- `FruitBushManager` owns fruit-bush population/query/consumption coordination and implements the Pokemon food-service contract. Initial bush placement is derived from the world generation seed.
- `FruitBushVisuals` is a Presentation projection that observes resource-state events and creates/toggles fruit meshes. It does not decide availability or consumption.
- `EcologySpawnLocationProvider` evaluates habitat requirements against the composed world ecology grid and implements the World-owned spawn-location query contract.
- `ResourceManager` owns a separate continuous vegetation-biomass grid and regeneration model, currently visualized by `ResourceMinimap`. It is not interchangeable with discrete fruit-bush instances and is not yet connected to Pokemon feeding.
