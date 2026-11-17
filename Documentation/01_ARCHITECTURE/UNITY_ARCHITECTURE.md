# Unity Architecture and Script-Driven Workflow

## Philosophy

Unity should primarily be the runtime engine, renderer and authoring environment for automated tools. Prefer script-driven setup and repeatable pipelines over repetitive Inspector configuration.

## Runtime/editor separation

### Runtime code
- simulation and gameplay;
- world/session runtime;
- rendering controllers;
- input and UI;
- runtime asset/data loading;
- network clients/server runtime as appropriate.

### Editor-only code
- AssetDatabase operations;
- create/update ScriptableObjects;
- create materials and TerrainLayers;
- configure prefabs and scenes;
- import processors;
- previews, validation windows and generation menus.

Editor code must be isolated under Editor folders or Editor-only assemblies. Runtime assemblies must not reference `UnityEditor`.

## Script-driven asset setup

A repeatable editor command should be able to:
1. find source textures/assets;
2. validate file presence and import settings;
3. create/update materials and terrain layers;
4. configure prefab components/references;
5. create/update data assets;
6. run validation;
7. produce a report.

Operations should be idempotent: running them repeatedly should update the same intended assets rather than create duplicates.

## Inspector policy

Inspector references are acceptable when they are stable, few, and genuinely useful to tune manually. Do not require users to drag dozens of references into fields to make the game work.

Prefer centralized catalogs, stable IDs, addressable/resource registries or generated references where appropriate. Select the loading technology only after evaluating build and platform constraints.

## Scene policy

Avoid essential gameplay setup hidden in manually configured scenes. Use a bootstrap process and validation that checks required scene objects and configuration. Keep scenes small and purposeful.

## Runtime scaling

For individual Pokémon size:
- stable root contains identity, networking and gameplay components;
- child `VisualRoot` contains mesh/rig/visual attachments;
- apply individual size factor to `VisualRoot`;
- define collider and interaction scaling deliberately;
- do not scale network root or movement coordinate system accidentally.


## Canonical Unity script layout

The physical layout follows module ownership. Folder placement is not a substitute for ownership contracts: cross-domain orchestration belongs in an explicit composition layer, Editor automation stays under `Editor`, and presentation must not own authoritative simulation rules.

```text
Assets/Project/
├── Data/
│   └── WorldGeneration/      # Source JSON configuration for procedural world generation
│       └── world_generation.json
├── Runtime/
│   ├── Composition/          # Explicit scene/bootstrap and cross-domain wiring
│   ├── Pokemon/
│   │   ├── Definitions/       # Species-level immutable definitions
│   │   ├── Contracts/         # Pokemon-facing factory/reproduction/population/event contracts
│   │   ├── Individual/        # Per-individual identity, needs and lifecycle systems
│   │   ├── Behaviour/         # Behaviour orchestration, actions and scheduling
│   │   └── Population/        # Population domain policy/service
│   ├── Ecology/
│   │   ├── Contracts/         # Ecology food and read-only resource contracts
│   │   ├── Generation/        # Ecological field generation
│   │   ├── Resources/
│   │   │   ├── Definitions/   # Resource type definitions
│   │   │   ├── Instances/     # World resource instances
│   │   │   └── Systems/       # Resource lifecycle and regeneration
│   │   └── World/             # Ecology-owned world data/layers and habitat query adapter
│   ├── World/
│   │   ├── Biomes/
│   │   │   ├── Definitions/
│   │   │   └── Data/
│   │   ├── Worlds/Definitions/
│   │   ├── Contracts/         # World-owned movement and spawn-location query contracts
│   │   ├── Environment/       # World environment data/grid
│   │   ├── Movement/          # Transform/locomotion adapter implementing IWorldMovement
│   │   ├── Generation/Biomes/ # Biome-region topology generation
│   │   └── Placement/         # World placement (vegetation, etc.)
│   ├── DataServices/         # Runtime contracts, loading and projections for source data
│   │   ├── Contracts/
│   │   ├── Loading/
│   │   └── Projections/
│   ├── Infrastructure/
│   │   ├── Content/
│   │   └── Telemetry/
│   └── Presentation/          # Client/UI controls and domain read-only views
│       ├── Ecology/Resources/ # Read-only fruit/resource visual projections
│       ├── World/             # World/biome map rendering
│       └── Simulation/
└── Editor/
    ├── Content/
    ├── Data/Pipeline/
    └── Tools/Blender/
```

### Source data versus runtime data services

There is exactly one project-level `Data` directory: `Assets/Project/Data`. It contains source data files and configuration, including `WorldGeneration/world_generation.json`.

Do not create `Assets/Project/Runtime/Data`. Runtime code that reads, validates, maps or projects those files belongs under `Assets/Project/Runtime/DataServices`:
- `Contracts/` — runtime-facing data contracts;
- `Loading/` — parsing, validation and loading adapters;
- `Projections/` — runtime models or generated projections derived from source data.

The JSON file is configuration input, not executable code and not a Unity scene asset. The loader must expose validated settings to the world-generation core without making that core depend on file paths, `UnityEditor`, or a particular storage backend. Choose the concrete loading mechanism to work in both Editor and player builds; do not assume a file under `Assets/Project/Data` is automatically available through `Resources`.

### Ownership placement rules

- Species definitions belong to `Pokemon/Definitions`; mutable needs, identity and lifecycle systems belong to `Pokemon/Individual`.
- Behaviour scheduling and action-specific logic belong to `Pokemon/Behaviour`; population rules/services belong to `Pokemon/Population`.
- Cross-domain startup and wiring belong to `Runtime/Composition`; composition roots may reference concrete adapters, domain modules may not.
- Pokemon behaviour uses narrow contracts for food/reproduction and World-owned movement/spawn queries. Ecology supplies habitat-aware spawn locations; the World movement adapter owns Transform mutation.
- Resource lifecycle belongs to Ecology; fruit-bush visual instantiation belongs to Presentation and observes resource state changes.
- Resource definitions, instances and lifecycle systems stay in `Ecology/Resources`, not in World or Presentation.
- Biome/world topology and spatial placement stay in `World`.
- Ecological maps and state are owned by Ecology; rendering/UI adapters belong in Presentation.
- Cross-domain telemetry/logging is infrastructure, not a gameplay-domain owner.
- Runtime data contracts, loading and generated projections are separate concerns under `Runtime/DataServices`. The authoritative source files (including world-generation JSON configuration) live under the single project-level `Assets/Project/Data` directory.
- `UnityEditor` APIs and AssetDatabase work remain under `Editor`.

Population tuning lives in `Content/Population/Profiles`, separate from species definitions. Moving a script must move its `.meta` file with it so Unity GUID references remain stable. A folder move alone does not prove runtime behaviour or compilation; Unity import/build validation is still required.
