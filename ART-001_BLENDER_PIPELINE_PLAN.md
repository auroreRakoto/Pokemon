# ART-001 — Blender Procedural Asset Pipeline Plan

- **Work package:** ART-001
- **Agent:** ART
- **Status:** PROPOSED — audit and architecture plan; no implementation started
- **Repository snapshot audited:** `Pokemon-main (3).zip`
- **Primary owner:** ART
- **Collaborators:** TOOLS, WORLD, POK, QA
- **Related decisions:** DEC-005, DEC-010, DEC-011, DEC-012 (DEC-012 remains PROPOSED), DEC-016
- **Plan milestone:** required before any implementation; commit and push this document on the working branch

## 1. Executive summary

The repository currently contains a small set of Unity-imported environmental FBX assets, but no discoverable Blender project, Blender Python generator, shared `_core`, `project_modules`, asset source folders, export manifests, or automated Blender-side validation.

The current content therefore demonstrates that FBX assets can be consumed by Unity, but it does **not** demonstrate a reproducible Blender-to-Unity production pipeline. The first work package should establish the pipeline foundation and prove it with one low-risk environmental asset before scaling to Pokémon generators.

**Recommendation:** create a project-local `Blender/` workspace at the repository root, with a shared `_core` package, reusable domain modules in `project_modules/`, one generator package per asset under `source/<domain>/<asset>/`, and generated deliverables under `exports/<domain>/`. Treat Unity's `Assets/Project/Content/Models/` as the imported runtime/content destination, not as Blender's authoring or export workspace.

Do not mass-produce Pokémon models until the common scene, validation, export, manifest and Unity-import contracts have been proven.

## 2. Audit scope and method

The audit inspected the supplied ZIP's file tree and the repository documents that govern:
- asset creation and source/export separation;
- Blender Python authoring rules;
- Git's plan/implementation milestones;
- module ownership and cross-domain interfaces;
- content validation and world/environment requirements.

The audit is limited to the supplied repository snapshot. It cannot establish the current state of the developer's working tree, Blender installation, local Git branch, untracked files, or files excluded from the ZIP.

## 3. Existing repository audit

### 3.1 Blender sources and scripts

| Item | Finding |
|---|---|
| `.blend` files | None found in the ZIP |
| Blender `.py` scripts | None found in the ZIP |
| `_core/` | No Blender `_core` found |
| `project_modules/` | No Blender package tree found |
| `source/` generators | No Blender source-generator tree found |
| Blender documentation/config | No dedicated Blender workspace/config found |

**Conclusion:** there is no existing Blender automation implementation to preserve or refactor in this snapshot. ART-001 is a greenfield pipeline foundation, not a migration of existing Blender scripts.

### 3.2 Existing Unity model assets

Six binary FBX files were found under `Assets/Project/Content/Models/Environment/Vegetation/`:

| Existing asset | Approx. size | Domain |
|---|---:|---|
| `FruitBush/Bush_Round.fbx` | 114 KB | Environment / vegetation |
| `FruitBush/Fruit_Blue.fbx` | 48 KB | Environment / vegetation |
| `FruitBush/Fruit_Red.fbx` | 48 KB | Environment / vegetation |
| `Grass_Short.fbx` | 29 KB | Environment / vegetation |
| `Grass_Tall.fbx` | 41 KB | Environment / vegetation |
| `Grass_Wild.fbx` | 272 KB | Environment / vegetation |

All six are binary FBX files. Their presence proves Unity-facing model exports exist, but their original editable Blender sources and generation history are not present in this ZIP.

No Pokémon model FBX/GLB/OBJ/STL/DAE or `.blend` asset was found in the inspected model paths. The `Models/Pokemon/` directory has metadata/directory structure but no model file in this snapshot.

### 3.3 Unity import metadata

Each listed FBX has a corresponding Unity `.meta` file. This is useful evidence that the assets are already imported/tracked by Unity, but it does not reveal a canonical Blender export preset or guarantee that future generated FBX files will use matching scale, axes, smoothing, material, animation or naming settings.

