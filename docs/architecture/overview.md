# Architecture overview

## Runtime shape

Papyrus runs three web applications over a shared PostgreSQL database:

- **Admin** — authenticated Minimal API and React SPA for content, taxonomy,
  relationships, and tenant-scoped media.
- **Blog** — read-only public Razor Pages site for technical posts, feeds, and
  sitemaps.
- **Portfolio** — read-only public Razor Pages site for projects, feeds, and
  sitemaps.

Aspire coordinates local startup and PostgreSQL. Docker Compose and the three
multi-stage, non-root Dockerfiles provide the deployment shape.

## Module responsibilities

| Module | Responsibility |
| --- | --- |
| `Papyrus.Core` | Domain entities, tenant markers, and safe Markdown rendering |
| `Papyrus.Application` | Validated CQRS commands, queries, and DTO projections |
| `Papyrus.Contracts` | Explicit REST request/response and pagination contracts |
| `Papyrus.Infrastructure` | EF Core, PostgreSQL, filters, save enforcement, migrations, and seed |
| `Papyrus.Api.Admin` | Admin API/UI, authentication, writes, and media |
| `Papyrus.Web.Shared` | Shared public presentation and URL/feed helpers |
| `Papyrus.AppHost` | Aspire orchestration |
| `Papyrus.ServiceDefaults` | Health, telemetry, and shared service defaults |
| `Papyrus.Tests` | xUnit unit, contract, security, and isolation coverage |

## Key flows

1. A request resolves a validated tenant, defaulting to `main` when configured.
2. Application handlers enforce use-case rules and project bounded DTOs.
3. Infrastructure applies tenant and soft-delete filters plus save-boundary
   checks before persistence.
4. Only the admin deployment identity applies migrations and seed data.
5. Public hosts read live, non-deleted content and never receive write authority.

## Security boundaries

Tenant-leading relationships, global filters, save validation, and tenant-scoped
media provide defense in depth. Admin clients use `X-Admin-Api-Key` and
tenant-scoped headers; the React SPA has no cookie-authenticated server UI.
Uploads are validated raster media and Markdown is rendered through the safe
renderer.
