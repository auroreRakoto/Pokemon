# DEC-049 — Optional independent gameplay and world capabilities

- **Owner requirement:** CONFIRMED by the owner's explicit instruction on 2026-11-19.
- **Architecture details:** OPEN pending MOD-001 review and approval.

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

## Open design questions for MOD-001

- Whether a missing dependency disables only its dependent capability or rejects the whole profile.
- Exact separation between biome classification and biome ground presentation.
- Whether environment assets without biomes use generic surface suitability or require an explicitly selected placement policy.
- Exact future Pokémon submodule boundaries.

This decision confirms the required product behaviour; it does not approve MOD-001 implementation until its work package and plan milestone are approved/pushed.