The first implementation must inspect representative `.meta` files and Unity importer settings in the live project before fixing the export contract. Existing FBX files should be treated as legacy/reference assets until their source provenance and import settings are confirmed.

### 3.4 Current gaps

1. No editable source of truth for the six FBX assets is included.
2. No deterministic generation entry point or shared configuration exists.
3. No shared Blender scene/collection/object naming validation exists.
4. No central export-path policy exists.
5. No automated geometry, transform, material, rig or budget validation exists.
6. No per-export manifest or generator version is present in the discovered tree.
7. No automated handoff/import validation from Blender exports to Unity is demonstrated.
8. Naming and FBX settings are not consolidated with TOOLS/Unity import tooling.
9. The visual art bible and concrete budgets for Pokémon/environment classes remain incomplete or unspecified in the inspected standards.

## 4. Governing documentation and constraints

The plan follows these repository rules:

- `Documentation/02_DOMAINS/ART_AND_ASSET_PIPELINE.md`: separate editable Blender sources, generated exports and Unity runtime content; use reusable core/domain modules; place each asset generator under `source/<domain>/<asset>/`; validate before export; emit a manifest.
- `Documentation/04_AI_INSTRUCTIONS/instructions/blender-python.instructions.md` and `.github/instructions/blender-python.instructions.md`: reuse `_core` and package modules; no asset-specific root scripts; centralize paths/config; no user-specific absolute paths; validate and report failures.
- `Documentation/00_GOVERNANCE/GIT_WORKFLOW.md`: commit and push the plan before implementation; implementation follows on the same branch; Conventional Commit subject and mandatory `Agent: ART` footer; QA PASS and owner approval required for closure.
- `Documentation/01_ARCHITECTURE/MODULE_BOUNDARIES.md`: ART owns visual content standards and pipeline; TOOLS owns authoring/import/validation tooling; WORLD owns environment/world meaning; POK owns Pokémon simulation/species meaning.
- `Documentation/03_ENGINEERING/TESTING_AND_VALIDATION.md`: content validation and idempotent editor generation are required quality concerns.
- `Documentation/00_GOVERNANCE/PROJECT_CHARTER.md`: stylized 3D, Blender + scripts, reusable assets and incremental scope are project directions.

DEC-012 currently records reusable core + domain packages + source/export separation as **PROPOSED**. This plan is consistent with that proposal but does not silently promote it to CONFIRMED. The project owner/ARCH should confirm the architecture decision before implementation is treated as approved.

## 5. Proposed Blender workspace architecture

The following is the proposed canonical structure. It deliberately keeps authoring sources, reusable code, generated exports and Unity-imported content distinct.

```text
Pokemon/
├── Blender/
│   ├── README.md
│   ├── config/
│   │   ├── project.toml
│   │   └── export_profiles/
│   │       ├── static_mesh.toml
│   │       ├── skinned_mesh.toml
│   │       └── animation.toml
│   ├── _core/
│   │   ├── __init__.py
│   │   ├── paths.py
│   │   ├── config.py
│   │   ├── scene.py
│   │   ├── collections.py
│   │   ├── naming.py
│   │   ├── transforms.py
│   │   ├── materials.py
│   │   ├── geometry.py
│   │   ├── validation.py
│   │   ├── export.py
│   │   ├── manifest.py
│   │   ├── logging.py
│   │   └── versioning.py
│   ├── project_modules/
│   │   ├── pokemon/
│   │   │   ├── __init__.py
│   │   │   ├── mesh_helpers.py
│   │   │   ├── rig_helpers.py
│   │   │   └── pokemon_validation.py
│   │   ├── environment/
│   │   │   ├── __init__.py
│   │   │   ├── vegetation.py
│   │   │   ├── rocks.py
│   │   │   └── props.py
│   │   ├── collectibles/
│   │   │   └── __init__.py
│   │   └── materials/
│   │       └── __init__.py
│   ├── source/
│   │   ├── pokemon/
│   │   │   └── <species>/
│   │   │       ├── generator.py
│   │   │       ├── parameters.py
│   │   │       ├── README.md
│   │   │       └── source.blend              # when authored/saved
│   │   ├── environment/
│   │   │   └── <asset>/
│   │   │       ├── generator.py
│   │   │       ├── parameters.py
│   │   │       ├── README.md
│   │   │       └── source.blend              # when authored/saved
│   │   ├── collectibles/
│   │   │   └── <asset>/
│   │   └── props/
│   │       └── <asset>/
│   ├── exports/
│   │   ├── pokemon/
│   │   ├── environment/
│   │   ├── collectibles/
│   │   └── props/
│   ├── tests/
│   │   ├── test_paths.py
│   │   ├── test_manifest.py
│   │   └── blender/
│   └── documentation/
│       ├── PIPELINE_OVERVIEW.md
│       ├── ASSET_AUTHORING_GUIDE.md
│       └── UNITY_HANDOFF.md
└── Assets/
    └── Project/Content/Models/                # Unity-imported content only
```

