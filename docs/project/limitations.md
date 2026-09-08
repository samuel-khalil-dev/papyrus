# Known limitations

This page records boundaries that are deliberate or still require operational
work. It prevents the showcase documentation from implying capabilities that
Papyrus does not currently provide.

## Product and architecture limitations

- Papyrus is not a complete multi-tenant SaaS product. Tenant-aware isolation
  exists for the current content and media model, but tenant provisioning,
  operator lifecycle, billing, identity management, and a full tenant-facing
  administration model are not included.
- Tenant isolation is enforced by the application and EF Core boundaries. RLS
  is not enabled in PostgreSQL, so future persistence paths must preserve the
  existing predicates and tests.
- The system remains a modular monolith. Independent service extraction is
  deferred until measured scale, deployment, or ownership needs justify it.
- Admin authentication is API-key based rather than a full user identity,
  role, SSO, or browser session system. Multi-operator workflows and richer
  revocation/audit requirements need additional design.

## Deployment and operations limitations

- PostgreSQL-backed integration tests require the test database service and are
  run in CI in the supported verification flow; they are not guaranteed to run
  on a fresh workstation without the test profile and configuration.
- The local .NET verification baseline is 70 passing non-integration tests. CI
  covers those plus 16 PostgreSQL integration tests, for 86 unique .NET tests;
  the separate Playwright browser smoke suite contains 5 tests. These counts
  are distinct from one another, and the browser suite is not part of the
  .NET count.
- Compose requires production public-base settings for Blog and Portfolio. A
  standalone deployment with `Site__PublicBaseUrl` unset can produce relative
  URLs; that fallback is safer than deriving URLs from an untrusted Host header,
  but it is not a release-ready public URL configuration.
- Backups, restore drills, media-volume recovery, ingress restrictions for
  `/health` and `/alive`, dashboards, alerting, and telemetry retention are
  deployment responsibilities and require evidence for release approval.
- The local media volume is durable only when the deployment preserves the
  configured volume. A container replacement without volume recovery loses
  uploaded media.
- The current telemetry stores and security-related process-local state are not
  a distributed coordination layer. Horizontal scaling requires an explicit
  review of those assumptions.

## Performance and compatibility limitations

- Public search, feeds, sitemaps, pagination, and related collections are
  bounded, but production performance budgets still need measurements for the
  target workload.
- The S30.10 optimization is intentionally not applied. The current design
  retains the simpler implementation until evidence justifies the optimization.
- The legacy `/api` surface remains compatibility behavior and is not the
  versioned published contract. New integrations should use `/api/v1`.
- The checked-in OpenAPI document describes the versioned Admin API; it does not
  promise undocumented legacy behavior or public Blog/Portfolio page shapes.
