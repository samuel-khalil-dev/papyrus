# Corrected Text for papyrus-docs/ — Conflicts C1–C6

Implementation spec for the agent executing step 2 of the reconciliation report. Each
section below gives the exact target wording for the affected file. Apply these verbatim
(adjust only for surrounding markdown formatting), then verify no other file in
`papyrus-docs/` still asserts the old, incorrect claim.

**Convention to apply everywhere below, and going forward:** every claim about system
behavior must be labeled either **Current** (true today, verifiable in `src/` or `docs/`)
or **Planned (M-number)** (not yet built). Never state a planned capability in unqualified
present tense. This is the rule that was violated and caused C1–C6; enforcing it is what
prevents recurrence.

---

## C1 — Admin Authentication

### `04-engineering-standards/security-baseline.md`

Replace the admin-authentication paragraph with:

> ## Admin Authentication
>
> **Current:** The Admin API is protected by a single factor — a static API key sent via
> the `X-Admin-Api-Key` header and validated with a constant-time comparison
> (`AdminApiKey.MatchesAny`). There is no session cookie and no second factor.
>
> **Planned (M3):** Two-factor authentication (TOTP) will be added on top of this,
> required for all Admin sessions once implemented, given the Admin control plane's
> growing scope (AI provider credentials, full content and site control). Recovery codes
> will be issued at enrollment. Until M3 ships, treat the API key itself as the sole
> credential and rotate it via the existing `AdminApi:ApiKeys` mechanism per the incident
> runbook.

### `02-architecture/api-standards.md`

Wherever authentication is described, replace any unqualified "two-factor protected"
phrasing with:

> **Current:** authenticated via a static API key (`X-Admin-Api-Key`). **Planned (M3):**
> two-factor authentication in addition to the key.

### `01-product/information-architecture.md`

Remove "authenticated, two-factor protected" from the Admin section header. Replace with:

> ### Admin (authenticated; two-factor protected once M3 ships)

### `07-governance/continuity-and-bus-factor.md`

Change "The two-factor authentication recovery codes for the Admin login (M3)" to
explicitly note this item does not yet apply:

> - The two-factor authentication recovery codes for the Admin login, **once M3 ships**
>   (not yet applicable — Admin currently uses a single static API key; ensure that key
>   is itself stored in a password manager in the meantime).

---

## C2 — Deployment Topology

### `06-operations/deployment-guide.md`

Replace the environments/edge description with:

> ## Current Deployment Topology
>
> Papyrus is deployed via Docker Compose directly — there is **no reverse proxy and no
> CDN in front of it today**. Services are reachable on their configured ports (Admin API,
> Admin UI, Blog, Portfolio, Postgres) as defined in `docker-compose.yml`.
>
> ## Planned Topology (M2, M13)
>
> M2 will introduce Caddy as a reverse proxy terminating TLS and routing each host to its
> container. M13 will place Cloudflare in front of that for CDN caching, DDoS mitigation,
> and Turnstile bot protection. Until both ship, do not assume TLS termination, edge
> caching, or Turnstile are active in production.

### `06-operations/environments.md`

Remove "identical host topology across all three environments" language implying Caddy/
Cloudflare exist today. Replace with:

> ## Host Topology
>
> **Current:** direct Docker Compose networking, no reverse proxy, no CDN.
> **Planned (M2, M13):** Caddy (reverse proxy/TLS) behind Cloudflare (CDN/DDoS/Turnstile),
> with identical topology shape intended across local, containerized-verification, and
> production environments once both milestones ship.

### `04-engineering-standards/performance-budget.md`

Change the infrastructure-headroom paragraph from present tense to:

> **Current:** the 4-core/8GB VPS hosts the application containers and PostgreSQL directly
> via Docker Compose. **Planned (M2, M5, M13):** a reverse proxy, a lightweight monitoring
> agent, and the Cloudflare edge layer will add to this footprint — per-container resource
> limits (M5) are scoped to prevent any of these additions from starving PostgreSQL once
> they exist.

---

## C3 — Migration Ownership

### `06-operations/deployment-guide.md`

Replace "Applied through the Infrastructure project's design-time factory as an explicit
release step" with:

> ## Migration Ownership
>
> **Current:** Only the Admin API applies and seeds migrations, via
> `InitializeDatabaseAsync()` at startup. Public hosts (Blog, Portfolio) never run
> migrations under any circumstance. The release pipeline additionally runs migrations
> explicitly in a dedicated `migrate` job using `dotnet-ef 10.0.11`, ahead of deploying the
> application containers. This ownership boundary is deliberate — see
> `docs/architecture/decisions/0005-migration-ownership.md` — and must not be altered
> without a corresponding decision-record update.

---

## C4 — Observability Stack

### `06-operations/observability-and-monitoring.md`

This section currently states the opposite of what's actually running. Replace entirely:

> ## Application-Level Observability (Current)
>
> Papyrus runs a real OTLP-based telemetry pipeline today: an OTLP collector feeding
> Prometheus and Grafana, with a dedicated `Papyrus` `ActivitySource`, controlled sampling
> (`Telemetry:SamplingRate`), a cardinality limit (`Telemetry:CardinalityLimit`), and nine
> live alert families defined in `docs/runbooks/alerts.md`. `EventId 1001` is intentionally
> never sampled. The Grafana dashboard definition lives at
> `ops/dashboards/papyrus-dashboard.json`.
>
> Do not describe this stack as absent, and do not propose replacing it — Prometheus and
> Grafana are not "too heavy" for this platform; they are already running successfully on
> it.
>
> ## Infrastructure-Level Monitoring (Planned, M5)
>
> M5 adds a **separate, lower-level** layer: host and container resource metrics (CPU,
> memory, disk) via a lightweight agent, private and never publicly exposed. This
> complements the existing application-level OTLP/Prometheus/Grafana stack — it does not
> replace it. M5 exists because the current stack observes the *application*, not the
> *VPS itself*.

