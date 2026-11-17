# Module Boundaries and Ownership

| Module | Owns | Does not own | Main collaborators |
|---|---|---|---|
| Pokemon | Individual identity, species profile use, lifecycle, needs, behaviour, population rules | Resource regeneration, terrain rendering, transport | Ecology contracts, World contracts, Combat, Server |
| Ecology | Resource lifecycle, food-web rules, habitat suitability, ecological resource queries | Animation, player UI, mesh generation, species biology | World contracts/data, Data |
| World | Spatial world, terrain, biome topology, environment and movement queries | Species biology, resource lifecycle, combat resolution | Data, Art, Server |
| Combat | Attack execution, damage/effects, combat state | Daily needs, resource growth | Pokemon, Art, Server |
| Art | Model/rig/material/animation/VFX/audio content standards | Gameplay authority and ecological rules | All visual consumers |
| Networking | Transport, replication, authority, session connectivity | Domain-specific rules | All runtime domains |
| Persistence | Save/load lifecycle and serialization | Deciding gameplay rules | Server, Data, all domains |
| Tools | Editor automation, Blender export discovery, manifest/path validation, FBX transfer/replacement, ModelImporter configuration, validation tooling and diagnostics | Reimplementing domain logic, generating meshes, authoring materials/prefabs, choosing destinations by guesswork | Art, World, Data |
| QA | Tests, profiling, acceptance evidence | Unapproved feature design | All domains |
| Composition | Scene bootstrap, dependency wiring, cross-domain orchestration | Domain rules, resource ownership, presentation decisions | World, Ecology, Pokemon, Infrastructure, Presentation |

## Dependency rules

- Domain modules communicate through narrow interfaces, events, commands, or queries.
- Avoid cyclic dependencies. If A needs B and B needs A, introduce a contract or coordinator owned by an appropriate layer.
- Do not let UI call deep domain internals.
- Do not let rendering code mutate authoritative simulation state.
- Do not let editor tooling become a required runtime dependency.
- Shared contracts must remain small and stable.

## Suggested assembly boundaries

These are conceptual; actual `.asmdef` layout is an implementation task:
- `Pokemon.Shared`
- `Pokemon.Domain`
- `Pokemon.World`
- `Pokemon.Ecology`
- `Pokemon.Combat`
- `Pokemon.Client`
- `Pokemon.Server`
- `Pokemon.Editor`
- `Pokemon.Tests`

Do not create all assemblies blindly. Introduce them when dependencies and compile times justify them.


## Supporting runtime layers

The domain table above is complemented by supporting layers that do not take ownership of gameplay rules:

| Layer | Owns | Does not own |
|---|---|---|
| Data | Runtime data contracts, loading, schema/projection adapters | Domain decisions or mutable gameplay authority |
| Infrastructure | Content lookup adapters, diagnostics, cross-domain telemetry | Pokémon/ecology/world rules |
| Presentation | UI, visual rendering, local controls and read-only views | Authoritative simulation state or domain validation |
| Editor | AssetDatabase, authoring menus, import/export and validation tools | Runtime services or required runtime dependencies |
| Composition | Runtime startup pipelines and explicit service wiring | Domain decisions and ownership of domain state |

These layers may depend on narrow domain-facing APIs. Domain rules must not depend on Presentation, Editor, or infrastructure implementations.

## Blender FBX synchronizer implementation

The initial TOOLS implementation is `Assets/Project/Editor/Tools/Blender/BlenderFbxSync.cs`. It is Editor-only, scans the configured Blender export root on a three-second Editor update interval, waits for stable FBX/manifest timestamps, validates the explicit manifest destination, performs a staged replacement with rollback on import failure, applies a centralized `ModelImporter` profile, and reports results in the Unity Console. It never edits `.meta` files directly. Test-only manifests may target `_ART_TEST`; production imports remain gated by per-profile round-trip validation. See `Documentation/02_DOMAINS/ART_AND_ASSET_PIPELINE.md` and the synchronizer README for operational details.
