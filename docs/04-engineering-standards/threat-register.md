# Threat Register

**Current:** this file consolidates the implemented security governance records (threat model, threat-model summary, and risk-acceptance) into the tracked documentation tree.

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

**Current:** consolidated from the implemented threat-model record.

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
| T-06a | Rate-limit bypass or mis-keying behind a proxy | The admin rate limiter keys on `RemoteIpAddress` (120/min fixed window); behind a reverse proxy/CDN this keys on the proxy IP unless a trusted client-IP header is honored | Document the trusted-proxy assumption and normalise the client identity (e.g., a trusted `X-Forwarded-For`) before multi-operator or public exposure | **Accepted risk; document before deployment** |
| T-07 | Secret or personal-data disclosure | Logs, screenshots, configuration, telemetry, and deployment edges remain release surfaces; absolute public URLs require configuration | Secret scanning, redaction, least privilege, configured public base URLs, trusted-host policy, minimal health responses, and sanitized showcase artifacts | **Mitigated in code; release gates remain** |
| T-08 | Host-header URL poisoning | Missing public base configuration now yields relative URLs; request `Host` is never used for canonical/feed/sitemap generation. M1 additionally derives a per-host `AllowedHosts` list and refuses a foreign Host header with 400 | Configure production public base URLs and enforce trusted reverse-proxy/allowed-host configuration. `PublicHostFilterTests` proves the 400 over real requests; the loopback exemption remains unconditional until M2 scopes it | **Closed in code; release configuration gate** |
| T-09 | Internal error-detail disclosure to API clients | Closed: the media upload handler returned `Results.BadRequest(exception.Message)`, echoing `LocalMediaService` rejection text to any authenticated caller | Every Admin error response is RFC 7807 `ProblemDetails` with fixed, client-safe `title`/`detail`; the original exception is logged server-side and never returned. Asserted in [`AdminApiTests.cs`](../../src/Papyrus.Tests/AdminApiTests.cs:1) against rejection text, the exception type name, and stack frames | **Closed; Phase 7** |
| T-10 | Vulnerability not found by automated static analysis | The repository is **private**, and GitHub code scanning (CodeQL) is not available on private repositories: the API returns 403 "Code scanning is not enabled" and the repository reports no `security_and_analysis` configuration. There is therefore no SAST in the pipeline, and dependency scanning covers known advisories in declared packages only — it does not find a vulnerable pattern in Papyrus's own code | Compensating controls in place: `TreatWarningsAsErrors` with `EnableNETAnalyzers` and `AnalysisLevel=latest`, NetArchTest fitness rules asserting the architectural boundaries, Trivy image scanning at `HIGH`/`CRITICAL`, `dotnet list --vulnerable --include-transitive` failing the release build, and gitleaks secret scanning. If the repository is ever made public, or GitHub Code Security becomes available on the private tier, enable code scanning and treat it as a release gate | **Accepted risk; revisit if visibility changes** |
| T-11 | DTLS heap disclosure or crash in the shared runtime base image (`CVE-2026-84782`, HIGH, CWE-125) | `libssl3t64 3.0.13-0ubuntu3.15` is present in the pinned `mcr.microsoft.com/dotnet/aspnet:10.0@sha256:2d584d81…` runtime that all three images inherit, so the advisory is live in every scanned image and Trivy fails the `containers` job in [`ci.yml`](../../.github/workflows/ci.yml) and the `images` job in [`release.yml`](../../.github/workflows/release.yml). It cannot be removed today: no digest published on MCR carries `libssl3t64 >= 3.0.13-0ubuntu3.16`, and the currently published `10.0` tag resolves to the pinned digest, so the pin is not what holds the fix back. Accepted for a fixed 14 days because the defect is in DTLS and no Papyrus image terminates a DTLS session — see the dated decision below | Re-pin all three `Dockerfile.*` runtime stages to an MCR digest carrying `libssl3t64 >= 3.0.13-0ubuntu3.16`, as a self-contained change on its own branch and pull request. Until then the Trivy step keeps `severity: HIGH,CRITICAL` with `exit-code: 1`, no `.trivyignore` is added, and `ignore-unfixed: true` is not used to suppress an advisory that ships a fix | **Accepted risk; hard review by 2026-10-14, then blocks release** |

### Required review cadence

Review this model at every release, after a security incident, after a new tenant or media capability, and before introducing GraphQL, gRPC, background workers, or service extraction. The former W1-W5, O1-O3, O5, and O6 risks were removed from active acceptance after current-code verification. W6-W8, O4, and O7-O10 remain operational or release-gate checks; O-NEW-01 through O-NEW-03 are remediated, while deployment portions of O-NEW-04 and O-NEW-05 remain gates.

---

## W/O Findings Risk Acceptance

**Current:** consolidated from the implemented risk-acceptance record.

### Decision

- **Decision date:** 2026-08-25
- **Decision owner:** Project owner
- **Tracked release gates:** W6-W8, O4, and O7-O10, plus deployment portions of O-NEW-04 and O-NEW-05.
- **Decision:** W1-W5, O1-O3, O5, and O6 are removed from this risk-acceptance record because the current implementation verifies that they are no longer threats. W6-W8, O4, and O7-O10 remain explicit operational or release-gate verification items and are not waived by this document.

The source audit supplied identifiers without original prose or evidence. The repository does not fabricate those artifacts. The current code and test evidence for the resolved items is recorded below; the retained operational items keep concrete release-gate steps in [release-gates.md](release-gates.md).

### Resolved findings removed from acceptance

These identifiers are historical dispositions, not accepted risks or open findings:

