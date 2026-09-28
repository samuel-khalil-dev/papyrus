# Known Limitations

**Current:** this page records boundaries that are deliberate or still require operational work (the legacy `docs/project/` limitations page was retired during the documentation consolidation). It prevents the showcase documentation from implying capabilities that Papyrus does not currently provide.

## Product and architecture limitations

- Papyrus is not a complete multi-tenant SaaS product. Tenant-aware isolation exists for the current content and media model, but tenant provisioning, operator lifecycle, billing, identity management, and a full tenant-facing administration model are not included.
- Tenant isolation is enforced by the application and EF Core boundaries. RLS is not enabled in PostgreSQL, so future persistence paths must preserve the existing predicates and tests.
- The system remains a modular monolith. Independent service extraction is deferred until measured scale, deployment, or ownership needs justify it.
- Admin authentication is API-key based rather than a full user identity, role, SSO, or browser session system. Multi-operator workflows and richer revocation/audit requirements need additional design.
- The Admin endpoint handlers read and write through `PapyrusDbContext` directly instead of the Application ports, and the public hosts' feed and sitemap handlers do the same. [ADR-0002](../02-architecture/decisions/adr-0002-application-owned-ports-with-infrastructure-adapters.md) is therefore not true end to end. [ADR-0010](../02-architecture/decisions/adr-0010-scoped-presentation-to-infrastructure-access.md) bounds this with a machine-checked boundary and records the port extraction as follow-on work.

## Deployment and operations limitations

- PostgreSQL-backed integration tests require the test database service and are run in CI in the supported verification flow; they are not guaranteed to run on a fresh workstation without the test profile and configuration.
- The local .NET verification baseline is 454 passing non-integration test cases, of which 280 are whole-file encoding cases in `SourceEncodingTests` and 174 assert behaviour. CI covers those plus 22 PostgreSQL integration tests, for 476 unique .NET test cases; the separate Playwright browser smoke suite contains 10 tests. These counts are distinct from one another, and the browser suite is not part of the .NET count. A case count is not a measure of how much behaviour is under test: the encoding guard contributes one case per source file, so the behavioural figure is the smaller number. The [feature matrix](feature-matrix.md) is the canonical count.
- Compose requires production public-base settings for Blog and Portfolio. A standalone deployment with `Site__PublicBaseUrl` unset can produce relative URLs; that fallback is safer than deriving URLs from an untrusted Host header, but it is not a release-ready public URL configuration. The same unset configuration also leaves the host allowlist permissive, because there is no canonical host to enforce — see [M1 routing truth](../05-milestones/m1-routing-truth.md).
- With `Site:PublicBaseUrl` configured, each public host serves only its own canonical host plus the loopback literals, and returns 400 for any other Host header. A deployment whose configured public base URL disagrees with the Host header its traffic actually arrives with therefore fails every request with a 400 rather than serving under the wrong host.
- The loopback exemption in the public-host allowlist is unconditional, because CI, the release workflow, the Aspire AppHost, and the local run path all probe over `127.0.0.1`. Papyrus currently runs under Docker Compose with no reverse proxy or CDN in front of it, so any co-located process or peer on the compose network can present a loopback Host header and be served. The exposure is bounded — the public hosts are read-only, hold no secrets, and canonical URLs are never derived from `Request.Host` — but the allowlist is not a complete defence in the current topology. Scoping the exemption to an explicit flag is tracked with the M2 reverse-proxy milestone, alongside the T-06a rate-limiter keying decision.
- A **malformed** `Site:PublicBaseUrl` (a typo such as `https//blog.example.com`, or a value with no scheme) silently leaves the allowlist permissive, because it is indistinguishable from an unset value. `docker-compose.yml` checks only that the variable is non-empty. Unlike a `Site:Kind` mismatch, this does not fail startup and logs no warning.
- Backups, restore drills, media-volume recovery, ingress restrictions for `/health` and `/alive`, dashboards, alerting, and telemetry retention are deployment responsibilities and require evidence for release approval.
- The local media volume is durable only when the deployment preserves the configured volume. A container replacement without volume recovery loses uploaded media.
- The current telemetry stores and security-related process-local state are not a distributed coordination layer. Horizontal scaling requires an explicit review of those assumptions.

## Performance and compatibility limitations

- Public search, feeds, sitemaps, pagination, and related collections are bounded, but production performance budgets still need measurements for the target workload.
- The S30.10 optimization is intentionally not applied. The current design retains the simpler implementation until evidence justifies the optimization.
- The legacy `/api` surface remains compatibility behavior and is not the versioned published contract. New integrations should use `/api/v1`.
- The checked-in OpenAPI document describes the versioned Admin API; it does not promise undocumented legacy behavior or public Blog/Portfolio page shapes.
