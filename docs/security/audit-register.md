# Architecture Audit Register

This is the fresh security and architecture audit of the Papyrus repository at
the Phase 5 completion state. The review covered the admin API and UI,
authentication and sessions, media handling, EF Core persistence and tenant
boundaries, application handlers, public Razor Pages and syndication, service
defaults, tests, and the security documentation.

## Severity conventions

- **[CRITICAL]** is a confirmed security vulnerability, breaking security bug,
  or deadlock risk requiring release-blocking remediation.
- **[WARNING]** is a material performance, availability, operational, or test
  assurance gap that should be tracked but does not require emergency code
  changes in this audit.
- **[OPTIMIZATION]** is a maintainability, hardening, or code-quality
  improvement.

## Executive result

- **[CRITICAL]** None found. No production code or tests required a critical
  remediation.
- **[WARNING]** W-NEW-01 through W-NEW-05 are remediated in this release.
- **[OPTIMIZATION]** O-NEW-01 through O-NEW-05 are remediated or have an
  explicit release-gate control where deployment configuration is required.
- SQL construction is parameterized through EF Core; no SQL injection path was
  found.
- The two intentional `Html.Raw` consumers use
  [`MarkdownRenderer.ToSafeHtml()`](../../src/Papyrus.Core/Content/MarkdownRenderer.cs:30).
- Tenant filters, save-boundary enforcement, composite tenant keys and foreign
  keys, opaque sessions, antiforgery, raster media validation, and security
  headers were re-verified against the current code.

## Previously accepted findings — current disposition

The original W/O report prose is still unavailable. Current code review removed
W1-W5, O1-O3, O5, and O6 from the active risk register because their associated
threats are no longer present. W6-W8, O4, and O7-O10 remain below as concrete
operational or release-gate verifications.

| Finding | Current assessment | Current evidence | Remaining verification | Status |
| --- | --- | --- | --- | --- |
| W6 | Container and deployment procedures exist, with non-root images and health checks | [`deployment.md`](../guides/deployment.md:1), [`docker-compose.yml`](../../docker-compose.yml:1), Dockerfiles, CI workflows | Execute clean deployment, restore, media persistence, and migration-ownership drill at release | **Mitigated; release gate** |
| W7 | OTel and security audit logging are present; alert/runbook evidence remains operational | [`Extensions.cs`](../../src/Papyrus.ServiceDefaults/Extensions.cs:47), [`StructuredAuditEventSink`](../../src/Papyrus.ServiceDefaults/Telemetry/StructuredAuditEventSink.cs:7), [`telemetry-runbook.md`](../runbooks/telemetry-runbook.md:1) | Record redaction review, dashboards, alerts, trace propagation, and runbook exercise before release | **Mitigated; release gate** |
| W8 | Release documentation and CI exist; showcase and secret-scan evidence remains operational | [`ci.yml`](../../.github/workflows/ci.yml:1), [`release.yml`](../../.github/workflows/release.yml:1) | Attach sanitized artifact review and secret-scan output to release approval | **Mitigated; release gate** |

The following formerly accepted findings were removed from the active register after
verification: **W1-W5, O1-O3, O5, and O6**. Their code evidence is summarized in
[`risk-acceptance.md`](risk-acceptance.md:20).

## Retained O4 and O7-O10 release-gate findings

| Finding | Current assessment | Current evidence | Remaining verification | Status |
| --- | --- | --- | --- | --- |
| O4 | OTel is wired; production dashboards, alerts, and retention are an operational release requirement | [`Extensions.cs`](../../src/Papyrus.ServiceDefaults/Extensions.cs:47), [`telemetry-runbook.md`](../runbooks/telemetry-runbook.md:1), [`phase6-baseline.md`](../performance/phase6-baseline.md:1) | Attach observability baseline and runbook linkage to release approval | **Mitigated; release gate** |
| O7 | Migration and recovery procedures are documented; execution is a release gate | [`deployment.md`](../guides/deployment.md:34) | Record backup, migration, restore, readiness, and media-volume drill | **Mitigated; release gate** |
| O8 | Pagination and public feed/search workloads are bounded in code | [`PaginateQueryAsync()`](../../src/Papyrus.Api.Admin/AdminApiEndpoints.cs:407), public query handlers | Record p95, query count, payload size, upload cost, and allocations | **Mitigated; performance gate** |
| O9 | Release artifacts are documented; sanitized publication evidence is a release gate | [`roadmap.md`](../project/roadmap.md:121) | Attach publication-artifact review and secret-scan output | **Mitigated; release gate** |
| O10 | The modular monolith remains the measured architecture decision | [`systemPatterns.md`](../../memory-bank/systemPatterns.md:5), roadmap scale gate | Record a new ADR only if measured bottlenecks require distributed components | **Mitigated; architecture gate** |

