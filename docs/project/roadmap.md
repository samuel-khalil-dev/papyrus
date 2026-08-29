# Papyrus Master Engineering Roadmap

**Status:** Architecture-audit baseline and implementation plan; Phase 0 finished by documented owner risk acceptance. **Scope:** this file authorizes no source, project, migration, or secret changes.

## 1. Executive blueprint and current state

Papyrus is a .NET 10 modular monolith with an authenticated admin API/UI, public blog, public portfolio, shared domain/infrastructure, PostgreSQL, and Aspire orchestration. [`Papyrus.AppHost/Program.cs`](../../src/Papyrus.AppHost/Program.cs) starts PostgreSQL and orders admin before the read-only public sites. [`Papyrus.Core`](../../src/Papyrus.Core) owns content entities and markdown safety; [`Papyrus.Infrastructure`](../../src/Papyrus.Infrastructure) owns EF configuration, migrations, and seed; [`Papyrus.Api.Admin`](../../src/Papyrus.Api.Admin) owns writes and local media; [`Papyrus.Web.Blog`](../../src/Papyrus.Web.Blog) and [`Papyrus.Web.Portfolio`](../../src/Papyrus.Web.Portfolio) own public reads; [`Papyrus.Web.Shared`](../../src/Papyrus.Web.Shared) owns shared presentation.

### Strengths to preserve

- Explicit EF configurations in [`Papyrus.Infrastructure/Configurations`](../../src/Papyrus.Infrastructure/Configurations), PostgreSQL/Npgsql, retry policy, and one admin migration owner.
- Published/live filtering, slug constraints, safe markdown rendering, media reference protection, RSS, sitemaps, accessibility work, and deployment documentation in [`deployment.md`](../guides/deployment.md).
- Aspire dependency ordering, persistent database volume, anonymous health/liveness endpoints, OpenTelemetry defaults, and 73 .NET tests plus 5 Playwright browser tests in [`Papyrus.Tests`](../../src/Papyrus.Tests).
- Clear read/write product seams, nullable C#, strict project separation, and zero-warning build history.

### Audit assessment and production blockers

- **C1–C4:** tenant isolation, API-key authentication, header-authenticated mutation protection, and hostile-media controls are implemented and closed for their code scope.
- **Security audit redo:** no critical findings; W-NEW-01…05 and O-NEW-01…05 are remediated. W6–W8, O4, and O7–O10 remain release-gate verification items.
- **Former W/O risk disposition:** current-code verification removed W1-W5, O1-O3, O5, and O6 from active risk acceptance. W6-W8, O4, and O7-O10 remain traceable operational/release-gate items; their original prose was not supplied here.

## 2. Target architecture and boundaries

Retain the modular monolith until measured scale or ownership requires extraction. Create [`Papyrus.Application`](../../src/Papyrus.Application) for commands, queries, authorization, DTO mapping, and ports. Keep [`Papyrus.Core`](../../src/Papyrus.Core) free of ASP.NET, EF, database, media, and configuration dependencies. Keep [`Papyrus.Infrastructure`](../../src/Papyrus.Infrastructure) responsible for persistence, migrations, outbox/audit storage, and adapters. Keep the admin delivery layer responsible for HTTP/API-key authentication and SPA composition, and public delivery layers read-only. Shared presentation in [`Papyrus.Web.Shared`](../../src/Papyrus.Web.Shared) must not mutate domain state.

### DDD, CQRS, and API decisions

- Use pragmatic DDD: [`Project`](../../src/Papyrus.Core/Content/Project.cs:5), [`BlogPost`](../../src/Papyrus.Core/Content/BlogPost.cs:5), taxonomy, and media references are the initial aggregates; application handlers enforce invariants and authorization.
- Use CQRS separation without separate databases first. Commands use handlers and transactions; queries use projection-specific read services.
- Use versioned REST for current page-oriented clients. Do not introduce GraphQL without a demonstrated client-composition need. Do not introduce gRPC without a proven internal throughput/service boundary.
- Create [`Papyrus.Contracts`](../../src/Papyrus.Contracts) for explicit request/response DTOs. Never expose EF entities. Standardize ASP.NET error responses, validation codes, correlation IDs, and pagination metadata.
- Publish OpenAPI from the admin API, contract-test it, and generate/validate any future client under ``clients``. CI must reject breaking schema drift; TypeScript clients must use strict named types with no untyped escape hatch.

