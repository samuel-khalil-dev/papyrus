# ADR-0004: Retain multi-tenancy-ready entity contracts in a single-tenant deployment

**Status:** Accepted

## Context
A fully single-tenant model would be simpler short-term.

## Decision
All entities implement `IEntity`/`ITenantEntity`; scoping applied uniformly via infrastructure interceptors.

## Consequences
Positive: isolation boundary exists and is tested at negligible cost. Negative: minor conceptual overhead; accepted as cheap insurance.

## Related Decision: Row-Level Security Declined

A separate, already-made decision (the RLS evaluation history below) establishes that **application-level tenant enforcement is the sole enforcement layer** — implemented via EF Core global query filters and an explicit `EnforceTenantBoundary()` check, not PostgreSQL Row-Level Security. RLS was evaluated and deliberately declined as a second layer. This ADR's entity-contract retention (`ITenantEntity`/`IEntity`) is the mechanism that makes that single application-level layer possible; it does not imply a database-level enforcement layer exists or is planned.

### Enforcement mechanics

The application-level enforcement mechanics described below and implemented in [`PapyrusDbContext`](../../../src/Papyrus.Infrastructure/PapyrusDbContext.cs:32) are:

- A scoped [`TenantContext`](../../../src/Papyrus.Infrastructure/MultiTenancy/TenantContext.cs:7) resolves the tenant from the `X-Tenant-Id` header (or the configured default, `main`), validates it against a strict `^[a-z0-9][a-z0-9-]{0,62}$` pattern, and normalizes it.
- EF Core global query filters scope every tenant-owned entity (`Project`, `BlogPost`, `ContentCategory`, `ContentTag`, `ProjectTag`, `BlogPostTag`) to the current tenant and exclude soft-deleted rows.
- [`EnforceTenantBoundary()`](../../../src/Papyrus.Infrastructure/PapyrusDbContext.cs:52) stamps `TenantId` on added entities and throws on any attempt to save an entity whose tenant differs from the current context, so the filter is not the sole control.

The threat register records cross-tenant read or mutation as **T-01, Closed; Phase 1**, with the required control being "scoped tenant context, EF global filters, save-boundary enforcement, tenant-partitioned media, and adversarial A/B tests" ([`threat-register.md`](../../04-engineering-standards/threat-register.md)).

### RLS evaluation history

Tenant isolation is a Phase 1 (P0) requirement; the RLS decision was evaluated under the application-level filtering model above. The evaluation was recorded as the optional S30.12 subtask of Phase 6 (below) and does not change production code, tests, or infrastructure.

**Option A — Adopt Postgres RLS as a second enforcement layer** would add a database-enforced policy so that even a query that bypassed the application filter could not read or mutate another tenant's rows:

- **Operational cost:** Every tenant-owned table would need a policy, a `SET app.current_tenant` (or equivalent) per connection/session, and a migration to enable RLS and backfill policies. The single shared `PapyrusDbContext` connection would need per-request tenant context propagated to the database, and the migration/seed identity would need a bypass role.
- **Migration complexity:** RLS interacts with the existing five applied migrations, the single controlled migration owner ([migration ownership](../../06-operations/deployment-guide.md#migration-ownership)), and the Npgsql retry execution strategy. It would add a new migration and a new class of failure (policy misconfiguration) to the release gate.
- **Test coverage:** The existing suite already proves isolation at the application layer. RLS would require a new policy-boundary integration test to prove the database layer independently, adding to the current 86-test CI .NET baseline and the separate 5-test Playwright browser baseline.
- **Benefit:** Defense in depth against a future query path that omits the EF filter (for example, raw SQL or a new read model). This is a real but currently hypothetical risk: no such path exists in the codebase today.

**Option B — Keep application-level filtering only (selected)**:

- The application already enforces tenant filtering at the query layer via EF global filters, backed by save-boundary enforcement that rejects cross-tenant writes even when an entity is attached by id ([`TenantIsolationTests.cs`](../../../src/Papyrus.Tests/TenantIsolationTests.cs:56)).
- Adversarial A/B tests prove a tenant cannot read, enumerate, or mutate another tenant's data across every tenant-owned entity ([`TenantIsolationTests.cs`](../../../src/Papyrus.Tests/TenantIsolationTests.cs:11)), and that published feed/sitemap source queries cannot include another tenant ([`TenantIsolationTests.cs`](../../../src/Papyrus.Tests/TenantIsolationTests.cs:35)).
- PostgreSQL integration tests verify the tenant and soft-delete filters apply to entities and relationships against a real provider ([`FilterAndProjectionTests.cs`](../../../src/Papyrus.Tests/Integration/FilterAndProjectionTests.cs:10)).
- The threat model already records T-01 as **Closed; Phase 1**, so RLS would not close an open finding; it would add a redundant layer.

**Decision:** **Decline PostgreSQL row-level security (RLS)** as a second tenant-isolation enforcement layer for the current architecture. Application-level tenant filtering remains the sole enforcement layer. The decision is not a rejection of RLS as a technique; it is a scoped decision that the operational cost and migration complexity of RLS are not justified today, because the application already enforces tenant filtering at the query layer with dedicated adversarial and PostgreSQL integration test coverage, and the threat model already records cross-tenant isolation as closed.

**Consequences:**

- **What changes:** Nothing in production code, tests, or infrastructure. The application-level tenant filtering remains the sole enforcement layer, and the enforcement point is documented here as the EF global query filters and save-boundary enforcement in [`PapyrusDbContext`](../../../src/Papyrus.Infrastructure/PapyrusDbContext.cs:32).
- **What stays:** The scoped [`TenantContext`](../../../src/Papyrus.Infrastructure/MultiTenancy/TenantContext.cs:7), the EF global filters, the save-boundary enforcement, tenant-partitioned media, and the adversarial isolation tests remain the compensating controls.
- **Compensating controls (documented):**
  1. **Adversarial isolation tests** — [`TenantIsolationTests.cs`](../../../src/Papyrus.Tests/TenantIsolationTests.cs:1) proves cross-tenant read, enumeration, and mutation are rejected, including the attach-by-id mutation case.
  2. **PostgreSQL integration tests** — [`FilterAndProjectionTests.cs`](../../../src/Papyrus.Tests/Integration/FilterAndProjectionTests.cs:10) verifies the tenant/soft-delete filters against a real PostgreSQL provider.
  3. **Review gates** — the threat model (T-01) and the release gates ([`release-gates.md`](../../04-engineering-standards/release-gates.md)) require the ADRs and threat model to be reviewed and linked from the change record at every release.
  4. **Roadmap standard** — the EF/PostgreSQL standard keeps "filters are defense in depth, not the sole control" as a standing requirement, so any future query path that bypasses the filter is a review gate.
- **Revisit condition:** Re-evaluate RLS if a raw-SQL or non-EF read/write path is introduced, if a second database consumer (background worker, outbox consumer, or service extraction) is added, or if a measured bottleneck or ownership change justifies database-enforced isolation.

**Alternatives considered:**

- **Adopt RLS now.** Rejected: the application already enforces tenant filtering at the query layer with dedicated adversarial and PostgreSQL integration test coverage, T-01 is already closed, and the operational cost and migration complexity are not justified for a currently hypothetical bypass risk.
- **Adopt RLS only for a subset of tables.** Rejected: partial RLS would add policy complexity without a coherent defense-in-depth boundary, and the application filter already covers all tenant-owned tables uniformly.
