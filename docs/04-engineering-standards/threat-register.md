# Threat Register

**Current:** this file is ported from the implemented security governance records in `docs/security/` (`threat-model.md`, `threat-model-summary.md`, `risk-acceptance.md`).

## Public-safe excerpt

**Current:** this is the sanitized public-facing version of the threat model. It describes the security posture of Papyrus without publishing credentials, private endpoints, deployment values, or personal data. It is an overview, not a guarantee that a deployment is secure by default. The full internal register follows below.

### Assets and trust boundaries

The important assets are tenant content, administrator credentials, uploaded media, PostgreSQL data, deployment configuration, and telemetry. Untrusted data can arrive through public query parameters, administrative requests, Markdown, uploads, host configuration, and operational settings.

The main boundaries are the HTTPS edge, admin authentication, tenant selection, application policy, persistence, media storage, and telemetry export. Public hosts are read-only readers and must never receive administrative credentials or write authority.

### Current controls

| Area | Control | Status |
| --- | --- | --- |
| Authentication | Protected Admin API routes require `X-Admin-Api-Key`; comparison is constant-time and production rejects missing/default configuration. | Implemented; deployment key management remains required |
| Tenant isolation | Validated tenant context, EF global filters, save-boundary enforcement, composite tenant relationships, and tenant-scoped media paths. | Implemented for the current application boundary |
| Authorization | Public hosts are read-only; administrative mutations are behind the Admin API authorization policy. | Implemented |
| Content safety | Parameterized EF queries, bounded search and pagination, safe Markdown rendering, and safe URL handling. | Implemented for the current feature set |
| Media safety | Raster signature/structure checks, size and dimension limits, generated names, atomic writes, and traversal-safe serving. | Implemented |
| Request protection | Per-IP Admin API rate limiting and cancellation-aware bounded operations. | Implemented; capacity remains an operational concern |
| Observability | Structured audit events, fail-closed redaction, bounded diagnostic cardinality, and optional W3C/OTLP tracing. | Implemented; dashboards and retention are deployment responsibilities |
| Runtime hardening | Multi-stage images run as non-root users and expose minimal health responses. | Implemented; ingress must restrict health probes |

### Known residual risks and release gates

- API-key authentication is intentionally simple and header-based. Operators must use a secret manager, rotate keys safely, protect the transport, and avoid putting keys in source control or browser-shared environments.
- Tenant isolation is application-enforced, not database-enforced. A future change to persistence, background work, or service extraction must preserve tenant predicates and add adversarial tests.
- Backups, restore drills, media-volume recovery, trusted proxy/host policy, dashboards, alerting, and telemetry retention require deployment evidence.
- The current process-local security stores are not a distributed session system. This matters if the deployment is scaled or operators need a richer identity and revocation workflow.

The detailed internal register is below, and the project's reporting guidance is in the security policy note in [security-baseline.md](security-baseline.md).

### Review triggers

Review this summary at every release, after an incident, when tenant or media capabilities change, or before adding background workers, GraphQL, gRPC, or service extraction. The absence of a critical finding in the current review is not a substitute for production verification.

---

## Full threat register

**Current:** ported from `docs/security/threat-model.md`.

### Scope and trust boundaries

The protected assets are tenant content, administrator credentials and sessions, media files, PostgreSQL data, deployment secrets, and telemetry. Untrusted inputs enter through public query parameters, admin headers/cookies/forms, markdown, uploaded files, host headers, and operational configuration.

The primary trust boundaries are the HTTPS edge, admin authentication, tenant resolution, application/domain policy, PostgreSQL, media storage, and telemetry exporters. Public applications are untrusted readers and must never receive admin credentials or write authority.

### Threat register

| ID | Threat | Current exposure | Required control | Gate |
| --- | --- | --- | --- | --- |
| T-01 | Cross-tenant read or mutation | Closed: runtime tenant isolation is enforced | Scoped tenant context, EF global filters, save-boundary enforcement, tenant-partitioned media, and adversarial A/B tests in [`TenantIsolationTests.cs`](../../src/Papyrus.Tests/TenantIsolationTests.cs:1) and [`AdminApiTests.cs`](../../src/Papyrus.Tests/AdminApiTests.cs:1) | **Closed; Phase 1** |
| T-02 | Credential replay or session theft | Closed: admin uses header-based API-key authentication and no browser session cookie | API keys are compared in fixed time, production configuration fails closed, credentials are not logged, and the SPA sends the key only to protected admin API routes | **Closed; Phase 1** |
| T-03 | Cross-site request forgery | Closed for the current header-authenticated API | State-changing admin routes require API-key authorization; the retired cookie-authenticated Razor mutation flow and its antiforgery dependency are removed | **Closed; Phase 2** |
| T-04 | Active media execution | Closed: only validated raster uploads with generated tenant-scoped names are stored and served | Magic-byte, structure, dimension, size, atomic-write, traversal, and safe-serving controls plus media validation tests | **Closed; Phase 2** |
| T-04a | Malicious media payload (undetected by magic-byte checks) | Accepted risk for the personal-site scope: uploads are validated by magic bytes, structure, dimensions, and a 5 MB cap, but are **not** scanned by antivirus or re-encoded | Add AV scanning (e.g., ClamAV) or image re-encoding before storage if the deployment serves untrusted third-party uploads | **Accepted risk; document before multi-operator use** |
| T-05 | Injection and unsafe rendering | Closed for the Phase 1 markdown/rendering scope; attacker-controlled search, URLs, and media remain separately governed | Parameterized queries, [`MarkdownRenderer.ToSafeHtml()`](../../src/Papyrus.Core/Content/MarkdownRenderer.cs:37), event-handler stripping, unsafe-scheme rejection, safe URL policy, and adversarial [`MarkdownRendererTests.cs`](../../src/Papyrus.Tests/MarkdownRendererTests.cs:1) | **Closed; Phase 1** |
| T-06 | Availability loss | Database, media, and dependency failure remain operational concerns; legacy collections, feeds, searches, and security stores are bounded in code | Health/readiness checks, request limits, bounded pagination/feed/search queries, cancellation, bounded store eviction, backups, and restore drills | **Mitigated in code; operational release gates remain** |
| T-06a | Rate-limit bypass or mis-keying behind a proxy | The admin rate limiter keys on `RemoteIpAddress` (120/min fixed window); behind a reverse proxy/CDN this keys on the proxy IP unless a trusted client-IP header is honored | Document the trusted-proxy assumption and normalize the client identity (e.g., a trusted `X-Forwarded-For`) before multi-operator or public exposure | **Accepted risk; document before deployment** |
| T-07 | Secret or personal-data disclosure | Logs, screenshots, configuration, telemetry, and deployment edges remain release surfaces; absolute public URLs require configuration | Secret scanning, redaction, least privilege, configured public base URLs, trusted-host policy, minimal health responses, and sanitized showcase artifacts | **Mitigated in code; release gates remain** |
| T-08 | Host-header URL poisoning | Missing public base configuration now yields relative URLs; request `Host` is never used for canonical/feed/sitemap generation | Configure production public base URLs and enforce trusted reverse-proxy/allowed-host configuration | **Closed in code; release configuration gate** |

