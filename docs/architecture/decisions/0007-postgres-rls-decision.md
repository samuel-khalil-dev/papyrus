# ADR 0007: Postgres Row-Level Security Decision

## Status

Accepted.

## Context

Tenant isolation is a Phase 1 (P0) requirement. The current enforcement model is
application-level tenant filtering, established by [ADR 0002](0002-tenant-enforcement.md)
and implemented in [`PapyrusDbContext`](../../src/Papyrus.Infrastructure/PapyrusDbContext.cs:32):

- A scoped [`TenantContext`](../../src/Papyrus.Infrastructure/MultiTenancy/TenantContext.cs:7)
  resolves the tenant from the `X-Tenant-Id` header (or the configured default,
  `main`), validates it against a strict `^[a-z0-9][a-z0-9-]{0,62}$` pattern, and
  normalizes it.
- EF Core global query filters scope every tenant-owned entity
  (`Project`, `BlogPost`, `ContentCategory`, `ContentTag`, `ProjectTag`,
  `BlogPostTag`) to the current tenant and exclude soft-deleted rows.
- [`EnforceTenantBoundary()`](../../src/Papyrus.Infrastructure/PapyrusDbContext.cs:52)
  stamps `TenantId` on added entities and throws on any attempt to save an entity
  whose tenant differs from the current context, so the filter is not the sole
  control.

The threat model records cross-tenant read or mutation as **T-01, Closed; Phase 1**,
with the required control being "scoped tenant context, EF global filters,
save-boundary enforcement, tenant-partitioned media, and adversarial A/B tests"
([`threat-model.md`](../security/threat-model.md:13)). The roadmap's EF/PostgreSQL
standard states that "filters are defense in depth, not the sole control"
([`roadmap.md`](../project/roadmap.md:140)), and Phase 1 item 4 explicitly asks to
"evaluate PostgreSQL row-level security as defense in depth and document the
enforcement point" ([`roadmap.md`](../project/roadmap.md:54)).

This ADR records that evaluation. It is the optional S30.12 (P2) subtask of
Phase 6 and does not change production code, tests, or infrastructure.

## Decision

Decline PostgreSQL row-level security (RLS) as a second tenant-isolation
enforcement layer for the current architecture. Application-level tenant
filtering remains the sole enforcement layer.

The decision is not a rejection of RLS as a technique. It is a scoped decision
that the operational cost and migration complexity of RLS are not justified
today, because the application already enforces tenant filtering at the query
layer with dedicated adversarial and PostgreSQL integration test coverage, and
the threat model already records cross-tenant isolation as closed.

## Options considered

### Option A — Adopt Postgres RLS as a second enforcement layer

RLS would add a database-enforced policy so that even a query that bypassed the
application filter could not read or mutate another tenant's rows.

- **Operational cost:** Every tenant-owned table would need a policy, a
  `SET app.current_tenant` (or equivalent) per connection/session, and a
  migration to enable RLS and backfill policies. The single shared
  `PapyrusDbContext` connection would need per-request tenant context propagated
  to the database, and the migration/seed identity would need a bypass role.
- **Migration complexity:** RLS interacts with the existing five applied
  migrations, the single controlled migration owner ([ADR 0005](0005-migration-ownership.md)),
  and the Npgsql retry execution strategy. It would add a new migration and a
  new class of failure (policy misconfiguration) to the release gate.
- **Test coverage:** The existing suite already proves isolation at the
  application layer. RLS would require a new policy-boundary integration test to
  prove the database layer independently, adding to the 73 .NET tests and 5
  Playwright browser tests baseline.
- **Benefit:** Defense in depth against a future query path that omits the EF
  filter (for example, raw SQL or a new read model). This is a real but
  currently hypothetical risk: no such path exists in the codebase today.

### Option B — Keep application-level filtering only (selected)

- The application already enforces tenant filtering at the query layer via EF
  global filters, backed by save-boundary enforcement that rejects cross-tenant
  writes even when an entity is attached by id
  ([`TenantIsolationTests.cs`](../../src/Papyrus.Tests/TenantIsolationTests.cs:56)).
- Adversarial A/B tests prove a tenant cannot read, enumerate, or mutate another
  tenant's data across every tenant-owned entity
  ([`TenantIsolationTests.cs`](../../src/Papyrus.Tests/TenantIsolationTests.cs:11)),
  and that published feed/sitemap source queries cannot include another tenant
  ([`TenantIsolationTests.cs`](../../src/Papyrus.Tests/TenantIsolationTests.cs:35)).
- PostgreSQL integration tests verify the tenant and soft-delete filters apply
  to entities and relationships against a real provider
  ([`FilterAndProjectionTests.cs`](../../src/Papyrus.Tests/Integration/FilterAndProjectionTests.cs:10)).
- The threat model already records T-01 as **Closed; Phase 1**, so RLS would not
  close an open finding; it would add a redundant layer.

## Consequences

- **What changes:** Nothing in production code, tests, or infrastructure. The
  application-level tenant filtering remains the sole enforcement layer, and the
  enforcement point is documented here as the EF global query filters and
  save-boundary enforcement in [`PapyrusDbContext`](../../src/Papyrus.Infrastructure/PapyrusDbContext.cs:32).
- **What stays:** The scoped [`TenantContext`](../../src/Papyrus.Infrastructure/MultiTenancy/TenantContext.cs:7),
  the EF global filters, the save-boundary enforcement, tenant-partitioned media,
  and the adversarial isolation tests remain the compensating controls.
- **Compensating controls (documented):**
  1. **Adversarial isolation tests** — [`TenantIsolationTests.cs`](../../src/Papyrus.Tests/TenantIsolationTests.cs:1)
     proves cross-tenant read, enumeration, and mutation are rejected, including
     the attach-by-id mutation case.
  2. **PostgreSQL integration tests** — [`FilterAndProjectionTests.cs`](../../src/Papyrus.Tests/Integration/FilterAndProjectionTests.cs:10)
     verifies the tenant/soft-delete filters against a real PostgreSQL provider.
  3. **Review gates** — the threat model (T-01) and the release gates
     ([`release-gates.md`](../release-gates.md:12)) require the ADRs and threat
     model to be reviewed and linked from the change record at every release.
  4. **Roadmap standard** — the EF/PostgreSQL standard keeps "filters are defense
     in depth, not the sole control" as a standing requirement, so any future
     query path that bypasses the filter is a review gate.
- **Revisit condition:** Re-evaluate RLS if a raw-SQL or non-EF read/write path
  is introduced, if a second database consumer (background worker, outbox
  consumer, or service extraction) is added, or if a measured bottleneck or
  ownership change justifies database-enforced isolation. This aligns with the
  Phase 8 decision gate, which defers distributed complexity until measured
  requirements justify it.

## Alternatives considered

- **Adopt RLS now.** Rejected: the application already enforces tenant filtering
  at the query layer with dedicated adversarial and PostgreSQL integration test
  coverage, T-01 is already closed, and the operational cost and migration
  complexity are not justified for a currently hypothetical bypass risk.
- **Adopt RLS only for a subset of tables.** Rejected: partial RLS would add
  policy complexity without a coherent defense-in-depth boundary, and the
  application filter already covers all tenant-owned tables uniformly.