# Testing and Validation

## Test layers

1. Unit tests: pure domain rules and calculations.
2. Integration tests: communication between modules.
3. PlayMode tests: Unity runtime behaviour.
4. Editor tests: asset/prefab/material/terrain generation tools.
5. Server tests: authority, commands, replication and session lifecycle.
6. Performance tests: population, world size, update rates, memory and bandwidth.
7. Content validation: meshes, rigs, textures, manifests and references.

## Essential early tests

- individual size factor is clamped and affects only intended visual/collider dimensions;
- species data and individual state remain separate;
- a Pokémon cannot consume an unavailable or out-of-range resource;
- two Pokémon cannot both consume the same final resource quantity;
- behaviour recovers from missing target/path;
- server rejects invalid client combat/resource requests;
- terrain/biome generation is reproducible for the same seed/configuration;
- editor generation is idempotent and does not create duplicate assets;
- missing content references produce actionable validation errors.

## Acceptance criteria

Every feature should define observable criteria before implementation. “Works” is not sufficient. Include expected inputs, outputs, edge cases and failure behaviour.

## Test data

Use small deterministic test worlds and fixed seeds. Provide fixtures for common species, resource patches and biome configurations. Do not rely only on a large random world to reproduce bugs.

## Regression

When fixing a bug, add a test that fails before the fix where practical. Record any known untestable/manual verification.