### 5.1 Directory ownership rules

- `_core/`: generic pipeline primitives only; no Pokémon species rules or individual asset geometry.
- `project_modules/`: reusable domain-level helpers and conventions; no one-off asset generators.
- `source/<domain>/<asset>/`: the only home for asset-specific generator logic, parameters, source `.blend` files and asset-specific notes.
- `exports/<domain>/`: generated FBX and manifest outputs. Never edit these as the only source of truth.
- `Assets/Project/Content/Models/`: Unity's imported content destination. The pipeline may copy/sync approved exports here, but Blender must not treat this as its authoring directory.
- `tests/`: automated checks for pipeline functions and Blender headless integration.
- `config/`: project-relative configuration and named export profiles; no machine-specific absolute paths.

### 5.2 Python package/import strategy

Use ordinary Python packages (`__init__.py`) for `_core` and reusable `project_modules` domains. The entry point must establish a deterministic project root and import path rather than relying on whichever directory Blender happens to launch from.

Avoid installing the workspace as a machine-global package for the initial version. Prefer a documented launcher that resolves the repository root and invokes Blender in background mode with the project's scripts. This keeps the setup manageable for a solo developer and reproducible in CI/local terminals.

The launcher itself and any Unity-side import automation should be coordinated with TOOLS; ART owns asset-generation semantics and Blender-side validation contracts.

## 6. Pipeline architecture and execution flow

```text
Asset parameters + versioned config
                |
                v
Asset generator (source/<domain>/<asset>/generator.py)
                |
                v
Reusable project_modules/<domain> helpers
                |
                v
_core scene / collections / materials / geometry
                |
                v
Pre-export validation
                |
         PASS / FAIL
                |
                v
Export profile + deterministic output path
                |
                v
exports/<domain>/<asset>/
       |                 |
       v                 v
  asset.fbx         asset.manifest.json
       |
       v
Unity import/sync tooling (TOOLS-owned integration)
       |
       v
Unity content validation / reference checks
```

Recommended generator lifecycle:

1. Resolve project root and load validated configuration.
2. Reset or create a dedicated, deterministically named scene/collection scope.
3. Build or update the asset from explicit parameters and seed.
4. Normalize object names, collection membership, transforms and origins.
5. Run pre-export validation; fail closed on blocking errors.
6. Export to a temporary/staging path, then promote only after successful export and manifest creation.
7. Generate the manifest from actual scene/export settings and dependency data.
8. Run post-export checks (file exists, non-zero size, manifest schema, expected outputs).
9. Report outputs, warnings and errors with stable asset ID and generator version.

Generation should be idempotent: rerunning the same generator/configuration should update the same intended outputs and must not create duplicate scene objects, collections or Unity assets.

## 7. `_core` responsibilities and API boundaries

The initial `_core` should stay deliberately small and stable.

