# M0 — Architectural Guardrails

## Objective
Establish clean structural boundaries before further feature milestones add surface area.

## Background
The Admin API's endpoints reside in a single large file; Presentation-layer Infrastructure references are unaudited.

## Scope

### In scope
- Split Admin API endpoints into feature-grouped classes.
- Standardize error handling on ProblemDetails.
- Audit Presentation projects for direct Infrastructure references outside the composition root.
- Add an architecture fitness test enforcing this boundary.

### Out of scope
- No new domain functionality.

## Domain changes
- No new aggregates.

## API changes
- Endpoints reorganized, not functionally changed.

## UI/Presentation changes
- Admin UI continues to function unchanged.

## Non-functional requirements
- Endpoint groups exceeding ~300 lines are a signal to split further.

## Risks & mitigations
- Standardizing error handling touches many endpoints at once.
- Mitigation: full regression run before merge.

## Definition of Done
- [ ] All endpoints organized into feature-scoped groups.
- [ ] All error responses conform to ProblemDetails.
- [ ] No Presentation file outside a composition root references Infrastructure.
- [ ] A fitness test enforces this and passes in CI.

## Test plan
- Full regression of unit/integration/e2e suites.
- New fitness test for the reference boundary.
