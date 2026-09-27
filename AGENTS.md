# AGENTS.md

Guidance for AI coding agents working in the **Papyrus** repository.
Papyrus is a **.NET 10 modular monolith** publishing technical writing and
portfolio work through read-only public hosts with an authenticated admin
surface. This file is intentionally minimal: it captures only what is hard to
discover and easy to get wrong. Link to the docs below for detail.

## Verify and build

```powershell
dotnet restore Papyrus.sln
dotnet build Papyrus.sln --nologo --no-incremental      # warnings-as-errors
dotnet build Papyrus.sln --configuration Release --no-restore /p:TreatWarningsAsErrors=true
dotnet test Papyrus.sln                                  # full suite incl. integration
dotnet test Papyrus.sln --filter "FullyQualifiedName!~Papyrus.Tests.Integration"  # non-integration only
npm ci --prefix src/Papyrus.Admin.UI
npm run build --prefix src/Papyrus.Admin.UI
npm run lint --prefix src/Papyrus.Admin.UI
```

`TreatWarningsAsErrors`, `EnableNETAnalyzers`, `AnalysisLevel=latest` and
`Nullable` are enforced globally via [Directory.Build.props](Directory.Build.props).
A zero-warning build is a hard gate. Postgres-backed integration tests run in
CI against the `postgres:17-alpine` service; they are not guaranteed to run on
a fresh workstation.

## Boundaries (do not cross)

- **`Papyrus.Core`** — domain entities, tenant markers, safe Markdown rendering.
- **`Papyrus.Contracts`** — explicit REST request/response and pagination contracts.
- **`Papyrus.Application`** — validated CQRS commands/queries, bounded DTO projections (no-tracking), and application-owned **ports** (`IPublicContentStore`, `IContentCommandStore`, `IContentQueryStore`, `ITaxonomyCommandStore`, `IContentLifecycleStore`) with thin facades. Must **not** depend on `Papyrus.Infrastructure` or `PapyrusDbContext`.
- **`Papyrus.Infrastructure`** — EF Core, filters, migration, seed, save enforcement, and the **adapters** that implement the Application ports (`Ef*Store`). Depends on `Papyrus.Application`.
- **`Papyrus.Api.Admin`** — authenticated Minimal API, media, SPA hosting, **sole** migration/seed owner.
- **`Papyrus.Web.Blog` / `Papyrus.Web.Portfolio`** — read-only public hosts. Never migrate, never write.
- **`Papyrus.Web.Shared`** — shared public layout and site helpers.

Dependency direction: `Application → Contracts, Core`; `Infrastructure → Application, Contracts, Core`.
Never add a `Papyrus.Infrastructure` reference to `Papyrus.Application` (that would recreate the
inverted dependency the port/adapter split removed).

A **Presentation** project may reference Infrastructure only inside its composition
root, as defined and enforced by
[ADR-0010](docs/02-architecture/decisions/adr-0010-scoped-presentation-to-infrastructure-access.md).
The read-only public hosts reach Infrastructure only from their entry point and never
reference an `Ef*Store` adapter; Admin's composition root is its entry point plus
`Endpoints/` and `AdminUi/`. Fitness tests enforce this, so widening it requires a
superseding ADR rather than a test edit. Note that the Admin endpoint handlers still
inject `PapyrusDbContext` directly instead of reaching the Application ports — that is
the accepted current shape under ADR-0010, not a pattern to copy. `ITenantContext` also
lives in `Papyrus.Infrastructure.MultiTenancy` while being consumed by Presentation code,
so the assembly does not imply a clean seam.

`Papyrus.Admin.UI` (React SPA) is **not** a `.sln` project. `src/` has 11
projects; the SPA is a standalone npm app referenced by Aspire as dev-only and
built into the admin image via a Node stage.

## Conventions that differ from defaults

- **Tenant enforcement is application-level, not RLS** (ADR 0004). `TenantContext`
  resolves `X-Tenant-Id` (default `main`, regex `^[a-z0-9][a-z0-9-]{0,62}$`);
  EF global filters plus `EnforceTenantBoundary()` are the sole enforcement layer.
- **Deletes are soft deletes.** `PapyrusSaveChangesInterceptor` converts deletes
  and sets audit timestamps/actors. Global filters exclude soft-deleted rows.
  Never hard-delete unless the migration owner explicitly does so.
- **Only Admin migrates/seeds**, through `InitializeDatabaseAsync()`. Never call
  EF migration APIs from Blog/Portfolio.
- **Admin auth is API-key based.** `ApiKeyAuthenticationHandler` reads
  `X-Admin-Api-Key`, constant-time `AdminApiKey.MatchesAny`. Multi-key
  `AdminApi:ApiKeys` rotation with `AdminApi:ApiKey` compatibility alias.
  Production **fails fast** if no key or `change-me`. No browser session cookie.