| Module | Responsibility | Must not own |
|---|---|---|
| `paths.py` | Resolve repo/workspace/source/export/Unity destination paths | Asset-specific paths embedded in code |
| `config.py` | Load/validate project and export-profile configuration | Domain gameplay data |
| `scene.py` | Scene lifecycle, deterministic setup/reset | Asset geometry |
| `collections.py` | Create/find/validate named collections | Species-specific collection rules |
| `naming.py` | Stable identifiers and naming checks | Final Unity naming policy before TOOLS agreement |
| `transforms.py` | Apply/validate scale, rotation, origin and axis conventions | Per-asset artistic decisions |
| `materials.py` | Shared material creation/assignment utilities | Shader implementation in Unity |
| `geometry.py` | Generic mesh integrity checks and helper operations | Species morphology rules |
| `validation.py` | Validation result model, severity, aggregation and exit status | Domain-specific acceptance rules |
| `export.py` | Export orchestration using named profiles | Asset creation |
| `manifest.py` | Manifest schema and deterministic serialization | Unity runtime content ownership |
| `logging.py` | Structured, actionable logs | Silent exception swallowing |
| `versioning.py` | Pipeline/generator/schema version identifiers | Git release policy |

Proposed shared validation result levels:
- `ERROR`: export-blocking failure;
- `WARNING`: export allowed only where the profile/policy permits it, and always recorded;
- `INFO`: informational evidence.

The generator should return a non-zero process exit code for blocking validation/export failures so local automation and CI can detect failure.

## 8. Asset generator contract

Each asset package should expose one documented entry point, initially proposed as:

```python
def generate(context) -> "GenerationResult":
    """Build/update this asset in Blender and return validation/export inputs."""
```

The exact API is intentionally not frozen until the first prototype and TOOLS review. The contract should include:
- stable `asset_id`;
- domain and asset type;
- generator version;
- explicit parameter/config input;
- optional deterministic seed;
- declared dependencies;
- expected Blender collections/objects;
- export profile;
- validation rules or profile selection;
- Unity destination identifier/path;
- output artifact list and warnings.

A generator must not duplicate generic export/path/manifest logic. Asset-specific geometry and art parameters remain in the asset package.

## 9. Export format and Unity handoff

### 9.1 Initial format

Use FBX as the initial Unity handoff format because the existing project already contains FBX models. This is a provisional pipeline choice, not proof that every future asset class should use FBX indefinitely.

Profiles should be separated at least by:
- static mesh;
- skinned mesh/armature;
- animation clip export (if/when required).

Do not lock exact axis, scale, smoothing, tangent, leaf-alpha, armature, animation, material or texture-embedding settings solely from assumptions. Validate them against the live Unity project, its Unity version, importer settings and a representative existing asset. Record the approved profile values in version-controlled config and test them with round-trip/import checks.

### 9.2 Export destination

Blender-generated files must first land under `Blender/exports/<domain>/<asset>/`. A separate, explicit handoff step should sync approved files to the Unity content destination.

Suggested Unity destination convention (to be confirmed with TOOLS):

```text
Assets/Project/Content/Models/
  Pokemon/<Species>/
  Environment/<Category>/<Asset>/
  Collectibles/<Category>/<Asset>/
  Props/<Category>/<Asset>/
```

Do not make Blender write directly into Unity's runtime content folder during initial generation. This preserves the source/export/import boundary and allows validation before content enters Unity.

### 9.3 Determinism and overwrite policy

- Stable output names derived from `asset_id`.
- No timestamps in filenames.
- A repeat run updates the same asset output.
- Staging + promotion prevents half-written exports replacing known-good outputs.
- Never delete unrelated exports or Unity assets as part of generation.
- Any destructive cleanup must be explicit and scoped to the current asset ID.

## 10. Export manifest

Every exported asset should receive a sidecar JSON manifest. Initial proposed schema:

