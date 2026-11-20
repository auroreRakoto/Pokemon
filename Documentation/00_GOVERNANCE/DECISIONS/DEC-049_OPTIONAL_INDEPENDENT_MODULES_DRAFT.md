> **SUPERSEDED:** this draft is retained for history. The owner-confirmed decision is `DEC-049_OPTIONAL_INDEPENDENT_MODULES.md` and is indexed in `DECISION_REGISTER.md`.

# DEC-049 — Optional independent gameplay and world capabilities

- **Owner requirement:** CONFIRMED by the owner's explicit instruction on 2026-11-19.
- **Architecture details:** APPROVED through MOD-001 on 2026-11-20.

## Decision statement

World sessions must be composable from independently enabled or disabled feature modules. The absence or deactivation of one optional module must not prevent World Core or unrelated enabled modules from operating.

Initial examples explicitly required by the owner:
- player spawn/control on or off;
- biome system on or off, with a generic/classic terrain fallback;
- environment assets/props on or off;
- Pokémon presence/simulation on or off.

The module model must support observation-only sessions with no player and future Pokémon/ecology modules without requiring player presence.

## Constraints retained

- World Core remains the authoritative source for topology/geography/surface queries in a generated spherical world.
- Optional modules consume explicit World Core contracts; World Core never depends on them.
- The same generated field set is shared; optional presentation must not regenerate or mutate it.
- DEC-044's four-biome definitions remain the default approved biome set when the biome capability is enabled.
- DEC-047's opaque terrain requirement remains in force. When biome presentation is disabled but terrain presentation is enabled, use a neutral/classic opaque fallback.
- DEC-048's shared biome blend shader remains the selected path when biome ground presentation is enabled.
- DEC-019's shared runtime-safe generation core remains in force.
- No old Pokémon/Ecology implementation is restored by this decision.

## Confirmed configuration lifecycle

- Module selection is made when creating the world.
- The selected module set remains fixed for the active world session.
- Runtime hot-toggle/unload/reload is not required and is excluded from MOD-001.
- A different module combination is selected by creating a new world/session.

## Confirmed dependency failure policy

- A missing dependency blocks only the dependent optional capability and emits a precise diagnostic.
- Unrelated capabilities continue when their own dependencies are valid.
- Invalid World Core blocks world creation and all requested optional capabilities.
- A missing provider for a disabled capability is not an error.
- Environment placement declares whether its selected policy requires biome classification; generic placement can operate without biome classification when that policy explicitly says it does not require biomes.
- Pokémon remains unavailable until an actual runtime implementation exists; enabling its profile flag reports a local blocked capability and never creates a placeholder.

## Remaining future design work

- Exact future Pokémon submodule boundaries remain deferred to a separate Pokémon work package.

This decision and MOD-001 were approved by the owner on 2026-11-20. The owner reports the plan milestone has been pushed; the commit hash was not supplied in this conversation.
