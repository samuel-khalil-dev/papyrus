# Security Baseline

## Transport and edge security

**Planned (M2, M13):** TLS will be terminated at a reverse proxy (Caddy) with Cloudflare providing DDoS mitigation and bot protection (Turnstile) ahead of it. Today services run directly via Docker Compose with no reverse proxy or CDN in front.

## Admin authentication

**Current:** The Admin API is protected by a single factor — a static API key sent via the `X-Admin-Api-Key` header and validated with a constant-time comparison (`AdminApiKey.MatchesAny`). There is no session cookie and no second factor.

**Planned (M3):** Two-factor authentication (TOTP) will be added on top of this, required for all Admin sessions once implemented, given the Admin control plane's growing scope (AI provider credentials, full content and site control). Recovery codes will be issued at enrollment. Until M3 ships, treat the API key itself as the sole credential and rotate it via the existing `AdminApi:ApiKeys` mechanism per the incident runbook.

## Tenant isolation

**Current:** enforced at the application layer through EF Core global query filters and an explicit `EnforceTenantBoundary()` check — not manual per-endpoint checks, and not PostgreSQL Row-Level Security (evaluated and declined; see `docs/architecture/decisions/0007-postgres-rls-decision.md`).

## Secrets

**Current:** no secret in source control; gitleaks scans every push/PR. Operational secrets are supplied via environment variables and configuration (e.g. `AdminApi:ApiKeys`, `POSTGRES_PASSWORD`) and are not encrypted at the application layer.

**Planned (M25, M30):** the AI provider API key will be encrypted at rest via the ASP.NET Core Data Protection API, masked in the UI, and explicitly excluded from audit logs and content export.

## Compliance
Any feature collecting visitor data ships its consent/compliance update in the same milestone. Visitor data subject to a deletion request (newsletter subscribers, stored contact messages if any) supports hard deletion on an admin-triggered erasure action.

## Backups
Automated, scheduled, off-site PostgreSQL backups (M4) with a tested restore procedure, not just a backup job assumed to work.
