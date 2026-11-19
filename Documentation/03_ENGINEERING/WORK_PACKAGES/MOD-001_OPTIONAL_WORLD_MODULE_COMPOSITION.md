# MOD-001 — Optional World Module Composition

- **Status:** PROPOSED — owner design approval required
- **Owner module:** Architecture / World composition (`ARCH`, implementation coordination with `WORLD`)
- **Priority:** P0 architecture prerequisite for continued WORLD-004 integration
- **Roadmap phase:** Phase 0 / Phase 1 boundary
- **Dependencies:** DEC-019, DEC-028, DEC-040, DEC-041, DEC-044, DEC-047, DEC-048; WORLD-002 geographic fields; WORLD-004 movement
- **Target milestone:** Establish independently activatable world capabilities without changing their domain rules

## 1. Goal

A world session can be composed from independently enabled or disabled capabilities. Disabling an optional module must not prevent the World Core or other enabled modules from initializing and operating.

The owner must be able to create configurations such as:
- spherical world with only core geography and a neutral/classic terrain presentation;
- world with biome classification and biome ground presentation;
- world with no environment prefabs/assets;
- world with no player spawned;
- world with a player but no environment assets;
- world with no Pokémon;
- later, a world with Pokémon but no player, and a world with observation-only ecosystem simulation.

This work package establishes the composition/configuration boundary for the modules that exist in the current source snapshot. It does not implement Pokémon gameplay, which is absent from the current Runtime.

## 2. User/developer value

The project can support different play, inspection, simulation and presentation configurations without maintaining separate world generators or separate copies of the same world data.

The module set becomes explicit, inspectable, testable and serializable as part of a world profile/session configuration.

## 3. Scope

### Included

1. Define a required World Core and optional capability modules.
2. Define one per-world module configuration, persisted with or referenced by the world profile.
3. Define explicit dependency validation and clear diagnostics for invalid combinations.
4. Refactor the current spherical composition root so it creates only enabled capabilities.
5. Separate geographic data generation from optional biome classification/presentation where required by the selected configuration.
6. Allow a neutral/classic opaque terrain presentation when biome presentation is disabled.
7. Allow environment asset placement to be disabled without affecting terrain/geography.
8. Allow player creation/controller/camera integration to be disabled without affecting world generation.
9. Define the future Pokémon module as optional in the architecture, without adding Pokémon Runtime stubs or restoring archived code.
10. Give each current visual capability an independent preview and each current capability a module-level test path.
11. Preserve existing profile schema compatibility and migrate only through an explicit versioned migration if profile fields change.

### Explicitly excluded

- Reintroducing `PokemonAgent`, `PokemonNeeds`, old Ecology, berries, Chenipotte or Mystherbe.
- Implementing Pokémon spawning/simulation; no Pokémon Runtime implementation exists in the supplied snapshot.
- Rewriting spherical topology, height, hydrology, ocean coverage or the approved biome classification rules.
- Changing DEC-044's four-biome definitions or DEC-047's opaque terrain requirement.
- Changing the shared biome shader defined by DEC-048; it is used only when biome presentation is enabled.
- Runtime hot-toggle/unload/reload during an active session. **Owner decision: module selection is made at world creation.** The selected module set is fixed for that world session; changing it requires creating a new world/session. Any future live reconfiguration would require a separate owner-approved lifecycle work package.
- Splitting every capability into a separate Unity assembly definition merely to call it a module.
- Adding a generic plugin framework, service locator, global mutable registry or speculative dependency-injection package.
- Unrelated fixes to the four currently failing `SphericalBiomeTextureGeneratorTests`.

## 4. Ownership and architecture

