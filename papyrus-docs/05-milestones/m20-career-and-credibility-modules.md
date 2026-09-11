# M20 — Career & Credibility Modules

## Objective
Add structured, admin-managed content directly supporting the career-development purpose.

## Background
The About section relies on unstructured narrative alone.

## Scope

### In scope
- Introduce SkillEntry, TimelineEntry, Testimonial, Certification.
- Build corresponding Admin management screens.
- Build Portfolio presentation components.
- Implement an on-demand generated CV/PDF export from structured data.

### Out of scope
- Automatic import from external sources.

## Domain changes
- SkillEntry, TimelineEntry, Testimonial, Certification aggregates as previously specified.

## API changes
- New Admin endpoint groups for each aggregate.
- New route producing the generated CV/PDF.

## UI/Presentation changes
- Admin management screens.
- Portfolio About section extended.
- CV download action.

## Non-functional requirements
- The generated PDF must remain visually consistent with the brand identity.

## Risks & mitigations
- Four aggregates in one milestone is a larger unit of work.
- Mitigation: delivered as sequential vertical-slice PRs within the milestone.

## Definition of Done
- [ ] All four aggregates are fully manageable from Admin.
- [ ] The generated CV reflects current data and matches the brand.

## Test plan
- Integration tests for all four adapters.
- Manual review of the generated CV's quality and accuracy.
