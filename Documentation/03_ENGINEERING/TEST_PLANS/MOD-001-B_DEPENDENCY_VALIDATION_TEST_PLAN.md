# MOD-001-B — Dependency validation test plan

- **Status:** IMPLEMENTED — execution NOT RUN
- **Assembly:** `Pokemon.Project.Tests.World.Spherical.EditMode`
- **Fixture:** pure module options plus explicit immutable provider-availability context.
- **Determinism:** same options/context must produce the same ordered seven-entry report and diagnostics.

## Required cases

1. Valid World Core is `Required` and active.
2. Invalid World Core blocks requested modules and preserves unrequested modules as `Disabled`.
3. Biome ground presentation is blocked when terrain or biome classification is disabled.
4. Biome ground presentation is blocked when biome rendering resources are unavailable.
5. Environment assets are blocked when the prefab catalog is unavailable.
6. Biome-dependent environment placement is blocked when classification is disabled.
7. Generic environment placement can be enabled without biome classification when the selected policy explicitly does not require biomes.
8. Player is blocked when its spawn or movement provider is unavailable, without blocking unrelated modules.
9. Pokémon is locally blocked while no Pokémon runtime is available; no placeholder is created.
10. All requested current modules are enabled when their declared providers are available.

## Execution

Run `SphericalWorldModuleValidatorTests` in Unity Test Runner > EditMode. Record exact passed/failed counts and any Console compiler errors. All cases are currently `NOT RUN` by the assistant.
