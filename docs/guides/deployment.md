# Deploy Papyrus with Docker Compose

This is a **Diátaxis How-To Guide**. It deploys the three non-root web images
and PostgreSQL 17, then verifies that the Admin API is the sole database
migrator. Run the commands from the repository root.

## 1. Set the required environment

Store secrets in the deployment platform or a local, uncommitted environment.
Compose fails before startup when a required value is absent.

Do not commit the environment file or paste its values into issue reports,
screenshots, logs, or public documentation.

| Variable | Used by | Purpose |
| --- | --- | --- |
| `POSTGRES_PASSWORD` | `db`, all web services | PostgreSQL password |
| `ADMIN_API_KEY` | `admin-api` | `X-Admin-Api-Key` value for Admin API mutations |
| `BLOG_PUBLIC_BASE_URL` | `blog` and public URL references | Blog's absolute public origin |
| `PORTFOLIO_PUBLIC_BASE_URL` | `portfolio` and public URL references | Portfolio's absolute public origin |

The Compose file also accepts optional image, port, and PostgreSQL-user
overrides. The default published ports are Admin API `5205`, Blog `5075`, and
Portfolio `5024`; each container listens internally on `8080`.

## 2. Start the stack

```powershell
docker compose up -d
```

The `db` service is PostgreSQL 17 with the durable `papyrus-postgres-data`
volume. `admin-api` waits for the database health check and mounts the durable
`papyrus-media` volume at `/data/media`. `blog` and `portfolio` are read-only
public hosts and do not apply migrations. The three images are multi-stage;
the Admin image builds the React SPA with Node 24.20.0 and the Blog/Portfolio images
build with the .NET 10 SDK. All runtime images run as the base image's
non-root `APP_UID`.

## 3. Verify health and reachability

Check container state and logs:

```powershell
docker compose ps
docker compose logs admin-api
```

Probe the published Admin API endpoint from a permitted monitoring or operator
network:

```powershell
Invoke-WebRequest http://localhost:5205/health
```

The hosts expose anonymous `/health` and `/alive` probes. Restrict those paths
at the ingress to orchestrator and monitoring networks; do not expose them on a
general public edge. The Admin SPA is served at the root, while Blog and
Portfolio serve their public sites on ports `5075` and `5024`.

## 4. Apply migrations through the sole migrator

The Admin API owns `InitializeDatabaseAsync()`, migration, and seed execution.
Do not add migration startup to Blog or Portfolio, and do not run concurrent
migration jobs. Back up PostgreSQL and verify migration history before a
release. The checked-in release workflow installs `dotnet-ef` `10.0.11`, runs a
dedicated `migrate` job, and only then starts `deploy`.

For a controlled manual migration, use the Admin startup project:

```powershell
dotnet ef database update `
  --project src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj `
  --startup-project src/Papyrus.Api.Admin/Papyrus.Api.Admin.csproj `
  --connection "<production connection string>"
```

The current history contains five migrations and ends at
`20260826005202_SyncSearchTrigramIndexesModel`. Applied migration files are
immutable; create a corrective migration for a later schema change.

For a normal release, prefer the repository's dedicated migration job rather
than running a migration concurrently with multiple web containers. The Admin
API remains the only application owner of initialization and seed behavior.

After `docker compose up`, the release workflow's `deploy` job polls the
published health endpoints (`/health` on the Admin API, `/` on Blog and
Portfolio) and fails the job if any host does not become reachable, so an
unhealthy deployment is surfaced immediately rather than silently accepted.

## Compose service reference

| Service | Role | Published port / storage |
| --- | --- | --- |
| `test-db` | PostgreSQL 17 test profile | `5432`, profile `test` |
| `db` | PostgreSQL 17 application database | durable `papyrus-postgres-data` volume |
| `admin-api` | API, SPA serving, auth, media, migration, seed | `5205:8080`, `papyrus-media` at `/data/media` |
| `blog` | Public read-only site | `5075:8080` |
| `portfolio` | Public read-only site | `5024:8080` |

The release workflow builds, scans, generates SBOMs, and pushes immutable image
tags before the migration job and self-hosted deployment job.