- **W1-W5:** explicit modular boundaries and migration ownership; tenant/query/relationship/save/media isolation; green security controls; antiforgery, secure cookies, rate limiting, and validated media. Evidence: [adr-0001](../02-architecture/decisions/adr-0001-adopt-a-modular-monolith-over-microservices.md), [`PapyrusDbContext`](../../src/Papyrus.Infrastructure/PapyrusDbContext.cs:32), [`InitializeDatabaseAsync()`](../../src/Papyrus.Infrastructure/DependencyInjection.cs:54), [`Program.cs`](../../src/Papyrus.Api.Admin/Program.cs:29), and the focused tenant/media/security tests.
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

---

## Time-boxed risk acceptance — T-11, `CVE-2026-84782`

### Decision

- **Decision date:** 2026-09-30
- **Decision owner:** Project owner
- **Accepted risk:** T-11 — `CVE-2026-84782` (`HIGH`, CWE-125) in OpenSSL's DTLS
  retransmission path, inherited through `libssl3t64 3.0.13-0ubuntu3.15` by all
  three runtime images from the pinned
  `mcr.microsoft.com/dotnet/aspnet:10.0@sha256:2d584d81…` base.
- **Advisory references:** upstream OpenSSL disclosure; Ubuntu `USN-8847-1`,
  published 2026-09-29.
- **Review by:** **2026-10-14** — a hard expiry, not a target date. This is the
  only date on which the acceptance can end without new evidence.
- **Fix:** `libssl3t64` `3.0.13-0ubuntu3.15` → `3.0.13-0ubuntu3.16`.
- **Decision:** accepted for the fixed window above. Upstream describes a
  suspended mid-message DTLS handshake write causing retransmission from a stale
  buffer offset, which can disclose heap memory to the peer as plaintext
  handshake data or crash the process. Ubuntu's own summary claims only
  incorrect handshake behaviour or denial of service, and does not claim
  information disclosure. Both branches require a DTLS peer; there is none, so
  neither is reachable in any Papyrus image.

### Scope of the acceptance

**Accepted:** the Trivy `containers` job in
[`ci.yml`](../../.github/workflows/ci.yml) and the `images` job in
[`release.yml`](../../.github/workflows/release.yml) continue to fail on
`CVE-2026-84782` until the base image is re-pinned. That failure is expected
and is not treated as a regression.

**Not accepted, and not waived:**

- the Trivy gate itself — it is not waived, relaxed, or narrowed;
- any change to scan configuration, scan severity, or scan exit code;
- a `.trivyignore` entry for this or any other advisory;
- a release on its own. This record authorises nothing by itself; a release
  still requires every other gate in [release-gates.md](release-gates.md).

### Evidence relied on

- **Non-reachability, from the repository.** A search for `DTLS`, `Quic`, and
  `QUIC` across `.cs`, `.json`, `.yml`, `.yaml`, `.md`, and `.csproj` returns
  zero matches: no QUIC/HTTP3 package, no `SslProtocols` override.
- **No outbound `HttpClient` in non-test source.** The only `HttpClient`
  references are OpenTelemetry *metrics* instrumentation and test
  `WebApplicationFactory` clients.
- **PostgreSQL does not use TLS.** It connects as `Host=db;Port=5432` with no
  `sslmode` configured anywhere, so that traffic is plaintext on the Compose
  network. `OTEL_EXPORTER_OTLP_ENDPOINT` defaults to empty and, when set, points
  at the in-network collector.
- **The one outbound-TLS operation is outside the scanned images.**
  `docker login`/pull against GHCR runs on the self-hosted runner's host Docker
  client, not inside a Papyrus image.
- **Fix unavailability, verified rather than assumed.** The `linux/amd64` child
  was pulled and `dpkg` read directly: `libssl3t64 3.0.13-0ubuntu3.15`.
  Adjacent tags were checked and ruled out — `10.0-noble` and `latest` resolve
  to the same digest, `10.0.6` is a different digest shipping the older
  `3.0.13-0ubuntu3.9`, and `10.0.16`, `10.0.17`, and `nightly` return 404. No
  MCR digest carries `>= 3.0.13-0ubuntu3.16`. The `10.0` tag digest is
  byte-identical to the pinned digest, so a tag-based reference would not have
  moved either: the pin costs nothing here, and Microsoft has not republished.
- **`ignore-unfixed` is irrelevant to this advisory.** The Trivy step already
  sets `ignore-unfixed: true`, but the advisory ships a fix, so the scan
  correctly continues to fail. The flag must not be used to suppress this or
  any advisory that ships a fix.

### Limits of this assessment

Non-reachability was established from the upstream advisory text plus an
exhaustive repository search. It was **not** established by dynamic test,
because there is no DTLS traffic to exercise. If DTLS or QUIC is ever added to
any Papyrus image, this assessment is void and must be redone.

### Preconditions

This acceptance is valid only while all of the following hold:

1. No DTLS or QUIC endpoint is added to any Papyrus image.
2. The Trivy step keeps `severity: HIGH,CRITICAL` with `exit-code: 1`.
3. `ignore-unfixed: true` is not relied upon to suppress this advisory or any
   advisory that ships a fix.
4. No `.trivyignore` entry is added for `CVE-2026-84782`.

If any precondition fails, the acceptance lapses immediately and T-11 returns to
an open finding.

### On expiry

On **2026-10-14**, if `libssl3t64 >= 3.0.13-0ubuntu3.16` is still not present
in the runtime image, the next release is **blocked** pending the re-pin. The
risk escalates from accepted risk to an open finding and requires explicit
re-approval by the decision owner; it does not renew silently.
