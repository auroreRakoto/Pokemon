# 07 — Testing

This folder is the canonical home for project-wide testing standards, test plans, coverage records and validation reports.

## Structure

- `TESTING_AND_VALIDATION.md` — mandatory project-wide testing standard:
  - EditMode / PlayMode / integration / Editor / performance / content validation;
  - common test categories and feature acceptance criteria;
  - numeric tolerance policy;
  - deprecated/obsolete API policy;
  - reporting and evidence requirements.
- `Work_Packages/` — testing and validation reports associated with individual work packages. The owning module specification remains in its module folder; test evidence and coverage history live here.

## Rules

1. Every substantial feature must link to `TESTING_AND_VALIDATION.md` from its owning module/work-package document.
2. Test code remains under the Unity project's `Assets/Project/Tests/...` folders. This documentation folder stores plans, standards, coverage matrices and results, not C# test scripts.
3. EditMode and PlayMode results must be reported separately.
4. A test is not considered passed merely because it was authored or compiled. Record actual Test Runner execution.
5. Record missing tests as coverage gaps. Do not mark unimplemented behaviours as tested.
6. Keep deprecated/obsolete API policy and numeric tolerance guidance centralized in the standard.

## WORLD-002 records

- `Work_Packages/WORLD-002_IMPLEMENTATION_PROGRESS_03_TESTING_STANDARD.md`
- `Work_Packages/WORLD-002_IMPLEMENTATION_PROGRESS_04_PLAYMODE_TESTS.md`
- `Work_Packages/WORLD-002_IMPLEMENTATION_PROGRESS_05_DEPRECATED_API_POLICY.md`


## Existing implementation test plans

- `COVERAGE_MATRIX.md` — inventory of tests that exist, coverage gaps and current implementation status.
- `Work_Packages/WORLD-001_SPHERICAL_WORLD_FOUNDATION_TEST_PLAN.md` — topology, height field, surface queries, meshes, presenter, movement, camera and demo lifecycle.
- `Work_Packages/WORLD-002_SPHERICAL_FIELDS_TEST_PLAN.md` — Planet Studio, geographic fields, graph distances and future spherical hydrology stages.
- `Work_Packages/PLANAR_WORLD_GENERATION_TEST_PLAN.md` — separate planar generator, terrain builder and config tooling.
- `Work_Packages/PLAYMODE_RUNTIME_TEST_PLAN.md` — runtime integration tests for already implemented components.
