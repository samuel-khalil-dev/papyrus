# Product Context

Papyrus serves public readers consuming the blog and portfolio, plus an operator or automation client managing content through the Admin SPA/API.

## User experience

- Blog and Portfolio are read-only Razor sites with live-content filtering, search, tags, pagination, RSS, sitemaps, and SEO presentation.
- The Admin SPA is the only admin UI. `Papyrus.Api.Admin` serves it at `/admin` and exposes the protected versioned API at `/api/v1`.
- The operator manages projects, blog posts, categories, tags, relationships, and media. Automation can use the same header-authenticated API boundary.

## Security boundaries

- Admin requests provide `X-Admin-Api-Key`; candidates are compared in constant time. `AdminApi:ApiKeys` supports multi-key rotation and legacy `AdminApi:ApiKey` remains compatible.
- `X-Tenant-Id` selects the tenant; absence defaults to `main`. IDs must match `^[a-z0-9][a-z0-9-]{0,62}$`.
- EF global filters, save-boundary enforcement, relationship constraints, and tenant-scoped media paths prevent cross-tenant access. PostgreSQL RLS is not used.
- Public projections exclude drafts, future-dated content, deleted content, and cross-tenant content.
- Media is raster-only, magic-byte validated, capped at 5 MB, atomically written with generated names, tenant-partitioned, and protected from deletion while referenced.
- Markdown disables HTML and neutralizes unsafe schemes/event attributes.
- Per-IP rate limiting is 120 requests/minute; request/multipart limits and security headers are active. `/health` and `/alive` are anonymous and must be network-restricted at ingress.
- There is no browser session cookie, interactive login, legacy cookie gate, or admin Razor-page flow.

## Status

Phases 0–6 are complete; Phases 7–8 are not started. The local baseline is 70 passing non-integration .NET tests. CI adds 14 PostgreSQL integration tests for 84 unique .NET tests; the separate Playwright suite contains 5 tests. The build has 0 warnings and 0 errors.
