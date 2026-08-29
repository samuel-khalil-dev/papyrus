# Papyrus deployment

Papyrus is deployed as containers behind an HTTPS reverse proxy. The checked-in
[`Dockerfile.admin`](../../Dockerfile.admin), [`Dockerfile.blog`](../../Dockerfile.blog),
and [`Dockerfile.portfolio`](../../Dockerfile.portfolio) define the three separate
multi-stage production images for the admin API/UI, public blog, and public
portfolio. [`docker-compose.yml`](../../docker-compose.yml) runs those services with
PostgreSQL 17, durable PostgreSQL and media volumes, service health checks, and
startup ordering. The release workflow runs a dedicated `migrate` job before
the deployment job starts the released images.

For incident response and telemetry operation, see the
[`runbooks`](../runbooks/incident-runbook.md): the
[incident runbook](../runbooks/incident-runbook.md) covers detection, triage,
containment, recovery, and post-incident review; the
[telemetry runbook](../runbooks/telemetry-runbook.md) covers operating and
querying the shared `Papyrus` ActivitySource, structured audit events,
redaction, cardinality, and sampling.

## Required production configuration

Set these environment variables in the container platform. ASP.NET Core maps
double underscores to configuration sections.

```text
AdminApi__ApiKey=<random secret, stored in the platform secret manager>
ConnectionStrings__papyrus=<PostgreSQL connection string>
Site__PublicBaseUrl=https://blog.example.com       # blog container; required for absolute public URLs
Site__MediaBaseUrl=https://admin.example.com       # blog and portfolio
Site__BlogBaseUrl=https://blog.example.com         # blog and portfolio
Site__PortfolioBaseUrl=https://portfolio.example.com # blog and portfolio
```

`Site__PublicBaseUrl` is required in production on every public-site container.
Set it to that container's exact public HTTPS origin, with no path suffix or
trailing slash: the Blog container receives the blog origin and the Portfolio
container receives the portfolio origin. The repository does not define the
production hostnames, so replace the example values with the deployment's real
DNS names rather than copying them. For Docker Compose, provide the values as
`BLOG_PUBLIC_BASE_URL` and `PORTFOLIO_PUBLIC_BASE_URL`; Compose now fails before
startup if either value is missing. In the GitHub Actions deployment, configure
the corresponding production environment variables with those same names.

The setting binds to the `Site` section as `PublicBaseUrl`. Each public app
passes that configured value through `SiteUrl.ResolvePublicBase` and then
`SiteUrl.Combine`: page canonical metadata, `/feed.xml` RSS item/channel links,
and `/sitemap.xml` locations are absolute URLs rooted at the supplied origin.
When the setting is absent, the application intentionally emits relative URLs
and never trusts `Request.Host`. The session and login-throttle stores are
process-local, so replica-safe storage is required before a scale-out
deployment.

The example domains in the production appsettings files are intentionally
documentation defaults. Override them with the real public HTTPS hostnames at
deployment time. `AdminApi__ApiKey` is intentionally absent from the base
admin appsettings file; the admin application refuses to start outside
Development when it is missing or set to `change-me`. The development-only
value is in `appsettings.Development.json` and must not be reused in production.

The admin media directory is configured by `Media:RootPath` and should be
mounted to durable storage, or replaced with an object-storage implementation,
because container filesystems are ephemeral. The public sites use
`Site:MediaBaseUrl` to generate absolute media URLs.

The checked-in Admin image pre-creates `/data/media` and assigns it to the
container's non-root `APP_UID` before switching users. This preserves write
access when Docker initializes the empty named `papyrus-media` volume; keep the
Compose mount at `/data/media` and verify persistence across an Admin restart at
release.

## Build and run the containers

Run these commands from the repository root (the directory containing
[`Papyrus.sln`](../../Papyrus.sln)):

```powershell
docker build -f Dockerfile.admin -t papyrus-admin .
docker run --rm -p 8080:8080 `
  -e ASPNETCORE_ENVIRONMENT=Production `
  -e ConnectionStrings__papyrus="Host=db;Database=papyrus;Username=papyrus;Password=<secret>" `
  -e AdminApi__ApiKey="<secret>" `
  -e Media__RootPath=/data/media `
  -v papyrus-media:/data/media `
  papyrus-admin
```

Build the public images from the same repository root with
`Dockerfile.blog` and `Dockerfile.portfolio`. For a complete local or
release-style stack, configure the required variables and run:

```powershell
docker compose --file docker-compose.yml up --build --detach
```

The container listens on port `8080`. Terminate TLS at the ingress/reverse
proxy and forward `/health`, `/alive`, `/api`, `/admin`, and `/media` to the
container. Health endpoints are anonymous so the load balancer can probe them,
but the ingress/firewall must allow `/health` and `/alive` only from the
orchestrator and internal monitoring networks. Do not publish these paths on a
general public edge. Responses intentionally contain status text only. This is
the network release gate for O-NEW-05; verify the firewall/ingress scope at
release.

`Site__PublicBaseUrl` must be set on both public containers (with the origin
appropriate to each site) before using the performance baseline as release
evidence for canonical, RSS, and sitemap routes (see
[`phase6-baseline.md`](../performance/phase6-baseline.md)).

## Database migrations

Only the admin application owns migrations, via
[`InitializeDatabaseAsync()`](../../src/Papyrus.Infrastructure/DependencyInjection.cs:38).
For the checked-in release workflow, the dedicated `migrate` job applies the
schema with `dotnet ef` and must complete before the `deploy` job starts the
three application containers. Run one coordinated migration job per release;
public services never run migrations.

For a controlled release, the equivalent EF command is:

```powershell
dotnet ef database update `
  --project src/Papyrus.Infrastructure/Papyrus.Infrastructure.csproj `
  --startup-project src/Papyrus.Api.Admin/Papyrus.Api.Admin.csproj `
  --connection "<production connection string>"
```

The admin startup migration remains available for a single-admin deployment.
Back up PostgreSQL and verify the migration history before applying a release.

The current applied migration set contains five migrations, ending with
`20260826005202_SyncSearchTrigramIndexesModel`.

## Release migration job

[`release.yml`](../../.github/workflows/release.yml) builds and scans all three images,
generates an SBOM for each image, then runs the dedicated `migrate` job using
the production connection string and the admin startup project. The `deploy`
job depends on both image publication and successful migration, and supplies
the immutable Admin, Blog, and Portfolio image tags to Compose. Each public
container needs the PostgreSQL connection string and its appropriate `Site__*`
values; public sites remain read-only and never apply migrations.
