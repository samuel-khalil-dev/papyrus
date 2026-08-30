# Project Brief

## Current truth

Papyrus is a modular monolith for technical writing and a professional showcase. It uses .NET 10, EF Core 10.0.11, PostgreSQL 17, Aspire, Docker Compose, a React 19 admin SPA, and two read-only public Razor sites.

The 11 .NET projects are `Papyrus.AppHost` (Aspire orchestration), `Papyrus.Api.Admin` (Admin Minimal API, SPA serving, writes, auth, media, migration, and seed), `Papyrus.Infrastructure` (EF Core, PostgreSQL, migrations, seed, tenant filters), `Papyrus.Core` (domain entities, tenant marker, safe Markdown), `Papyrus.ServiceDefaults` (health, telemetry, shared defaults), `Papyrus.Web.Portfolio`, `Papyrus.Web.Blog`, `Papyrus.Web.Shared`, `Papyrus.Tests`, `Papyrus.Application` (CQRS and projections), and `Papyrus.Contracts` (REST DTOs and pagination). `Papyrus.Admin.UI` is a standalone npm app, not a solution project; Aspire registers it only as a dev-only `AddNpmApp` resource and production embeds its output in the Admin image.

## Requirements

- Publish technical posts and portfolio work; relate projects to blog posts and organize both with categories and tags.
- Manage content, taxonomy, relationships, and tenant-scoped media through the React Admin SPA and versioned `/api/v1` REST API.
- Serve read-only public Blog and Portfolio sites.
- Keep tenant data isolated through application-enforced boundaries.
- Run with Aspire or Docker Compose and deploy non-root containers with PostgreSQL 17.
- Keep `Papyrus.Api.Admin` as the sole migration and seed owner.

## Milestone

Phases 0–6 are complete. Phase 7 (release readiness/showcase) and Phase 8 (measured future scale decision gate) are not started. The local baseline is 70 passing non-integration .NET tests. CI adds 14 PostgreSQL integration tests for 84 unique .NET tests; the separate Playwright browser suite contains 5 tests. The warnings-as-errors build has 0 warnings and 0 errors.
