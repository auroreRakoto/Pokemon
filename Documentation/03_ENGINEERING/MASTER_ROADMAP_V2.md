# Master Roadmap v2 — From Prototype to First Complete Release

- **Status:** PROPOSED — owner approval required
- **Version:** 2.0
- **Date:** 2026-11-19
- **Purpose:** fixed sequencing and scope baseline. This roadmap defines order and exit criteria, not calendar promises.

## 1. Product direction

The game combines:
- a large explorable spherical world;
- four initial land biomes (Plains, Forest, Desert, Mountains);
- a visible autonomous ecosystem;
- the ability to act as a trainer;
- Pokémon with individual identity, needs and behaviour;
- long-distance exploration.

The project is currently developed for the owner's PC. The first playable milestone must be achieved before expanding into broad simulation systems.

## 2. Release ladder

| Milestone | Meaning | Player value |
|---|---|---|
| P0 — Technical prototype | Core world generation and movement are stable | Developer can inspect and traverse a generated planet |
| P1 — First playable | A coherent small gameplay loop exists in the world | Player can explore a region, see biomes/environment and encounter a Pokémon |
| P2 — Ecosystem alpha | A small autonomous ecosystem behaves coherently | Player can observe Pokémon/resource interactions over time |
| P3 — Trainer alpha | Player can interact with Pokémon as a trainer | Capture/party/basic training loop is playable |
| P4 — Feature-complete candidate | Core world, ecology, trainer and combat loops are connected | A complete representative game loop exists |
| V1.0 — First correct release | Core systems are reliable, readable, saved and balanced enough for a full play session | A coherent, stable first release rather than a feature demo |

A milestone is not achieved by source-code presence. It requires its exit criteria and evidence.

## 3. Phase 0 — Governance and engineering baseline

**Goal:** establish a reliable way to build, inspect, test and integrate every module without discarding already approved work or decisions.

Deliverables:
- approve the development constitution and architecture baseline;
- reconcile duplicate/contradictory documentation;
- one canonical testing standard;
- one feature/work-package template;
- consistent Editor tool/menu conventions;
- module preview/test framework;
- baseline Unity compile/test procedure;
- current repository inventory and discrepancy register;
- repeatable build/validation entry points.

Exit:
- every active module has an owner and dependency description;
- every visual module has an independent preview path or an approved reason why not;
- every active module has at least one automated test path;
- no unknown project-wide compile failure;
- development queue agrees with actual repository evidence.

Do not reset or restart already approved work. The current project context identifies WORLD-011 as an approved work package, beginning with WORLD-011-A (asset/prefab readiness audit). Keep that work visible in the active queue while the engineering-standardization tasks are planned around it. Any change to this order requires an explicit owner decision.

## 4. Phase 1 — Spherical world foundation (P0)

**Goal:** generate a planet with reproducible geography and a reliable player inspection experience.

Sequence:
1. topology and cube-sphere mapping;
2. height-field and geographic data contracts;
3. ocean/water classification and target coverage;
4. four-biome classification;
5. surface query and local tangent frame;
6. terrain/surface mesh generation;
7. biome rendering and water presentation;
8. planet profiles/configuration;
9. player spawn and spherical locomotion;
10. camera and ground probe;
11. world diagnostics and inspection overlay.

Validation:
- fixed-seed determinism;
- face seam and pole tests;
- land/water correctness;
- biome coverage and classification;
- player spawn on valid land;
- walking, slope, water boundary, falling/landing and camera behaviour;
- second-seed regression;
- generated-world performance baseline on owner's PC.

Exit:
- player can reliably explore the spherical world;
- all four land biomes are identifiable;
- water and terrain boundaries are coherent;
- no unresolved critical integration or compile issue;
- scene can be regenerated repeatedly without duplicate objects.

## 5. Phase 2 — Environment and world readability (P0 to P1)

**Goal:** make the world readable and visually convincing at player scale.

Sequence:
1. asset inventory and prefab readiness;
2. prefab pivot/scale/bounds/orientation validation;
3. biome-specific environment placement configuration;
4. deterministic spherical candidate sampling;
5. placement orientation using local radial up;
6. density/spacing/exclusion rules;
7. ground contact and visual scale;
8. independent placement preview;
9. integration with the generated planet;
10. performance profiling and only then batching/instancing decisions.

Initial placement scope:
- Plains and Forest first, as approved in WORLD-011.
- Desert and Mountains only after their assets/rules are specified.
- Do not claim full forest canopy until suitable tree assets are available.
- Do not add colliders to every plant by default.

Exit:
- the module preview proves placement without the main playable scene;
- integration uses the authoritative geographic field set;
- same seed/settings reproduce placement;
- no placement on prohibited water classes;
- density is useful around the player and across representative terrain;
- regeneration/clear lifecycle creates no duplicates;
- generation time and instance count are recorded.

## 6. Phase 3 — First Pokémon encounter (P1)

**Goal:** prove that a Pokémon can exist visibly and correctly in the world.

Sequence:
1. select one species and approved model/placeholder;
2. species definition and individual identity;
3. spawn suitability query;
4. deterministic spawn location;
5. spherical ground alignment and scale;
6. minimal idle/locomotion presentation;
7. debug identity/state display;
8. isolated spawn preview;
9. integration in the playable scene.

