# Roadmap and Vertical Slice

This is a sequencing guide, not a promise of dates.

## Phase 0 — Foundations
- repository and documentation;
- Unity/Blender version and package baseline;
- script-driven asset/import tooling skeleton;
- coding/test conventions;
- architecture decisions needed for prototype.

## Phase 1 — Visual and world prototype
- one small test area;
- one terrain approach prototype;
- a small material/texture library;
- one biome;
- scripted vegetation/object placement;
- asset validation.

## Phase 2 — Pokémon vertical slice
- one species (Chenipotte is a candidate);
- individual data model;
- base visual, rig/animations as required;
- size factor via VisualRoot;
- movement and simple perception;
- needs and behaviour loop.

## Phase 3 — Ecology interaction
- one resource type;
- resource instance lifecycle;
- forage/approach/consume loop;
- rest/shelter behaviour;
- observable debug tools.

## Phase 4 — Multi-species ecosystem
- add Mystherbe and Roucool as candidate species;
- define distinct species profiles;
- add food-web/resource competition rules;
- validate interactions and performance.

## Phase 5 — Multiplayer/server foundation
- dedicated server bootstrap;
- session lifecycle;
- authoritative state and client commands;
- replication of essential state;
- persistence prototype.

## Phase 6 — Combat and player experience
- real-time combat slice;
- ability data and validation;
- animation/VFX/audio integration;
- player UI/interactions.

## Phase 7 — Expansion
- more biomes/resources/species;
- reproduction, aging and death;
- weather/seasons and deeper ecology;
- richer world traces and habitat systems;
- optimization and deployment hardening.

Reorder based on technical dependencies and risk. Do not build broad content before proving the core loop.


## World authoring — configurable planet profiles
- Select a named JSON world profile in Planet Studio.
- Tune and save custom profiles without changing the shared spherical generation pipeline.
- Validate small and large radii and ocean targets below and above 50%.