- **Owning module:** World / Application Composition.
- **Authoritative state owner:** World Core owns generated topology/geography and the shared `SphericalGeographicFieldSet`. Each optional module owns only its own state.
- **Composition root:** one explicit world-session composition root. Existing inspection/demo bootstraps are prototype composition roots and must not become domain owners.
- **Public contracts consumed:** existing topology, geographic field, surface-query and world movement contracts, after caller audit.
- **Public contracts changed:** to be specified after detailed caller/serialization audit; no public API change is authorized by this plan alone.
- **Allowed dependencies:** optional modules may consume World Core contracts and explicitly declared provider contracts.
- **Forbidden dependencies:**
  - World Core -> player, biomes, environment assets, Pokémon or presentation.
  - Player -> terrain renderer, biome material, environment prefab placement or Pokémon.
  - Environment placement -> player or renderer.
  - Pokémon -> player presence, camera or renderer.
  - Domain/generation -> UnityEditor.
  - One optional module -> another optional module unless a declared capability dependency is validated by the composition root.
- **Data category:** world profile/module configuration = authored definition; generated fields = generated data; module runtime state = instance state; mesh/material/camera/asset instances = presentation state.

### Initial capability catalogue

| Capability | Initial default | Dependency | Disabled behaviour |
|---|---:|---|---|
| World Core (topology, height, water, surface queries) | Required | None | Not disableable for a generated spherical world |
| 3D terrain presentation | Enabled | World Core | No generated planet mesh; geography remains available |
| Biome classification | Enabled | World Core | No terrestrial biome classification; generic surface classification/fallback must be explicit |
| Biome ground presentation | Enabled | Terrain presentation + biome data | Use neutral/classic opaque URP terrain material; never a missing/transparent shader |
| Environment assets (vegetation/rocks/props) | Disabled by default until content is ready | World Core; biome data only if a selected placement rule explicitly requires it | No prefab instances; no effect on geography or player movement |
| Player presence and movement | Enabled for playable profile; disabled for observation profile | World Core + movement query | Do not spawn player, controller, input adapter or player camera |
| Pokémon presence/simulation | Disabled until implemented | World Core for placement; optional content/presentation contracts | No Pokémon objects or simulation; no effect on world/player |
| Debug/inspection overlays | Optional | The data source being inspected | No overlay objects; no effect on authoritative data |

Defaults above are proposed starting values, not a new product decision. Existing profile defaults and scene behaviour must be preserved until the owner approves the migration.

### Dependency principle

A disabled provider is not silently replaced by a fake object. The composition root validates the requested module set:
- if a selected optional capability has a required dependency disabled, report the exact missing dependency and refuse only that capability or reject the configuration according to the agreed policy;
- unrelated capabilities continue when their own dependency graph is valid;
- optional consumers must handle an unavailable capability explicitly;
- no null-reference-driven optionality.

The final failure policy (disable dependent capability vs reject whole profile) must be confirmed during design review. Preferred proposal: disable only the dependent capability and emit a structured diagnostic, unless World Core itself is invalid.

## 5. Current-state audit

- **Current Runtime:** `Assets/Project/Runtime/World/Spherical/` contains topology, geography, presentation, environment placement, inspection, movement and Studio/profile code in one `Pokemon.Project.World.Spherical` assembly.
- **Current assembly layout:** one Spherical World runtime asmdef references `Unity.InputSystem`; EditMode and PlayMode test assemblies reference that runtime assembly. This plan does not assume that separate asmdefs are required for runtime optionality.
- **Current settings:** `SphericalPlanetStudioSettings` contains relief-layer visibility flags and biome/climate/ocean settings, but no general module activation configuration.
- **Current profile:** `SphericalWorldProfile` serializes world-generation values and schema version 1; it has no optional module set.
- **Current geography:** `SphericalGeographicFieldGenerator.Generate` always creates biome arrays and classifies terrestrial biomes. `SphericalGeographicFieldSet` stores and exposes biome data as part of the field set.
- **Current inspection composition:** `SphericalBiomeInspectionBootstrap.GenerateInspectionWorld` generates fields, creates generated roots, chooses a spawn biome, generates environment placement and builds biome surface meshes/material. These concerns are currently composed together in the inspection bootstrap.
- **Current environment placement:** `SphericalEnvironmentPlacementGenerator.Generate` filters placement candidates using water and biome data. A non-biome placement policy does not currently appear in the inspected API.
- **Current player:** `SphericalPlayerController` consumes the geographic field set through `SphericalGroundProbe` and uses `SphericalThirdPersonCamera` for its tangent movement basis. Player setup is performed by a composition/bootstrap path; it must be proven removable without changing field generation.
- **Current Pokémon:** no Pokémon simulation Runtime source is present in this archive.
- **Current tests:** movement EditMode assembly contains 10 tests and appeared green in the owner's Test Runner screenshot. The broader EditMode assembly showed 4 failing cases under `SphericalBiomeTextureGeneratorTests`; failure details were not supplied and are not diagnosed by this work package.
- **Current visual preview:** spherical Studio and biome inspection paths exist; independent preview paths must be preserved and made module-aware.