```json
{
  "schema_version": 1,
  "asset_id": "environment.vegetation.grass_short",
  "display_name": "Grass Short",
  "domain": "environment",
  "asset_type": "static_mesh",
  "source": {
    "blend_path": "source/environment/grass_short/source.blend",
    "generator_path": "source/environment/grass_short/generator.py"
  },
  "pipeline": {
    "version": "0.1.0",
    "generator_version": "1.0.0",
    "blender_version": "x.y.z"
  },
  "export": {
    "format": "fbx",
    "profile": "static_mesh",
    "relative_path": "exports/environment/grass_short/SM_Grass_Short.fbx",
    "units": "meter",
    "scale": 1.0,
    "forward_axis": "TO_CONFIRM",
    "up_axis": "TO_CONFIRM"
  },
  "dependencies": [],
  "materials": [],
  "unity": {
    "destination": "Assets/Project/Content/Models/Environment/Vegetation/Grass_Short.fbx",
    "import_profile": "TO_CONFIRM"
  },
  "validation": {
    "status": "passed",
    "errors": [],
    "warnings": []
  }
}
```

This is a proposal, not a finalized schema. Do not write placeholder values such as `TO_CONFIRM` into successful production manifests. During implementation, define required/optional fields, path normalization, schema validation, version migration policy and whether source hashes/export hashes are included.

The manifest must be generated from actual configuration and validation results, not hand-maintained separately from the export.

## 11. Validation strategy

Validation should be layered so asset-specific rules do not contaminate `_core`.

### 11.1 Generic pre-export validation

- expected scene and collection structure exists;
- object names are unique and comply with naming rules;
- no unexpected hidden/disabled objects are exported;
- transforms are applied or conform to the approved profile;
- object origin and stable gameplay root conventions are respected;
- scale/units and axis conventions match the selected profile;
- meshes have valid geometry, non-empty faces and no known invalid topology;
- normals/smoothing are valid according to profile;
- material slots are present and assigned as expected;
- export destination resolves inside the configured exports directory;
- dependencies exist and are declared;
- polygon/vertex counts remain within configured budgets where a budget exists.

### 11.2 Domain-specific validation

**Environment/static props**
- expected mesh/object count and collection labels;
- pivot/origin policy;
- dimensions and scale sanity;
- material-slot and UV requirements;
- optional interaction markers only when WORLD requires them;
- no unnecessary rig/animation data.

**Pokémon**
- stable visual root and base mesh structure;
- agreed origin/forward direction and gameplay bounds;
- material slots and texture references;
- rig/armature and bone naming where applicable;
- skin-weight sanity and deform checks;
- required attachment sockets;
- optional LODs/forms only where the species specification requires them;
- no per-individual size mesh variants: ordinary size variation remains runtime scaling of the visual child (DEC-011).

**Textures/materials**
- image paths resolve and are portable;
- resolution/format/color-space/packing comply with the class profile;
- material references and Unity shader compatibility are declared;
- avoid generating unused PBR maps for stylized assets.

### 11.3 Post-export validation

- expected export file exists and is non-empty;
- manifest exists and passes schema validation;
- manifest asset ID, output path and generator version match the run;
- exported artifact list matches actual files;
- no output escapes the configured export root;
- Unity import/sync stage reports missing references or importer errors.

Blender-side validation cannot prove Unity runtime compatibility by itself. The TOOLS-owned Unity import check is a separate required stage.

## 12. Pokémon and environment asset strategy

### 12.1 First proof asset: environment vegetation

Use one simple static environmental asset as the first end-to-end proof. A short grass/vegetation asset is a suitable candidate because the repository already contains `Grass_Short.fbx` as a reference, and the asset does not require a Pokémon rig or unresolved species-specific semantics.

The first proof should:
- generate from parameters;
- use `_core` and `project_modules/environment`;
- be repeatable and deterministic;
- produce FBX + manifest;
- pass Blender validation;
- import into Unity at the intended scale/orientation/material setup;
- not overwrite the legacy asset until the project owner explicitly chooses to replace it.

### 12.2 Next: reusable environmental families

After the proof, implement parameterized families:
1. grass variants;
2. simple rocks/logs/props;
3. berry/fruit bush components and interaction marker conventions;
4. other environmental objects required by WORLD's content profiles.

WORLD owns habitat meaning, placement constraints and interaction semantics. ART owns visual geometry/materials and asset metadata. The pipeline should accept declared interaction markers but should not invent their gameplay meaning.

