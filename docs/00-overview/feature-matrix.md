# Feature Matrix

**Current:** this matrix is a release-facing summary of the current Papyrus baseline (the legacy `docs/project/` feature matrix was retired during the documentation consolidation). A feature is marked **Implemented** only when the repository contains the relevant implementation and verification. Operational or product work that still needs release evidence is not presented as complete.

| Area | Status | Current truth |
| --- | --- | --- |
| Public Blog and Portfolio hosts | **Implemented** | Read-only public hosts render published content from the shared model. |
| Authenticated Admin API | **Implemented** | API-key protected content, category, tag, project, and media operations are exposed under `/api/v1`. |
| React Admin UI | **Implemented** | The SPA is served at the root and uses the protected Admin API. |
| PostgreSQL persistence | **Implemented** | EF Core persistence, migrations, auditing, soft delete, indexes, and concurrency handling are present. |
| Tenant-aware content and media boundaries | **Implemented** | Application and EF Core predicates enforce the current tenant model; `main` is the default tenant. |
| PostgreSQL row-level security (RLS) | **Deferred** | RLS is intentionally not enabled; application-level enforcement remains the sole layer. |
| Full multi-tenant SaaS | **Deferred** | Tenant provisioning, operator lifecycle, billing, identity management, and a tenant-facing administration model are not included. |
| Local PostgreSQL integration-test workflow | **Deferred** | PostgreSQL integration tests run in CI; a fresh workstation is not guaranteed to have a configured local test database listener. |
| Automated test baseline | **Implemented** | **This row is the canonical test count; other documents link here rather than restating it.** 456 non-integration .NET test cases pass without a database, of which 280 are whole-file encoding cases in `SourceEncodingTests` (one per source file, asserting UTF-8) and 176 assert behaviour. CI adds 22 PostgreSQL integration tests, for 478 unique .NET test cases. The separate Playwright browser suite contains 10 tests and is not part of the .NET count. |
| Architecture fitness tests (layer and migration ownership) | **Implemented** | NetArchTest rules constrain Core, Contracts, Web.Shared, the public delivery hosts, and Application; migration-ownership tests assert that only Admin startup migrates. |
| Presentation-to-Infrastructure boundary | **Implemented** | Scoped and enforced by [ADR-0010](../02-architecture/decisions/adr-0010-scoped-presentation-to-infrastructure-access.md). Three `LayerDependencyTests` rules keep the read-only public hosts off Infrastructure outside their entry point, deny them the `Ef*Store` adapters, and stop Admin Presentation code outside `Endpoints/` and `AdminUi/` from reaching Infrastructure. |
| RFC 7807 `ProblemDetails` error responses | **Implemented** | Every Admin error response that carries a body is `application/problem+json`; 404 and 204 responses are bodiless. Enforced by `AdminApiTests` for the media missing-file, malformed-body, rejected-upload, and referenced-asset delete paths. |
| Canonical, RSS, and sitemap production URLs | **Release verification pending** | `Site__PublicBaseUrl` must be set and re-measured in the target production deployment before release approval. |
| S30.10 optimization | **Deferred** | The optimization was intentionally not applied; the simpler implementation remains. |
| Backup, restore, media recovery, observability, and ingress gates | **Release verification pending** | Operational evidence, recovery drills, dashboards, alerting, retention, and probe restrictions still require release-time verification. |
| Horizontally scaled process-local state | **Planned** | Current telemetry and security-related state assume process-local coordination; scaling requires an explicit design review. |
| Legacy `/api` routes | **Compatibility behavior** | Legacy behavior remains for compatibility and is not the published contract; new integrations should use `/api/v1`. |
| Independent service extraction | **Planned** | Papyrus remains a modular monolith until measured scale, deployment, or ownership needs justify extraction. |

See [known limitations](limitations.md), the [roadmap](../Papyrus_Roadmap.md), and the [release gates](../04-engineering-standards/release-gates.md) for detail and evidence requirements.

The test counts above are machine-checked. The `verify` job in
[CI](../../.github/workflows/ci.yml) derives every figure below from the TRX
files the test steps already write, and fails the build if this marker or any
restatement of the figures in `README.md`, `limitations.md`, or
`Papyrus_Roadmap.md` disagrees with the run. Adding, removing, or renaming a
test — or adding any source file, since the encoding guard contributes one case
per file — requires updating this marker and those three documents in the same
change.

<!-- test-baseline: non-integration=456 integration=22 total=478 encoding=280 behavioural=176 behaviour-total=198 playwright=10 -->