### Evidence inspected

Owner-supplied `Pokemon-clean.zip`:
- SHA-256: `829ea7a6abec16886f0133825b376f91f2eece6092125b306fb3447aea3b7a57`
- Inspected current Runtime tree, world profile/settings, geographic field generator/set, environment placement generator, inspection bootstrap, movement scripts, asmdefs, current World-004 work package, architecture baseline, decision register, roadmap, preview/test standard and Git workflow.
- This is a source snapshot, not the owner's live working tree.

## 6. Design

**Owner decision confirmed 2026-11-19:** module selection occurs at world creation only. There is no requirement to enable/disable modules during an active session. The configuration is read and validated before composing the session, then treated as immutable for that session.

### Inputs

| Input | Type | Unit/range | Default | Validation |
|---|---|---|---|---|
| Module configuration | Versioned serializable data | Per world profile, selected when creating a world | Preserve current profile behaviour during migration | Validate schema and dependencies before composition; immutable for the active session |
| Enable terrain presentation | bool | on/off | Preserve current profile behaviour | If off, do not instantiate terrain render objects |
| Enable biome classification | bool | on/off | Preserve current generated profile behaviour | If off, provide explicit generic/no-biome result |
| Enable biome presentation | bool | on/off | Preserve current profile behaviour | Requires terrain presentation and biome data |
| Enable environment assets | bool | on/off | Off for profiles without approved content | Requires valid prefab catalog only when enabled |
| Enable player | bool | on/off | Preserve current playable scene behaviour | Requires valid spawn query and movement configuration |
| Enable Pokémon | bool | on/off | Off until future implementation | Must not create placeholders/stubs for absent code |

### Outputs

| Output | Type | Owner | Observable result |
|---|---|---|---|
| Validated module plan | Runtime-safe data | Composition | Enabled, disabled and dependency-blocked capabilities reported |
| World Core field set | Generated data | World Core | Available independently of presentation/player/assets/Pokémon |
| Terrain presentation root | Scene objects | Presentation | Created only when enabled |
| Biome output/presentation | Field data and scene objects | Biomes/Presentation | Created only when enabled |
| Environment placement root | Scene objects | Environment | Created only when enabled |
| Player root | Scene objects | Player/Movement | Created only when enabled |
| Pokémon root | Future scene/runtime objects | Pokémon | Not implemented by this work package |

### Invariants

- Exactly one authoritative generated field set per generated world/session.
- Selecting or omitting a presentation/gameplay capability at world creation never regenerates or mutates authoritative geography.
- The module set is immutable during the active world session; no runtime hot-toggle lifecycle is required in MOD-001.
- Disabled modules create no runtime GameObjects, controllers, renderers or asset instances owned by that module.
- The player can be disabled without affecting World Core.
- Environment assets can be disabled without affecting terrain/geography.
- Biome presentation can be disabled while preserving a valid opaque terrain fallback when terrain presentation is enabled.
- Biome classification can be disabled only if the field-set contract and all consumers explicitly support its absence; do not silently label generic terrain as Plains.
- Pokémon is not a dependency of World Core, Player, Biomes or Environment placement.
- Player is not a dependency of Pokémon.
- Repeated Generate/Clear operations are idempotent and do not leave duplicate generated roots.
- No optional module reads state back from a renderer, material, camera or generated prefab.

### Failure behaviour

