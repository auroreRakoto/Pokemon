# STD-001 — Formalize Code Standard and Audit Compliance

- **Status:** PROPOSED
- **Owner module:** Tools / Documentation
- **Priority:** P1
- **Dependencies:** owner-confirmed three-rule standard; current project archive
- **Target milestone:** engineering quality baseline

## 1. Goal

Establish a measurable code standard for Pokémon v2 and produce a traceable inventory of current compliance work without changing runtime behaviour.

## 2. Scope

### Included
- Add `CODING_STANDARD_POKEMON_V2.md` with the three owner-confirmed mandatory rules.
- Record the counting convention and exception process.
- Audit the supplied archive's C# and Python source files.
- Create a remediation sequence for C# and Python compliance work.
- Preserve existing coding, architecture, module and networking rules.

### Explicitly excluded
- Any source-code refactoring.
- Any Unity asset, scene, prefab, `.meta`, GUID or serialized-reference changes.
- Deleting or migrating old data/scripts.
- Introducing a linter package, framework, assembly definition or test framework.
- Resolving unrelated compile blockers or architecture conflicts.

## 3. Current-state audit

Snapshot inspected: `Pokemon-main (1).zip`, SHA-256 prefix `df3061a74844331f`.

- 37 C# files under `Assets/Project`.
- 45 Python files.
- No `.asmdef` files found.
- No C# test source files found under a Tests folder.
- No `.git` metadata in the archive; branch history and current commit cannot be verified.
- Four C# files exceed 10 detected functions.
- Four Python files exceed 10 detected functions.
- Static scan identified 12 C# functions and 5 Python functions exceeding 50 physical code-bearing lines under the scan's approximate counting convention.
- The counts are an audit signal, not a final compliance certificate; the implementation phase must use the adopted logical-line convention and inspect declarations manually.

### Additional blockers recorded, not fixed by this work package

1. `Assets/Project/Runtime/World/Biomes/WolrdDefinition.cs` and `WorldDefinition.cs` both declare `WorldDefinition`. Their `.meta` files carry the same GUID `83e7a11a16a9d2e48a0f9113a1ff6158`.
2. `PokemonAgent.cs` references `EntityId` and `GetEntityId()`, with no corresponding declaration found in the 37 C# files in this archive.
3. Legacy planar data and `Tools/generate_demo_maps.py` are still present in this snapshot. This package does not perform CLEAN-002/CLEAN-003 work.

These findings require separate bounded correction/migration work. Do not hide them inside a style refactor.

## 4. Validation

- Confirm the standard document contains only the three mandatory rules approved by the owner.
- Confirm the audit lists all source files included in its scan.
- Confirm no runtime source, Unity asset or serialized reference is changed by STD-001.
- No Unity compile or tests are claimed by this documentation-only work package.

## 5. Acceptance criteria

- [ ] The code standard is committed and pushed.
- [ ] The audit and remediation sequence are committed and pushed.
- [ ] C# and Python remediation work packages are separately scoped.
- [ ] Existing architecture and confirmed decisions remain unchanged.
- [ ] Owner accepts STD-001.

## 6. Implementation steps

1. Add the code standard document.
2. Add the source compliance audit.
3. Add the C# and Python remediation work packages.
4. Review the document index/changelog impact before implementation.
5. Commit and push the plan milestone.

## 7. Commit message

`docs(standards): define Pokemon v2 code limits and compliance audit`

Footer:
`Agent: DOC`
