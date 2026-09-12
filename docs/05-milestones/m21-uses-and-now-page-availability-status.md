# M21 — Uses & Now Page, Availability Status

## Objective
Add two small, high-fit personal-brand features: a current-focus page and a visible availability signal.

## Background
Neither exists today despite being low-cost, high-relevance additions given the platform's career purpose.

## Scope

### In scope
- Build a `/now` page (current focus, admin-editable) on the Portfolio.
- Add an availability-status flag (open to work / not looking) driven by SiteSetting, displayed on the Portfolio hero.

### Out of scope
- A full "uses" gear/tooling inventory beyond the current-focus text (can be added later if desired).

## Domain changes
- No new aggregates; reuses SiteSetting (M12).

## API changes
- No new endpoints beyond existing SiteSetting management.

## UI/Presentation changes
- A `/now` page template.
- An availability badge component on the Portfolio hero.

## Non-functional requirements
- The availability status must be trivially updatable - a single toggle, not a multi-step edit.

## Risks & mitigations
- None significant; this is low-risk, additive content.

## Definition of Done
- [ ] The `/now` page renders admin-entered content.
- [ ] The availability badge reflects the current SiteSetting value and updates without a redeploy.

## Test plan
- End-to-end test verifying the availability badge reflects an Admin-side change.