## Current audit findings

### [CRITICAL]

None. The audit found no SQL injection, stored/reflected XSS, unsafe media
execution, cross-tenant persistence bypass, credential-in-cookie exposure,
CSRF bypass for cookie-authenticated mutations, or async deadlock pattern.

### [WARNING]

| ID | Finding and impact | Evidence | Recommendation |
| --- | --- | --- | --- |
| W-NEW-01 | Host-derived absolute public URLs are no longer generated. Missing configuration produces relative URLs. | [`SiteUrl.ResolvePublicBase()`](../../src/Papyrus.Web.Shared/SiteUrl.cs:18), public feed/sitemap routes, host-header test | Configure `Site__PublicBaseUrl` and trusted proxy/host policy at release. **Fixed; code and test evidence.** |
| W-NEW-02 | Search uses provider-native `ILIKE`, input is capped, and feed/sitemap materialization is bounded. | [`PublicContentQueryHandlers`](../../src/Papyrus.Application/Content/PublicContentQueries.cs:16), feed routes, trigram migration | Record production query plans and budgets. **Fixed; code and migration evidence.** |
| W-NEW-03 | Legacy `/api` collections now honor bounded page/pageSize values and detail projections cap related collections. | [`MapAdminApi()`](../../src/Papyrus.Api.Admin/AdminApiEndpoints.cs:24), projection helpers | Compat pagination contract test. **Fixed; code evidence.** |
| W-NEW-04 | Feed/sitemap/media operations accept cancellation; media directory walking is offloaded and cancellation-aware on the target runtime. | [`LocalMediaService.ListAsync()`](../../src/Papyrus.Api.Admin/Media/LocalMediaService.cs:62), feed routes, API media endpoints | Release cancellation drill. **Fixed; code evidence.** |
| W-NEW-05 | Admin authentication is header-based and does not require process-local browser sessions or login throttling. | [`ApiKeyAuthenticationHandler`](../../src/Papyrus.Api.Admin/Authentication/ApiKeyAuthenticationHandler.cs:8), [`AdminApiKey`](../../src/Papyrus.Api.Admin/Authentication/AdminApiKey.cs:6) | Use a managed secret/rotation mechanism before multi-operator or multi-key deployment. **Fixed for single-key scope; release gate for key management.** |

### [OPTIMIZATION]

| ID | Finding | Evidence | Recommendation |
| --- | --- | --- | --- |
| O-NEW-01 | Markdown keeps HTML disabled and applies a single defensive URL/event-attribute sanitization boundary with adversarial tests; no new sanitizer dependency was introduced. | [`MarkdownRenderer`](../../src/Papyrus.Core/Content/MarkdownRenderer.cs:7), Markdown tests | Reassess if Markdown features expand. **Fixed for current feature set.** |
| O-NEW-02 | Search uses invariant fallback normalization and PostgreSQL `ILIKE`; trigram indexes are in a new migration. | [`PublicContentQueries`](../../src/Papyrus.Application/Content/PublicContentQueries.cs:18), new migration | Verify provider plans. **Fixed; code and migration evidence.** |
| O-NEW-03 | Inline theme initialization moved to an external asset and CSP no longer allows inline scripts/styles. | [`_PapyrusLayout.cshtml`](../../src/Papyrus.Web.Shared/Pages/Shared/_PapyrusLayout.cshtml:29), [`SecurityHeadersExtensions`](../../src/Papyrus.ServiceDefaults/SecurityHeadersExtensions.cs:12) | Browser smoke test in release. **Fixed; code evidence.** |
| O-NEW-04 | Duplicate public HSTS registrations removed. The development fallback connection string was not changed under secret/config guardrails; production configuration remains deployment-provided. | Public host [`Program.cs`](../../src/Papyrus.Web.Blog/Program.cs:20), [`deployment.md`](../guides/deployment.md:12) | Confirm production environment contains no fallback configuration. **Fixed HSTS; config release gate.** |
| O-NEW-05 | Health responses now expose only status text; ingress must restrict `/health` and `/alive` to orchestrator/monitoring networks. | [`MapDefaultEndpoints()`](../../src/Papyrus.ServiceDefaults/Extensions.cs:109), [`deployment.md`](../guides/deployment.md:62) | Verify firewall/ingress scope. **Fixed response disclosure; network release gate.** |

