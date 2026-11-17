# WORLD-002 — Spherical Geographic Fields and Environment Preview

- **Work package:** WORLD-002
- **Owning domain:** WORLD
- **Status:** APPROVED / PUSHED — owner-confirmed; asset-readiness amendment added before implementation
- **Agent:** WORLD
- **Predecessor:** WORLD-001 layered Planet Studio (QA/owner acceptance remains a separate gate)
- **Related decisions:** DEC-022, DEC-024, DEC-031, DEC-034, DEC-040, DEC-041, DEC-042

## 1. Purpose and expected result

Extend the existing spherical Planet Studio from terrain-layer inspection into a deterministic geographic-field pipeline. The owner must be able to inspect the same generated data in the spherical view and planisphere, enable individual thematic maps, understand each stage's dependencies, and export each generated field independently.

The resulting data becomes the shared source of truth for later biome materials, vegetation rendering, settlement placement, habitat suitability and route planning. Do not create separate contradictory field generators in those downstream systems.

The chosen vegetation presentation direction is **hybrid**: biome-aware ground textures/materials plus actual 3D vegetation instances (trees, shrubs, grasses and other configured categories). WORLD-002 must generate and validate vegetation suitability before visual placement. Placement must consume the suitability field and biome profiles; it must not be an unrelated random scatter pass.

## 2. Current baseline inspected

The plan is based on the latest supplied project snapshot `Pokemon-WORLD-001-layered-planet-studio-project-fix-v1.zip`.

- Unity project version: 6000.4.11f1.
- Existing spherical generation code is under `Assets/Project/Runtime/World/Spherical/`.
- The current Planet Studio core is `SphericalPlanetStudioGenerator` with persistent `SphericalPlanetStudioSettings`.
- The current Studio preview supports base sphere and three terrain relief layers, spherical and equirectangular planisphere presentation, and Terrain / Altitude / Temperature previews.
- The current temperature output is explicitly a prototype normalized field, not a climate simulation.
- Moisture, hydrology, slope, vegetation, biome, settlement suitability and route network are not implemented in this spherical Studio slice. Existing UI options marked unavailable must remain unavailable until real field generation exists.
- Existing planar generation files are present under `Assets/Project/Runtime/World/Generation/`; they are not to be silently treated as a spherical implementation or modified by this work package unless a separately approved integration change requires it.

## 3. Scope

### 3.1 Included geographic fields

1. **Height / terrain input review:** consume the existing WORLD-001 composed height field. Add only the metadata, validation or small corrections needed to make it a stable upstream input. Do not redesign the three approved relief layers in this package.
2. **Temperature:** replace the prototype preview field with a versioned, deterministic normalized field combining broad latitude/world-scale trends, a dedicated seeded Perlin component and configurable altitude cooling from the height field.
3. **Water:** generate an independently inspectable water classification field from dedicated seeded water-potential noise plus height, basin/shoreline constraints and deterministic hydrological rules. Distinguish ocean/sea, lake/pond and river where supported by the chosen prototype scale. Rivers must follow downhill flow and avoid arbitrary ridge placement or disconnected noise fragments.
4. **Slope:** derive slope from the spherical height field using seam-aware neighbouring samples and world-space distances. Define behaviour at cube-face boundaries and near poles.
5. **Distance to water:** produce a world-space nearest-water distance field on the sphere. The planar two-pass EDT described in the existing domain documentation must not be applied naïvely to an equirectangular image or individual cube faces.
6. **Moisture:** derive ecological ground-moisture from water proximity, terrain retention/slope and temperature-dependent evaporation. Do not add an independent moisture Perlin map.
7. **Vegetation suitability / potential density:** generate a dedicated seeded variation field and combine it with temperature, moisture, elevation and biome suitability. This is a field, not individual plant placement.
8. **Biome classification:** generate an independently inspectable categorical biome map from height, temperature, moisture and water constraints. Preserve the currently confirmed initial biome scope: plains, forest and desert; settlements are not biomes. Additional biome categories require a separate documented decision.
9. **Settlement suitability:** generate separate candidate-suitability outputs for villages, towns and large cities using slope/elevation, fresh-water access, flood risk, contiguous buildable area and deterministic suitability variation. This is a suitability field, not actual village layout or final placement.

