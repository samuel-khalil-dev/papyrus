# Papyrus Threat Model

## Scope and trust boundaries

The protected assets are tenant content, administrator credentials and sessions, media files, PostgreSQL data, deployment secrets, and telemetry. Untrusted inputs enter through public query parameters, admin headers/cookies/forms, markdown, uploaded files, host headers, and operational configuration.

The primary trust boundaries are the HTTPS edge, admin authentication, tenant resolution, application/domain policy, PostgreSQL, media storage, and telemetry exporters. Public applications are untrusted readers and must never receive admin credentials or write authority.

## Threat register

| ID | Threat | Current exposure | Required control | Gate |
| --- | --- | --- | --- | --- |
| T-01 | Cross-tenant read or mutation | Closed: runtime tenant isolation is enforced | Scoped tenant context, EF global filters, save-boundary enforcement, tenant-partitioned media, and adversarial A/B tests in [`TenantIsolationTests.cs`](../../src/Papyrus.Tests/TenantIsolationTests.cs:1) and [`AdminApiTests.cs`](../../src/Papyrus.Tests/AdminApiTests.cs:1) | **Closed; Phase 1** |
| T-02 | Credential replay or session theft | Closed: admin uses header-based API-key authentication and no browser session cookie | API keys are compared in fixed time, production configuration fails closed, credentials are not logged, and the SPA sends the key only to protected admin API routes | **Closed; Phase 1** |
| T-03 | Cross-site request forgery | Closed for the current header-authenticated API | State-changing admin routes require API-key authorization; the retired cookie-authenticated Razor mutation flow and its antiforgery dependency are removed | **Closed; Phase 2** |
| T-04 | Active media execution | Closed: only validated raster uploads with generated tenant-scoped names are stored and served | Magic-byte, structure, dimension, size, atomic-write, traversal, and safe-serving controls plus media validation tests | **Closed; Phase 2** |
| T-05 | Injection and unsafe rendering | Closed for the Phase 1 markdown/rendering scope; attacker-controlled search, URLs, and media remain separately governed | Parameterized queries, [`MarkdownRenderer.ToSafeHtml()`](../../src/Papyrus.Core/Content/MarkdownRenderer.cs:37), event-handler stripping, unsafe-scheme rejection, safe URL policy, and adversarial [`MarkdownRendererTests.cs`](../../src/Papyrus.Tests/MarkdownRendererTests.cs:1) | **Closed; Phase 1** |
| T-06 | Availability loss | Database, media, and dependency failure remain operational concerns; legacy collections, feeds, searches, and security stores are bounded in code | Health/readiness checks, request limits, bounded pagination/feed/search queries, cancellation, bounded store eviction, backups, and restore drills | **Mitigated in code; operational release gates remain** |
| T-07 | Secret or personal-data disclosure | Logs, screenshots, configuration, telemetry, and deployment edges remain release surfaces; absolute public URLs require configuration | Secret scanning, redaction, least privilege, configured public base URLs, trusted-host policy, minimal health responses, and sanitized showcase artifacts | **Mitigated in code; release gates remain** |
| T-08 | Host-header URL poisoning | Missing public base configuration now yields relative URLs; request `Host` is never used for canonical/feed/sitemap generation | Configure production public base URLs and enforce trusted reverse-proxy/allowed-host configuration | **Closed in code; release configuration gate** |

## Required review cadence

Review this model at every release, after a security incident, after a new tenant or media capability, and before introducing GraphQL, gRPC, background workers, or service extraction. The former W1-W5, O1-O3, O5, and O6 risks were removed from active acceptance after current-code verification. W6-W8, O4, and O7-O10 remain operational or release-gate checks; O-NEW-01 through O-NEW-03 are remediated, while deployment portions of O-NEW-04 and O-NEW-05 remain gates.
