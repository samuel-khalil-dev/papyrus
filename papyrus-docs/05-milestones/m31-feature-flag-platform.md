# M31 — Feature Flag Platform

## Objective
Enable safe, reversible rollout of new capabilities without requiring a redeploy.

## Background
Every prior milestone ships directly into production behavior at deploy time.

## Scope

### In scope
- Introduce the FeatureFlag aggregate.
- Build an Admin screen for managing flags.
- Integrate flag checks at key extension points established by prior milestones.

### Out of scope
- A full experimentation/A-B testing platform.

## Domain changes
- FeatureFlag: key, enabled flag, description, scope.

## API changes
- New Admin endpoint group for flag management.

## UI/Presentation changes
- Admin feature-flag management screen.

## Non-functional requirements
- Evaluation must be fast enough for the page-render hot path.

## Risks & mitigations
- Flags left enabled indefinitely accumulate as technical debt.
- Mitigation: each flag documents its purpose and expected removal point.

## Definition of Done
- [ ] A capability can be toggled from Admin without redeploy.
- [ ] At least one prior milestone's feature is retrofitted behind a flag as a working example.

## Test plan
- Unit test for flag evaluation logic.
- Performance test confirming no render-time regression.
