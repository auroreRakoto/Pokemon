---
applyTo: "**/*.cs"
---
# Unity C# Instructions

- Follow the project architecture and module boundaries.
- Keep pure domain logic testable and separate from MonoBehaviour presentation where practical.
- Do not reference `UnityEditor` from runtime code.
- Use ScriptableObjects for definitions/configuration, not per-individual mutable state.
- Prefer script-driven and idempotent setup tools.
- Validate inputs and handle missing references with useful diagnostics.
- Do not use scene-wide object searches as routine architecture.
- Respect server authority for shared state.
- Inspect callers/tests before changing public APIs or serialized fields.
- Update domain docs/contracts/tests when behaviour or data changes.
