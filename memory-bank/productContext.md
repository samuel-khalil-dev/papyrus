# Product Context

Verified against source 2026-08-29 · Phases 0–5 complete · S30.3, S30.6, and S30.7 complete · 63 tests · 5 migrations · 0-warn/0-error build

Papyrus is a personal technical-writing and professional-showcase platform
with a public blog, a public portfolio, and an authenticated React admin
surface.

## Current product shape

- Blog and Portfolio are read-only Razor Pages sites with live-content
  filtering, search, tags, pagination, RSS, sitemaps, About pages, and SEO
  metadata.
- Admin owns content, taxonomy, relationships, and media mutations through the
  versioned `/api/v1` minimal API and is consumed by the React SPA
  (`Papyrus.Admin.UI`), which is the sole admin UI served from `Papyrus.Api.Admin`
  at `/admin`.
- Projects and posts share categories/tags; projects can link related posts.
- Aspire coordinates local PostgreSQL and service startup, including a dev-only
  `admin-ui` npm resource (Vite HMR + `/api/v1` proxy). The admin API owns
  migration and seed execution.

## User context

- The single operator/author manages content and media through the SPA using an
  API key; there is no interactive login or browser session.
- Public readers consume the blog and portfolio read-only sites.
- Automation/integrations can call the admin API using the same header-based
  key.

## Safety constraints implemented

- Draft, future-dated, deleted, and cross-tenant content is excluded from
  public projections.
- Tenant context, EF filters, relationship constraints, save-boundary checks,
  and tenant-scoped media enforce isolation.
- Admin automation and the SPA use `X-Admin-Api-Key` and `X-Tenant-Id`; the UI
  has no server-side browser session cookie.
- Markdown is rendered by `MarkdownRenderer.ToSafeHtml()` with HTML disabled and
  unsafe schemes/event attributes neutralized. Media uses magic-byte validation,
  SVG rejection, generated names, atomic writes, and reference-checked delete.
- CSP, HSTS, frame/referrer/permissions/nosniff headers, rate limits, multipart
  limits, and fail-closed production API-key settings are active.

## Delivery status

Phases 0–5 are complete. Phase 6 is active: S30.3, telemetry core S30.6, and
tenant-safe telemetry S30.7 are complete; deeper tests and performance budgets remain. Phases 7 and 8 are not
started. The current baseline is 63 passing tests, five applied migrations, and a zero-warning,
zero-error warnings-as-errors build. Residual release gates are W6–W8, O4, and
O7–O10, plus deployment configuration for `Site:PublicBaseUrl` and managed
API-key rotation for multi-operator/replica-scale deployment.
`docs/guides/contributing.md` and `docs/security/policy.md` remain placeholders
until Phase 7.
