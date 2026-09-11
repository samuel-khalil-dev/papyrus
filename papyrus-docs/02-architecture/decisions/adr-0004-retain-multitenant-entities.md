# ADR-0004: Retain multi-tenancy-ready entity contracts in a single-tenant deployment

**Status:** Accepted

## Context
A fully single-tenant model would be simpler short-term.

## Decision
All entities implement `IEntity`/`ITenantEntity`; scoping applied uniformly via infrastructure interceptors.

## Consequences
Positive: isolation boundary exists and is tested at negligible cost. Negative: minor conceptual overhead; accepted as cheap insurance.

## Related Decision: Row-Level Security Declined

A separate, already-made decision (`docs/architecture/decisions/0007-postgres-rls-decision.md`) establishes that **application-level tenant enforcement is the sole enforcement layer** — implemented via EF Core global query filters and an explicit `EnforceTenantBoundary()` check, not PostgreSQL Row-Level Security. RLS was evaluated and deliberately declined as a second layer. This ADR's entity-contract retention (`ITenantEntity`/`IEntity`) is the mechanism that makes that single application-level layer possible; it does not imply a database-level enforcement layer exists or is planned.