### 3.2 Included inspection and presentation

- All implemented fields must be inspectable in both the spherical view and planisphere from the same underlying spherical dataset.
- Add single-map debug visualisation with configurable colour ramp/legend, normalized or categorical range, and sampled value inspection.
- Add layered thematic overlays with independent visibility and opacity/draw order where practical.
- Keep geometry and source field data unchanged when changing map selection, palette, opacity or view.
- Add independent map export at native resolution with metadata sidecars. Use precision-preserving numeric formats for continuous fields, documented integer/colour encodings for categorical fields, and lossless PNG for colour previews.
- Display unavailable stages explicitly; never show fabricated placeholder maps.

### 3.3 Included vegetation presentation direction

WORLD-002 includes a first integration-ready visual validation of the confirmed hybrid vegetation approach, after the underlying suitability and biome fields pass validation:

- biome-aware ground material/texture assignment using the shared biome and field data;
- actual 3D vegetation instances using configured prototype prefabs where suitable assets exist;
- deterministic placement from world seed, generator version, biome profile and stable spherical cell/instance identifiers;
- density, species/category, scale and exclusion rules driven by vegetation suitability, moisture, temperature, elevation, slope and water masks;
- no placement on invalid surfaces, deep water, excessive slopes or other configured exclusion areas;
- a clear distinction between the continuous vegetation-suitability map and discrete vegetation instances.

If required production vegetation prefabs or material assets are not present in the inspected project, use clearly identifiable temporary primitives/billboards for validation and record the missing assets. Do not invent final art assets or block the field pipeline on their absence.


## 3.4 Required asset inventory and readiness

This section is based on the asset files present in the supplied project snapshot used for WORLD-002 planning. It is an inventory, not an assumption that every listed model is already a production-ready Unity prefab. Re-run the inventory against the active branch before implementation if assets have changed since that snapshot.

### Existing environment assets available for the first preview

| Asset/category | Snapshot evidence | WORLD-002 use | Readiness |
|---|---|---|---|
| Short grass | `Models/Environment/Vegetation/Grass_Short/SM_Grass_Short.fbx` and Blender generator `Blender/source/environment/grass_short/` | First ground-cover instance prototype | Model present; prefab/material assignment still required |
| Tall grass | `Models/Environment/Vegetation/Grass_Tall/SM_Grass_Tall.fbx` and generator `grass_tall/` | Height/density variation in suitable biomes | Model present; prefab/material assignment still required |
| Wild grass | `Models/Environment/Vegetation/Wild_Grass/SM_Grass_Wild.fbx` and generator `wild_grass/` | Natural ground-cover variation | Model present; prefab/material assignment still required |
| Reed clump | `Models/Environment/Vegetation/Reed_Clump/SM_Reed_Clump.fbx` and generator `reed_clump/` | Wetland / water-edge preview, subject to biome rules | Model present; prefab/material assignment still required |
| Fallen log | `Models/Environment/NaturalFeatures/Fallen_Log/SM_Fallen_Log.fbx` and generator `fallen_log/` | Optional forest-floor prop, not a living plant | Model present; not a vegetation category |
| Tree stump | `Models/Environment/NaturalFeatures/Tree_Stump/SM_Tree_Stump.fbx` and generator `tree_stump/` | Optional forest-floor prop, not a tree substitute | Model present; not a vegetation category |
| Red mushroom | `Models/Environment/NaturalFeatures/Mushroom_Red/SM_Mushroom_Red.fbx` and generator `mushroom_red/` | Optional small natural feature | Model present; material/prefab still required |
| Leaf litter | `Models/Environment/Ground/Leaf_Litter/SM_Leaf_Litter.fbx` and generator `leaf_litter/` | Forest-floor detail / ground prop | Model present; not a terrain ground material |
| Small and large stone clusters | `Models/Environment/Rocks/Stone_Cluster_Small/` and `Stone_Cluster_Large/` FBX files and matching generators | Optional rock dressing / slope validation | Models present; prefab/material assignment still required |

