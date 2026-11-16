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
