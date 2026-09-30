# M23 — Visitor Experience Toggle

## Objective
Offer a curated Day/Night presentation variant layered on the admin-defined base theme.

## Background
The theming system supports one admin-defined base theme with no visitor-facing variation.

## Scope

### In scope
- Define two curated token variants (Day/Night) derived from the base SiteTheme.
- Build a visitor-facing toggle persisted per visitor.
- Ensure both variants meet the accessibility standard.

### Out of scope
- Fully free-form visitor theme customisation.

## Domain changes
- No new aggregate; variants computed from the existing SiteTheme record.

## API changes
- No new authenticated API surface.

## UI/Presentation changes
- Toggle control on both public sites.

## Non-functional requirements
- Both variants must independently satisfy WCAG AA.

## Risks & mitigations
- A poorly designed variant could dilute the brand.
- Mitigation: both variants constrained by the same base tokens and reviewed against the accessibility standard.

## Definition of Done
- [ ] Visitors can toggle without a page reload.
- [ ] Both variants pass WCAG AA.

## Test plan
- Automated accessibility check on both variants.
