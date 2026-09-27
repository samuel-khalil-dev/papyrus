# M0 — Architectural Guardrails

## Objective
Establish clean structural boundaries before further feature milestones add surface area.

## Background
At milestone definition the Admin API's endpoints resided in a single large file
and Presentation-layer Infrastructure references were unaudited. Endpoint grouping
has since been delivered; the error-response and Presentation-to-Infrastructure
items remain open and are tracked as issue #53.

## Status
One of four Definition-of-Done items is met. The three open items are uniform
`ProblemDetails` error responses, the Presentation-to-Infrastructure reference
audit, and the fitness test that would enforce that audit.

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
- [x] All endpoints organized into feature-scoped groups. — Delivered by `src/Papyrus.Api.Admin/Endpoints/`; 23 versioned registrations and their order are unchanged.
- [ ] All error responses conform to ProblemDetails. — Open: the media upload and delete paths return plain-text 400/409 (`MediaEndpoints.cs`); all other Admin endpoints return `ValidationProblem` via `Endpoints/AdminEndpointSupport.cs`.
- [ ] No Presentation file outside a composition root references Infrastructure. — Open: the audit found `PapyrusDbContext` injected into the post, project, taxonomy, and media endpoint groups, into `Endpoints/AdminEndpointSupport.cs`, and into `AdminUi/AdminContentSupport.cs`. Writes reach the Application command handlers, but endpoint reads and the tag and media-reference writes bypass `IContentQueryStore` and `ITaxonomyCommandStore`.
- [ ] A fitness test enforces this and passes in CI. — Open: `LayerDependencyTests` constrains Core, Contracts, Web.Shared, the public delivery hosts, and Application, but not the Presentation-to-Infrastructure boundary. A public-host rule must exempt composition roots, which resolve Infrastructure through `AddPapyrusInfrastructure()`.

## Test plan
- Full regression of unit/integration/e2e suites.
- New fitness test for the reference boundary.
