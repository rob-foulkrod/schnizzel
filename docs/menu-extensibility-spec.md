# BudgetMenu Extensibility Spec (Planning Only)

## Status
Planning document only.
No implementation changes are included in this phase.

## Objective
Improve the menu architecture so new capabilities can be added with minimal changes to existing code and low regression risk.

## Problem Statement
The current menu workflow should evolve toward modular feature addition. New options should be pluggable rather than requiring repeated edits in one central class.

## Scope
- In scope:
  - Define extensibility goals and contracts for menu actions.
  - Define composition/wiring strategy for menu modules.
  - Define test strategy for extension scenarios.
  - Define rollout plan and acceptance criteria.
- Out of scope:
  - Any production code edits.
  - Any test code edits.
  - Any changes to runtime behavior.

## Extensibility Goals
1. Add a new menu option by adding a single new module/class and registration line.
2. Preserve existing options and user-visible behavior while introducing extension points.
3. Keep orchestration concerns separate from business logic and persistence concerns.
4. Keep extension flow deterministic and testable.

## Proposed Architecture (Design Only)
### 1) Menu Action Contract
Define a contract representing one menu option:
- Key: user selection value (for example, 1, 2, 3)
- Label: display text in menu
- Execute method: performs action and returns whether app continues running

### 2) Action Registry
Introduce a registry/list that holds menu actions.
- BudgetMenu renders from this list.
- BudgetMenu dispatches by key lookup.
- BudgetMenu contains no option-specific switch blocks.

### 3) Composition Root Registration
Wire actions in Program startup.
- Existing actions become individual action classes.
- Future actions are added by adding one new action class and one registration entry.

### 4) Shared Context for Actions
Define a small shared context object so actions can access current transactions and common services without tightly coupling to BudgetMenu internals.

## Design Constraints
1. Keep current console text and flow stable unless deliberately changed.
2. Keep CSV compatibility unchanged: Date,Description,Category,Amount.
3. Avoid broad abstractions not needed for the current menu domain.
4. Preserve deterministic tests (no global Console mutation in action-level tests).

## Non-Functional Requirements
1. Open/Closed Principle at menu-option level: add options without modifying core dispatch logic.
2. High cohesion: each action class owns one user intent.
3. Low coupling: actions depend on a small context contract.
4. Testability: each action testable with fake input/output.

## Test Strategy (Future)
1. Unit tests per action class:
- successful flow
- invalid input flow
- cancellation/exit flow where applicable
2. Menu-level tests for:
- rendering registered options
- dispatch to correct action by key
- invalid key handling
3. Contract tests for registry uniqueness:
- no duplicate keys
- non-empty labels

## Rollout Plan (Future)
1. Introduce contracts and registry without changing behavior.
2. Migrate one low-risk option to validate pattern.
3. Migrate remaining options incrementally.
4. Remove legacy dispatch only after parity checks pass.

## Risks and Mitigations
- Risk: feature parity drift during migration.
  - Mitigation: option-by-option migration with baseline behavioral tests.
- Risk: registration errors (missing/duplicate keys).
  - Mitigation: startup validation and dedicated tests.
- Risk: over-engineering.
  - Mitigation: keep action contract minimal and avoid unnecessary inheritance hierarchies.

## Acceptance Criteria (For Future Implementation)
1. A new menu option can be added without editing core dispatch logic.
2. Existing menu options preserve current behavior.
3. Registry validation catches duplicate option keys.
4. Unit tests exist for action classes and registry behavior.
5. Full test suite passes after migration.

## Checklist (Planning)
- [x] Define extensibility objective and scope.
- [x] Define proposed menu action contract.
- [x] Define action registry and composition approach.
- [x] Define future test strategy.
- [x] Define rollout and risk mitigation.
- [ ] Approve implementation phase start date.
- [ ] Implement contracts and first migrated action.
- [ ] Complete incremental migration and validation.