## 3. Phase roadmap

### Phase 0 — Audit closure and guardrails (P0) — Complete

**Objective:** make every finding executable and block unsafe releases. **Paths:** [`roadmap.md`](roadmap.md), [`memory-bank/activeContext.md`](../../memory-bank/activeContext.md), [`memory-bank/progress.md`](../../memory-bank/progress.md), create [`threat-model.md`](../security/threat-model.md), [`decisions`](../architecture/decisions), ``Papyrus.Tests/ArchitectureTests.cs``, and ``scripts``.

1. Catalogue C1-C4, W1-W8, and O1-O10 with severity, owner, evidence, test, and release impact.
2. Record ADRs for tenant resolution, auth/session, media policy, REST, migrations, and module boundaries.
3. Add architecture tests preventing public write dependencies, domain-to-infrastructure dependencies, and non-admin migration ownership.
4. Define release gates, backup/restore objectives, rate-limit policy, log redaction, supported environments, and no-secret rules.

**Dependencies:** audit and repository baseline. **Gate:** every label maps to a control and deliberate boundary violations fail tests. Phase 0 is complete; code-level findings are remediated, while W6-W8, O4, and O7-O10 remain explicit release-gate verifications. **Rollback/risk:** additive documentation/tests are reversible; do not weaken a gate to obtain a green build.

### Phase 1 — Tenant isolation and admin session hardening (P0) — Complete

**Objective:** eliminate C1 and C2. **Paths:** [`Papyrus.Core/MultiTenancy`](../../src/Papyrus.Core/MultiTenancy), create ``Papyrus.Core/MultiTenancy/ITenantContext.cs`` and ``Papyrus.Core/MultiTenancy/TenantId.cs``, [`Papyrus.Infrastructure/PapyrusDbContext.cs`](../../src/Papyrus.Infrastructure/PapyrusDbContext.cs), [`Papyrus.Infrastructure/DependencyInjection.cs`](../../src/Papyrus.Infrastructure/DependencyInjection.cs), [`Papyrus.Api.Admin/Authentication`](../../src/Papyrus.Api.Admin/Authentication), [`Papyrus.Api.Admin/Program.cs`](../../src/Papyrus.Api.Admin/Program.cs), and [`Papyrus.Tests/TenantIsolationTests.cs`](../../src/Papyrus.Tests/TenantIsolationTests.cs).

1. Choose and document tenant resolution from trusted host/header/token context; reject missing, ambiguous, or unauthorized context.
2. Add scoped tenant context and enforce it on every query, command, relationship, seed, media, feed, and sitemap operation.
3. Add EF global filters where appropriate, save validation, tenant-scoped repositories, composite tenant foreign keys, and tenant-leading indexes.
4. Evaluate PostgreSQL row-level security as defense in depth and document the enforcement point.
5. Use header authentication for the admin SPA and automation with constant-time comparison, scoped key rotation, fail-closed production configuration, and no credential logging.

**Dependencies:** Phase 0 ADRs and tenant data inventory. **Gate:** adversarial tests prove tenant A cannot read, mutate, link, delete, enumerate, or infer tenant B data; expired/revoked/rotated sessions fail; raw keys are absent from cookies. **Rollback/risk:** back up and rehearse restore; use observe-only rollout only for telemetry, never as a permanent isolation fallback.

### Phase 2 — Request integrity, media, and HTTP security (P0) — Complete

**Objective:** eliminate C3/C4 and OWASP baseline gaps. **Paths:** [`Papyrus.Api.Admin/Media/LocalMediaService.cs`](../../src/Papyrus.Api.Admin/Media/LocalMediaService.cs), [`Papyrus.Api.Admin/Media/MediaOptions.cs`](../../src/Papyrus.Api.Admin/Media/MediaOptions.cs), create ``Papyrus.Api.Admin/Security``, ``Papyrus.Api.Admin/Filters``, and ``Papyrus.Tests/SecurityTests.cs``.

