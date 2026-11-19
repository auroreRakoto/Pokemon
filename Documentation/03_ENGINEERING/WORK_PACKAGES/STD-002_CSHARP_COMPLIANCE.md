# STD-002 — C# Code Compliance Refactor

- **Status:** PROPOSED
- **Owner module:** Cross-domain; implementation ownership assigned per file/domain
- **Priority:** P1
- **Dependencies:** STD-001 plan committed and pushed; owner approval of this work package; baseline compile blockers triaged
- **Target milestone:** C# codebase conforms to Pokémon v2 Code Standard 1.0

## 1. Goal

Bring all project-authored C# source files under `Assets/Project` into compliance with the 10-functions-per-file, 50-logical-code-lines-per-function and one-short-description-per-function rules, without changing gameplay behaviour or public contracts.

## 2. Scope

### Included
- Audit and refactor all 37 C# source files in the supplied snapshot.
- Split files/classes only along coherent responsibility boundaries.
- Extract long methods into meaningful private methods or cohesive collaborators.
- Add concise descriptions above every declared function.
- Preserve existing public APIs unless a separately approved contract change is required.
- Preserve Unity serialization and asset references.

### Explicitly excluded
- Fixing unrelated gameplay defects or changing simulation behaviour.
- Resolving the duplicate `WorldDefinition` declaration/GUID issue unless a dedicated migration plan is approved.
- Implementing `EntityId` identity architecture.
- Adding `.asmdef`, packages, frameworks or test infrastructure.
- Deleting assets, scripts or data.
- Reordering roadmap milestones or changing networking/server authority.

## 3. Initial high-priority files

Static scan flags include:

- `Runtime/Pokemon/Behaviour/PokemonAgent.cs` — 18 detected functions; `UpdateSeekingFood` and `UpdateReproduction` exceed 50 lines.
- `Runtime/Infrastructure/Diagnostics/EcosystemLogger.cs` — 16 functions; `CreateSession` exceeds 50 lines.
- `Editor/Tools/AssetSync/BlenderEnvironmentSync.cs` — 15 functions; `Execute` exceeds 50 lines.
- `Editor/DataPipeline/JsonToScriptableObjectProjection.cs` — 12 functions.
- `Runtime/Ecology/Vegetation/BiomeEcologyGenerator.cs` — `GenerateHumidity` exceeds 50 lines.
- `Runtime/Ecology/Vegetation/ResourceManager.cs` — `FindNearestFood` exceeds 50 lines.
- `Editor/ContentCatalogBuilder.cs` — `BuildOrUpdateCatalog` and `ValidateCatalog` exceed 50 lines.
- `Runtime/World/Placement/GrassVegetationSpawner.cs` — `Initialize` exceeds 50 lines.
- `Runtime/Ecology/Telemetry/EcosystemDataCollector.cs` — two collection methods exceed 50 lines.
- `Runtime/World/Generation/WorldBiomeGenerator.cs` — `Generate` exceeds 50 lines.

The full file inventory and final counts must be refreshed against the owner's current branch before implementation; the archive contains no Git metadata.

## 4. Ownership and architecture

- Keep domain rules in their existing owning modules.
- Do not make Runtime depend on UnityEditor.
- Keep Editor-only asset operations in Editor code.
- Preserve server-authoritative domain intent and existing cross-domain contracts.
- Do not create cyclic dependencies or global mutable state.
- Do not introduce new packages or assembly boundaries in this work package.

## 5. Validation plan

- Static compliance report: function count per file, overlength methods, missing descriptions.
- Compare public signatures before/after.
- Inspect Unity serialized references and `.meta`/GUID impact for any file move/rename.
- Run available static checks and tests after each coherent batch.
- Owner runs Unity compilation and relevant Editor/PlayMode checks.
- QA reviews behaviour-preservation evidence and regression results.

The archive currently contains no C# tests or `.asmdef` files. Test infrastructure is not silently added here; any required test infrastructure needs its own approved scope.

## 6. Acceptance criteria

- [ ] No project-authored C# file exceeds 10 declared functions, unless an owner-approved exception is recorded.
- [ ] No function exceeds 50 logical code lines, unless an owner-approved exception is recorded.
- [ ] Every function has a concise description.
- [ ] Public signatures and serialized references are preserved or covered by an approved migration.
- [ ] Unity compilation is run by the owner and its exact result is recorded.
- [ ] Relevant tests/QA are recorded as PASS, FAIL, BLOCKED or NOT RUN.
- [ ] No gameplay behaviour change is introduced unintentionally.

## 7. Risks and rollback

- Method extraction can alter execution order, Unity lifecycle timing, event ordering or serialized field behaviour.
- File moves can affect `.meta` GUIDs and asset references.
- Keep changes in small domain batches and retain the pre-refactor snapshot for rollback.
