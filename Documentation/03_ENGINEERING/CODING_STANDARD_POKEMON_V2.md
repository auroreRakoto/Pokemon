# Pokémon v2 — Code Standard

- **Status:** APPROVED — owner confirmation in chat, 2026-11-19
- **Version:** 1.0
- **Scope:** C# and Python source code in Pokémon v2
- **Authority:** project-owner decision; this document supplements `CODING_STANDARDS.md`

## 1. Mandatory rules

### 1.1 Maximum 10 functions per source file

A source file may declare **at most 10 functions/methods**.

Count:
- C#: constructors, methods, static methods, Unity lifecycle callbacks, event handlers and local functions.
- Python: `def` and `async def`, including nested functions.

Do not count:
- properties and fields;
- C# property accessors (`get`/`set`) as separate functions;
- lambdas and anonymous callbacks.

If a file exceeds the limit, split code by coherent responsibility. Do not create arbitrary `Helper`, `Utils` or `Common` files solely to evade the limit. A helper belongs in a separate file only when it owns a clear, reusable or independently testable responsibility.

### 1.2 Maximum 50 lines per function

A function body may contain **at most 50 logical code lines**.

Counting convention:
- exclude blank lines;
- exclude comment-only lines;
- include executable statements and structural lines that form part of the body;
- count a multiline statement on each physical line containing code;
- do not count the function signature or surrounding class declaration.

A function over 50 lines must be decomposed around meaningful operations while preserving its observable behaviour and error handling.

### 1.3 One short description above every function

Every declared function must have a short description immediately above it.

- Prefer one concise sentence describing intent or responsibility.
- The description must add meaning; do not merely repeat the function name.
- Public C# APIs may use XML documentation where appropriate, with a concise summary.
- Python functions may use a one-line docstring when that is the local convention; otherwise use a concise comment.
- Keep comments accurate when code changes.

## 2. Refactoring principles

- Preserve existing public contracts unless a work package explicitly approves a contract change.
- Preserve serialized Unity references, asset GUIDs and `.meta` files unless a migration is explicitly planned and validated.
- Do not split code into meaningless micro-functions to satisfy a counter.
- Prefer cohesive classes and modules with one clear responsibility.
- Do not introduce dependencies, frameworks, global mutable state or architecture changes without approval.
- Do not change gameplay behaviour as part of a style-only refactor.
- Generated or third-party source may be exempt only when identified and documented; do not silently exempt project-authored code.

## 3. Exceptions

An exception requires:
1. the exact file and function;
2. the reason the limit cannot reasonably be met;
3. why decomposition would harm clarity, performance or a required framework contract;
4. the approving owner decision.

An exception is local and explicit; it does not waive the rule for the rest of the project.

## 4. Validation

For each compliance work package:
- report function counts per file;
- report every function exceeding 50 logical code lines;
- verify every function has its required description;
- run available language-level/static checks;
- run available project tests;
- ask the owner to run Unity compilation and relevant Editor/PlayMode checks where the environment is owner-operated.

Never report a check as passed unless it was actually run. Use `NOT RUN`, `BLOCKED` or `N/A` with a reason as appropriate.

## 5. Relationship to existing standards

This document adds measurable limits to `Documentation/03_ENGINEERING/CODING_STANDARDS.md`. Existing requirements remain in force, including domain/presentation separation, narrow public interfaces, Unity Editor/runtime separation, deterministic and idempotent tooling, and clear diagnostics.

This standard does not alter module ownership, network authority, data ownership, roadmap sequencing or any confirmed architecture decision.