The snapshot also contains Blender source generators for `flower_blue` and `flower_yellow`, but corresponding production FBX files were not found under `Assets/Project/Content/Models/Environment/` in that snapshot. Treat these as source generators only until a production export/import is confirmed. Files under `_ART_TEST` are test assets and must not be silently promoted or used as production references.

### Missing assets / explicit requirements

| Required asset | Why it is needed | Required for field generation? | Required for hybrid visual preview? |
|---|---|---:|---:|
| Tree models (at least 2 distinct silhouettes for a useful first forest preview) | Actual tree canopy/trunk instances; stumps do not replace trees | No | Yes, unless clearly labelled temporary tree placeholders are used |
| Biome ground materials/textures: plains/grass, forest floor, desert/sand or dry ground | Hybrid surface presentation and biome transitions | No | Yes for final-quality preview; temporary procedural debug materials may validate the pipeline |
| Water material/texture | Ocean, lake and river surface presentation | No | Yes for water rendering; a debug colour is acceptable during field validation |
| Unity prefabs for each selected vegetation/prop prototype | Stable placement unit with renderer, transform and optional LOD/collider settings | No | Yes for actual instance placement; can be composed after model review |
| Unity biome material assets and biome-to-material mapping | Apply shared biome fields to the spherical surface | No | Yes for biome-aware ground presentation |
| Optional low-detail vegetation representations (billboard/impostor or LOD variants) | Keep large-scale previews and later world rendering performant | No | Not required for the first small validation scene; required before large-scale production density |
| Optional shoreline / riverbank ground material | Improve transitions around water | No | Not required for initial field validation |

The supplied snapshot contains no imported environment `.mat`, environment texture image, or environment prefab files under the canonical `Content/Materials/Environment/`, `Content/Textures/Environment/`, and `Content/Prefabs/Environment/` roots. This does not prove that no reusable materials exist elsewhere in the project; search the full active branch before creating duplicates.

### Asset preparation tasks and ownership

1. **Audit first:** inspect the current active branch for reusable models, materials, textures, prefabs, shaders and existing LOD setups. Record exact paths and Unity GUIDs; do not duplicate assets because a folder was absent in the planning snapshot.
2. **Select a small preview set:** start with short grass, tall grass, wild grass and reeds from the existing imported models. Add at least two tree models before claiming that a forest canopy preview is visually representative. Use temporary clearly labelled primitives/billboards only if tree assets are not yet available.
3. **Prepare ground materials:** define a minimal preview material set for plains, forest floor and desert/dry ground, plus water. Reuse compatible existing shaders and project conventions. Material creation/assignment is an explicit Art/Unity content task; the FBX synchronizer must not create materials as a side effect.
4. **Compose Unity prefabs:** create stable prefabs for the chosen instance prototypes under the canonical `Assets/Project/Content/Prefabs/Environment/` hierarchy, with explicit orientation, scale, renderer, shadow and optional LOD/collider settings. Raw FBX models remain under `Models/Environment/`.
5. **Preserve pipeline contracts:** environment model exports must follow `ART_AND_ASSET_PIPELINE.md`, including canonical categories, manifests and the `generate_all.py` production/test policy. `_ART_TEST` content remains test-only. Materials, textures and prefabs follow their own content roots and must not be implicitly created by FBX import.
6. **Record missing production assets:** if tree models or biome materials are unavailable, keep field generation and map export progressing with debug presentation; report the gap and do not label the hybrid production preview complete.

### Minimum asset acceptance for WORLD-002

