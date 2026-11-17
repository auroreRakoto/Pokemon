# WORLD-002 — Deprecated API Policy and PlayMode Compile Fix

- **Status:** PATCH PREPARED — Unity compilation pending
- **Scope:** testing standard plus fixes directly indicated by the latest Unity Console screenshot.

## Policy added

The shared testing standard now prohibits introducing deprecated/obsolete APIs in Runtime, Editor, tooling and tests. New obsolete warnings are treated as issues; exceptions require documented compatibility reasons and owner approval.

## PlayMode test fixes

The screenshot reported:
- `CS0103`: `CubeSphereFace` not found in the PlayMode test namespace.
- `CS0618`: `Object.GetInstanceID()` is deprecated.

Corrections:
1. Added `using Pokemon.Project.World.Spherical;` to make the parent namespace containing `CubeSphereFace` explicit.
2. Replaced instance-ID comparisons with `Assert.AreNotSame(first, second)`, which directly tests that the second generation returns a different Mesh object without calling a deprecated API.

## Validation status

- Static source inspection: completed.
- Unity compilation: owner reports current project compiles and tests pass.
- PlayMode discovery and execution: owner reports current tests pass.
- No tests are claimed as passed.


## Latest owner validation

The project owner confirmed that all currently available tests pass after applying the fixes. Exact test counts were not supplied; see `../COVERAGE_MATRIX.md` for the distinction between passing existing tests and remaining untested behaviours.
