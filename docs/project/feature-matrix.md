# Feature matrix

This matrix is a release-facing summary of the current Papyrus baseline. A
feature is marked **Implemented** only when the repository contains the relevant
implementation and verification. Operational or product work that still needs
release evidence is not presented as complete.

| Area | Status | Current truth |
| --- | --- | --- |
| Public Blog and Portfolio hosts | **Implemented** | Read-only public hosts render published content from the shared model. |
| Authenticated Admin API | **Implemented** | API-key protected content, category, tag, project, and media operations are exposed under `/api/v1`. |
| React Admin UI | **Implemented** | The SPA is served at `/admin` and uses the protected Admin API. |
| PostgreSQL persistence | **Implemented** | EF Core persistence, migrations, auditing, soft delete, indexes, and concurrency handling are present. |
| Tenant-aware content and media boundaries | **Implemented** | Application and EF Core predicates enforce the current tenant model; `main` is the default tenant. |
| PostgreSQL row-level security (RLS) | **Deferred** | RLS is intentionally not enabled; application-level enforcement remains the sole layer. |
| Full multi-tenant SaaS | **Deferred** | Tenant provisioning, operator lifecycle, billing, identity management, and a tenant-facing administration model are not included. |
| Local PostgreSQL integration-test workflow | **Deferred** | PostgreSQL integration tests run in CI; a fresh workstation is not guaranteed to have a configured local test database listener. |
| Automated test baseline | **Implemented** | 70 non-integration .NET tests pass locally. CI adds 14 PostgreSQL integration tests for 84 unique .NET tests; the separate Playwright browser suite contains 5 tests. |
| Canonical, RSS, and sitemap production URLs | **Release verification pending** | `Site__PublicBaseUrl` must be set and re-measured in the target production deployment before release approval. |
| S30.10 optimization | **Deferred** | The optimization was intentionally not applied; the simpler implementation remains. |
| Backup, restore, media recovery, observability, and ingress gates | **Release verification pending** | Operational evidence, recovery drills, dashboards, alerting, retention, and probe restrictions still require release-time verification. |
| Horizontally scaled process-local state | **Planned** | Current telemetry and security-related state assume process-local coordination; scaling requires an explicit design review. |
| Legacy `/api` routes | **Compatibility behavior** | Legacy behavior remains for compatibility and is not the published contract; new integrations should use `/api/v1`. |
| Independent service extraction | **Planned** | Papyrus remains a modular monolith until measured scale, deployment, or ownership needs justify extraction. |

See [known limitations](limitations.md), the [roadmap](roadmap.md), and the
[release gates](../architecture/release-gates.md) for detail and evidence
requirements.
