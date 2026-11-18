# Coding Standards

## General

- Prefer clear, explicit names and small cohesive classes.
- Separate domain rules from Unity presentation and infrastructure.
- Avoid god objects and hidden global mutable state.
- Keep public interfaces narrow.
- Validate external data and user/network input.
- Use units in names or types where ambiguity is possible.
- Document non-obvious invariants and trade-offs, not every line.

## C# / Unity

- Use `MonoBehaviour` for Unity lifecycle/presentation/integration where appropriate, not as the default home for every rule.
- Prefer plain C# domain classes for testable logic when practical.
- Avoid allocations in high-frequency loops where profiling shows impact.
- Cache component/reference lookups when appropriate.
- Avoid `FindObjectOfType`/scene-wide searches as a routine runtime architecture.
- Use ScriptableObjects for definitions/configuration, not mutable per-individual runtime state.
- Keep `UnityEditor` code in Editor-only assemblies/folders.
- Make generators idempotent and deterministic when possible.
- Handle missing references with clear diagnostics.


## Common Unity/C# compile errors to prevent

### Type or namespace cannot be found (`CS0246`)

Before committing or delivering a script that references a type declared elsewhere:

1. **Locate the authoritative declaration** of the type in the current project. Do not infer its namespace from its folder path or from a similarly named type.
2. Copy the exact namespace from the declaration and either add the matching `using` directive or use the fully qualified type name.
3. Check the type's accessibility (`public` when another assembly must consume it).
4. Check assembly boundaries: an Editor assembly can reference Runtime assemblies, but Runtime code must never reference Editor-only code. If the type is in another assembly definition (`.asmdef`), verify that the consuming assembly has the required reference and that the dependency direction remains valid.
5. Check the exact spelling and casing of the type and namespace.
6. Compile the affected scripts in Unity before packaging the change. A static review or a successful compile of a different assembly is not sufficient.

**Required pre-delivery reference check**
- [ ] Every newly referenced project type has been located in its actual source declaration.
- [ ] Its exact namespace and accessibility have been verified.
- [ ] The consuming assembly can reference the declaring assembly.
- [ ] No Runtime-to-Editor dependency has been introduced.
- [ ] Unity compilation has been run after the change, and all new compiler errors are resolved.

**Observed example (WORLD-002 visual integration):** `SphericalPlanetGenerationStudioWindow.cs` referenced `CubeSphereFace`, but the Editor script did not resolve the type. Unity reported `CS0246` at line 177. The correct response is to inspect the declaration of `CubeSphereFace` and verify its namespace and assembly visibility, then fix the import/reference. Do not guess a namespace based only on the `Spherical` folder name.

Treat repeated `CS0246` errors as a process failure: inspect the declaration and assembly graph before adding more imports or moving files.

## Blender Python

- Reuse `_core` and package modules.
- Keep per-asset generators under the asset's source folder.
- Centralize output paths/configuration.
- Avoid writing to drive roots or hardcoded user-specific absolute paths.
- Validate scene/collections before export.
- Report errors and generated outputs.

## Comments and documentation

Explain why a non-obvious rule exists. Update docs when public behaviour, data schemas, interfaces, architecture or workflows change.

## Dependency policy

Before adding a package:
- identify the need;
- compare built-in alternatives;
- check Unity version/platform compatibility;
- consider licensing, security, maintenance and build size;
- record the choice in the decision register if it is architectural.


## Domain decomposition and Unity adapters

- A `MonoBehaviour` that coordinates an individual must not become the owner of needs, lifecycle, every behaviour and movement implementation at once.
- Put each independently changing rule/process in a cohesive domain class (needs, schedule, mortality, reproduction, action behaviour).
- Cross-domain communication uses narrow interfaces/events/queries. A composition root wires concrete implementations.
- Domain behaviour sends movement intent through a contract; only the World movement adapter mutates the Transform.
- Resource instances own authoritative quantities and lifecycle. Presentation components observe state/events and own visual instantiation.
- Do not use static global gameplay event buses. Publish instance/domain events through an explicit owner/coordinator.