- **Telemetry:** shared `Papyrus` ActivitySource + W3C propagation; audit events
  logger-backed and never sampled; keys/cookies/bodies/raw secrets never recorded.
- **Generated artifacts are reviewable evidence:** `docs/api/openapi.json` and
  the Admin UI `src/api/schema.d.ts` are generated and diff-checked in CI via
  `.ExcludeFromDescription()` SPA routes and the `AdminOpenApiDocumentTransformer`.
  Regenerate, don't hand-edit, and keep them in sync.

## Adding an EF migration

```powershell
dotnet ef migrations add <Name> `
  --project src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj `
  --startup-project src/Papyrus.Api.Admin/Papyrus.Api.Admin.csproj
```

Review the generated migration and model snapshot, run migration-drift tests
and the full build. Never edit/delete an applied migration. Release applies
migrations with `dotnet-ef 10.0.11` in a dedicated `migrate` job.

## Operations caveats

- **Admin API throws at startup** in non-Development environments without a
  configured API key. CI OpenAPI steps set `ASPNETCORE_ENVIRONMENT=Development`
  and `Papyrus__SkipDatabaseInitialization=true` for this reason.
- Production Release workflow requires a **self-hosted runner** plus secrets
  (`PRODUCTION_CONNECTION_STRING`, `POSTGRES_PASSWORD`, `ADMIN_API_KEY`,
  `BLOG_PUBLIC_BASE_URL`, `PORTFOLIO_PUBLIC_BASE_URL`).
- `tests/e2e` and `tests/load` are separate npm projects with their own deps
  (Playwright/Chromium, k6).
- CI uses shallow-checkout-immune secret scanning and OpenAPI diffing that is
  CRLF-aware; pinned GitHub Actions to immutable commits and pinned container
  digests is the established pattern.

## Review gates and security posture

- **Agent review playbook:** [docs/07-governance/agent-review-playbook.md](docs/07-governance/agent-review-playbook.md)
  is a focused checklist (OWASP-mapped security, architecture boundaries,
  correctness, report format) for running a review in this repo.
- **OWASP-informed, production-readiness review:** see
  [docs/04-engineering-standards/security-baseline.md](docs/04-engineering-standards/security-baseline.md) (threat model, policy,
  audit register, risk acceptance) and
  [docs/04-engineering-standards/threat-register.md](docs/04-engineering-standards/threat-register.md).
  The list of closed controls vs. remaining release gates is the review
  reference for what must be true before merge.
- **Architecture review:** ADRs in [docs/02-architecture/decisions](docs/02-architecture/decisions/)
  are the authoritative record (modular monolith, tenant enforcement, REST
  contracts, migration ownership, no RLS). Respect accepted decisions unless an
  ADR change is proposed.
- **Release gates:** [docs/04-engineering-standards/release-gates.md](docs/04-engineering-standards/release-gates.md)
  defines what must be satisfied before a release/tag.
- **Performance evidence:** [docs/04-engineering-standards/performance-budget.md](docs/04-engineering-standards/performance-budget.md)
  holds PostgreSQL projection and query-plan evidence; keep claims aligned.
- **Feature claims:** keep code, docs, and the
  [feature matrix](docs/00-overview/feature-matrix.md) /
  [limitations](docs/00-overview/limitations.md) in sync — drift is treated as a review failure.

## Documentation index

- [docs/README.md](docs/README.md) — documentation home and current status.
- [docs/02-architecture/architecture-overview.md](docs/02-architecture/architecture-overview.md) — runtime shape, C4 views, security boundaries.
- [docs/01-product/getting-started-tutorial.md](docs/01-product/getting-started-tutorial.md) with [docs/06-operations/environments.md](docs/06-operations/environments.md) and [docs/06-operations/deployment-guide.md](docs/06-operations/deployment-guide.md) — run and deploy.
- [docs/07-governance/contribution-workflow.md](docs/07-governance/contribution-workflow.md) — contribution and PR conventions.
- [docs/04-engineering-standards/security-baseline.md](docs/04-engineering-standards/security-baseline.md) — security review reference.
- [docs/00-overview/changelog.md](docs/00-overview/changelog.md), [docs/Papyrus_Roadmap.md](docs/Papyrus_Roadmap.md).
- [docs/07-governance/documentation-maintenance.md](docs/07-governance/documentation-maintenance.md) — the single source of truth is `docs/`. Do not keep a parallel, untracked copy of project facts; session and environment context belongs to agent tooling, not to a second documentation tree.

## Contributing rules for agents

- Scoped branches, Conventional Commit subjects (`feat:`, `fix:`, `docs:`, `test:`, `refactor:`, `chore:`).
- Never commit secrets, connection strings, or unreviewed generated output.
- Update source-aligned documentation when behavior or an operational contract changes (the docs are treated as reviewable evidence).
- Keep unrelated formatting or generated-file changes out of a change set.
