# System Architecture

## 1. Architectural overview

The game is divided into domain systems, infrastructure, content, and tools.

- **Client/Unity presentation:** renders the world, accepts player input, presents UI, plays animation/audio/VFX.
- **Dedicated server:** owns authoritative session state and validates consequential actions.
- **Simulation domains:** Pokémon individuals, ecology, world state, combat.
- **Shared contracts:** stable data types and messages crossing module or process boundaries.
- **Content pipeline:** Blender source assets, import, validation, prefab/material setup.
- **Persistence:** stores the subset of world/session/individual state required by the game design.
- **Tools:** editor and offline automation; they must not become hidden runtime dependencies.

## 2. Dependency direction

Preferred direction:

`Domain logic -> domain interfaces/contracts -> infrastructure adapters`

Domain logic should not depend directly on Unity Editor APIs, UI, rendering, or a specific networking vendor.

Presentation may depend on domain-facing read models, but authoritative domain state must not depend on presentation.

Editor tools may use Unity Editor APIs and call runtime-safe factories or validators. Runtime assemblies must not reference `UnityEditor`.

## 3. High-level flow

1. Session host creates/loads a session.
2. World generator constructs or loads world topology and biome data.
3. Ecology system initializes resources and environmental conditions.
4. Pokémon population system spawns individual entities according to species and habitat rules.
5. Simulation scheduler advances individual and world processes.
6. Behaviour system selects actions using needs, perception and context.
7. World interaction systems validate movement, feeding, resting, combat and resource changes.
8. Server publishes relevant state updates to clients.
9. Clients render presentation and send player intents.
10. Persistence saves agreed authoritative state at defined checkpoints.

## 4. Separation of state

Distinguish:
- **Definition data:** species, abilities, biome definitions, resource definitions.
- **Instance state:** individual Pokémon, resource patches, world conditions.
- **Transient state:** current animation, short-lived VFX, temporary perception results.
- **Derived state:** visual scale, UI labels, cached suitability scores.
- **Presentation state:** camera, particles, local animation interpolation.

Do not persist or replicate derived/presentation state unless there is a demonstrated need.

## 5. Runtime vs editor

Runtime:
- session execution;
- simulation;
- player interaction;
- rendering;
- network communication;
- runtime loading of supported data.

Editor/offline:
- create/update ScriptableObjects;
- create materials and TerrainLayers;
- configure prefabs;
- import/validate assets;
- generate previews;
- bake or prepare content;
- run authoring diagnostics.

Keep Editor-only code in Editor folders or Editor-only assemblies. Never place `UnityEditor` references in runtime assemblies.

## 6. Architecture decision gates

The following are intentionally OPEN and must be decided before their implementation milestone:
- networking framework/protocol;
- server runtime and hosting platform;
- world chunking and terrain representation;
- simulation tick model and scheduling;
- persistence technology and save format;
- AI API use cases/provider/fallback;
- multiplayer player count and target population scale;
- content streaming strategy;
- procedural vs authored world proportions.

## 7. Core architectural invariants

- A species definition is not an individual.
- A visual GameObject is not the authoritative Pokémon entity.
- A client request is not an authoritative state change.
- A resource visual is not necessarily a simulated resource instance.
- A biome definition is not a generated biome instance.
- A Blender source file is not a Unity runtime asset.
- A Unity Editor tool is not a runtime service.
