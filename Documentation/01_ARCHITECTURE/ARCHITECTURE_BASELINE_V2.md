# Architecture Baseline v2

- **Status:** PROPOSED — owner approval required
- **Version:** 2.0
- **Date:** 2026-11-19
- **Purpose:** stable ownership, dependency direction, module boundaries and independent testability.

## 1. Architectural principles

1. One responsibility has one authoritative owner.
2. Domain logic is independent of Unity presentation and Editor tooling wherever practical.
3. Modules expose narrow, documented contracts; they do not reach into each other's private implementation.
4. Composition roots wire modules; they do not own domain rules.
5. Every substantial module has an independent test path and a preview path when its output is visual.
6. Integration is an adapter/composition concern, not a reason to make every module depend on the whole game.
7. Runtime state, authored definitions, generated data and presentation state remain distinct.
8. Keep the current implementation simple; introduce new architectural machinery only when a concrete need is demonstrated.

## 2. Canonical layers

### 2.1 Domain

Owns rules, calculations, state transitions and domain-facing contracts.

- Must not reference `UnityEditor`.
- Must not depend on UI, camera, scene objects or renderer implementation.
- Prefer plain C# for deterministic calculations and state transitions.
- Unity types are acceptable only where they are intrinsic to the domain's spatial/runtime requirement and cannot be reasonably isolated.

### 2.2 Application / composition

Owns startup, dependency construction, lifecycle coordination and cross-module wiring.

- Runtime composition roots live under `Assets/Project/Runtime/Composition/` or an explicitly documented domain bootstrap when the project is still in prototype form.
- Composition may reference concrete implementations from multiple domains.
- Domain modules must not reference composition roots.
- Composition does not duplicate domain rules.

### 2.3 Infrastructure

Owns filesystem/data loading, asset lookup adapters, telemetry sinks, serialization adapters and external integrations.

- Expose narrow interfaces to domain/application code.
- Keep vendor-specific APIs behind adapters.
- No domain rule should require a specific infrastructure provider.

### 2.4 Presentation

Owns meshes, shaders, materials, camera, animation, audio, VFX, UI and visual/debug projections.

- May read domain data or read models.
- Must not become the authoritative owner of simulation state.
- Must not reclassify authoritative biomes or independently regenerate geographic data to make visuals work.

### 2.5 Editor and content tools

Owns `AssetDatabase`, prefab/material/scene authoring, importers, preview windows and validation commands.

- Editor code stays in Editor-only folders/assemblies.
- Editor tools may call runtime-safe factories, validators and pure generation logic.
- Runtime code never references Editor code.
- Tool actions should be repeatable and idempotent.

### 2.6 Tests

Tests are a first-class consumer of module contracts. They must not require a full production scene for every rule.

## 3. Domain ownership

| Domain | Owns | Must not own |
|---|---|---|
| World | Planet topology, terrain/geography, biome classification, world coordinates, ground/surface queries, world environment placement and movement adapters | Pokémon needs/biology, resource consumption rules, UI |
| Pokémon | Species definitions, individual identity/state, needs, lifecycle, behaviour, population policy, trainer-facing Pokémon rules | Terrain generation, resource regeneration, camera/mesh rendering |
| Ecology | Resource definitions/instances/lifecycle, food-web rules, habitat suitability and ecological interactions | Pokémon physiology, terrain mesh generation, player UI |
| Combat | Battle/attack rules, ability execution, damage/status calculations and combat state | World topology, general needs and resource regeneration |
| Trainer / Player | Player progression, party, inventory, capture/training interactions and player-facing commands | Pokémon species biology, authoritative terrain generation |
| Presentation | Visual projections, animation, VFX/audio, camera, UI and debug visualization | Authoritative domain state |
| Content / Art | Source assets, rigs, materials, animation authoring, asset metadata and content validation | Gameplay decisions |
| Data / Persistence | Serialization contracts, schema migration, save/load adapters and persistence lifecycle | Domain rules |
| Infrastructure | File/config loading, telemetry, diagnostics, external API adapters | Domain ownership |
| Composition | Dependency wiring and startup/shutdown orchestration | Domain rules |
| Editor / Tools | Editor automation, preview builders, asset validation and developer menus | Runtime services |
| QA | Test plans, automated tests, regression suites, performance evidence and acceptance review | Unapproved product/architecture decisions |

A feature belongs to the module that owns the rule/state, not the module that happens to display it.

## 4. Dependency direction

Allowed high-level direction:

`Presentation -> Application/Read Models -> Domain Contracts`
`Composition -> Domain + Infrastructure + Presentation adapters`
`Infrastructure -> Domain-facing interfaces/contracts`
`Editor -> Runtime-safe factories/validators + UnityEditor APIs`
`Tests -> Public module contracts and test seams`

Forbidden:
- Domain -> Presentation.
- Domain -> Editor.
- Runtime -> `UnityEditor`.
- Pokémon -> direct terrain mesh/renderer implementation.
- Ecology -> direct UI/visual prefab ownership.
- Presentation -> mutation of authoritative simulation state.
- Two domains referencing each other's private implementation.
- Circular assembly dependencies.
- Static global mutable registries as hidden service locators.

Cross-domain communication uses explicit interfaces, queries, commands or events. Choose the smallest mechanism that fits; do not introduce an event bus or generic framework by default.

## 5. World architecture

### 5.1 Current approved direction