### 12.3 Pokémon generators

Only after the static pipeline is validated:
1. agree a representative species and visual brief with POK;
2. define a common Pokémon base-mesh/rig strategy;
3. prototype one species before generalized species generation;
4. define shared rig compatibility and exception policy;
5. add Pokémon-specific validators and manifest metadata;
6. validate size variation using one base visual mesh and runtime scale, not duplicate size exports.

Procedural generation should not imply that all Pokémon can be generated convincingly from one generic parameter set. The system should support reusable anatomy helpers while allowing species-specific generators and art direction.

## 13. Collaboration and ownership

| Topic | ART | TOOLS | WORLD | POK | QA |
|---|---|---|---|---|---|
| Blender workspace and visual source standards | Own | Consult | Consult | Consult | Review |
| Shared Blender `_core` and asset generator contract | Own | Review launcher/integration boundary | Consult | Consult | Test |
| Blender-to-Unity export profiles | Propose/own asset side | Own Unity import side | Consult | Consult | Verify |
| Naming and import settings | Propose | Co-define canonical Unity convention | Consult | Consult | Verify |
| Environment interaction markers | Implement visual markers per contract | Tool support | Own semantics/requirements | — | Verify |
| Pokémon mesh/rig requirements | Implement visual pipeline | Tool support | — | Own species/behaviour needs | Verify |
| Content validation | Own Blender checks | Own Unity/editor checks | Supply environment constraints | Supply species constraints | Independent review |

### 13.1 Required coordination before implementation

**TOOLS**
- Review the proposed Unity destination layout and import/sync boundary.
- Inspect current Unity version and representative FBX `.meta` importer settings in the live project.
- Agree naming conventions, GUID/meta preservation behaviour, importer profiles and how validation is invoked.
- Decide whether the launcher/CI entry point belongs in `Blender/` or a TOOLS-owned tooling folder; do not duplicate launchers.

**WORLD**
- Identify the first required environment asset family and any existing interaction marker/socket conventions.
- Confirm whether the first grass proof is decorative only or must carry any gameplay metadata.
- Supply relevant dimensions/placement assumptions only where WORLD already owns them.

**POK**
- Identify the first representative Pokémon species for a later prototype.
- Supply species-specific visual/rig constraints and identify any shared rig expectations.
- Confirm that per-individual size remains runtime visual scaling rather than exported size variants.

No cross-domain gameplay rules should be embedded in ART generators.

## 14. Phased work plan

### Phase 0 — plan approval (this deliverable)

- Complete repository audit.
- Propose architecture and ownership.
- Record open decisions and measurable acceptance criteria.
- Commit and push this plan before implementation.

**Gate:** project owner approves ART-001 plan; DEC-012 architecture direction is confirmed or explicitly amended.

### Phase 1 — pipeline skeleton

- Create `Blender/` workspace and README.
- Create `_core` package and configuration/path resolver.
- Create `project_modules/environment` and a minimal asset package under `source/environment/<asset>/`.
- Add launcher/background invocation instructions in coordination with TOOLS.
- Add structured logging and deterministic generator result model.

**Gate:** Blender can run the sample generator headlessly from a clean repository checkout without machine-specific paths.

### Phase 2 — first asset proof

- Implement one parameterized static vegetation generator.
- Add static-mesh export profile, with values validated against Unity.
- Add staging/promotion export flow.
- Generate FBX and JSON manifest.
- Add idempotence and failure-path tests.

**Gate:** same inputs produce the same intended scene/output identity; rerun creates no duplicates; invalid inputs fail with actionable errors.

### Phase 3 — validation and Unity handoff

- Complete generic pre-export and post-export validators.
- Coordinate Unity import/sync and importer checks with TOOLS.
- Verify model scale, orientation, normals, material slots and destination.
- Capture a reproducible test procedure and evidence.

**Gate:** generated asset passes Blender checks and Unity import/content checks.

### Phase 4 — reusable environment families

