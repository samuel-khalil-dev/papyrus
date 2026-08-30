# Architecture overview

Papyrus is a **modular monolith** (.NET 10, EF Core 10.0.11, React 19 admin SPA,
Aspire orchestration, PostgreSQL 17 via Docker Compose). It is documented with
the **C4 Model** — see the [C4 index](README.md) for the System Context,
Container, and Component views.

## Runtime shape

Papyrus runs three web applications over a shared PostgreSQL database:

- **Admin** — authenticated Minimal API and React SPA for content, taxonomy,
  relationships, and tenant-scoped media. It is the **sole** migration/seed
  owner.
- **Blog** — read-only public Razor Pages site for technical posts, feeds, and
  sitemaps.
- **Portfolio** — read-only public Razor Pages site for projects, feeds, and
  sitemaps.

Aspire coordinates local startup and PostgreSQL. Docker Compose and the three
multi-stage, non-root Dockerfiles provide the deployment shape.

## C4 views

| Level | View | File |
| --- | --- | --- |
| 1 | System Context | [`c4/context.md`](c4/context.md) |
| 2 | Containers | [`c4/container.md`](c4/container.md) |
| 3 | Components (Admin API) | [`c4/component.md`](c4/component.md) |

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

`Papyrus.Admin.UI` (React SPA) is **not** a `.sln` project — it is a standalone
npm app referenced by Aspire as dev-only and built into the admin image via a
Node stage.

## Key flows

1. A request resolves a validated tenant, defaulting to `main` when configured.
2. Application handlers enforce use-case rules and project bounded DTOs.
3. Infrastructure applies tenant and soft-delete filters plus save-boundary
   checks before persistence.
4. Only the admin deployment identity applies migrations and seed data.
5. Public hosts read live, non-deleted content and never receive write authority.

## Security boundaries

- **API-key auth:** `ApiKeyAuthenticationHandler` reads `X-Admin-Api-Key` and
  compares it in constant time via `AdminApiKey.MatchesAny`. Multi-key rotation
  is supported through the current multi-key configuration and a legacy
  compatibility setting. Production fails fast if no key is configured or a
  default placeholder is used.
- **Tenant isolation:** `TenantContext` resolves `X-Tenant-Id` (default `main`),
  validated against `^[a-z0-9][a-z0-9-]{0,62}$`. EF global filters plus
  `EnforceTenantBoundary()` in `PapyrusDbContext` are the enforcement layer.
  Media is tenant-partitioned under `{root}/{tenantId}/`. **RLS is not used**
  (ADR 0007); application-level filtering is the sole enforcement layer.
- **Admin clients** use `X-Admin-Api-Key` and tenant-scoped headers; the React
  SPA has no cookie-authenticated server UI.
- **Uploads** are validated raster media (magic-byte checked, 5 MB cap) and
  Markdown is rendered through the safe renderer.

## Observability

All delivery hosts use the shared `Papyrus` ActivitySource with W3C trace
propagation. Audit events are logger-backed, never sampled, and redacted at the
sink seam. OTLP export is gated on `OTEL_EXPORTER_OTLP_ENDPOINT`.

## Decision log

Architecture decisions are recorded in the [ADR log](decisions/README.md)
(ADR 0001–0007).