- At least one usable instance prefab for each vegetation category actually enabled in the preview.
- At least two distinct tree prefabs before accepting a forest-canopy preview as representative; otherwise show a labelled placeholder and record the limitation.
- Preview ground materials for plains, forest and desert/dry ground, plus a water debug/presentation material.
- All referenced assets resolve to stable project paths and GUIDs; no `_ART_TEST` asset is treated as production content.
- The vegetation renderer consumes biome/vegetation suitability fields and stable deterministic placement IDs; asset selection must not change the generated field values.
- Missing assets are listed in the implementation report with owner/action required.

## 3.5 Exhaustive WORLD-002 visual asset catalogue

The complete scoped asset list, proposed stable keys, source generators, export destinations, Unity prefabs, required materials, optional props, Blender batch registration set and owner approval register are maintained in `WORLD-002_EXHAUSTIVE_VISUAL_ASSET_CATALOG.md`. This catalogue is exhaustive for the WORLD-002 validation preview only, not for the final game. It must be reviewed and approved before production visual generation.

## 3.5 Mandatory asset approval gate before visual generation

**This is a blocking gate, not an optional checklist.** Do not start the visualisation-generation stage (biome ground materials, vegetation instances, settlement markers represented by final art, or other environment presentation assets) until the asset inventory and visual asset list have been reviewed and explicitly approved by the project owner.

The gate must be completed after the field/data pipeline is ready for visual validation and before any production visual asset generation or batch export is launched.

Required approval record:
- The exact asset list, grouped by role (ground materials, water material, vegetation models, tree canopy models, natural props, Unity prefabs, optional LOD/billboards).
- For each item: stable asset key/ID, intended biome/use, source generator path (if generated), Unity destination, current status (existing production asset / source-only / missing / temporary placeholder), owner/action, and approval status.
- Exact source/export/import paths and Unity GUIDs for existing assets; do not rely on names alone.
- Explicit distinction between production assets and `_ART_TEST` assets.
- A decision for each missing item: create through the Blender pipeline, reuse an existing asset, or accept a clearly labelled temporary placeholder for this validation only.

The owner must approve the complete list before the visual generation batch is run. Approval of the WORLD-002 technical plan does not automatically approve a later asset list or promote test assets to production.

### Blender generation-loop integration (required)

Every approved Blender-generated visual asset must be integrated into the existing canonical environment batch workflow; standalone one-off generation is not an acceptable production route.

The implementation must update and validate:
- `Blender/source/environment/<asset_key>/generator.py` as the editable source for each generated asset.
- `Blender/source/environment/generate_all.py` so the approved asset is discoverable by the batch loop and has an explicit canonical production route in `PRODUCTION_ASSETS`; update `ASSET_FILTER` when a filtered batch is used.
- The shared batch policy and generator metadata so production/test status, asset category, stable asset ID, and Unity destination are consistent.
- The generated FBX and manifest under the canonical export path, followed by the documented Unity import/synchronisation workflow.
- The resulting Unity prefab/material composition as an explicit Unity content step; the Blender batch must not implicitly create Unity materials or prefabs.

Before executing the Blender batch, add a preflight report that compares the owner-approved asset inventory with discovered generator folders and production routes. It must fail closed (no production visual generation) if an approved required asset has no generator, no production route, an invalid destination, or an unresolved production/test status. It must also report unapproved discovered generators and exclude them from the production run. The preflight must not silently treat every discovered generator as approved merely because `GENERATE_ALL=True`.

The batch may continue to support TEST and CUSTOM modes, but those modes must not bypass the production asset approval gate or write test assets into production destinations. Keep `_ART_TEST` assets test-only.

### Gate acceptance criteria

- A versioned, owner-approved asset inventory exists before visual generation begins.
- Every approved Blender-generated asset is represented in the batch registry and has a canonical production route.
- The batch preflight confirms one-to-one consistency between approved asset keys, discovered source generators and production routes.
- Unapproved generators are not run in PRODUCTION mode.
- Existing assets are referenced by stable paths/GUIDs and are not duplicated.
- The batch report records the approved inventory version, selected assets, skipped unapproved assets, generated outputs and failures.
- No final-quality visualisation is declared complete while required approved assets remain missing or unresolved.