- Spherical planet.
- Pokémon-style third-person camera at a distance, with adjustable angle.
- Large planet and long journeys across large regions.
- Subtle curvature visible at ground level and clear curvature from high altitude.
- First prototype: spherical planet, terrain, player movement and visual inspection.
- Initial land biomes: Plains, Forest, Desert and Mountains.
- Mountains are classified primarily by altitude and slope.
- Initial ocean-coverage target/default: 65% of planet surface. This is a target, not a fixed sea-level value.
- Both autonomous ecosystem observation and trainer activity are product goals.
- Target is the owner's PC; profile and optimize against actual machine measurements.

### 5.2 World module decomposition

| Submodule | Responsibility | Independent proof |
|---|---|---|
| Topology | Cube-sphere mapping, face adjacency, directions | Numeric/property tests and seam preview |
| Terrain/geography | Height, slope, climate, moisture, water and derived fields | Deterministic field tests, map/planet preview |
| Biomes | Classification and boundary rules | Classification tests and biome-colour preview |
| Surface query | Ground position, normal, tangent frame, water/land query | Coordinate fixtures and probe gizmos |
| Presentation | Surface meshes, materials, textures and atmosphere | Generated preview scene |
| Movement | Motor, ground probe, camera-relative movement | Small test planet with controller |
| Environment placement | Candidate sampling, density, prefab choice, orientation | Placement preview on a fixture planet |
| Planet authoring | Profiles, generation settings, import/export and validation | Editor preview and profile tests |

No submodule should require the full game bootstrap merely to demonstrate its core output.

### 5.3 Shared world data rule

There must be one authoritative generated field set per generated planet/session. Presentation, movement, environment placement and later ecology consume the same field set or explicit projections of it. They must not independently regenerate or reinterpret the world.

The composition root owns creation and lifetime of the field set. It passes explicit references/contracts to consumers.

## 6. Pokémon and ecology architecture

- Species definitions are immutable authored data.
- Individual Pokémon state is separate and uniquely identified.
- Needs, lifecycle, behaviour selection, action execution, movement intent and presentation are separate responsibilities.
- Pokémon behaviour requests movement through a World-owned movement contract; it does not directly own the world transform adapter.
- Ecology owns resource quantities, regeneration, availability and consumption contracts.
- Pokémon asks Ecology to consume a resource through a narrow contract; it does not mutate resource internals.
- A visual object is a projection of a domain entity, not the entity itself.
- A species prefab is not a species definition.
- A trainer party/roster is separate from the wild population.

## 7. Data ownership categories

Every data structure must be identified as one of:

- **Definition:** authored, shared, normally immutable (species, biome, resource, ability, world profile).
- **Instance state:** mutable state of one individual/resource/world/session.
- **Generated data:** output of a generator for a specific seed/config/version.
- **Derived data:** cache or projection recalculable from authoritative data.
- **Presentation data:** camera, animation, UI and transient visual state.
- **Diagnostic data:** debug-only metrics, visualizations and reports.

ScriptableObjects are suitable for authored definitions/configuration, not mutable runtime state shared by multiple individuals.

## 8. Runtime/editor/content layout

The exact tree should evolve from actual modules; do not create empty folders or assemblies speculatively.

```text
Assets/Project/
├── Runtime/
│   ├── Composition/
│   ├── World/
│   │   └── Spherical/
│   │       ├── Topology/
│   │       ├── Geography/
│   │       ├── Biomes/
│   │       ├── Surface/
│   │       ├── Movement/
│   │       ├── Environment/
│   │       ├── Presentation/
│   │       └── Studio/
│   ├── Pokemon/
│   │   ├── Definitions/
│   │   ├── Individual/
│   │   ├── Behaviour/
│   │   └── Population/
│   ├── Ecology/
│   │   ├── Definitions/
│   │   ├── Resources/
│   │   └── Systems/
│   ├── Trainer/
│   ├── Combat/
│   ├── Data/
│   ├── Infrastructure/
│   └── Presentation/
├── Editor/
│   ├── Tools/
│   ├── Preview/
│   └── Validation/
└── Tests/
    ├── EditMode/
    ├── PlayMode/
    └── Performance/
```

This is a logical target, not an instruction to move existing files immediately. Preserve `.meta` files and GUIDs for every Unity asset move.

## 9. Assembly definitions

Do not create one assembly per folder. Add or split `.asmdef` files only when:
- ownership boundaries are stable;
- circular references have been removed;
- a concrete test/runtime isolation benefit exists;
- all references are documented and compile-validated.

Any assembly change requires a dependency graph review and Unity compilation.

## 10. Architecture decision gates

The repository contains existing server/network architecture decisions and proposals. Preserve their current approved status; do not reinterpret an OPEN proposal as approved or an approved decision as cancelled. Any conflict between those documents and this baseline must be recorded for owner resolution before implementation.

Keep these topics OPEN unless an existing approved decision already governs them:
- terrain chunking/LOD/streaming;
- large-world precision/origin shifting;
- runtime vegetation rendering strategy;
- simulation scheduling/tick rate;
- save format and persistence technology;
- future multiplayer/networking implementation scope (while preserving existing approved architecture decisions);
- AI API provider/use cases;
- content streaming/addressables;
- target population scale.

The first playable version is local on the owner's PC. Do not implement networking or dedicated-server features as part of the current world prototype unless an approved work package requires them. However, existing approved server-authority/network architecture decisions in the repository remain in force until explicitly reviewed and changed through the decision process. This baseline does not silently cancel them.

## 11. Architecture compliance review

Every work package must state:
- owner module;
- allowed dependencies;
- forbidden dependencies;
- public contracts used/changed;
- authoritative state owner;
- composition root;
- preview and test seam.

A change that cannot answer these questions is not ready for implementation.
