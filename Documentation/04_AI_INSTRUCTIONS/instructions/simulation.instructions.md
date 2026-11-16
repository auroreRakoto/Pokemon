---
applyTo: "**/*Simulation*.*,**/*Behaviour*.*,**/*Ecology*.*"
---
# Simulation Instructions

- Keep individual state distinct from species definitions.
- Keep needs, physiology, emotions, actions and locomotion as separate dimensions.
- Use explicit, testable transitions and bounded values.
- Do not assume every Pokémon species shares identical behaviour.
- Query world/ecology systems through contracts; do not reach into their internals.
- Validate consequential state changes on the authoritative server.
- Consider scheduling frequency and entity scale; do not update everything every frame by default.
- Add deterministic tests for important rules.
