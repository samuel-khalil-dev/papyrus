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
| `MEDIA_BASE_URL` | `blog`, `portfolio` and public URL references | Absolute origin uploaded media is served from |

The `BLOG_PUBLIC_BASE_URL`, `PORTFOLIO_PUBLIC_BASE_URL`, and `MEDIA_BASE_URL` values are not secrets, so they live in the repository as Actions **variables** rather than secrets. Set them once under *Settings → Secrets and variables → Actions → Variables*; the `deploy` job fails with the names of any that are missing before it attempts to deploy, and the `migrate` job does not need them at all.

The Compose file also accepts optional image, port, and PostgreSQL-user overrides. The default published ports are Admin API `5205`, Blog `5075`, and Portfolio `5024`; each container listens internally on `8080`.

## Volumes and runtime hardening (Current)

The `db` service is PostgreSQL 17 with the durable `papyrus-postgres-data` volume. `admin-api` waits for the database health check and mounts the durable `papyrus-media` volume at `/data/media`; the volume must survive Admin restarts. The three images are multi-stage: the Admin image builds the React SPA with Node 24.21.0 and the Blog/Portfolio images build with the .NET 10 SDK. All runtime images run as the base image's non-root `APP_UID`.

| Service | Role | Published port / storage |
| --- | --- | --- |
| `test-db` | PostgreSQL 17 test profile | `5432`, profile `test` |
| `db` | PostgreSQL 17 application database | durable `papyrus-postgres-data` volume |
| `admin-api` | API, SPA serving, auth, media, migration, seed | `5205:8080`, `papyrus-media` at `/data/media` |
| `blog` | Public read-only site | `5075:8080` |
| `portfolio` | Public read-only site | `5024:8080` |

The hosts expose anonymous `/health` and `/alive` probes. Restrict those paths at the ingress to orchestrator and monitoring networks; do not expose them on a general public edge. The Admin SPA is served at the root, while Blog and Portfolio serve their public sites on ports `5075` and `5024`.

## Restart policy and host reboots

Every production service declares `restart: unless-stopped`. This is load-bearing, not a default to inherit.

On 2026-09-29 the host rebooted for a kernel upgrade and the entire application stack stayed offline for more than four hours. All services had defaulted to `restart: no`, so Docker had no instruction to restore anything and the containers stayed exited until an operator started them by hand. The signature is `Exited (255)` with an empty `State.Error` and `OOMKilled=false`: that is a host-initiated stop, not an application fault, and it is the first thing to check when the site refuses connections without a crash in the logs.

`unless-stopped` brings the services back after a daemon restart or a crash while still honouring a deliberate `docker compose stop`. The `test-db` service is in the `test` profile and deliberately has no restart policy.

Check policy after any change with `docker inspect <container> --format '{{.HostConfig.RestartPolicy.Name}}'`. A host reboot is a normal event, not an incident, provided this holds.

## Planned Topology (M2, M13)

M2 will introduce Caddy as a reverse proxy terminating TLS and routing each host to its container. M13 will place Cloudflare in front of that for CDN caching, DDoS mitigation, and Turnstile bot protection. Until both ship, do not assume TLS termination, edge caching, or Turnstile are active in production.

## Release flow
1. A tagged release triggers `release.yml`.
2. Images are built and pushed to GHCR using pinned base images.
3. Deployment executes via a self-hosted runner that mints its own short-lived GHCR pull credential.
4. A post-deploy health check verifies the service before the release is considered successful.

## Registry credentials (Current)

The `deploy` job pulls images on a self-hosted runner, whose per-job `GITHUB_TOKEN` has no access to GHCR. It therefore mints a **GitHub App installation token** at run time and uses that for `docker/login-action`.

The App is installed on this repository alone with `packages: read`, so the credential it produces is strictly narrower than a personal access token and expires in one hour. There is no long-lived registry PAT in the repository, and none should be added.

| Value | Kind | Purpose |
| --- | --- | --- |
| `APP_ID` | secret | The GitHub App's numeric ID, the JWT issuer |
| `APP_PRIVATE_KEY` | secret | RSA key that signs the installation JWT |
| `APP_INSTALLATION_ID` | variable | Which installation to mint a token for |
| `APP_SLUG` | variable | The App's slug, the registry username |

The private key is used only to sign a JWT that is exchanged for a narrower token. Rotate it from the App's settings page; nothing in the repository needs to change.

**The deploy job is the only consumer.** The `images` job pushes with the job's own `GITHUB_TOKEN` and `packages: write`, which requires no stored credential at all.

## Secrets (Current)

Operational secrets are supplied via environment variables and configuration — `AdminApi:ApiKeys` / `ADMIN_API_KEY`, `POSTGRES_PASSWORD` — following standard `appsettings.{Environment}.json` override conventions. None of these are encrypted at the application layer today; they rely on environment/deployment-level protection. The three public-origin values (`BLOG_PUBLIC_BASE_URL`, `PORTFOLIO_PUBLIC_BASE_URL`, `MEDIA_BASE_URL`) are configuration rather than secrets and are stored as repository variables, not repository secrets.

## Secrets (Planned, M25)

M25 introduces one new, specific credential type — the AI provider API key — which **will** be encrypted at rest via the ASP.NET Core Data Protection API and masked in the Admin UI, because it is entered and stored through the application rather than supplied as deployment configuration. This encryption approach is scoped to that one new credential; it does not retroactively apply to the existing environment-variable-based secrets above unless a future milestone explicitly extends it.

## Migration Ownership

**Current:** only the Admin API applies and seeds migrations, via `InitializeDatabaseAsync()` at startup. Public hosts (Blog, Portfolio) never run migrations under any circumstance. The release pipeline additionally runs migrations explicitly in a dedicated `migrate` job using `dotnet-ef 10.0.11`, ahead of deploying the application containers. This ownership boundary is deliberate — see the [Migration Ownership](#migration-ownership) section above — and must not be altered without a corresponding decision-record update.

### The migrate job's database endpoint

The `migrate` job runs on the host, not inside the Compose network. The `db` service therefore needs a host-reachable endpoint, and the compose file publishes it on loopback only:

```yaml
ports:
  - "127.0.0.1:${DB_PORT:-5432}:5432"
```

`PRODUCTION_CONNECTION_STRING` must use `Host=127.0.0.1` (or `localhost`), **not** `Host=db`. The `db` hostname only resolves on the Compose network; from the host it fails with a socket error that looks like a connection or credentials problem but is neither. The database stays off every routable interface: binding to `127.0.0.1` means nothing outside the host can reach it, and Docker never publishes on `0.0.0.0`, so UFW cannot be bypassed.

The application containers are unaffected and continue to use `Host=db` over the Compose network.

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
