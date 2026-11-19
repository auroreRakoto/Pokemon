# WP-XXX — <Feature name>

- **Status:** PROPOSED
- **Owner module:**
- **Priority:**
- **Estimate:**
- **Roadmap phase:**
- **Dependencies:**
- **Target milestone:**

## 1. Goal

Describe one observable outcome. Avoid implementation-first descriptions.

## 2. User/developer value

What becomes possible or easier when this is accepted?

## 3. Scope

### Included
- 

### Explicitly excluded
- 

## 4. Ownership and architecture

- Owning module:
- Authoritative state owner:
- Composition root:
- Public contracts consumed:
- Public contracts changed:
- Allowed dependencies:
- Forbidden dependencies:
- Data category (definition/instance/generated/derived/presentation/diagnostic):

## 5. Current-state audit

- Existing implementation:
- Existing tests:
- Existing assets/prefabs:
- Existing Editor tools/previews:
- Callers/consumers:
- Known defects:
- Evidence inspected:

## 6. Design

### Inputs
| Input | Type | Unit/range | Default | Validation |
|---|---|---|---|---|

### Outputs
| Output | Type | Owner | Observable result |
|---|---|---|---|

### Invariants
- 

### Failure behaviour
- 

### Determinism
- Seed:
- Generator/version:
- Same-input expectation:
- Different-seed expectation:

## 7. Independent module validation

- Preview type:
- Preview entry point:
- Fixture/profile:
- Seed:
- What must be visible:
- Diagnostics:
- Clear/reset behaviour:

The feature must be testable without relying solely on the full playable scene.

## 8. Implementation plan

| Step | Change | Files | Depends on | Output |
|---|---|---|---|---|

## 9. Tests and acceptance

| Test category | Test / expected result | Evidence | Required |
|---|---|---|---|
| Unit/EditMode | | | |
| Determinism | | | |
| Boundary/invariants | | | |
| Failure behaviour | | | |
| Module preview | | | |
| Contract/integration | | | |
| PlayMode | | | |
| Regression | | | |
| Performance | | | |
| Manual visual/UX | | | |

### Acceptance criteria
- [ ] 
- [ ] 

## 10. Risks and rollback

- Risks:
- Serialized data/scene/prefab impact:
- `.meta`/GUID impact:
- Rollback:
- Cleanup/deprecation candidates:

## 11. Documentation impact

- [ ] Domain document
- [ ] Architecture/contracts
- [ ] Test standard/test plan
- [ ] Feature/file index
- [ ] Roadmap/development queue
- [ ] Decision register
- [ ] Changelog

## 12. Work breakdown

| Subtask | Output | Estimate | Validation |
|---|---|---:|---|

## 13. Status report

- Implemented:
- Validated:
- Blocked:
- Not run:
- Remaining:
- Next objective:

## 14. Commit messages

### Plan milestone
`docs(<scope>): add approved WP-XXX implementation plan`

### Implementation milestone
`feat(<scope>): implement <observable outcome>`

Include the mandatory `Agent: <ID>` footer and adapt the exact message to actual changes. If incomplete, use a checkpoint message that does not claim completion.