- Invalid World Core configuration: fail world creation with a clear diagnostic.
- Missing dependency for one optional module: apply the approved dependency failure policy and report the exact dependency.
- Missing asset catalog when assets are disabled: no error.
- Missing player prefab/controller references when player is disabled: no error.
- Missing biome material mapping when biome presentation is disabled: no error; use neutral/classic opaque terrain material if terrain presentation is enabled.
- Missing Pokémon assets/code while Pokémon is disabled: no error.
- Missing required references for an enabled module: fail that module with a clear diagnostic; never silently create a fake module.

### Determinism

- World Core continues to use the existing seed and generator version.
- Optional presentation/placement modules consume the same generated fields and deterministic seed derivation.
- Enabling/disabling a presentation module must not alter generated geography or the output of another module.
- Environment placement must be reproducible for the same field set, module configuration, catalog and seed.

## 7. Independent module validation

- **Preview type:** current Studio/inspection previews, extended to run each module alone or in valid combinations.
- **Preview entry point:** dedicated module preview commands/scenes, not only the full playable inspection bootstrap.
- **Fixtures:** fixed small spherical fixture and one fixed full-planet profile.
- **Required configurations:**
  1. World Core + neutral terrain presentation only.
  2. World Core + biome classification + biome presentation.
  3. World Core + environment assets, with biome-aware and generic placement policies tested separately.
  4. World Core + player, with terrain presentation disabled where ground-query-only movement is supported.
  5. World Core with player, assets, biomes and Pokémon all disabled.
  6. World Core + Pokémon in a future work package, with Player disabled.
- **Diagnostics:** module activation report, dependency report, generated object counts and clear/reset result.
- **Clear/reset:** each module clears only its owned output root; clearing one module must not clear shared World Core data or another module's output.

## 8. Implementation plan

| Step | Change | Files (expected; confirm against latest owner snapshot before implementation) | Depends on | Output |
|---|---|---|---|---|
| 1 | Add versioned module configuration to world profile | `SphericalWorldProfile.cs`, profile JSON/serialization tests | Approved schema | Persisted module set |
| 2 | Add dependency validation and module activation report | New focused composition/configuration files under World/Composition | Step 1 | Deterministic validated module plan |
| 3 | Separate World Core generation from optional biome classification | `SphericalGeographicFieldGenerator.cs`, `SphericalGeographicFieldSet.cs`, related tests/contracts | Step 2 + contract review | Core fields available without biome capability |
| 4 | Make terrain presentation choose biome blend or neutral opaque fallback | `SphericalBiomeSurfaceMeshGenerator.cs`, `SphericalPlanetPresenter.cs`, material mapping adapter | Step 3 | Renderer optional; biome visual optional |
| 5 | Make environment placement independently activatable | `SphericalEnvironmentPlacementGenerator.cs`, inspection composition | Step 2/3 | Assets optional and clearable |
| 6 | Make player composition optional | `SphericalBiomeInspectionBootstrap.cs` or successor composition root; movement integration callers | Step 2 | No player objects when disabled |
| 7 | Add module-level tests and previews | Existing `Assets/Project/Tests/...` assemblies and preview tools | Steps 2–6 | Isolation and combination evidence |
| 8 | Update documentation/queue/decision records | Domain, architecture, contracts, queue, index, changelog | All implementation steps | Current docs reflect shipped capability |

No implementation begins until this plan is approved and the plan milestone is pushed.

## 9. Tests and acceptance

| Test category | Test / expected result | Required |
|---|---|---|
| Unit/EditMode | Module config schema, dependency graph, defaults, invalid combinations | Yes |
| Determinism | Same world seed/config yields same World Core fields regardless of optional modules | Yes |
| Boundary/invariants | Missing optional assets/player/Pokémon/biome presentation does not fail World Core | Yes |
| Failure behaviour | Enabled module with missing required dependency reports precise failure | Yes |
| Module preview | Each existing visual module previews independently | Yes |
| Contract/integration | World Core -> Player/Presentation/Environment consumers use same field set | Yes |
| PlayMode | Player disabled means no player/controller/camera root; player enabled works | Yes |
| Regression | Existing topology/geography/movement/biome tests retained and classified | Yes |
| Performance | Compare generation cost with optional biome/placement modules on/off; no optimization target asserted yet | Yes |
| Manual visual/UX | Inspect generic terrain, biome terrain, assets on/off, player on/off | Yes |