## 4. Explicit exclusions

- Full settlement layouts, streets, buildings, populations and points of interest.
- Actual village/town/city placement beyond deterministic candidate selection/preview markers derived from suitability.
- Road/path graph construction and A*/Dijkstra routing (separate follow-up work package; the existing domain specification remains authoritative).
- Full atmospheric/weather simulation, rainfall, wind, seasonal climate evolution or physically realistic climate.
- Final biome art bible, final production texture library and final vegetation asset catalogue.
- Pokémon habitats, population, resource simulation and species-specific ecosystem behaviour.
- Adaptive quadtree LOD, planetary streaming and production planet-radius selection.
- Arbitrary terrain deformation.
- Replacing or deleting the existing planar generator.

## 5. Spherical field and hydrology design gate

The existing domain document describes an exact two-pass Euclidean Distance Transform for a planar X/Z grid. WORLD-002 must preserve the requirement that moisture is based on a correct nearest-water distance, but must define a sphere-correct implementation before coding it.

Required design/validation work:
- Use a canonical spherical sampling/topology representation, not independent per-face or per-planisphere maps.
- Ensure neighbour relationships cross all six Cube-Sphere faces and wrap consistently at the planisphere seam; polar behaviour must be explicitly tested.
- Distances must be expressed in world-space units and account for spherical geometry. A planar pixel EDT, a face-local EDT or a naïve equirectangular EDT is not acceptable as the production spherical result.
- Compare candidate approaches for correctness, determinism and cost at the prototype resolutions. Candidate approaches may include a geodesic graph distance transform over the canonical spherical sample graph or another exact/controlled-error spherical nearest-source method. The selected algorithm, error bound and scaling behaviour must be recorded in the implementation report before the moisture field is accepted.
- If the selected method is approximate, document its error bound and demonstrate that it preserves the required monotonic water-proximity trend within that bound. Do not label an approximation as exact EDT.
- Water flow must use global spherical neighbours and a deterministic downhill/drainage policy; avoid independent face-local flow that breaks at seams.

This is a mandatory technical gate in WORLD-002. If no acceptable spherical distance/hydrology method meets the prototype's correctness and performance requirements, stop before deriving moisture and return an updated design proposal rather than silently using the planar algorithm.

## 6. Generation dependency order

Use an explicit dependency graph and stage outputs. The intended order is:

1. Validate generation request, seed, generator version, spherical resolution and settings snapshot.
2. Consume the composed WORLD-001 height field; derive slope and any required terrain masks.
3. Generate temperature from its independent field plus latitude/world-scale and altitude influences.
4. Generate water potential and classify ocean/sea, lakes/ponds and river candidates using height and spherical hydrology.
5. Calculate spherical nearest-water distance using the approved method from Section 5.
6. Derive moisture from water distance, terrain retention/slope and temperature-dependent evaporation.
7. Calculate vegetation suitability/potential density from dedicated variation plus environmental suitability.
8. Classify biomes from height, temperature, moisture and water.
9. Calculate village/town/large-city suitability and deterministic candidate markers.
10. Build biome-aware material assignments and validate the hybrid vegetation placement preview using the shared fields.
11. Generate selected visual outputs and independent exports.
12. Validate field ranges, topology continuity, hydrology, distance behaviour, biome constraints, settlement exclusions and deterministic reproducibility.

Stages must declare their input dependencies. Changing a presentation property must not regenerate fields. Changing one field's settings invalidates only that field and its downstream dependants.

## 7. Data model and contracts

