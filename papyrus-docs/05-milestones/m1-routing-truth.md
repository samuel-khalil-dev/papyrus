# M1 — Routing Truth

## Objective
Make Portfolio-as-root / Blog-as-subdomain an explicit, enforced application-level configuration.

## Background
SiteOptions/SiteUrl do not currently encode which site owns which host.

## Scope

### In scope
- Define explicit host configuration for Portfolio (root) and Blog (subdomain).
- Update Aspire AppHost and docker-compose bindings.
- Add a test asserting correct host resolution.

### Out of scope
- The actual internet-facing TLS/routing layer (see M2).

## Domain changes
- No new aggregates; configuration-only.

## API changes
- No breaking API changes.

## UI/Presentation changes
- No new screens.

## Non-functional requirements
- Must work identically in local, containerized, and production environments.

## Risks & mitigations
- Misconfigured host mapping could make a site unreachable.
- Mitigation: automated resolution test plus manual per-environment verification.

## Definition of Done
- [ ] Root domain resolves exclusively to Portfolio in all environments.
- [ ] Blog reachable only via subdomain in all environments.
- [ ] An automated test enforces the rule.

## Test plan
- Integration test for site resolution per host.
- Manual verification per environment.
