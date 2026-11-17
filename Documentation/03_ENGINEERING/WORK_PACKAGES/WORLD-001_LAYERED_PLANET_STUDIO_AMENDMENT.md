# WORLD-001 Amendment — Layered Planet Generation Studio

- **Work package:** WORLD-001
- **Amendment:** Layered generation and dual-view inspection
- **Status:** APPROVED — project owner confirmed approval and plan milestone push (2026-11-17)
- **Date:** 2026-11-17
- **Parent plan:** `WORLD-001_SPHERICAL_WORLD_FOUNDATION.md`

## 1. Purpose

The first prototype must be an iterative planet-generation workspace, not a one-shot sphere generator. The user must be able to inspect a smooth base sphere, apply terrain-generation layers one at a time, compare intermediate results, disable layers and return to an earlier stage without losing later-layer settings.

The same generated planet data must be inspectable in both a 3D spherical view and a 2D planisphere view. A view change is presentation-only and must not regenerate or mutate the underlying data.

## 2. Confirmed workflow

The generation stack is ordered and non-destructive:

0. **Base sphere:** perfectly smooth reference sphere; no elevation displacement.
1. **Layer 1 — macro relief:** broad landform structure such as large plains, plateaus, basins, valleys and mountain masses.
2. **Layer 2 — regional relief:** hills, ridges, secondary valleys and regional variations.
3. **Layer 3 — fine details:** small-scale surface variation constrained by mesh/sample resolution and anti-aliasing requirements.

Each terrain layer must have:
- an independent enabled/disabled state;
- its own settings and deterministic seed derivation;
- an inspectable contribution/output;
- a documented combination rule;
- the ability to be regenerated without silently changing other layers.

The final height field is composed from the base radius plus the contributions of enabled terrain layers. Disabling Layer 2 or 3 must reproduce the earlier-stage result using the same seed and settings. Hiding a layer in the preview must not erase its settings or generated source data.

## 3. Independent thematic maps

The studio must also expose independent data maps. Initial map targets:
- altitude / final height;
- temperature;
- humidity/moisture (derived according to the existing terrain specification, not an unrelated independent noise map);
- slope;
- water and hydrology;
- biome classification;
- settlement suitability and selected village/town/city locations.

A map selector displays one selected field at a time using a suitable grayscale or thematic colour ramp. A map visualisation is an overlay/material/presentation mode only: it must not alter terrain geometry, source values or the generation stack. Raw fields and derived fields must be distinguishable, and each map must expose its range/legend and generation metadata.

Village locations are data/markers at this prototype stage; generating complete village layouts is outside this amendment unless separately approved.

## 4. Required inspection views

### 4.1 Spherical view
- Display the generated planet as a 3D globe.
- Allow orbit, zoom and inspection of the surface.
- Support terrain-layer visibility and thematic map display.
- Preserve the same seed, field values and selected layer state when switching views.

### 4.2 Planisphere view
- Display the same spherical data in a 2D world map layout.
- Provide a stable mapping from planisphere coordinates to spherical directions and back.
- Make the selected projection explicit in the UI and metadata.
- Ensure that the planisphere is a view of the globe, not a separately generated flat world.
- Document and test seam/wrap behaviour at the map boundary and distortion characteristics.

An equirectangular projection is an initial implementation candidate because it is straightforward to inspect and map to latitude/longitude. It is not locked as the final projection; compare it with alternatives if polar distortion or readability becomes a problem. The projection must not affect generation or world-space measurements.

## 5. Minimal studio controls

The first usable tool should expose:
- world seed and prototype radius;
- base sphere visibility;
- per-layer enable/disable controls and each layer's settings foldout;
- a selected generation stage / preview stage (Base, Layer 1, Layer 2, Layer 3, Final);
- a thematic map selector;
- view selector: Spherical / Planisphere;
- regenerate selected stage / regenerate all;
- reset view and restore default layer visibility;
- clear preview without deleting source settings/configuration.

The UI may be a dedicated Editor window first. The underlying generation data/core must remain runtime-safe and independent from Editor APIs. Play Mode support can use the same core and data contracts; do not fork generation logic for the Editor.

## 6. Architecture and data contracts

Keep these responsibilities separate:
- **Planet definition:** radius, seed, generator version and spherical coordinate conventions.
- **Terrain layer stack:** ordered layer descriptors, settings, deterministic seed IDs, enabled state and per-layer contribution data.
- **Thematic fields:** independently addressable raw/derived map data.
- **Composition:** deterministic combination of enabled terrain contributions into final height.
- **Presentation:** spherical mesh view, planisphere view and thematic colourisation.
- **Inspection UI:** controls state and selected view/map; it does not own or mutate generation algorithms.

Generated data should be cached by a stable generation key derived from seed, generator version and complete relevant settings. Changing a presentation-only property (view, palette, opacity, map selection) must not invalidate terrain generation. Changing a layer setting invalidates that layer and downstream composed outputs only; independent climate/settlement maps are invalidated only when their documented dependencies change.

The planisphere should sample the same spherical field functions / map dataset as the sphere. Do not generate a separate planar terrain map with different noise coordinates.

## 7. Exclusions

- Full procedural village/building layout and asset placement.
- Final biome art, vegetation, water rendering and production materials.
- Adaptive quadtree LOD and streaming.
- Production terrain resolution and final planet radius.
- Pokémon movement, ecosystem, combat and trainer features.
- Final projection choice beyond the prototype candidate and validation.

## 8. Acceptance criteria

1. A smooth, undisplaced sphere can be generated and displayed by itself.
2. Layer 1 can be enabled and disabled without changing the base sphere settings or seed.
3. Layers 2 and 3 can be applied incrementally; the user can return to Layer 1 and restore the same result deterministically.
4. Each layer's contribution can be inspected independently and its settings remain available when hidden.
5. The final terrain is composed from enabled layers using documented, testable rules.
6. At least altitude, temperature, moisture, slope, water, biome and settlement-suitability maps can be selected for inspection when their generation stages are implemented; unimplemented maps must be clearly marked unavailable, not fabricated.
7. Switching between spherical and planisphere views preserves the same seed and generated data; no generation rerun is required solely for view switching.
8. Known planisphere projection distortion and wrap/seam behaviour are documented and tested.
9. Thematic map display does not alter geometry or source data.
10. Repeated generation with the same seed, generator version and settings reproduces the same layer outputs and maps.
11. A reproducible Editor demo scene/window and automated tests cover layer enable/disable, deterministic recomposition, spherical/planisphere coordinate round trips and presentation/data separation.

## 9. Documentation affected

- `DECISION_REGISTER.md`
- `SPHERICAL_WORLD_ARCHITECTURE_PROPOSAL.md`
- `WORLD_ARCHITECTURE.md`
- `TERRAIN_AND_RENDERING.md`
- `WORLD-001_SPHERICAL_WORLD_FOUNDATION.md` (reference this amendment; preserve the original approved scope/history)
- `DOCUMENT_INDEX.md`
- `DOCUMENTATION_CHANGELOG.md`

## 10. Plan milestone gate

This amendment materially expands WORLD-001 from a visible spherical runtime demo into a layered planet-generation and inspection studio. Per `GIT_WORKFLOW.md`, this document is the proposed updated plan. The project owner confirmed approval and push of this amendment on the same working branch on 2026-11-17. Implementation may proceed against this approved scope.
