# Data Architecture

## 1. Data categories

1. **Static definitions:** species, abilities, resources, biomes, items.
2. **Runtime instances:** individual Pokémon, resource patches, world regions, sessions.
3. **Configuration:** tuning, generation parameters, quality settings.
4. **Transient runtime values:** local perception, short-lived action progress, temporary effects.
5. **Presentation data:** model, material, animation, VFX, audio references.
6. **Persistence data:** authoritative fields required to restore a session or individual.

## 2. Species vs individual

Species data describes what members of a species generally are and what rules/profile they use.

Individual data describes one particular Pokémon. Individual size is a per-individual factor, not a new species or new model. Ordinary size variation should reuse a base visual asset.

Sex should not automatically imply a size difference. Visual dimorphism should be species-specific and represented only when required by the selected design/canon reference. Do not assume every species has visible male/female differences.

## 3. Size and weight

- Store a normalized size factor separately from species identity.
- Apply visual scaling to a dedicated `VisualRoot` child, not the stable gameplay/network root.
- Define whether collider dimensions, interaction distances, navigation, camera framing and animation presentation respond to size.
- Do not infer biologically accurate weight solely from scale unless that is an explicit model. Geometric volume scales approximately with scale factor cubed, but game weight/body condition may use separate rules.
- Clamp size to species-specific valid bounds.

## 4. Data ownership

Each mutable field must have one authoritative owner. Other systems may hold read-only views or derived caches.

Examples:
- hunger value: Pokémon physiology/needs domain;
- resource quantity: Ecology domain;
- world position: movement/world authority;
- attack cooldown/result: Combat domain;
- visual scale: derived presentation from individual size data;
- animation state: presentation, derived from authoritative action/state.

## 5. Serialization

Every persisted schema should define:
- stable IDs;
- schema version;
- units;
- optional/default fields;
- migration strategy;
- invalid-data handling;
- compatibility policy.

Avoid serializing Unity object references as the only durable identity. Prefer stable IDs and resolvable asset catalogs.

## 6. Data-driven authoring

ScriptableObjects are suitable for Unity-authored definitions and tuning. JSON is useful for external generation, interchange, bulk data and tools. Runtime instances should use explicit runtime models, not treat ScriptableObjects as mutable per-individual storage.

Do not choose one serialization format for every purpose. Select per use case and document it.

## 7. Population configuration versus species definition

Initial spawn count and per-species population cap are scenario/population configuration, not biological species facts. They live in `PokemonPopulationProfile` assets under `Assets/Project/Content/Population/Profiles` and reference a `PokemonDefinition`. The population service consumes these profiles; `PokemonDefinition` remains the species profile. These Unity-authored scenario settings are intentionally not generated from species JSON unless a future canonical population-scenario schema is approved.