---

## C5 — Tenant Isolation and Row-Level Security

### `02-architecture/decisions/adr-0004-retain-multi-tenancy-ready-entity-contracts-in-a-single-tenant.md`

Add a new section to this ADR:

> ## Related Decision: Row-Level Security Declined
>
> A separate, already-made decision (`docs/architecture/decisions/0007-postgres-rls-decision.md`)
> establishes that **application-level tenant enforcement is the sole enforcement layer** —
> implemented via EF Core global query filters and an explicit `EnforceTenantBoundary()`
> check, not PostgreSQL Row-Level Security. RLS was evaluated and deliberately declined as
> a second layer. This ADR's entity-contract retention (`ITenantEntity`/`IEntity`) is the
> mechanism that makes that single application-level layer possible; it does not imply a
> database-level enforcement layer exists or is planned.

### `00-overview/glossary.md`

Update the "Tenant" definition to:

> | Tenant | An isolation boundary on data, enforced at the application layer via EF Core
> global query filters and `EnforceTenantBoundary()` — not database-level Row-Level
> Security, which was evaluated and explicitly declined. Retained even in single-tenant
> deployment for architectural safety. |

### `04-engineering-standards/security-baseline.md`

Change "Enforced automatically through infrastructure interceptors, not manual
per-endpoint checks" to:

> **Tenant isolation:** enforced at the application layer through EF Core global query
> filters and an explicit `EnforceTenantBoundary()` check — not manual per-endpoint checks,
> and not PostgreSQL Row-Level Security (evaluated and declined; see
> `docs/architecture/decisions/0007-postgres-rls-decision.md`).

---

## C6 — Secrets Configuration

### `06-operations/deployment-guide.md`

Replace the secrets paragraph with:

> ## Secrets (Current)
>
> Operational secrets are supplied via environment variables and configuration —
> `AdminApi:ApiKeys` / `ADMIN_API_KEY`, `POSTGRES_PASSWORD`, `BLOG_PUBLIC_BASE_URL`,
> `PORTFOLIO_PUBLIC_BASE_URL` — following standard `appsettings.{Environment}.json`
> override conventions. None of these are encrypted at the application layer today; they
> rely on environment/deployment-level protection.
>
> ## Secrets (Planned, M25)
>
> M25 introduces one new, specific credential type — the AI provider API key — which
> **will** be encrypted at rest via the ASP.NET Core Data Protection API and masked in the
> Admin UI, because it is entered and stored through the application rather than supplied
> as deployment configuration. This encryption approach is scoped to that one new
> credential; it does not retroactively apply to the existing environment-variable-based
> secrets above unless a future milestone explicitly extends it.

### `02-architecture/domain-model.md`

In the `AiProviderProfile` security note, change "explicitly excluded from the Admin audit
log (M30) and the self-service content export (M30)" — this stays correct as written,
since it already correctly scopes the claim to M25/M30 (both planned). No change needed
here; this file was accurate. Flagging it as verified-correct so it isn't second-guessed
during the merge.

---

## Also Apply While in These Files (Mechanical Fixes)

1. Rename:
   - `adr-0003-react-for-admin-server-rendered-razor-pages-for-portfolio-an….md` → drop
     the trailing ellipsis, use a filename under the same length limit the renamer
     enforces (e.g. `adr-0003-react-admin-razor-public.md`), and update its own internal
     `# ADR-0003:` title line to match if shortened.
   - `adr-0004-retain-multi-tenancy-ready-entity-contracts-in-a-single-tena….md` →
     `adr-0004-retain-multitenant-entities.md`.
   - `adr-0008-include-right-to-left-layout-support-in-the-localization-fou….md` →
     `adr-0008-rtl-layout-from-inception.md`.
2. Update every in-repo reference to the three filenames above (search the whole
   `papyrus-docs/` tree for the old names, including truncated forms).
3. Fix `02-architecture/architecture-overview.md`'s reference to
   `decisions/adr-0009-reverse-proxy-and-edge-layer.md` → the correct on-disk name
   `decisions/adr-0009-reverse-proxy-and-edge-layer-caddy-behind-cloudflare.md`.
4. Fix `docs/runbooks/alerts.md`'s anchor `incident-runbook.md#3-triage` →
   `incident-runbook.md#3-containment-and-recovery-rules`.

## Verification After Applying

- Grep `papyrus-docs/` for "2FA", "two-factor", "Netdata", "reverse proxy", "Cloudflare",
  "Row-Level Security", "encrypted at rest" — confirm every remaining instance is
  correctly labeled Current or Planned per the convention above.
- Grep for the three original truncated filenames — zero remaining references.
- Confirm `adr-0009` reference resolves to an existing file.
- Do not touch `docs/api/openapi.json`, `docs/architecture/release-gates.md`, or any file
  on the report's "never delete" list as part of this pass — this spec only covers
  `papyrus-docs/` corrections plus the two named `docs/` anchor/link fixes.