- Keep the runtime generation core independent of `UnityEditor`.
- Use explicit immutable generation requests/settings snapshots containing world seed, generator version, spherical topology/resolution, world radius/configuration reference, requested stages and output policy.
- Each field must declare stable identity/name, data type (continuous/categorical/mask/vector where needed), dimensions/topology, units/range, seed stream or derivation, dependencies and generator version.
- Continuous fields use normalized `[0,1]` values unless a documented physical/world-space range is required (e.g. distance in metres, elevation in metres). Categorical fields use stable documented IDs, not display colours as data.
- Preserve canonical spherical coordinates and direction-based sampling as the source of truth. Spherical and planisphere views are presentation projections over the same field values.
- Use deterministic independent seed streams/offsets per field. Do not use uncontrolled global random state.
- Vegetation instances use stable IDs derived from seed, generator version, biome/profile and canonical spherical cell/instance identity; view changes must not alter placement.
- Keep generation, data storage, map visualisation, material assignment and asset instantiation as separate responsibilities.

## 8. Performance and lifecycle

- Profile on the project owner's PC, per DEC-039; do not invent a universal minimum hardware target.
- Record generation time and memory by stage and map resolution.
- Avoid per-frame regeneration. Generation is an explicit Editor action and a runtime-safe request through the same core; preview-only versus apply-to-scene must be explicit.
- Cache by stable generation keys. A settings change invalidates the affected stage and downstream outputs only.
- Vegetation instances must be generated/cleared as generator-owned preview content; Clear Preview must not delete user-authored scene objects.
- Avoid creating thousands of separate heavy scene objects for a prototype where instancing/batching is appropriate. The implementation plan must compare the project's available rendering approach before choosing it.

## 9. Tests and validation

### Automated tests
- Deterministic output for each field with same seed/version/settings; changed field seed changes its intended field.
- Field ranges, units and categorical IDs are valid.
- Temperature responds in the documented direction to latitude and altitude.
- Water does not appear on forbidden high ridges unless explicitly allowed by a documented exception; lake/pond candidates respect basins; river flow is downhill and seam-continuous.
- Spherical nearest-water distances agree with a trusted small reference implementation within the documented error bound; tests cover face seams, planisphere wrap and polar regions.
- Moisture is derived from water distance, terrain retention and temperature; no independent noise creates contradictory wet patches.
- Vegetation suitability decreases in configured unsuitable environmental conditions and respects water/slope/elevation exclusions.
- Biome classification respects configured temperature/moisture/elevation/water constraints and the confirmed initial biome set.
- Settlement suitability rejects submerged/steep/flood-prone cells and provides distinct village/town/city outputs.
- Spherical/planisphere samples match the same underlying data; map visualisation does not mutate source fields or geometry.
- Export round-trip preserves values/encodings and metadata.
- Vegetation instance placement is deterministic and stable across view changes; instances obey suitability and exclusion masks.
- Preview cleanup removes only generator-owned objects/assets.

### Manual Unity validation
- Inspect every implemented field on the globe and planisphere with legends.
- Inspect all six Cube-Sphere boundaries, planisphere left/right wrap and both poles.
- Compare raw fields and derived outputs; confirm moisture gradients around water and plausible basin/river behaviour.
- Confirm biome colours/material transitions and vegetation density are readable at close, regional and distant views.
- Confirm the hybrid vegetation result includes both ground treatment and 3D instances, and can be toggled/cleared without losing source maps.
- Regenerate the same seed and compare outputs; switch views without changing the generated result.
- Run EditMode and relevant PlayMode tests, then independent QA review.

## 10. Acceptance criteria

