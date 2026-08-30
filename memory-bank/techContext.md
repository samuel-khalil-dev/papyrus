# Tech Context

## Exact stack

- .NET 10, EF Core 10.0.11, Npgsql, PostgreSQL 17.
- React 19.1.1, Vite 7.1.3, TypeScript 5.9.2 strict, Tailwind v4.
- TanStack React Query 5, `openapi-fetch`, `react-hook-form`, `zod`, TipTap.
- Aspire, Docker Compose, multi-stage non-root Dockerfiles.
- xUnit, Playwright, NetArchTest, and `openapi-typescript`.

## Projects

The 11 solution projects are `Papyrus.AppHost`, `Papyrus.Api.Admin`, `Papyrus.Infrastructure`, `Papyrus.Core`, `Papyrus.ServiceDefaults`, `Papyrus.Web.Portfolio`, `Papyrus.Web.Blog`, `Papyrus.Web.Shared`, `Papyrus.Tests`, `Papyrus.Application`, and `Papyrus.Contracts`. `src/Papyrus.Admin.UI` is a standalone npm app and is not a `.sln` project.

## Runtime and API

- Versioned API `/api/v1`; legacy `/api` is excluded from the OpenAPI description.
- CRUD covers categories, tags, projects, and blog posts; media supports list/upload/delete.
- SPA shell `/admin`; OpenAPI `/openapi/v1.json`; anonymous probes `/health` and `/alive`.
- Compose ports: Admin API 5205, Blog 5075, Portfolio 5024. Aspire dev UI uses Vite port 5173 and proxies `/api/v1`.

## Configuration and deployment

Required Compose inputs: `POSTGRES_PASSWORD`, `ADMIN_API_KEY`, `BLOG_PUBLIC_BASE_URL`, `PORTFOLIO_PUBLIC_BASE_URL`. Relevant settings are `AdminApi:ApiKeys` plus legacy `AdminApi:ApiKey`, `Site:PublicBaseUrl`, `Site:MediaBaseUrl`, and `OTEL_EXPORTER_OTLP_ENDPOINT`.

Compose services are `test-db` (test profile), `db`, `admin-api`, `blog`, and `portfolio`. PostgreSQL has a durable volume and healthcheck; Admin has a media volume and owns initialization. CI runs restore, warnings-as-errors build, tests, PostgreSQL integration, gitleaks, NuGet vulnerability scan, OpenAPI drift check, and container/Trivy checks. Release builds images, runs a dotnet-ef 10.0.11 migration job, and deploys.

## Data and telemetry

Entities are `Project`, `BlogPost`, `ContentCategory`, `ContentTag`, `ProjectTag`, and `BlogPostTag`; all are tenant-aware, audited, and soft-deletable. Five migrations are applied. `PapyrusSaveChangesInterceptor` manages audit data and soft deletes; Npgsql retries 5 times with a 10-second delay.

Telemetry uses the `Papyrus` ActivitySource, W3C propagation, request middleware, structured audit events, fail-closed redaction, default cardinality 100, and default diagnostic sampling 1.0. Audit events are never sampled; OTLP is gated by `OTEL_EXPORTER_OTLP_ENDPOINT`. Verification is 70 passing non-integration .NET tests locally; CI adds 14 PostgreSQL integration tests for 84 unique .NET tests, and the separate Playwright suite contains 5 tests. The warnings-as-errors build has 0 warnings and 0 errors.