Explicitly out of scope for this phase:
- complete AI;
- capture;
- battle;
- reproduction;
- multi-species population;
- persistence;
- network replication.

Exit:
- one Pokémon appears reliably on valid land;
- placement and orientation are correct;
- individual identity is stable;
- visual and domain state are separated;
- tests cover spawn eligibility and placement.

## 7. Phase 4 — First playable loop (P1)

**Goal:** a short, coherent experience rather than a collection of demos.

Required scope:
- enter a generated planet;
- spawn at a valid location;
- walk and explore;
- see distinct biomes and environment;
- encounter at least one Pokémon;
- inspect basic Pokémon information;
- basic player interaction appropriate to the selected first species;
- return/restart/regenerate without broken state.

UX:
- minimal readable HUD;
- clear controls/help;
- pause and resume;
- restart/regenerate workflow for development;
- no debug-only controls presented as finished gameplay.

Exit:
- a fresh session can be started and completed without Editor intervention;
- no manual object dragging is required to make the loop work;
- critical gameplay path passes PlayMode/manual QA;
- known limitations are explicit.

## 8. Phase 5 — Autonomous ecosystem alpha (P2)

**Goal:** a small ecosystem behaves plausibly and can be observed.

Sequence:
1. resource definitions and instances;
2. resource availability/consumption contract;
3. one Pokémon needs model;
4. perception and target selection;
5. action/behaviour state machine or approved equivalent;
6. movement intent through World contract;
7. foraging/consumption loop;
8. resting/shelter behaviour;
9. time progression and simulation scheduling;
10. diagnostic overlays and event log;
11. deterministic test fixtures;
12. multi-individual interaction;
13. additional species only after one-species loop is robust.

Exit:
- an individual selects and executes valid actions;
- resource consumption is authoritative and cannot double-consume;
- missing targets and invalid paths recover safely;
- time/needs/state transitions are testable;
- observation tools explain why an action was selected;
- performance is measured at a defined population count.

## 9. Phase 6 — Trainer gameplay alpha (P3)

**Goal:** the player has a meaningful trainer role.

Sequence:
1. player interaction/input command contracts;
2. encounter interaction;
3. capture eligibility and outcome rules;
4. party/roster ownership;
5. storage/party limits if approved;
6. basic training/progression;
7. item/inventory model;
8. UI projections;
9. save/load of trainer state;
10. regression and usability tests.

Exit:
- capture and party state transitions are reliable;
- player UI reflects authoritative state;
- save/load round-trip works for the agreed trainer data;
- invalid actions are rejected predictably.

## 10. Phase 7 — Combat and abilities (P4)

**Goal:** one complete, understandable combat loop.

Sequence:
1. combat domain model and contracts;
2. turn/real-time combat decision (owner approval required);
3. ability definitions and validation;
4. targeting/range/line-of-sight rules;
5. damage, status and effect resolution;
6. battle state lifecycle;
7. animation/VFX/audio adapters;
8. combat UI;
9. AI opponent behaviour;
10. edge-case and balance tests.

Do not choose turn-based or real-time mechanics implicitly. Resolve this product decision before implementation.

Exit:
- battle can start, progress and finish;
- invalid actions are rejected;
- outcomes are deterministic where required;
- presentation cannot mutate combat authority.

## 11. Phase 8 — World depth and content expansion

**Goal:** expand only proven systems.

Potential sequence:
- full asset coverage for all four biomes;
- more species and population profiles;
- reproduction, aging and mortality;
- richer food webs and competition;
- day/night, weather and seasons;
- habitat/shelter systems;
- additional world points of interest;
- audio/VFX and polish;
- map and exploration UX.

Each item requires its own approved work package and must not be treated as automatic scope.

## 12. Phase 9 — Reliability and V1.0 hardening

**Goal:** make the first complete version robust for sustained play.

Required areas:
- save/load and schema migration;
- deterministic world/session identity;
- recovery from invalid/missing content;
- settings and accessibility basics;
- onboarding and controls;
- UI consistency;
- profiling and performance budgets measured on owner's PC;
- memory/resource lifecycle;
- build reproducibility;
- regression suite;
- backup/recovery and error reporting;
- content validation;
- full-session QA.

Exit:
- a representative long play session completes without critical failure;
- save/load and restart are reliable;
- all core loops are connected;
- no known critical/high-severity issue remains without explicit owner acceptance;
- release build is reproducible and tested.

## 13. Deferred architecture topics

These are not prerequisites for P0/P1:
- multiplayer;
- dedicated server;
- network replication;
- external AI APIs;
- cloud services;
- large-scale streaming infrastructure.

They require separate proposals when product scope and measured needs justify them.

## 14. Roadmap change rules

- Do not reorder phases because a feature is interesting or easy.
- A dependency blocker may justify a local reorder; record why and what remains blocked.
- New feature ideas go to a backlog, not directly into the active phase.
- A phase exit requires evidence, not subjective confidence.
- The development queue contains only the next actionable work packages, not the full product vision.
- The owner approves any change to milestone scope or release criteria.