1. All included fields are real generated outputs, not UI-only enum entries or fabricated placeholders.
2. Height, temperature, water, slope, distance-to-water, moisture, vegetation suitability, biome and settlement-suitability outputs are independently inspectable.
3. The same generated data is displayed in spherical and planisphere views; projection switching does not regenerate or mutate fields.
4. Spherical face seams, planisphere wrap and polar behaviour pass automated and visual checks.
5. Spherical nearest-water distance uses a documented, validated globe-correct algorithm with stated accuracy/cost; no planar EDT is misapplied to the globe.
6. Water, moisture, vegetation, biome and settlement dependencies follow the approved order and shared source fields.
7. Initial biome outputs include plains, forest and desert; villages remain settlement candidates, not a biome.
8. Settlement suitability remains a separate output for village, town and large-city candidates; actual settlement layouts are not generated in this package.
9. Hybrid vegetation preview uses both biome-aware ground materials/textures and deterministic 3D vegetation instances, driven by suitability and exclusion fields.
10. Every implemented field can be exported independently at native resolution with correct precision/encoding and metadata.
11. Same seed, generator version and settings reproduce field values, candidate markers and vegetation placement.
12. Stage timing, validation diagnostics and known limitations are visible and documented.
13. Existing planar generation and WORLD-001 terrain-layer settings remain intact.
14. Editor and runtime entry points use the same runtime-safe generation core; editor APIs remain isolated.
15. Automated tests pass, Unity manual validation is recorded, QA reports PASS and the project owner approves closure.

## 11. Open questions / decisions deliberately not invented

- Exact water-level/sea-level model and whether the prototype uses one global sea level or additional basin rules.
- Spherical nearest-water algorithm selection, accuracy target and performance threshold (must be resolved by the Section 5 design gate).
- Exact temperature latitude curve, noise weights and seasonal baseline (WORLD-002 prototype only; full climate remains future work).
- Exact thresholds and blend widths for plains, forest and desert.
- Initial vegetation prefab catalogue, per-biome species profiles and visual density targets; inspect available assets before selecting final references.
- Whether village/town/city suitability is only a continuous score map or also includes deterministic candidate marker extraction thresholds; the plan requires both score fields and preview candidate markers, but final settlement counts remain open.
- Water body size thresholds and river ordering/width presentation.
- Numeric performance budgets; profile on the owner's PC as required by DEC-039.

## 12. Expected documentation and code locations

### Documentation
- `Documentation/00_GOVERNANCE/DECISION_REGISTER.md` (DEC-042)
- `Documentation/00_GOVERNANCE/DOCUMENT_INDEX.md`
- `Documentation/00_GOVERNANCE/DOCUMENTATION_CHANGELOG.md`
- `Documentation/01_ARCHITECTURE/WORLD_ARCHITECTURE.md`
- `Documentation/02_DOMAINS/TERRAIN_AND_RENDERING.md`
- `Documentation/02_DOMAINS/WORLD_AND_BIOMES.md`
- `Documentation/03_ENGINEERING/WORK_PACKAGES/WORLD-002_SPHERICAL_GEOGRAPHIC_FIELDS.md`
- `Documentation/01_ARCHITECTURE/SPHERICAL_WORLD_ARCHITECTURE_PROPOSAL.md` (only where spherical field/hydrology decisions are validated)

### Expected code areas (subject to inspection and approved implementation)
- `Assets/Project/Runtime/World/Spherical/` — spherical field contracts, generation stages and topology-aware spatial operations.
- `Assets/Project/Runtime/World/Spherical/Studio/` — settings, requests/results and shared runtime-safe pipeline integration.
- `Assets/Project/Runtime/World/Spherical/Presentation/` — spherical/planisphere map visualisation, biome materials and vegetation preview presentation.
- `Assets/Project/Editor/World/Spherical/` — Editor UI, map export, legends and diagnostics.
- `Assets/Project/Tests/EditMode/World/Spherical/` and relevant PlayMode tests — automated coverage.

Do not assume exact filenames or create parallel field systems before inspecting the current code and ownership contracts. Do not modify existing planar generator files as a shortcut.

## 13. Plan milestone gate

This is Milestone A for WORLD-002. It is a proposed plan, not permission to begin implementation. Per `Documentation/00_GOVERNANCE/GIT_WORKFLOW.md`, implementation may begin only after the project owner approves this plan and confirms it has been committed and pushed to the same working branch. The implementation milestone must then report actual tests, Unity validation evidence, documentation updates and QA status. QA PASS and project-owner approval are required before closure.


## Implementation traceability

