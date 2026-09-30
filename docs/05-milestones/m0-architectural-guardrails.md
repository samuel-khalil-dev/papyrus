# M0 — Architectural Guardrails

## Objective
Establish clean structural boundaries before further feature milestones add surface area.

## Background
At milestone definition the Admin API's endpoints resided in a single large file
and Presentation-layer Infrastructure references were unaudited. Both are now
resolved: endpoint grouping and the uniform error-response contract are
delivered, and the reference audit produced
[ADR-0010](../02-architecture/decisions/adr-0010-scoped-presentation-to-infrastructure-access.md).

## Status
All four Definition-of-Done items are met. The milestone is complete. ADR-0010
scopes the Presentation-to-Infrastructure boundary and records the port
extraction that remains follow-on work rather than a gate.

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
- [x] All endpoints organised into feature-scoped groups. — Delivered by `src/Papyrus.Api.Admin/Endpoints/`; 23 versioned registrations and their order are unchanged.
- [x] All error responses conform to ProblemDetails. — Delivered: `MediaEndpoints` returns `Results.Problem` for a missing file, an unreadable multipart body, a rejected upload, and a referenced-asset delete, and the endpoint metadata declares `ProducesProblem` for 400 and 409. Covered by four `AdminApiTests` cases.
- [x] No Presentation file outside a composition root references Infrastructure. — Audited and closed by [ADR-0010](../02-architecture/decisions/adr-0010-scoped-presentation-to-infrastructure-access.md). The audit found every Infrastructure reference in `Papyrus.Api.Admin` inside the entry point, `Endpoints/`, or `AdminUi/`, and the only data-access reference in each public host inside its own entry point, so the accepted shape is the baseline. Extracting the Admin read paths and the feed and sitemap queries behind Application ports is recorded there as follow-on work, not a gate.
- [x] A fitness test enforces this and passes in CI. — `LayerDependencyTests` now carries three ADR-0010 rules: public hosts may not reference Infrastructure outside their entry point, no public host may reference the `Ef*Store` adapters, and Admin Presentation code outside its composition root may not reach Infrastructure except `ITenantContext`. The rules were verified to fail on an introduced violation before the probe was removed.

## Test plan
- Full regression of unit/integration/e2e suites.
- New fitness test for the reference boundary.