### Required review cadence

Review this model at every release, after a security incident, after a new tenant or media capability, and before introducing GraphQL, gRPC, background workers, or service extraction. The former W1-W5, O1-O3, O5, and O6 risks were removed from active acceptance after current-code verification. W6-W8, O4, and O7-O10 remain operational or release-gate checks; O-NEW-01 through O-NEW-03 are remediated, while deployment portions of O-NEW-04 and O-NEW-05 remain gates.

---

## W/O Findings Risk Acceptance

**Current:** ported from `docs/security/risk-acceptance.md`.

### Decision

- **Decision date:** 2026-08-25
- **Decision owner:** Project owner
- **Tracked release gates:** W6-W8, O4, and O7-O10, plus deployment portions of O-NEW-04 and O-NEW-05.
- **Decision:** W1-W5, O1-O3, O5, and O6 are removed from this risk-acceptance record because the current implementation verifies that they are no longer threats. W6-W8, O4, and O7-O10 remain explicit operational or release-gate verification items and are not waived by this document.

The source audit supplied identifiers without original prose or evidence. The repository does not fabricate those artifacts. The current code and test evidence for the resolved items is recorded below; the retained operational items keep concrete release-gate steps in [`audit-register.md`](../security/audit-register.md).

### Resolved findings removed from acceptance

These identifiers are historical dispositions, not accepted risks or open findings:

- **W1-W5:** explicit modular boundaries and migration ownership; tenant/query/relationship/save/media isolation; green security controls; antiforgery, secure cookies, rate limiting, and validated media. Evidence: [`0001-modular-monolith-boundaries.md`](../architecture/decisions/0001-modular-monolith-boundaries.md:5), [`PapyrusDbContext`](../../src/Papyrus.Infrastructure/PapyrusDbContext.cs:32), [`InitializeDatabaseAsync()`](../../src/Papyrus.Infrastructure/DependencyInjection.cs:47), [`Program.cs`](../../src/Papyrus.Api.Admin/Program.cs:29), and the focused tenant/media/security tests.
- **O1-O3:** versioned DTO contracts, no-tracking projections, tenant/live public filtering, and bounded feed/sitemap source queries are implemented in [`AdminApiEndpoints`](../../src/Papyrus.Api.Admin/AdminApiEndpoints.cs:14), [`PublicContentQueryHandlers`](../../src/Papyrus.Application/Content/PublicContentQueries.cs:8), and [`TenantIsolationTests`](../../src/Papyrus.Tests/TenantIsolationTests.cs:35).
- **O5-O6:** projection SQL and PostgreSQL execution evidence show bounded, non-N+1 read paths, while all runtime images are multi-stage and non-root. Evidence: [`projection-evidence.md`](../history/projection-evidence.md:66), [`Dockerfile.admin`](../../Dockerfile.admin:20), and the CI/release workflows.

### Rationale and boundaries

This governance record is limited to release verification conditions. It does not:

- represent unavailable source audit evidence as present;
- waive the P0 production-blocker requirements for C1-C4;
- assert that any planned operational verification has passed; or
- authorize a production release by itself.

W6-W8, O4, and O7-O10 are `Mitigated; release gate`, not accepted residual defects. The required deployment, monitoring, backup/restore, network, performance, architecture, and publication-artifact evidence must be attached to the release approval.

### Revisit conditions

This decision must be revisited when any of the following occurs:

1. The original audit report or evidence becomes available; it must then be attached to each applicable register row and the findings re-evaluated.
2. A public or production release is proposed; all applicable W/O verification gates must be completed and recorded before release approval.
3. New evidence changes the severity, owner, remediation, or acceptance criteria for any W/O finding.

### Approval

**Approved by:** Project owner  
**Date:** 2026-08-25