1. Require API-key authorization on every state-changing admin request and review header-authenticated mutation boundaries explicitly.
2. Add route rate limits, replay controls, and audit events for failures and mutations.
3. Validate decoded content/signature, size, dimensions, generated names, allowlisted formats, and storage isolation. Reject SVG by default or use a reviewed sanitizer policy.
4. Prevent traversal, overwrite races, polyglots, decompression bombs, and media execution; serve safe content type/disposition.
5. Require HTTPS/HSTS, CSP, frame/referrer/content-type permissions, secure cookies, request limits, and redacted errors.

**Dependencies:** Phase 1 session design. **Gate:** complete CSRF matrix, malicious media corpus, security-header tests, and OWASP P0/P1 review pass. **Rollback/risk:** quarantine rejected uploads; never feature-flag off antiforgery or executable-media controls.

### Phase 3 — Domain/application layering and persistence correctness (P1) — Complete

**Objective:** make writes transactional and maintainable. **Paths:** create [`Papyrus.Application`](../../src/Papyrus.Application), [`Papyrus.Infrastructure/Interceptors`](../../src/Papyrus.Infrastructure/Interceptors), [`Papyrus.Infrastructure/Migrations`](../../src/Papyrus.Infrastructure/Migrations), existing [`Papyrus.Infrastructure/Configurations`](../../src/Papyrus.Infrastructure/Configurations), and refactor [`Papyrus.Api.Admin/AdminApiEndpoints.cs`](../../src/Papyrus.Api.Admin/AdminApiEndpoints.cs).

1. Define aggregate invariants, commands, queries, authorization policies, ports, and mappings; move direct page mutation behind use cases.
2. Add UTC audit fields, soft-delete state, actor/reason, restore semantics, and optimistic concurrency tokens.
3. Add tenant composite keys, check constraints, partial indexes for live/non-deleted content, and evidence-based search indexes.
4. Use explicit transaction boundaries for content/taxonomy/link changes; add an outbox record only for required side effects.
5. Generate new EF migrations through the approved process, review SQL/locks/backfills, and never edit applied files under [`Papyrus.Infrastructure/Migrations`](../../src/Papyrus.Infrastructure/Migrations).

**Dependencies:** Phases 1-2. **Gate:** PostgreSQL tests cover concurrency, soft delete, restore, rollback, composite keys, no-tracking projections, and reviewed complex SQL; upgrade and restore rehearsal passes. **Rollback/risk:** expand/contract migrations, batched reversible backfills, backups; do not routine-down-migrate production.

### Phase 4 — REST contracts, projections, and frontend synchronization (P1) — Complete

**Objective:** stabilize integration boundaries. **Paths:** ``Papyrus.Api.Admin/Contracts/AdminContracts.cs``, create [`Papyrus.Contracts`](../../src/Papyrus.Contracts), ``Papyrus.Api.Admin/OpenApi``, ``Papyrus.Api.Admin/Errors``, [`Papyrus.Web.Blog/Pages`](../../src/Papyrus.Web.Blog/Pages), [`Papyrus.Web.Portfolio/Pages`](../../src/Papyrus.Web.Portfolio/Pages), [`openapi.json`](../api/openapi.json), ``clients``, and [`Papyrus.Tests/ApiContractTests.cs`](../../src/Papyrus.Tests/ApiContractTests.cs).

1. Version routes and define DTOs, validation, pagination, filters, idempotency, authorization, and ProblemDetails codes.
2. Publish/review OpenAPI and contract-test status codes, auth, tenant scope, and response shape.
3. Build bounded public listing/detail/feed/sitemap projections excluding drafts, deleted rows, and cross-tenant joins.
4. Generate or validate clients from OpenAPI; CI detects breaking changes and synchronized Razor/client models.

**Dependencies:** Phase 3. **Gate:** no EF entity crosses delivery boundaries; representative clients consume DTOs; projection SQL and latency evidence are recorded. **Rollback/risk:** additive versioning and a compatibility window before removing old routes.

