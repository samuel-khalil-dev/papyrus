# Deployment Guide

## Current Deployment Topology

**Current:** Papyrus is deployed via Docker Compose directly — there is **no reverse proxy and no CDN in front of it today**. Services are reachable on their configured ports (Admin API, Admin UI, Blog, Portfolio, Postgres) as defined in `docker-compose.yml`.

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

**Current:** only the Admin API applies and seeds migrations, via `InitializeDatabaseAsync()` at startup. Public hosts (Blog, Portfolio) never run migrations under any circumstance. The release pipeline additionally runs migrations explicitly in a dedicated `migrate` job using `dotnet-ef 10.0.11`, ahead of deploying the application containers. This ownership boundary is deliberate — see `docs/architecture/decisions/0005-migration-ownership.md` — and must not be altered without a corresponding decision-record update.

## Backups
Automated, scheduled, off-VPS PostgreSQL and media backups (M4), with a documented, tested restore procedure.

## Rollback
A failed health check halts promotion. Rollback redeploys the previous pinned image.
