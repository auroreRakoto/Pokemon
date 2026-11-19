# Development Queue — Spherical World Vertical Slice

- **Updated:** 2026-11-19
- **Current goal:** explore the spherical world on foot, see the four land biomes clearly, and optionally encounter one Pokémon.
- **Status convention:** `BLOCKED` = a known issue prevents reliable progress; `NEXT` = highest-priority work; `READY` = scoped but waiting on dependencies; `PROPOSED` = scope requires owner confirmation before implementation.

## Queue

### P0 — WORLD-004 integration verification
**Status: IN_PROGRESS — previous CS7036 report is stale against the 2026-11-19 source snapshot.**

Static inspection of `Pokemon-clean.zip` shows the current
`SphericalPlayerController.Initialize(...)` declaration and its call in
`SphericalBiomeInspectionBootstrap` agree on the five arguments. The current
`SphericalWorldDemoBootstrap` has no `SphericalPlayerController.Initialize`
call. The owner also supplied a Unity Console screenshot showing 0 errors and
0 warnings after the diagnostics/test cleanup.

This clears the previously reported compile blocker as a current known issue,
but does **not** prove that the intended playable scene starts or that
movement is validated.

- [x] Re-audit the current `SphericalPlayerController.Initialize` declaration and all C# call sites; the inspected call is in `SphericalBiomeInspectionBootstrap` and supplies the five declared arguments.
- [ ] Confirm with the owner which scene is the intended traversal composition root.
- [ ] Enter that scene in Play Mode and verify startup, spawn and movement.
- [x] Owner supplied a Console screenshot with 0 errors and 0 warnings (compile evidence only; exact project state at screenshot time).
- [ ] Record the actual scene name, Unity version, runtime result and any remaining issue.

**Done when:** the intended scene starts successfully, the controller receives the authoritative fields/planet centre/radius/camera/capsule height, and basic traversal is manually verified. The current Console screenshot is recorded as compile evidence, not as PlayMode acceptance.

### P1 — Validate globe traversal
**Status: NEXT after P0.**

- [ ] Run WORLD-004 EditMode tests.
- [ ] Enter the intended PlayMode scene and verify spawn on valid land.
- [ ] Walk forward/back/left/right and verify camera-relative direction.
- [ ] Cross at least one Cube-Sphere face seam and test a high-latitude area.
- [ ] Test slope blocking, water boundary, jump, falling and landing.
- [ ] Repeat on a second seed.
- [ ] Record defects, Unity version, seed and test evidence; do not mark unrun tests as passed.

**Done when:** movement is stable and reproducible in the intended scene; all acceptance failures are fixed or explicitly deferred with owner approval.

### P2 — WORLD-005: biome ground material integration
**Status: READY FOR SPECIFICATION / IMPLEMENTATION AFTER P1.**

The repository snapshot already contains candidate assets:
- `Assets/Project/Content/Textures/Environment/Biomes/Generated/TEX_Ground_Plains.png`
- `.../TEX_Ground_Forest.png`
- `.../TEX_Ground_Desert.png`
- `.../TEX_Ground_Mountains.png`
- Matching `MAT_Ground_*` materials under `Assets/Project/Content/Materials/Environment/`.

These files are **candidates, not proof of runtime integration**. Verify import settings, shaders, tiling/scale, references and actual visual output in Unity.

- [ ] Inspect the four textures and material settings in Unity.
- [ ] Decide the material assignment/blending method supported by the current spherical mesh/renderer (for example per-vertex biome weights or a validated submesh/material strategy); do not assume the renderer supports it.
- [ ] Feed presentation from the authoritative WORLD-002 biome data, not a second classification or random per-face choice.
- [ ] Keep ocean/lake/river rendering separate from terrestrial biome materials.
- [ ] Verify plains, forest, desert and mountains on the globe and at face seams.
- [ ] Verify seed determinism and that switching inspection views does not mutate source fields.

**Done when:** all four biomes are visibly distinct in the playable scene, boundaries are coherent, water remains distinct, and the chosen rendering strategy is documented.

### P3 — WORLD-006: first Pokémon in the world (optional proof of life)
**Status: PROPOSED — no Pokémon simulation Runtime source is present in the 2026-11-19 snapshot; requires a new bounded implementation plan and asset/species choice.**

The former Pokémon simulation sources are absent from the current spherical project. This task is a future first-encounter implementation, not a continuation/refactor of the archived `PokemonAgent`.

- [ ] Inventory existing Pokémon models/prefabs; the current repository snapshot does not show an obvious Pokémon model in the inspected content paths.
- [ ] Select one species and confirm whether to use an existing approved model or a temporary placeholder.
- [ ] Define one reproducible spawn location/direction on valid land, initially in one suitable biome.
- [ ] Place the Pokémon using the same spherical ground query and orient it to the local surface normal.
- [ ] Verify scale, visibility, stable ground contact and no spawning in water/inside terrain.
- [ ] Keep AI, autonomous movement, needs, animation, capture, combat and population simulation out of this first placement task.

**Done when:** one Pokémon is reliably visible in Play Mode on valid terrain and its placement is repeatable. This does not mean Pokémon simulation is complete.

### P4 — Small environment dressing
**Status: LATER.**

- [ ] Add a limited set of existing vegetation/natural-feature assets.
- [ ] Drive placement from WORLD-002 biome and vegetation-suitability fields.
- [ ] Respect water, slope and density constraints; make generation deterministic.
- [ ] Profile only after correctness is established.

## Explicitly not next

- Full autonomous ecosystem or thousands of Pokémon.
- Combat, capture, inventory, quests or multiplayer.
- Swimming/flying/climbing traversal.
- Full collision against every tree/rock/building.
- More biomes before the four approved initial biomes are visually integrated.

## Working rule

Only one item is `NEXT` at a time. A blocked item must have a named blocker and a concrete unblock action. Update this queue and the roadmap whenever a work package is completed, split or reordered.


## Commit-message checkpoint rule

- After every independently implemented and validated feature/increment, provide the exact Conventional Commit message before moving to the next feature.
- If the work session ends before validation or completion, provide an honest checkpoint commit message and a concise handoff stating what changed, what remains unverified, the blocker (if any), and the next action.
- Never describe a partial checkpoint as complete or QA-approved. Never claim a commit or push occurred unless confirmed.
- Follow `Documentation/00_GOVERNANCE/GIT_WORKFLOW.md`; do not invent a new commit type or omit the required `Agent` footer.
