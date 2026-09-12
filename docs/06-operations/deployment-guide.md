# Deployment Guide

## Current Deployment Topology

**Current:** Papyrus is deployed via Docker Compose directly — there is **no reverse proxy and no CDN in front of it today**. Services are reachable on their configured ports (Admin API, Admin UI, Blog, Portfolio, Postgres) as defined in `docker-compose.yml`.

## Environment variables (Current)

Store secrets in the deployment platform or a local, uncommitted environment. Compose fails before startup when a required value is absent. Do not commit the environment file or paste its values into issue reports, screenshots, logs, or public documentation.

| Variable | Used by | Purpose |
| --- | --- | --- |
| `POSTGRES_PASSWORD` | `db`, all web services | PostgreSQL password |
| `ADMIN_API_KEY` | `admin-api` | `X-Admin-Api-Key` value for Admin API mutations |
| `BLOG_PUBLIC_BASE_URL` | `blog` and public URL references | Blog's absolute public origin |
| `PORTFOLIO_PUBLIC_BASE_URL` | `portfolio` and public URL references | Portfolio's absolute public origin |

The Compose file also accepts optional image, port, and PostgreSQL-user overrides. The default published ports are Admin API `5205`, Blog `5075`, and Portfolio `5024`; each container listens internally on `8080`.

## Volumes and runtime hardening (Current)

The `db` service is PostgreSQL 17 with the durable `papyrus-postgres-data` volume. `admin-api` waits for the database health check and mounts the durable `papyrus-media` volume at `/data/media`; the volume must survive Admin restarts. The three images are multi-stage: the Admin image builds the React SPA with Node 24.20.0 and the Blog/Portfolio images build with the .NET 10 SDK. All runtime images run as the base image's non-root `APP_UID`.

| Service | Role | Published port / storage |
| --- | --- | --- |
| `test-db` | PostgreSQL 17 test profile | `5432`, profile `test` |
| `db` | PostgreSQL 17 application database | durable `papyrus-postgres-data` volume |
| `admin-api` | API, SPA serving, auth, media, migration, seed | `5205:8080`, `papyrus-media` at `/data/media` |
| `blog` | Public read-only site | `5075:8080` |
| `portfolio` | Public read-only site | `5024:8080` |

The hosts expose anonymous `/health` and `/alive` probes. Restrict those paths at the ingress to orchestrator and monitoring networks; do not expose them on a general public edge. The Admin SPA is served at the root, while Blog and Portfolio serve their public sites on ports `5075` and `5024`.

## Planned Topology (M2, M13)

M2 will introduce Caddy as a reverse proxy terminating TLS and routing each host to its container. M13 will place Cloudflare in front of that for CDN caching, DDoS mitigation, and Turnstile bot protection. Until both ship, do not assume TLS termination, edge caching, or Turnstile are active in production.

## Release flow
1. A tagged release triggers `release.yml`.
2. Images are built and pushed to GHCR using pinned base images.
3. Deployment executes via a self-hosted runner with scoped GHCR pull credentials.
4. A post-deploy health check verifies the service before the release is considered successful.

## Secrets (Current)

Operational secrets are supplied via environment variables and configuration — `AdminApi:ApiKeys` / `ADMIN_API_KEY`, `POSTGRES_PASSWORD`, `BLOG_PUBLIC_BASE_URL`, `PORTFOLIO_PUBLIC_BASE_URL` — following standard `appsettings.{Environment}.json` override conventions. None of these are encrypted at the application layer today; they rely on environment/deployment-level protection.

## Secrets (Planned, M25)

M25 introduces one new, specific credential type — the AI provider API key — which **will** be encrypted at rest via the ASP.NET Core Data Protection API and masked in the Admin UI, because it is entered and stored through the application rather than supplied as deployment configuration. This encryption approach is scoped to that one new credential; it does not retroactively apply to the existing environment-variable-based secrets above unless a future milestone explicitly extends it.

## Migration Ownership

**Current:** only the Admin API applies and seeds migrations, via `InitializeDatabaseAsync()` at startup. Public hosts (Blog, Portfolio) never run migrations under any circumstance. The release pipeline additionally runs migrations explicitly in a dedicated `migrate` job using `dotnet-ef 10.0.11`, ahead of deploying the application containers. This ownership boundary is deliberate — see the [Migration Ownership](#migration-ownership) section above — and must not be altered without a corresponding decision-record update.

For a controlled manual migration, use the Admin startup project:

```powershell
dotnet ef database update `
  --project src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj `
  --startup-project src/Papyrus.Api.Admin/Papyrus.Api.Admin.csproj `
  --connection "<production connection string>"
```

The current history contains five migrations and ends at `20260826005202_SyncSearchTrigramIndexesModel`. Applied migration files are immutable; create a corrective migration for a later schema change. Do not run migration jobs concurrently; after `docker compose up`, the release workflow's `deploy` job polls `/health` on the Admin API and `/` on Blog and Portfolio and fails if any host does not become reachable.

## Backups
Automated, scheduled, off-VPS PostgreSQL and media backups (M4), with a documented, tested restore procedure.

## Rollback
A failed health check halts promotion. Rollback redeploys the previous pinned image.
