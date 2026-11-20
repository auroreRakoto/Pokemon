# DEC-049 — Optional independent world and gameplay capabilities

- **Owner requirement:** CONFIRMED.
- **Module-selection lifecycle:** CONFIRMED by owner.
- **Failure policy:** CONFIRMED by owner.
- **Design clarifications:** CONFIRMED by owner on 2026-11-20.
- **Implementation status:** not implied by this decision; each work package requires its own approval and pushed plan milestone.

## Decision statement

A world session can be composed from independently enabled or disabled capabilities. Disabling one optional capability must not prevent World Core or unrelated enabled capabilities from operating.

Module selection is made at world creation. The selected set remains fixed for that session. Runtime hot-toggle/unload/reload is outside MOD-001.

## Confirmed design clarifications

1. **Generic spawn without biome classification:** YES. If the player module is enabled while biome classification is disabled, spawn selection may use explicit generic World Core criteria such as land/water class, slope and altitude. A spawn rule that specifically requires a biome must instead be rejected with a clear module-local diagnostic. It must never silently substitute Plains.
2. **World without 3D terrain presentation:** YES. World Core may be generated and queried without creating planet mesh, renderer, terrain material, terrain presentation collider or visual planet root. This is a valid observation/simulation world mode. The existing Editor generation workflow and its explicit Generate/Apply-to-Scene capability remain available when terrain presentation is requested; this decision does not remove that tooling capability.
3. **Pokémon simulation without Pokémon assets:** YES. Simulation data and behaviour must be separable from visual prefabs/assets. Missing visual assets may suppress only Pokémon presentation, not the simulation module, provided the actual simulation runtime and required contracts are available.
4. **Missing optional dependency:** disable/block only the affected optional capability and report the exact missing dependency. Do not reject the whole world unless World Core itself is invalid.
5. **Missing/invalid World Core:** fail world creation with an explicit diagnostic.

## Retained constraints

- World Core owns authoritative topology, geography and surface-query data.
- Optional modules consume explicit World Core/provider contracts; World Core never depends on optional modules.
- One generated field set is shared by all active consumers; optional presentation must not regenerate or mutate it.
- DEC-044's four-biome definitions remain unchanged when biome classification is enabled.
- DEC-047's opaque terrain requirement remains in force whenever terrain presentation is enabled. Without biome ground presentation, use a neutral/classic opaque material.
- DEC-048's shared opaque biome blend shader remains the path when biome ground presentation is enabled.
- DEC-019's shared runtime-safe generation core remains in force. Editor tooling must retain the ability to generate/apply real terrain presentation when that capability is selected.
- DEC-007's dedicated-server/server-authoritative architecture remains in force.
- No old Pokémon/Ecology implementation is restored by this decision.

## Implementation boundary

This decision confirms product behaviour and the high-level failure policy. It does not approve any code change, public contract change, serialized-schema migration, package/assembly change, or implementation beyond the separately approved MOD-001 work packages.
