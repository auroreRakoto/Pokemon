# Deprecation and Cleanup Policy

- **Status:** PROPOSED — owner approval required
- **Version:** 1.0
- **Purpose:** remove obsolete or non-compliant project elements without breaking Unity references or deleting useful work.

## 1. Principle

The project must converge toward one clear architecture. However, "not currently compliant" does not automatically mean "delete immediately".

Cleanup is a controlled migration with evidence, replacement and rollback.

## 2. Classification

Every cleanup candidate receives one category:

- `DUPLICATE`: another authoritative implementation/document exists.
- `OBSOLETE`: replaced by a newer approved implementation.
- `MISPLACED`: valid responsibility stored in the wrong module/folder.
- `UNREFERENCED`: no code, asset, scene, test, build or documentation reference found.
- `NON_COMPLIANT`: active but violates an approved boundary/standard.
- `HISTORICAL`: retained as a record, not active guidance.
- `UNKNOWN`: insufficient evidence; do not delete.

## 3. Required audit

Before removal, search:
- C# references and type usages;
- Unity serialized references in scenes, prefabs, ScriptableObjects and animation/controllers;
- `AssetDatabase` paths and string-based loads;
- build scenes and addressable/resource catalogs if present;
- tests and test fixtures;
- `.asmdef` references;
- documentation links, menu paths and automation scripts;
- GUID references and `.meta` files.

A text search alone is insufficient for Unity serialized references.

## 4. Migration sequence

1. Identify candidate and owner.
2. Explain why it violates the approved standard.
3. Identify all consumers/references.
4. Select replacement or mark functionality as intentionally removed.
5. Migrate consumers and documentation.
6. Run static checks and Unity import/compile.
7. Run EditMode, PlayMode and relevant Editor validation.
8. Verify scenes, prefabs, ScriptableObjects and build settings.
9. Obtain QA and owner approval.
10. Delete obsolete source/assets and associated `.meta` only when safe.
11. Record removal in cleanup register and commit.

## 5. Unity-specific rules

- Never regenerate or invent `.meta` GUIDs for an asset move.
- Move the `.meta` file with its asset.
- Replacing a file in place should preserve its `.meta` when the asset identity is intended to remain the same.
- Do not delete a prefab/model/material because a C# search found no reference; scenes and serialized assets may reference it.
- Do not rename serialized fields casually. Use migration/`FormerlySerializedAs` where appropriate and validate actual assets.
- Do not delete scenes until Build Settings, references and owner intent are checked.
- Do not bulk-delete generated assets until their generator and regeneration procedure are validated.

## 6. Documentation cleanup

- One canonical document per policy.
- Other documents link to it or contain a short scoped summary.
- Historical plans and implementation reports may remain as historical evidence but must be clearly marked `HISTORICAL`.
- Do not keep two active documents with different versions of the same standard.
- Update `DOCUMENT_INDEX.md`, changelog and links in the same documentation migration.
- Do not delete old decisions or QA reports that explain why a design exists.

## 7. Compliance register

Track each candidate with:
- ID;
- path;
- category;
- owner;
- reason;
- references found;
- replacement;
- migration steps;
- validation evidence;
- status;
- removal commit.

## 8. Definition of clean

A module is clean when:
- each responsibility has one owner;
- no forbidden dependency remains;
- no duplicate active implementation remains without a documented reason;
- all consumers use the approved contract;
- tests and preview paths are maintained;
- obsolete code/docs are removed or explicitly historical;
- Unity references and GUIDs are valid;
- Unity compile and relevant tests pass.

A cleanup task is not accepted merely because the folder tree looks tidy.
