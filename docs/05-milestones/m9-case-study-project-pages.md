# M9 — Case-Study Project Pages

## Objective
Elevate project presentation into a structured case-study format.

## Background
The existing Project aggregate supports basic content but presentation doesn't communicate problem/approach/outcome.

## Scope

### In scope
- Extend project page presentation into problem/approach/stack/outcome/links.
- Surface live GitHub repository metadata (stars, last commit, language), cached server-side.

### Out of scope
- Automatic commit-history rendering.

## Domain changes
- No new aggregates; presentation-layer extension.

## API changes
- Server-side, cached GitHub API integration.

## UI/Presentation changes
- Case-study page template.
- GitHub metadata badge component.

## Non-functional requirements
- GitHub calls must be cached, not requested per page view.

## Risks & mitigations
- GitHub API rate limits/outages.
- Mitigation: server-side caching with a graceful no-metadata fallback.

## Definition of Done
- [ ] At least one flagship project renders as a full case study.
- [ ] GitHub metadata degrades gracefully if unavailable.

## Test plan
- Integration test for fetch-and-cache logic including the fallback path.
- End-to-end test verifying all case-study sections render.
