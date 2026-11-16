# Network Architecture

## Confirmed direction

A dedicated server is the intended hosting model. The server is authoritative for consequential shared world state.

## Authority principles

The server validates and owns:
- session membership and lifecycle;
- Pokémon existence and authoritative individual state;
- resource availability and consumption;
- reproduction and lifecycle transitions;
- combat results and damage;
- world changes with shared consequences;
- persistence checkpoints.

Clients may:
- render replicated state;
- predict/interpolate presentation where appropriate;
- send player intents/commands;
- maintain non-authoritative cosmetic state.

A client must not be trusted to report final damage, resource consumption, reproduction success, or other valuable outcomes.

## Replication principles

Replicate only what clients need. Distinguish:
- high-frequency movement state;
- lower-frequency needs/status changes;
- discrete events;
- cosmetic-only presentation;
- data loaded once per species/content catalog.

Define update rates, relevance/interest management, interpolation, reconciliation and bandwidth budgets before scaling population.

## Simulation scheduling

Individual simulation is confirmed as a design goal, but the tick/scheduling architecture is OPEN. Candidate approaches include fixed simulation ticks, staggered updates, event-driven updates and different rates by system. Measure before committing.

## Security

Validate IDs, ranges, ownership, timing, distance, cooldowns and state transitions on the server. Treat all client input and external API output as untrusted.

## Open decisions

Networking framework, transport, protocol, hosting provider, player count, target Pokémon count, interest management, reconnect policy and server migration are OPEN.