- Generalize environment helpers only where multiple real assets need them.
- Add asset-specific packages for agreed environment assets.
- Integrate WORLD's metadata requirements without embedding WORLD domain logic.

**Gate:** at least two distinct assets use shared modules without copy/paste of pipeline code.

### Phase 5 — Pokémon pipeline prototype

- Agree representative species with POK.
- Define visual root, base mesh, rig, socket and material conventions.
- Implement a single species generator and validators.
- Test runtime size variation assumptions with Unity/POK.

**Gate:** representative Pokémon exports and imports correctly with documented rig/material/socket validation.

### Phase 6 — production hardening

- Add broader tests and CI/headless execution where practical.
- Add manifest schema evolution and pipeline compatibility checks.
- Document recovery, regeneration and asset deprecation workflows.
- Expand asset classes incrementally.

**Gate:** QA reports PASS against ART-001 acceptance criteria and the owner approves closure.

## 15. Testing plan and acceptance criteria

### Automated tests

1. Path resolver always stays inside the repository's configured Blender workspace/export roots.
2. Missing or invalid config produces a clear failure and non-zero exit.
3. Generator can be invoked in Blender background mode.
4. Running the same generator twice does not duplicate objects/collections or produce unstable output naming.
5. Invalid scene structure, missing material, bad transform or missing dependency is detected before export.
6. Export failure does not replace the last known-good output.
7. Manifest is emitted only for a successful export and passes schema validation.
8. Manifest asset ID, generator version, export profile and relative path match the run.
9. Output path traversal or absolute external destinations are rejected.
10. Unity import check detects missing model/material references and reports importer failures.

### Manual/integration verification

- Open generated FBX in the target Unity project.
- Verify scale, forward/up orientation, pivot, normals, smoothing, UVs, materials and bounds.
- Confirm the FBX importer profile is reproducible and no manual Inspector repair is required for the proof asset.
- Re-run generation and confirm no duplicate Unity assets or accidental GUID churn.
- Verify legacy FBX assets remain unchanged during the proof.

### ART-001 completion criteria

ART-001 is eligible for QA only when:
- canonical Blender workspace exists and follows the approved directory rules;
- `_core` and reusable domain package boundaries are implemented;
- at least one environment generator uses those shared modules;
- export is repeatable and isolated from sources and Unity runtime content;
- FBX and a valid sidecar manifest are produced;
- Blender pre/post-export validation is automated;
- Unity handoff/import validation is agreed and demonstrated with TOOLS;
- documentation and test evidence are updated;
- known limitations are recorded.

The work package is not closed until independent QA reports PASS and the project owner approves it.

## 16. Explicit exclusions for ART-001

- Recreating all six existing FBX assets or claiming their original sources have been recovered.
- Replacing legacy Unity models without owner approval.
- Generating a full Pokédex or a large library of environmental assets.
- Choosing a final art bible, polygon budgets or texture budgets without project-level agreement.
- Deciding WORLD habitat/ecology rules or POK species behaviour/lifecycle rules.
- Implementing Unity runtime gameplay, spawning or world placement systems.
- Introducing external Python dependencies/frameworks without a separate need and compatibility review.
- Claiming Unity import compatibility based only on Blender-side export success.

## 17. Open questions / decisions required

1. **DEC-012:** confirm the proposed reusable core + domain package + source/export separation as the canonical Blender workspace architecture.
2. **Unity destination:** confirm whether the suggested per-asset subfolders under `Models/Environment`, `Models/Pokemon`, etc. match TOOLS' current import conventions.
3. **FBX profile:** inspect and approve exact axis, scale, smoothing, tangent, material, armature and animation settings against the live Unity version.
4. **Naming:** consolidate Blender object/collection names, exported filenames and Unity asset names with TOOLS.
5. **Manifest:** approve schema versioning, source hash policy and whether manifest copies are also imported into Unity.
6. **Source `.blend` policy:** decide whether every generated asset must commit a `.blend` file or whether deterministic Python + parameters are sufficient for some asset classes. The source-of-truth must remain recoverable either way.
7. **Budgets:** define class-specific geometry/texture budgets before mass production.
8. **First proof asset:** confirm the grass/vegetation candidate with WORLD and project owner.
9. **Pokémon prototype:** POK to nominate the first species only after the static pipeline proof.