First implementation tranche and validation status: `WORLD-002_IMPLEMENTATION_PROGRESS_01.md`. This report does not claim the work package is complete; hydrology/moisture remain gated by the spherical distance-to-water design validation.


## 9. Testing & Validation

- Standard: `Documentation/07_Testing/TESTING_AND_VALIDATION.md`
- Test layer: Unity EditMode for deterministic field/topology logic; Editor/manual visual checks for Planet Studio; performance measurements on the project owner's PC.
- Deterministic fixture: fixed `SphericalPlanetStudioSettings`, fixed seed and explicit face/planisphere resolutions.
- Physical units: altitude and graph distance in metres; slope in degrees; normalized temperature/moisture/suitability in `[0,1]`.

| Category | WORLD-002 acceptance criterion | Evidence |
|---|---|---|
| Functional | Every implemented field has a valid output at every supported sample | EditMode field tests |
| Determinism | Same seed, generator version and settings produce identical fields | Repeated-run tests |
| Invariants | Temperature/moisture/suitability stay in declared ranges; graph distances are non-negative | Property/range tests |
| Boundaries | Cube-face seams, cube corners, poles and planisphere wrap are continuous and unambiguous | Cross-face and round-trip tests |
| Robustness | Invalid resolution, radius, direction and empty-source requests fail explicitly | Negative tests |
| Regression | Existing WORLD-001 terrain geometry and projection behaviour remain stable | Studio regression tests |
| Integration | Globe and planisphere inspect the same direction-based field data | Editor/manual and automated sampling tests |
| Performance | Graph construction and distance propagation are measured at representative resolutions on owner PC | Benchmark report |
| Visual/content QA | Legends, map ranges, seam appearance and unavailable-stage states are correct | Planet Studio checklist/screenshots |

Hydrology-specific numerical error bounds and production performance budgets remain OPEN until prototype benchmark evidence is reviewed. No production hydrology acceptance may be claimed before those values are approved.


### 9.1 PlayMode validation

The current WORLD-002 geographic-field algorithms are suitable for EditMode coverage. PlayMode coverage is required when the fields are integrated into runtime world generation or runtime scene presentation.

| Runtime behaviour | PlayMode acceptance criterion | Status |
|---|---|---|
| Runtime generation entry point | A runtime caller can request generation with a fixed seed/settings and receive a valid field result | NOT IMPLEMENTED / NOT TESTED |
| Scene application | Generated spherical terrain/field presentation is attached under the intended generator-owned root | NOT TESTED |
| Refresh and cleanup | Re-generation replaces only generator-owned objects; clearing removes generated objects without deleting unrelated scene content | NOT TESTED |
| Runtime lifecycle | Disable/enable and scene unload/reload do not leave duplicate roots or stale references | NOT TESTED |
| Runtime performance | Generation time, allocations, mesh counts and cleanup cost are measured in a representative scene | NOT MEASURED |
| Runtime physics | If terrain colliders are added, representative raycasts/ground contact agree with generated terrain | NOT IMPLEMENTED / NOT TESTED |

EditMode remains the primary mode for the current pure spherical field and graph algorithms. The PlayMode matrix above is not evidence that those runtime integrations already exist.


### 9.2 Initial PlayMode tests added

Assembly: `Pokemon.Project.Tests.World.Spherical.PlayMode`

Initial runtime tests:
- generate and inspect six globe face meshes;
- generate and inspect a planisphere mesh;
- attach a generated face mesh to a scene GameObject and verify it remains valid after a frame;
- verify scene object/mesh cleanup;
- verify repeated generation creates a fresh mesh instance.

These are smoke/integration tests for the current mesh-generation API only. They do not imply that runtime world streaming, physics, player movement, water, vegetation or biome systems are implemented or tested.


## Testing documentation

Detailed current coverage and outstanding tests are maintained in `Documentation/07_Testing/`. See `COVERAGE_MATRIX.md` and `Work_Packages/WORLD-002_SPHERICAL_FIELDS_TEST_PLAN.md`.