### Phase 5 — Containers, Aspire, compose, and CI/CD (P1) — Complete

**Objective:** make releases repeatable and recoverable. **Paths:** [`Papyrus.AppHost/Program.cs`](../../src/Papyrus.AppHost/Program.cs), [`Papyrus.ServiceDefaults/Extensions.cs`](../../src/Papyrus.ServiceDefaults/Extensions.cs), existing [`Dockerfile.admin`](../../Dockerfile.admin), create [`Dockerfile.blog`](../../Dockerfile.blog), [`Dockerfile.portfolio`](../../Dockerfile.portfolio), [`docker-compose.yml`](../../docker-compose.yml), ``compose``, [`.dockerignore`](../../.dockerignore), create [`.github/workflows/ci.yml`](../../.github/workflows/ci.yml) and [`.github/workflows/release.yml`](../../.github/workflows/release.yml), and [`deployment.md`](../guides/deployment.md).

1. Define multi-stage non-root minimal images, pinned bases, read-only assumptions, and durable media volume ownership.
2. Preserve Aspire local ordering; make compose parity and production secret injection explicit.
3. Separate one migration job/identity from web startup; gate on backup, compatibility, readiness, and smoke tests.
4. CI runs restore, analyzers, build, all tests, OpenAPI diff, dependency/container scan, SBOM, signing, and approval checks.
5. Verify `/health`, `/alive`, dependency failure, graceful shutdown, and media persistence.

**Dependencies:** Phases 1-4. **Gate:** clean environment builds/runs all images; no root process; CI blocks warnings, failed tests, vulnerable images, leaked secrets, and breaking contracts. **Rollback/risk:** immutable tags, prior image retention, canary/blue-green deployment, and tested DB/media restore.

### Phase 6 — Observability, test depth, and performance budgets (P1/P2) — In progress

**Objective:** make failures diagnosable and behavior measurable. **Paths:** [`Papyrus.ServiceDefaults/Extensions.cs`](../../src/Papyrus.ServiceDefaults/Extensions.cs), create ``Papyrus.ServiceDefaults/Telemetry``, ``Papyrus.Tests/Integration``, ``Papyrus.Tests/Architecture``, ``tests/load``, and ``docs/runbooks``.

1. Define structured events, trace propagation, tenant-safe redaction, audit events, dashboards, alerts, and runbooks.
2. Expand the testing pyramid: domain unit, handler, PostgreSQL integration, HTTP security/contract, browser smoke, migration, architecture, and focused load tests.
3. Replace EF InMemory-only persistence confidence with PostgreSQL tests for provider behavior, indexes, transactions, concurrency, filters, and RLS if selected.
4. Baseline p95 public/admin latency, query count, payload size, upload cost, error rate, and bounded allocation/GC budgets.
5. Use async I/O, cancellation tokens, projections, bounded results, and measured optimization; avoid speculative allocation tricks.

**Dependencies:** Phases 3-5. **Gate:** every P0/P1 path has a test, signal, budget, and runbook. **Rollback/risk:** cap telemetry cardinality and sample safely; retain security/audit events even if performance requires reducing diagnostic volume.

**Status:** Phase 6 is complete. S30.0–S30.12 are all done (architecture/test
depth, PostgreSQL/integration, browser smoke, telemetry core, tenant-safe
telemetry, dashboard, performance budgets, runbooks, K6 load evidence, and the
Postgres RLS decision ADR). S30.5 load evidence was captured and committed
(public run HTTP p95 23.574 ms, payload p95 9,030 bytes, 0% failed, 100% checks;
admin run HTTP p95 21.660 ms, payload p95 9,256 bytes, 0% failed, 100% checks,
0 5xx). S30.12 is complete: ADR 0007 created, decision DECLINE Postgres RLS;
application-level tenant filtering remains the sole enforcement layer.

### Phase 7 — Release readiness and GitHub showcase (P2) — Not started

**Objective:** publish a credible, reproducible project. **Paths:** [`README.md`](../../README.md), [`contributing.md`](../guides/contributing.md), [`policy.md`](../security/policy.md), [`changelog.md`](changelog.md), [`architecture`](../architecture), ``runbooks``, ``screenshots``, and ``demo``.