## 18. Documentation expected to change

During implementation, update only what the implementation actually establishes:

- `Documentation/02_DOMAINS/ART_AND_ASSET_PIPELINE.md` — canonical workspace, generator/manifest/export contracts and validation rules.
- `Documentation/04_AI_INSTRUCTIONS/instructions/blender-python.instructions.md` — concrete package/API and invocation rules, if they become stable.
- `.github/instructions/blender-python.instructions.md` — keep synchronized with the canonical instruction document.
- `Documentation/00_GOVERNANCE/DECISION_REGISTER.md` — update DEC-012 only after owner approval.
- `Blender/README.md` and `Blender/documentation/*` — operational setup, authoring and Unity handoff guides.
- Any TOOLS-owned Unity import documentation — TOOLS authors/maintains its own changes.

Do not edit the standards now merely to make the proposed architecture appear already approved.

## 19. Git workflow and handoff

### Milestone A — plan

Recommended commit:

```text
docs(art): plan procedural Blender asset pipeline

Audit existing Blender and Unity model assets and define the
proposed source, generator, validation, export and manifest architecture.

Work-Package: ART-001
Agent: ART
```

Push this plan to the working branch before starting Phase 1. The ZIP is a repository snapshot and does not expose the user's Git branch, remotes or credentials; ART cannot truthfully claim to have committed or pushed it from this audit.

### Milestone B — implementation

Only after the plan is approved and pushed. Use a separate implementation commit on the same branch, with `Plan-Commit: <hash>` and `Agent: ART`. Record deviations and seek approval for material changes before expanding scope.

### QA / owner gate

QA independently checks the acceptance criteria and reports PASS / FAIL / BLOCKED. The project owner gives final approval. A successful Blender export alone is not closure.

---

## NEXT HANDOFF

### 1. Rapport produit
`ART-001_BLENDER_PIPELINE_PLAN.md`

### 2. Agent suivant
`TOOLS` (review technique de la frontière Blender → Unity et des outils partagés)

### 3. Documents à lui transmettre
- `ART-001_BLENDER_PIPELINE_PLAN.md`
- `Documentation/02_DOMAINS/ART_AND_ASSET_PIPELINE.md`
- `Documentation/00_GOVERNANCE/GIT_WORKFLOW.md`
- `Documentation/01_ARCHITECTURE/MODULE_BOUNDARIES.md`
- `Documentation/03_ENGINEERING/TESTING_AND_VALIDATION.md`
- Les `.meta` des six FBX environnementaux existants et le projet Unity correspondant

### 4. Travail attendu du prochain agent
- Vérifier le layout Unity proposé et la destination d'import.
- Inspecter la version Unity et les réglages d'import FBX réels.
- Définir la stratégie de synchronisation/export vers `Assets/Project/Content/Models/` sans casser les `.meta`/GUID.
- Déterminer l'emplacement et la responsabilité du launcher/headless Blender, sans dupliquer les outils.
- Proposer les contrôles d'import Unity et les critères d'acceptation correspondants.
- Renvoyer ses arbitrages à ART avant l'implémentation.

### 5. Livrable attendu
`ART-001_TOOLS_REVIEW.md`

### 6. Instructions au propriétaire
1. Télécharger `ART-001_BLENDER_PIPELINE_PLAN.md`.
2. Le committer et le pousser sur la branche de travail avec le message proposé ci-dessus (ou demander à ART de le faire dans un environnement ayant accès au dépôt).
3. Ouvrir le chat TOOLS.
4. Joindre le plan, les documents listés et les `.meta`/projet Unity nécessaires.
5. Demander à TOOLS de produire `ART-001_TOOLS_REVIEW.md` sans commencer l'implémentation.
6. Transmettre le retour à ART pour arbitrage, puis valider formellement le plan avant le Milestone B.