## Current audit/telemetry state

The audit and telemetry pipeline is implemented in
[`src/Papyrus.ServiceDefaults/Telemetry`](../../src/Papyrus.ServiceDefaults/Telemetry)
and wired through the shared request seam used by Admin, Blog, and Portfolio.
See [`telemetry-runbook.md`](../runbooks/telemetry-runbook.md) for operation and
querying guidance.

- **Structured audit events:** emitted through
  [`StructuredAuditEventSink`](../../src/Papyrus.ServiceDefaults/Telemetry/StructuredAuditEventSink.cs:7)
  as structured log entries (EventId `1001`, name `AuditEvent`) and as
  `Papyrus.AuditEvent` activities on the shared `Papyrus` ActivitySource. Events
  carry tenant, action, target, outcome, status code, correlation id, and trace
  id. There is no persisted audit table or migration.
- **Redaction:** fail-closed via
  [`Redactor`](../../src/Papyrus.ServiceDefaults/Telemetry/Redaction/Redactor.cs:5).
  Any field that does not match a strict allowlist pattern is replaced with
  `[redacted]`. API keys, cookies, request bodies, and raw credentials are never
  emitted.
- **Cardinality:** diagnostic tags are bounded by
  [`CardinalityGuard`](../../src/Papyrus.ServiceDefaults/Telemetry/CardinalityGuard.cs:6)
  (default limit `100`), collapsing excess values to `[other]`.
- **Sampling:** request diagnostics are sampled by
  [`TelemetrySamplingPolicy`](../../src/Papyrus.ServiceDefaults/Telemetry/TelemetrySamplingPolicy.cs:5)
  via `Telemetry:SamplingRate`. **Audit events are not sampled**; audit logging
  survives a forced `0%` diagnostic sampling rate, verified by telemetry tests
  (S30.7).

## Test and code-review coverage

The current suite contains 73 .NET tests plus 5 Playwright browser tests and
covers tenant A/B filtering and mutation attempts, header-based API-key
authentication and matching, raster media signatures and tenant paths, Markdown
HTML and scheme attacks, public live-content filtering, tag resolution, admin
create/update relationships, PostgreSQL integration behavior,
architecture/layer and migration-ownership rules, and telemetry
redaction/sampling. Opaque session expiry/revocation/rotation and login
throttling are no longer covered because those mechanisms were retired under
ADR 0006 (Razor/session removal); authentication is header-key based and
process-local. The review also confirmed EF no-tracking on the current
projection/read paths and parameterized LINQ query construction.

The code-level gaps identified in W-NEW-01 through W-NEW-05 are remediated and
covered by the current 73-test .NET baseline plus the 5-test browser smoke
suite. Remaining assurance work is operational: production-provider tenant
integration, HTTP security/header and feed/sitemap smoke coverage, cancellation
drills, and replica behavior for the process-local security stores. These are
release-gate or Phase 6 work, not unresolved critical defects.

## Current verification record

- Full-solution [`dotnet build`](../../Papyrus.sln:1) completed successfully on
  2026-08-29 with 0 warnings and 0 errors.
- Full-solution [`dotnet test`](../../Papyrus.sln:1) completed successfully on
  2026-08-29 with 73 passing tests, 0 failures, and 0 skipped tests; the
  Playwright browser smoke suite passes 5/5.
- No [CRITICAL] finding was discovered. Remediation changed production code, one
  new migration, and focused tests; no secrets, dependencies, or existing
  migrations were changed.

## Risk-acceptance disposition

The former blanket acceptance is superseded. W1-W5, O1-O3, O5, and O6 were
removed after current-code verification. W6-W8, O4, O7, O8, O9, and O10 remain
tracked release-gate verifications rather than blanket acceptance; O-NEW-04 and
O-NEW-05 retain explicit deployment gates.