1. Publish architecture, threat-model summary, local setup, seed/demo flow, API/deployment docs, and limitations.
2. Add sanitized screenshots/GIFs for blog, portfolio, admin, health, and Aspire; exclude keys, private URLs, and personal data.
3. Add contribution, disclosure, license, changelog, support, and truthful feature matrices.
4. Tag a release only after all gates and rollback drills pass.

**Dependencies:** P0/P1 gates. **Gate:** fresh contributor setup succeeds and showcase claims have evidence. **Rollback/risk:** replace misleading assets or claims; never advertise deferred multi-tenancy as complete.

### Optional Phase 8 — Measured future scale (P3) — Not started; decision gate

Only after Phase 6 identifies a bottleneck consider read replicas, object storage/CDN, dedicated search, background workers, outbox consumers, GraphQL, gRPC, or service extraction. Preserve the modular monolith by default.

## 4. EF Core/PostgreSQL and security standards

- Tenant-owned tables use tenant keys, composite foreign keys, tenant-leading indexes, and authorization checks; filters are defense in depth, not the sole control.
- Reads are asynchronous, cancellable, no-tracking, projected, bounded, and checked for N+1 behavior. Review generated SQL for complex queries.
- Soft delete is default-excluded and authorized-restorable; partial unique indexes cover live rows. Concurrency conflicts return standardized error responses.
- Transactions cover one business operation; retries are safe only for idempotent commands. One controlled identity applies generated, reviewed migrations.
- Apply OWASP controls for access control, sessions, injection, misconfiguration, dependencies, logging, and SSRF; enforce secret-manager injection, key rotation, rate limits, safe headers, HTTPS, CSP, upload quarantine/scanning, and redacted errors.

## 5. Milestones and release sequence

1. **M0 audit baseline:** Phase 0 finished with documented W/O risk acceptance; no production release is authorized by this milestone alone.
2. **M1 security release:** Phases 1-2 close C1-C4 and pass adversarial gates.
3. **M2 data/API release:** Phases 3-4 pass transactional, DTO, OpenAPI, and compatibility gates.
4. **M3 operations release:** Phase 5 passes signed-image, CI/CD, health, migration, backup, and rollback drills.
5. **M4 production candidate:** Phase 6 passes test, telemetry, load, and budget evidence.
6. **M5 public showcase:** Phase 7 publishes sanitized, truthful assets.
7. **Scale review:** Phase 8 remains a decision gate, not a commitment.

## 6. Traceability matrix

| Finding | Roadmap response | Verification |
| --- | --- | --- |
| C1 | Phases 0-1; EF standards | Cross-tenant adversarial reads, writes, links, deletion, media, and database-policy review |
| C2 | Phases 0-1 | Expiry, revocation, rotation, cookie inspection, and log-redaction tests |
| C3 | Phase 2 | Antiforgery integration matrix for every cookie mutation |
| C4 | Phase 2 | Malicious SVG/HTML/polyglot/path corpus and storage execution tests |
| W1-W4 | Phases 0, 3, 5 | ADRs, architecture, transaction, and release-gate tests |
| W5-W8 | Phases 2, 5, 6, 7 | Security checklist, runbooks, deployment drills, and docs review |
| O1-O5 | Phases 4-6 | OpenAPI diff, projection, dashboard, trace, and query evidence |
| O6-O10 | Phases 5-8 | SBOM, restore/load drills, budgets, showcase, and scale ADR |
| Strengths | All phases | Preserve module seams, Aspire ordering, shared UI, media checks, feeds, health, OTel, and test gates |

## 7. Definition of done

An item is done only when it is in the correct boundary, has tests and telemetry where applicable, passes build/analyzer/security/contract gates, documents migration and rollback behavior, preserves tenant and credential safety, and updates ADRs/runbooks. A release is done only when P0/P1 findings are closed, backup/restore and rollback are rehearsed, public isolation is proven, and no checked-in secret or unsafe production default remains. “Works locally” is insufficient.