### Acceptance criteria

- [ ] A world profile can independently enable/disable terrain presentation, biome classification, biome presentation, environment assets and player.
- [ ] The default existing playable profile preserves its current behaviour after migration.
- [ ] World Core generates once and remains authoritative regardless of enabled optional modules.
- [ ] Biomes off produces an explicit generic/no-biome state and a valid neutral opaque terrain presentation when rendering is enabled.
- [ ] Assets off creates no environment prefab instances and does not block the world.
- [ ] Player off creates no player, controller, input adapter or player camera, and does not block the world.
- [ ] The absence of Pokémon code/assets does not block any current profile; future Pokémon work can be enabled without requiring Player.
- [ ] Each existing visual module has an independent preview and diagnostics.
- [ ] Existing movement tests remain green; the four known biome texture test failures are diagnosed separately and not hidden.
- [ ] No full game scene is required to test World Core, biome data, presentation or environment placement independently.

## 10. Risks and rollback

- **Risk:** `SphericalGeographicFieldSet` currently always stores biome arrays; optional classification changes its contract and consumers.
- **Risk:** environment placement currently uses biome matching; generic placement must be an explicit policy, not an accidental bypass.
- **Risk:** the inspection bootstrap currently composes generation, spawn-biome selection, assets and biome rendering in one flow.
- **Risk:** world profile schema version 1 needs a controlled migration if module configuration is added.
- **Risk:** current serialized scene references and generated roots may be coupled to the bootstrap.
- **Serialized data/scene/prefab impact:** audit all profile assets, scenes and bootstrap references before edits.
- **`.meta`/GUID impact:** preserve existing script GUIDs; no deletion until scene/prefab reference audit.
- **Rollback:** retain legacy profile defaults; disable the new composition path behind an explicit profile schema/version until verified; restore complete prior files only if schema migration and serialized references are reconciled.

## 11. Documentation impact

- [ ] `Documentation/00_GOVERNANCE/DECISION_REGISTER.md` — add confirmed owner requirement and open design details.
- [ ] `Documentation/01_ARCHITECTURE/ARCHITECTURE_BASELINE_V2.md` — add optional capability/composition rules after architecture approval.
- [ ] `Documentation/01_ARCHITECTURE/WORLD_ARCHITECTURE.md` — update world submodule contracts and optional dependencies.
- [ ] `Documentation/02_DOMAINS/WORLD_AND_BIOMES.md` — define generic/no-biome fallback and data ownership.
- [ ] `Documentation/03_ENGINEERING/DEVELOPMENT_QUEUE.md` — add MOD-001 and reorder WORLD-004/WORLD-005 as approved.
- [ ] Test plans, source-reference index, document index and changelog.

## 12. Work breakdown

| Subtask | Output | Estimate | Validation |
|---|---|---:|---|
| Contract/dependency design | Capability catalogue and dependency matrix | 1 plan increment | Architecture review |
| Profile/configuration | Versioned per-world module selection | 1 increment | EditMode/schema tests |
| World Core/biome separation | Explicit generic/no-biome data path | 1 increment | Determinism + contract tests |
| Presentation/asset/player composition | Independent lifecycle roots | 2–3 increments | Module previews + PlayMode |
| Documentation and QA | Updated source of truth | 1 increment | QA review |

## 13. Status report

- **Implemented:** none.
- **Validated:** static audit of supplied `Pokemon-clean.zip`.
- **Blocked:** owner approval of the remaining module dependency/failure policy and plan milestone. Module selection timing is CONFIRMED: world creation only.
- **Not run:** Unity compilation, tests and previews by assistant.
- **Remaining:** owner review, push plan milestone, then implementation in bounded increments.
- **Next objective:** approve MOD-001 design and plan commit before touching runtime code.

## 14. Commit messages

### Plan milestone

`docs(architecture): plan optional world module composition`

`Work-Package: MOD-001`
`Agent: ARCH`

### Implementation milestone

`feat(world): compose optional world capabilities from profile`

`Work-Package: MOD-001`
`Plan-Commit: <plan-commit-hash>`
`Agent: ARCH`
