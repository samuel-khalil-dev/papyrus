# Phase 0 Release Gates

The following controls define the Phase 0 baseline and remain mandatory for
subsequent release work:

Phase 0 is complete for this repository. Code-level audit findings have been
remediated. W1-W5, O1-O3, O5, and O6 are no longer active risks. W6-W8, O4,
and O7-O10 remain concrete operational release-gate verifications, not
blanket-accepted defects. Deployment portions of O-NEW-04 and O-NEW-05 also
remain gates.

1. [`audit-register.md`](../security/audit-register.md) contains current evidence and release status for C1-C4 and the retained W6-W8/O4/O7-O10 gates; [`risk-acceptance.md`](../security/risk-acceptance.md) records the resolved historical dispositions.
2. The threat model and ADRs are reviewed and linked from the change record.
3. The current automated suite has 73 .NET tests plus 5 Playwright browser tests
   and the solution build has zero warnings/errors with warnings treated as
   errors. Architecture and release-gate tests must still be exercised in the
   production-like gate.
4. No source, project, migration, or secret change is accepted without an applicable roadmap phase and verification plan.
5. Production defaults are fail-closed for credentials, tenant context, media policy, and error disclosure.

---

## Release-gate evidence

This section records the concrete evidence attached to each open operational
gate. Evidence status is marked accurately; where evidence is genuinely pending
a later sub-task, it is marked **Pending** rather than fabricated.

### W6 — Deployment and operations

**Status: Evidence attached (runbooks + deployment drills + docs review).**

- [`incident-runbook.md`](../runbooks/incident-runbook.md) — incident response
  (detection, triage, containment, recovery, post-incident review) tailored to
  the Papyrus architecture, including a deployment/restore drill checklist.
- [`deployment.md`](../guides/deployment.md) — container build/run, required
  production configuration, migration ownership, and health-endpoint network
  scoping.
- Non-root multi-stage images and health checks are documented in
  [`deployment.md`](../guides/deployment.md) and the Dockerfiles.
- **Remaining:** execute the clean deployment, restore, media-persistence, and
  migration-ownership drill at release and record the dated outcome.

### W7 — Observability and alerting

**Status: Evidence attached (telemetry runbook + telemetry outputs).**

- [`telemetry-runbook.md`](../runbooks/telemetry-runbook.md) — how to operate
  and query the shared `Papyrus` ActivitySource, structured audit events,
  redaction, cardinality, sampling, and audit-volume-drop investigation.
- Telemetry implementation and tests from S30.6/S30.7 (shared `Papyrus` W3C
  ActivitySource, structured audit sink, fail-closed redaction, bounded
  cardinality, configurable sampling, audit retention at 0% sampling).
- **Remaining:** record the redaction review, dashboards, alerts, trace
  propagation, and runbook exercise at release.

### W8 — Release documentation and CI

**Status: Evidence attached (docs review).**

- [`deployment.md`](../guides/deployment.md) and the CI/release workflows
  ([`ci.yml`](../../.github/workflows/ci.yml),
  [`release.yml`](../../.github/workflows/release.yml)) document the release
  path, image scanning, SBOM generation, and the dedicated `migrate` job.
- **Remaining:** attach sanitized artifact review and secret-scan output to the
  release approval.

### O4 — Telemetry/audit signal evidence

**Status: Evidence attached (telemetry runbook + S30.6/S30.7 outputs).**

- [`telemetry-runbook.md`](../runbooks/telemetry-runbook.md) documents the
  telemetry/audit signals and how to investigate an audit-volume drop.
- S30.6/S30.7 telemetry outputs: structured audit events, fail-closed redaction,
  bounded diagnostic cardinality, configurable sampling, and tests proving audit
  logging survives 0% diagnostic sampling.
- **Remaining:** production dashboards, alerts, and retention configuration at
  release.

### O7 — Restore/load drills

**Status: Closed — restore, migration-ownership, media-volume, and load evidence attached (2026-08-29 UTC).**

- Restore/migration/readiness/media-volume drill procedures are documented in
  [`incident-runbook.md`](../runbooks/incident-runbook.md) and
  [`deployment.md`](../guides/deployment.md).
- S30.5 was executed against the Docker Compose PostgreSQL 17 deployment on
  2026-08-29 UTC. The public run recorded HTTP p95 **23.574 ms**, payload p95
  **9,030 bytes**, 0.00% failed requests, and 100.00% checks. The admin run
  recorded HTTP p95 **21.660 ms**, payload p95 **9,256 bytes**, 0.00% failed
  requests, 100.00% checks, and zero 5xx responses.
- Raw K6 summaries: [`s30.5-k6-public-20260829.json`](../performance/s30.5-k6-public-20260829.json)
  and [`s30.5-k6-admin-20260829.json`](../performance/s30.5-k6-admin-20260829.json).
- Restore drill: `pg_dump -Fc` created a 26,130-byte backup from the Compose
  PostgreSQL 17 `papyrus` database; `pg_restore --exit-on-error` restored it into
  a disposable `papyrus_restore` database. Verification found four applied EF
  migration-history rows and seven public tables in the restored database.
- Migration-ownership drill: the solution build passed with 0 warnings/errors;
  the two `MigrationOwnershipTests` passed; source inspection found the sole
  production `MigrateAsync()` call in Infrastructure and the sole Admin startup
  initialization call in `Papyrus.Api.Admin`. No applied migration was edited or
  deleted, and no database update was run as part of the drill.
- Media-volume drill: the initial write exposed that a new named volume was root
  owned and not writable by the non-root Admin container. `Dockerfile.admin` now
  creates `/data/media` and assigns it to `APP_UID`; after rebuilding, a sentinel
  file was written to `/data/media`, the Admin container was restarted, and the
  sentinel remained readable. Inspection confirmed the writable named volume is
  mounted at `/data/media`.
- The combined restore/load gate is closed. Temporary drill credentials, backup
  artifact, restore database, and sentinel file were removed after verification.

### O8 — Performance budgets

**Status: Evidence attached (S30.9/S30.10).**

- [`phase6-baseline.md`](../performance/phase6-baseline.md) records the measured
  baseline: public feed DB p95 below 0.1 ms, four public queries, two admin
  collection queries, and explicit latency/error/payload/upload/allocation/GC
  budgets.
- PostgreSQL query-count budget tests (`PerformanceBudgetTests`) enforce the
  public `≤ 4` and admin `≤ 2` command budgets.
- **Remaining:** re-measure with a production-like dataset and `Site__PublicBaseUrl`
  set before using the baseline as release evidence for canonical/RSS/sitemap
  routes.

### O10 — Architecture evidence

**Status: Evidence attached (S30.0 architecture tests).**

- S30.0 architecture tests (NetArchTest) verify modular-monolith boundaries,
  layer rules, and migration ownership.
- [`systemPatterns.md`](../../memory-bank/systemPatterns.md) and the ADRs
  document the modular-monolith decision.
- **Remaining:** record a new ADR only if measured bottlenecks require
  distributed components.
