# Sessions and Persistence

## Scope

Owns session lifecycle and the technical storage/recovery of authoritative state. Domain systems define what their state means; persistence defines how it is serialized and restored.

## Session lifecycle

A session may have stages such as:
- configured;
- starting;
- loading/generating;
- running;
- paused/maintenance;
- shutting down;
- closed/failed.

Exact lifecycle and reconnect policy are OPEN.

## Persistence candidates

Potentially persisted:
- session/world seed and generator version;
- player/session membership data;
- Pokémon individual state;
- resource instance state;
- world changes that must survive;
- time/environment state if required;
- progression/inventory if later included.

Do not persist transient particles, local animation blend values or other presentation state unless a specific requirement exists.

## Save schema

Every saved aggregate should include schema version, stable IDs, units, required/optional fields and migration rules.

## Ownership

Domain modules expose save/load data transfer models or serialization interfaces. Persistence must not reach into arbitrary private fields or Unity scene objects.

## Failure handling

Define checkpoint frequency, write atomicity, corruption handling, backup/rollback, duplicate requests, shutdown handling and recovery before production deployment.
