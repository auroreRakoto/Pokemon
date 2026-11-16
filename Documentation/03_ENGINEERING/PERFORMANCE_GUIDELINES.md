# Performance Guidelines

## Principle

Individual simulation does not mean every individual must run every subsystem every rendered frame.

## Areas to profile

- number of active Pokémon;
- simulation update frequency;
- pathfinding and perception queries;
- spatial queries and neighbour searches;
- terrain chunks and draw calls;
- vegetation instances;
- animation/rig cost;
- VFX/audio concurrency;
- network replication bandwidth;
- memory and asset loading;
- persistence serialization.

## Candidate optimizations

Use only when supported by profiling:
- staggered or multi-rate simulation;
- event-driven updates;
- spatial partitioning;
- relevance/interest management;
- pooled objects/effects;
- instancing and LOD;
- simplified distant simulation;
- cached environment queries;
- batched persistence/network updates.

## Budgets

Target hardware, player count, Pokémon count, world dimensions, frame rate and server tick rate are OPEN. Define budgets before optimizing prematurely.

## Performance change rule

Any new always-running system must state expected frequency, entity count, complexity and profiling plan.
