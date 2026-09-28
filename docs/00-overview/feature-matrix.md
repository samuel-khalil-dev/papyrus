# Feature Matrix

**Current:** this matrix is a release-facing summary of the current Papyrus baseline (the legacy `docs/project/` feature matrix was retired during the documentation consolidation). A feature is marked **Implemented** only when the repository contains the relevant implementation and verification. Operational or product work that still needs release evidence is not presented as complete.

| Area | Status | Current truth |
| --- | --- | --- |
| Public Blog and Portfolio hosts | **Implemented** | Read-only public hosts render published content from the shared model. |
| Public host identity and host filtering | **Implemented** | Each public host fails startup if `Site:Kind` or its canonical base URLs disagree with the site it serves, and serves only its configured canonical host plus the loopback literals; every other Host header is refused with a 400. See [M1 routing truth](../05-milestones/m1-routing-truth.md). |
| Duplicate-slug conflict handling | **Implemented** | A slug already held by another non-deleted row in the same tenant returns 409 from the create and update paths for blog posts, projects, categories, and tags, via a pre-check plus a PostgreSQL `23505` translation that closes the race. |
| Authenticated Admin API | **Implemented** | API-key protected content, category, tag, project, and media operations are exposed under `/api/v1`. |
| React Admin UI | **Implemented** | The SPA is served at the root and uses the protected Admin API. |
| PostgreSQL persistence | **Implemented** | EF Core persistence, migrations, auditing, soft delete, indexes, and concurrency handling are present. |
| Tenant-aware content and media boundaries | **Implemented** | Application and EF Core predicates enforce the current tenant model; `main` is the default tenant. |
| PostgreSQL row-level security (RLS) | **Deferred** | RLS is intentionally not enabled; application-level enforcement remains the sole layer. |
| Full multi-tenant SaaS | **Deferred** | Tenant provisioning, operator lifecycle, billing, identity management, and a tenant-facing administration model are not included. |
| Local PostgreSQL integration-test workflow | **Deferred** | PostgreSQL integration tests run in CI; a fresh workstation is not guaranteed to have a configured local test database listener. |
| Automated test baseline | **Implemented** | 158 non-integration .NET tests pass locally. CI adds 20 PostgreSQL integration tests for 178 unique .NET tests; the separate Playwright browser suite contains 10 tests. |
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