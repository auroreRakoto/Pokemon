# WORLD-004 — Player Movement Test Plan

## Scope

Validate the first spherical PC movement slice: field-aware ground contact, radial gravity, grounded-only jump, camera-relative walking, slope threshold and water blocking. This plan does not claim prop collision, swimming, animation or multiplayer authority.

## Automated EditMode tests

Assembly: `Pokemon.Project.Tests.World.Spherical.Movement.EditMode`

| Test | Expected result |
|---|---|
| Gravity toward centre on six cardinal radial directions | Velocity is accelerated inward for every direction |
| Jump while airborne | No impulse is applied |
| Jump while grounded | Impulse follows the supplied ground normal and leaves grounded state |
| Tangential acceleration while airborne | Radial velocity component is preserved |
| Ground contact | Only velocity directed into the surface is removed |

## Manual PlayMode validation

Run `WORLD-002_BiomeInspection` in Unity 6.4 using the current Planet Studio settings.

1. Spawn: player capsule rests on plains without visible penetration or a sustained gap.
2. Idle: player remains stable when no input is held.
3. Walk: WASD and arrow keys move relative to camera heading; diagonal speed is not faster than straight speed.
4. Orbit: hold right mouse and move mouse; movement direction follows the updated camera heading.
5. Jump: Space jumps only while grounded; gravity returns the player to land.
6. Fall: temporarily raise the player above the surface in the Inspector; it falls toward the planet and lands.
7. Coast: grounded movement into ocean/lake/river is blocked; water is not treated as a floor.
8. Slope: the configured maximum slope is respected; no uphill movement beyond the threshold.
9. Globe continuity: walk across a visible cube-sphere face seam and check that orientation and movement remain continuous.
10. Cleanup: exit Play Mode; generated runtime objects disappear and no source scene content is modified.
11. Physics diagnostics: no six-face planet-scale MeshCollider or oversized-triangle warnings are introduced.

## Evidence to record

- Unity version and render pipeline.
- World seed and face resolution.
- Player tuning values (speed, acceleration, gravity, jump impulse, max slope).
- EditMode test result count.
- Screenshots/video for spawn, walking, jump/landing, coast and seam checks.
- Any failed step with Console errors/warnings and reproduction sequence.

## Current status

Tests are authored but not run in the assistant environment. Manual PlayMode checks require the user's Unity Editor. WORLD-004 must remain in progress until both are reported passing or failures are documented.
