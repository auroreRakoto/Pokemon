# AI API Integration

## Confirmed high-level direction

The project has selected API-based AI at a high level. Exact provider, use cases, model, deployment, latency and fallback remain OPEN.

## Critical boundary

Do not assume every Pokémon decision should call a remote AI API. The individual simulation must remain deterministic/controllable enough for server authority, testing, performance and offline operation.

## Candidate use cases to evaluate

- high-level dialogue or natural-language interaction;
- content authoring assistance;
- offline generation of behaviour profiles or descriptions;
- optional advanced planning for a limited subset of entities.

These are candidates, not approved features.

## Requirements before integration

Document:
- exact problem solved;
- why local rules/state machines/utility AI are insufficient;
- request/response schema;
- latency and rate limits;
- cost budget;
- timeout/retry/circuit-breaker behaviour;
- deterministic fallback;
- safety/content filtering where relevant;
- privacy and data minimization;
- secret management;
- server-only credential handling;
- logging/monitoring;
- tests with mocked responses.

Never put API secrets in Unity client builds. Treat external output as untrusted data and validate it before use.
