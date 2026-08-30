# Threat-model summary

This public summary describes the security posture of Papyrus without publishing
credentials, private endpoints, deployment values, or personal data. It is an
overview, not a guarantee that a deployment is secure by default.

## Assets and trust boundaries

The important assets are tenant content, administrator credentials, uploaded
media, PostgreSQL data, deployment configuration, and telemetry. Untrusted data
can arrive through public query parameters, administrative requests, Markdown,
uploads, host configuration, and operational settings.

The main boundaries are the HTTPS edge, admin authentication, tenant selection,
application policy, persistence, media storage, and telemetry export. Public
hosts are read-only readers and must never receive administrative credentials or
write authority.

## Current controls

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

## Known residual risks and release gates

- API-key authentication is intentionally simple and header-based. Operators must
  use a secret manager, rotate keys safely, protect the transport, and avoid
  putting keys in source control or browser-shared environments.
- Tenant isolation is application-enforced, not database-enforced. A future
  change to persistence, background work, or service extraction must preserve
  tenant predicates and add adversarial tests.
- Backups, restore drills, media-volume recovery, trusted proxy/host policy,
  dashboards, alerting, and telemetry retention require deployment evidence.
- The current process-local security stores are not a distributed session
  system. This matters if the deployment is scaled or operators need a richer
  identity and revocation workflow.

The detailed internal register is in the [audit register](audit-register.md),
and the project’s reporting guidance is in the [security policy](policy.md).

## Review triggers

Review this summary at every release, after an incident, when tenant or media
capabilities change, or before adding background workers, GraphQL, gRPC, or
service extraction. The absence of a critical finding in the current review is
not a substitute for production verification.
