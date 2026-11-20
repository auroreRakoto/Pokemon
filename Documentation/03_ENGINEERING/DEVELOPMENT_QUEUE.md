# Development Queue — Spherical World Vertical Slice

- **Updated:** 2026-11-18
- **Current goal:** explore the spherical world on foot, see the four land biomes clearly, and optionally encounter one Pokémon.
- **Status convention:** `BLOCKED` = a known issue prevents reliable progress; `NEXT` = highest-priority work; `READY` = scoped but waiting on dependencies; `PROPOSED` = scope requires owner confirmation before implementation.

## P0 — MOD-001-C: World Core without biome classification
**Status: IMPLEMENTED — owner Unity validation required.**

- [x] Add an explicit generator path that skips biome arrays/classification.
- [x] Preserve the legacy Generate(settings) behaviour.
- [x] Expose explicit biome availability and TryGet/TrySample APIs; legacy biome access fails clearly when absent.
- [x] Keep ground probing independent of biome classification.
- [x] Add deterministic EditMode tests comparing World Core outputs with/without biomes.
- [ ] Run the new EditMode tests and the relevant World/Spherical regression suites in Unity.
- [ ] Validate the isolated geography preview without renderer/player.

**Boundary:** this is a data/contract increment. Biome presentation, environment placement and the inspection bootstrap are not yet composed around optional activation; those remain MOD-001-D/E/F work.

**Next:** MOD-001-D is BLOCKED pending owner Unity validation of MOD-001-C.


## Queue

### P0 — MOD-001-A: world profile module configuration
**Status: IMPLEMENTED — owner Editor-window/profile validation required.**

- [x] Add versioned optional module selection to JSON world profiles.
- [x] Preserve legacy profile defaults through a module sub-schema migration.
- [x] Add an Editor window to configure modules on an existing world profile.
- [ ] Run the new EditMode tests and verify the Editor window in Unity.
- [x] Implement runtime dependency validation and activation report in MOD-001-B.

**Boundary:** module choices are saved at world creation/profile authoring and remain fixed for the active session. Runtime systems are not yet activated/deactivated by these options.


### P0 — MOD-001-B: dependency validation and activation report
**Status: VALIDATED at module-test level — owner screenshot confirms 10/10 EditMode tests passed; full composition integration pending.**

- [x] Add provider-availability validation context and deterministic activation report.
- [x] Add 10 EditMode tests for dependency and local-failure behaviour.
- [x] Owner ran `SphericalWorldModuleValidatorTests`: 10 passed, 0 failed, 0 skipped.
- [ ] Wire activation report into the real world composition root (subsequent MOD-001-D/E/F work).




### P0 — Unblock WORLD-004 integration
**Status: BLOCKED — Unity CS7036 reported.**

- [ ] Resolve the `SphericalPlayerController.Initialize` call in `SphericalWorldDemoBootstrap`.
- [ ] Decide and document the correct composition root: WORLD-002 biome-inspection scene or an upgraded dedicated bootstrap.
- [ ] Ensure the controller receives the authoritative `SphericalGeographicFieldSet`, planet centre, planet radius, camera rig and capsule height.
- [ ] Search all call sites for stale signatures.
- [ ] Recompile in Unity and attach/record Console evidence.

**Done when:** zero compile errors; no dummy/null argument workaround; intended scene starts successfully.

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
**Status: PROPOSED — requires asset/species choice.**

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
