# Documentation Index

## Governance
- `PROJECT_CHARTER.md` — vision, constraints, principles.
- `WORKING_AGREEMENTS.md` — collaboration and code-change rules.
- `GIT_WORKFLOW.md` — Conventional Commits, agent attribution, two milestones, QA and owner approval.
- `PROJECT_ALIGNMENT_PLAN.md` — scope and deliverables for Holy Grail integration and repository audit.
- `DECISION_REGISTER.md` — approved and pending decisions.
- `CHAT_TEAM_STRUCTURE.md` — specialist conversation roles.
- `NEW_CONCEPT_WORKFLOW.md` — process for adding undocumented concepts.
- `CHANGE_PROPOSAL_TEMPLATE.md` — cross-domain RFC template.

## Architecture
- `SYSTEM_ARCHITECTURE.md` — high-level modules and dependencies.
- `MODULE_BOUNDARIES.md` — ownership and dependency direction.
- `INTERFACE_CONTRACTS.md` — cross-domain contracts.
- `DATA_ARCHITECTURE.md` — species, individual, world and resource data.
- `NETWORK_ARCHITECTURE.md` — authority and replication principles.
- `WORLD_ARCHITECTURE.md` — world/session/terrain/biome design.
- `SPHERICAL_WORLD_ARCHITECTURE_PROPOSAL.md` — proposed Cube-Sphere topology, subtle-curvature requirement, adaptive terrain tiles, spherical fields, gravity and validation phases.
- `UNITY_ARCHITECTURE.md` — Unity project and runtime/editor split.

## Domains
- `POKEMON_SIMULATION.md`
- `ECOLOGY_SIMULATION.md`
- `WORLD_AND_BIOMES.md`
- `COMBAT_AND_ABILITIES.md`
- `ART_AND_ASSET_PIPELINE.md`
- `TERRAIN_AND_RENDERING.md`
- `AUDIO_AND_VFX.md`
- `AI_API_INTEGRATION.md`
- `PERSISTENCE_AND_SESSIONS.md`

## Engineering
- `CODING_STANDARDS.md`
- `TESTING_AND_VALIDATION.md`
- `PERFORMANCE_GUIDELINES.md`
- `BUILD_AND_DEPLOYMENT.md`
- `SECURITY_AND_API_GUIDELINES.md`
- `ROADMAP_AND_VERTICAL_SLICE.md`

## Audit
- `../06_AUDIT/REPOSITORY_AUDIT_REPORT.md` — static audit of the supplied Unity repository snapshot.
- `../06_AUDIT/REPOSITORY_ACTION_REGISTER.md` — prioritized findings and recommended follow-up work.
- `../06_AUDIT/CODE_FUNCTION_OWNERSHIP_INVENTORY.md` — inventory of all 59 C# files and 196 declared methods after the refactor.
- `../../CODE_ARCHITECTURE_COMPLIANCE_REPORT.md` — implementation results, static checks and remaining Unity validation gaps.

## AI instructions
- `.github/copilot-instructions.md`
- `.github/instructions/*.instructions.md`

## Work packages
- `03_ENGINEERING/WORK_PACKAGES/WORLD-001_SPHERICAL_WORLD_FOUNDATION.md` — first spherical-world foundation prototype plan and implementation progress.
- `03_ENGINEERING/WORK_PACKAGES/WORLD-001_IMPLEMENTATION_REPORT.md` — initial code slice, validation evidence and remaining Unity checks.
- `03_ENGINEERING/WORK_PACKAGES/WORLD-001_IMPLEMENTATION_PROGRESS_01.md` — visible runtime demo, camera and temporary surface movement implementation progress.

## WORLD-002
- `03_ENGINEERING/WORK_PACKAGES/WORLD-002_ASSET_READINESS_CHECKLIST.md` — snapshot-based inventory of existing environment models, missing tree/material/prefab requirements and preparation/acceptance checklist.
- `03_ENGINEERING/WORK_PACKAGES/WORLD-002_SPHERICAL_GEOGRAPHIC_FIELDS.md` — approved/pushed work package for spherical climate, water, moisture, slope, vegetation suitability, biomes, settlement suitability, map exports, hybrid vegetation preview and required-asset readiness inventory.
- `03_ENGINEERING/WORK_PACKAGES/WORLD-002_IMPLEMENTATION_PROGRESS_02.md` — ocean coverage and four-biome implementation status, limitations and owner validation steps.
- `03_ENGINEERING/WORK_PACKAGES/WORLD-003_PLAYABLE_BIOME_INSPECTION.md` — temporary Play Mode scene for inspecting the generated spherical biomes at player height.
- `07_Testing/Work_Packages/WORLD-002_OCEAN_BIOME_TEST_PLAN.md` — automated and manual acceptance checks for ocean coverage, water distance and biome classification.

## WORLD-001 amendments
- `03_ENGINEERING/WORK_PACKAGES/WORLD-001_LAYERED_PLANET_STUDIO_PROGRESS_01.md` — first implementation slice of the layered generation studio and dual-view preview.
- `03_ENGINEERING/WORK_PACKAGES/WORLD-001_LAYERED_PLANET_STUDIO_AMENDMENT.md` — approved scope amendment for the non-destructive base-sphere + three-layer generation workflow, independent thematic maps, and spherical/planisphere inspection views.

## WORLD-002 asset planning

| `WORLD-002_EXHAUSTIVE_VISUAL_ASSET_CATALOG.md` | Exhaustive scoped visual asset inventory, Blender batch set and owner approval register for WORLD-002. |
\n\n## WORLD-010\n- `03_ENGINEERING/WORK_PACKAGES/WORLD-010A_BIOME_TEXTURE_RENDERING_AND_PLAINS_FOREST.md` — integrate the existing spherical biome-weight renderer and world-space triplanar shader; author initial Plains and Forest detail textures.\n