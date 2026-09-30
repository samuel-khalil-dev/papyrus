# Release Gates

The following controls define the release baseline and remain mandatory for
subsequent release work. They are enforced by the CI and release workflows
([`ci.yml`](../../.github/workflows/ci.yml),
[`release.yml`](../../.github/workflows/release.yml)) and by the operational
gates recorded below.

## CI pipeline gates

The `verify` job in [`ci.yml`](../../.github/workflows/ci.yml) runs on every
push to `main` and every pull request:

1. **Restore** — `dotnet restore Papyrus.sln`.
2. **Build with warnings treated as errors** —
   `dotnet build Papyrus.sln --configuration Release --no-restore /p:TreatWarningsAsErrors=true`.
3. **Tests** — `dotnet test Papyrus.sln` runs the full suite: the
   provider-independent/non-integration tests, then the
   `Papyrus.Tests.Integration` tests against a `postgres:17-alpine` service.
   Every case must pass, and none may be skipped: the baseline check below fails
   on any TRX outcome other than `Passed`, so a skipped case cannot quietly
   reduce coverage. The current case count lives in the
   [feature matrix](../00-overview/feature-matrix.md) and is deliberately not
   restated here, because a count is not a gate — the gate is that the job runs
   and nothing fails, is skipped, or errors.
4. **PostgreSQL integration tests** — the workflow runs the full suite first and
   then repeats the `Papyrus.Tests.Integration` filter explicitly. The repeated
   run is a re-assertion, not additional coverage; the unique test count is
   unchanged by it.
5. **Documented test baseline** — the `verify` job derives the published figures
   from the TRX files the two test steps already write, and fails if the
   `<!-- test-baseline: ... -->` marker in the feature matrix, or any figure
   restated in `README.md`, `limitations.md`, or `Papyrus_Roadmap.md`, disagrees
   with the run. The figures are never hand-counted. This is what stops a
   test-adding change from leaving a stale "verified baseline" behind: adding
   any source file shifts both the total and the encoding-guard count, so the
   marker must be updated in the same change. The count is checked because it is
   quoted as evidence; it is not itself a gate on what the suite covers.
6. **Secret scan** — `gitleaks/gitleaks-action@v3.0.0`.
7. **NuGet vulnerability scan** — `dotnet list ... package --vulnerable
   --include-transitive`; fails on any vulnerable package.

The `browser-smoke` job (after `verify`) builds the Admin UI, runs the 10
Playwright browser smoke tests against the Admin API, Blog, and Portfolio, and
uploads the report. The `load-smoke` job runs the K6 public and admin load
smokes. The `containers` job builds each image and scans it with Trivy for
`HIGH`/`CRITICAL` vulnerabilities (`exit-code: 1`, `ignore-unfixed: true`).

At the time of writing the `containers` job fails on one known advisory:
`CVE-2026-84782` (`HIGH`, DTLS) in the shared runtime base image, which no
published MCR digest yet fixes. It is a time-boxed accepted risk recorded as
T-11 in the [threat register](threat-register.md), not a new gate and not a
regression. The scan is not to be weakened to clear it.

OpenAPI drift is verified by the dedicated
[`openapi-schema.yml`](../../.github/workflows/openapi-schema.yml) workflow,
which starts the Admin API, fetches `/openapi/v1.json`, and diffs it against
the committed [`docs/api/openapi.json`](../api/openapi.json).

## Architecture baseline note (2026-09-08)

The `Application → Infrastructure` dependency was inverted to align with
[ADR-0002 (application-owned ports with Infrastructure adapters)](../02-architecture/decisions/adr-0002-application-owned-ports-with-infrastructure-adapters.md).
`Papyrus.Application` owns the persistence ports (`IPublicContentStore`,
`IContentCommandStore`, `IContentQueryStore`, `ITaxonomyCommandStore`,
`IContentLifecycleStore`) and thin facades; `Papyrus.Infrastructure` implements
them with EF Core adapters (`Ef*Store`). The application layer no longer depends
on `PapyrusDbContext`. The test harness registers
`PapyrusSaveChangesInterceptor` so soft-delete and audit behaviour is exercised
through the real mechanism. This is an implementation alignment with the
existing ADR and does not change the release-gate evidence recorded below.

## Release pipeline gates

The [`release.yml`](../../.github/workflows/release.yml) workflow runs on
`v*.*.*` tags and `workflow_dispatch`:

1. **dependency-scan** — NuGet vulnerability scan and gitleaks secret scan.
2. **images** — builds the admin, blog, and portfolio images, scans each with
   Trivy, generates an SPDX SBOM, and publishes immutable image tags to GHCR.
3. **migrate** — a dedicated migration job in the `production-migrations`
   environment installs `dotnet-ef` 10.0.11 and applies
   `dotnet ef database update` against the production connection string.
4. **deploy** — runs only after `images` and `migrate` succeed, on a
   self-hosted runner in the `production` environment, via
   `docker compose up --detach --remove-orphans`, then polls the published
   health endpoints (`/health` on the Admin API, `/` on Blog and Portfolio) and
   fails the job if any host does not become reachable.

## Release-gate evidence

This section records the concrete evidence attached to each open operational
gate. Evidence status is marked accurately; where evidence is genuinely pending
a later sub-task, it is marked **Pending** rather than fabricated.

### W6 — Deployment and operations

**Status: Evidence attached (2026-09-02 UTC local release drill).**

- [`runbooks.md`](../06-operations/runbooks.md) — incident response
  (detection, triage, containment, recovery, post-incident review) tailored to
  the Papyrus architecture, including a deployment/restore drill checklist.
- [`deployment-guide.md`](../06-operations/deployment-guide.md) — container
  build/run, required production configuration, migration ownership, and
  health-endpoint network scoping.
- Non-root multi-stage images and health checks are documented in
  [`deployment-guide.md`](../06-operations/deployment-guide.md) and the
  Dockerfiles.
- **2026-09-02 drill outcome — PASS (local Compose release candidate):** a
  clean isolated project (`papyrus-release-drill`) built all three images and
  started PostgreSQL 17, Admin, Blog, and Portfolio detached. The database was
  healthy and `/health` returned HTTP 200 on all three web hosts. A custom-format
  `pg_dump` completed at 21,661 bytes; `pg_restore --exit-on-error` restored a
  disposable database, where four EF migration-history rows and seven public
  tables were verified. A sentinel written to `/data/media` remained readable
  after an Admin restart. The migration-ownership test filter passed 2/2 tests;
  the mandated solution build passed with 0 warnings and 0 errors. The
  disposable database, named volumes, stack, sentinel, and backup were removed
  after verification. No credentials or connection strings are recorded.

### W7 — Observability and alerting

**Status: Platform deployed and probes trustworthy; alert delivery and the
fire-and-recover drill unverified.**

- [`observability-and-monitoring.md`](../06-operations/observability-and-monitoring.md)
  — how to operate and query the shared `Papyrus` ActivitySource, structured
  audit events, redaction, cardinality, sampling, the alert families, and the
  audit-volume-drop investigation.
- Telemetry implementation and tests from S30.6/S30.7 (shared `Papyrus` W3C
  ActivitySource, structured audit sink, fail-closed redaction, bounded
  cardinality, configurable sampling, audit retention at 0% sampling).
- **2026-09-02 local outcome — PASS for code/config checks:** the telemetry test
  filter passed 6/6 tests, including fail-closed redaction, W3C activity IDs,
  audit logging without an exporter, and audit survival at 0% diagnostic
  sampling. The dashboard JSON
  ([`observability/dashboards/papyrus-dashboard.json`](../observability/dashboards/papyrus-dashboard.json))
  parsed successfully, and a synthetic W3C `traceparent` health request returned
  HTTP 200. The runbook and alert-family mapping were reviewed against the
  dashboard contract.
- **2026-09-29 — the dashboard's datasource was unbound, and parsing was not
  evidence of anything. Now fixed in the file and verified on a fresh volume;
  the deployed instance is not yet corrected.** The
  2026-09-02 entry recorded that the dashboard's panels "use the documented
  Prometheus datasource placeholder". That is true of the file and was the reason
  the defect survived: the dashboard referenced `${DS_PROMETHEUS}` in every panel
  datasource block it declared, which is a Grafana *import-time* variable. The
  dashboard is mounted by file provisioning
  (`ops/observability/grafana/provisioning/dashboards/papyrus.yml`), which does
  not resolve it, and the provisioned datasource set no `uid` to resolve it to.
  Every panel therefore had no datasource to bind to. The dashboard still
  rendered, so nothing failed: a green Prometheus, a healthy collector, and a
  dashboard of empty panels are each consistent with a correct deployment, which
  is precisely why O4's "live dashboard data" item was the one that would have
  caught it and had not yet been executed.
  The provisioned datasource now declares `uid: papyrus-prometheus`
  (`ops/observability/grafana/provisioning/datasources/prometheus.yml`) and the
  dashboard names that same uid in every datasource block it declares
  (`docs/observability/dashboards/papyrus-dashboard.json`, which is a runtime
  artifact rather than a document): 26 blocks, being the per-panel, per-`target`,
  and nested-`fieldConfig` blocks of its 12 data panels plus the `app` template
  variable's own block. The built-in `Annotations & Alerts` entry is bound to
  Grafana's internal `-- Grafana --` datasource and is not one of the 26. Note
  that this file's `panels[]` array has 15 entries — the other figure that
  circulates — because three of them are `row` containers that declare no
  datasource; the two counts measure different layers and neither is wrong.
  **The fix was verified against a real Grafana 12.1.0 container, but only on a
  fresh volume — the one case that can succeed — and the production upgrade path
  is not verified.** Reading back what Grafana had actually stored, the fresh
  insert honours the declared `uid`: before the fix the panels held the literal
  `${DS_PROMETHEUS}` against a generated datasource uid of `PBFA97CFB590B2093`,
  and after it every block reports `papyrus-prometheus`. That run does not
  transfer to the deployed instance. Grafana reads its provisioning files once
  per process and stores its state in the named volume `grafana-data`
  (`ops/observability/docker-compose.observability.yml`, `volumes:`), so against
  an existing volume the provisioning is an *update* of a datasource row that
  already exists, and Grafana preserves the **stored** uid rather than adopting
  the incoming one. What that does to the container was then measured on
  Grafana 12.1.0 against the production image rather than inferred. The update
  path looks the row up by **id and uid**, so a stale stored uid matches nothing,
  provisioning raises `ErrDataSourceNotFound`, and the process aborts at
  startup:

      provisioning.ProvisioningServiceImpl run error:
      Datasource provisioning error: data source not found

  **The container exits. Grafana does not come up serving unbound panels — it is
  down**, so the symptom to look for is a container that will not start, and an
  operator has to read `logs grafana` rather than only the dashboard. The check
  therefore exercised the branch that could not have failed and skipped the one
  that will; re-running it on another fresh volume will not close this. **Until
  the production path is exercised — by a recreate of the Grafana container
  against the existing volume, or by discarding that volume — this item is fixed
  in the file but not delivered in the deployment, and the O4 "live dashboard
  data" gate must be treated as unmet on that basis.** A targeted
  `up --detach --force-recreate` limited to the Grafana service has since been
  added to the `deploy` job, which is the right shape for it, but it has not been
  exercised by a real release yet and the claim above does not rest on it. That
  step is only safe on a volume whose stored uid is already `papyrus-prometheus`;
  the production volume was migrated out of band on 2026-09-30, and shipping the
  step over an unmigrated one takes Grafana down rather than leaving it unbound.
  **The regression guard now exists** —
  `PipelineWiringTests.Dashboard_panels_bind_to_the_provisioned_prometheus_datasource`
  asserts that the two `uid` values agree, so a later edit to either file fails
  the build instead of silently reintroducing this.
  **Whether the panels return data is still unverified** and remains an O4
  checklist item; binding is correct in the file, but binding is not
  connectivity. The full description is in
  [`06-operations/observability-and-monitoring.md`](../06-operations/observability-and-monitoring.md).
- **Production observability stack is now tracked and deployed by the pipeline.**
  The collector, Prometheus, Grafana, and blackbox-exporter configuration lives
  in [`ops/observability/`](../../ops/observability) and is started by the
  `deploy` job in [`release.yml`](../../.github/workflows/release.yml) with
  `docker compose -f docker-compose.yml -f ops/observability/docker-compose.observability.yml up -d`
  followed by the four observability service names. Naming them is load-bearing:
  `up` without a service list reconciles every service in the *merged* config,
  which recreated the entire application stack under the observability project
  and collided with the deployed database. It is no longer hand-placed on the
  host. `promtool check config` validated the scrape configuration and all rules.
- **Production verification checklist:** the required production observability
  platform (OTLP collector, Prometheus alert evaluator, Grafana, retention
  policy) and the exact verification steps to close this gate are documented in
  the W7/O4 section of
  [`observability-and-monitoring.md`](../06-operations/observability-and-monitoring.md).
  Live redaction-output inspection, cross-host trace correlation, alert firing
  and recovery, and the audit-volume-drop runbook exercise still require
  executing that checklist against the live production deployment; they are not
  closed by the configuration work above.
- **2026-09-29 — the availability probes were silently blind for two of three
  applications, and the cause is worth recording.** `PapyrusApplicationDown`
  evaluates `min by (job) (up{job=~"admin-api|blog|portfolio"})`, and the
  `up` series came from blackbox probes of each host's `/health`. Blog and
  portfolio both returned 400 for the whole of that period, so `up` was `0` for
  them at all times and the alert could never be distinguished from a real
  outage. The 400 was `Bad Request - Invalid Hostname`: the public hosts derive
  their allowed Host list from the configured public base URL plus loopback
  (`SiteHostValidator.ResolveAllowedHosts`), and blackbox sends the target's own
  hostname by default, so `http://blog:8080/health` was refused. Admin was the
  only host passing, which is what made the fault look like a partial outage
  rather than a configuration error.
  The application was **not** wrong and must not be relaxed — the restriction is
  a Host-header-injection defence. Each public host is now probed by a module
  that sends the host it actually answers to. Those headers are literals
  because blackbox-exporter v0.27.0 does not expand `${VAR}` in its config
  (there is no `--config.expand-env` flag), which is a mistake this change made
  and then measured and reverted; `PipelineWiringTests` now asserts the literals
  match the configured public domains, and asserts the module is actually
  selected in the scrape config, so a drift or a silent regression fails the
  build rather than blinding the alert.
- **The first version of that fix was itself broken, and the lesson is about how
  it was verified rather than about YAML.** It sourced the header from
  environment variables that the deploy job never set, so the stack would not
  have started, and the substitution form does not work anyway. It was reported
  as verified because the probe returned 200 when aimed at the real host — which
  accepts loopback, and so masked an empty `Host` header. Re-testing against a
  target that returns 400 unless the `Host` is exactly right separated the two
  cases immediately. **A probe that returns 200 is only evidence if the target
  would have failed otherwise**; a permissive target cannot distinguish a fix
  from a no-op. The new test is mutation-checked for that reason.
- **Alerts are visible but not delivered. No Alertmanager is configured, so
  nothing notifies anyone.** The rules evaluate, `PapyrusRequestLatencyHigh`
  has been observed in `pending`, and the health panels on the dashboard read
  `up` — but a firing alert produces no page, no issue, and no message. This is
  a deliberate deferral, not an oversight: at this stage the platform has no
  delivery channel. Until one exists, **the dashboard and
  `/api/v1/alerts` are the only way a human learns of a firing alert**, and a
  reader who does not look will not know.
  This gate therefore stays open on two independent counts: the probes are now
  trustworthy, but delivery does not exist, and no alert has yet been observed
  firing and recovering against a real outage. A green `up` series is evidence
  that a probe works, not evidence that detection works.

### W8 — Release documentation and CI

**Status: Evidence attached (2026-09-03 UTC gitleaks secret scan).**

- [`deployment-guide.md`](../06-operations/deployment-guide.md) and the
  CI/release workflows ([`ci.yml`](../../.github/workflows/ci.yml),
  [`release.yml`](../../.github/workflows/release.yml)) document the release
  path, image scanning, SBOM generation, and the dedicated `migrate` job.
- **2026-09-02 sanitized artifact review — PASS:** the dashboard JSON parsed;
  the committed showcase asset set contains only `admin.png`, `aspire.png`,
  `blog.png`, `health.png`, and `portfolio.png`; and the tracked-file name
  check found no committed `.env`, private-key, or certificate files. Restore
  and release-candidate build checks also passed. No secrets, private URLs, or
  personal data are recorded here.
- **2026-09-03 gitleaks secret scan — PASS:** the repository was scanned with
  `gitleaks detect` (SARIF report format, `--redact`) via the official
  `zricethezav/gitleaks` container. The scan covered 8 commits and
  ~23.08 MB, completed with **no leaks found**, and produced a valid SARIF
  report (v2.1.0, 1 run, 0 results). The redacted SARIF artifact is maintained
  locally and is not included in this repository. This satisfies the CI
  secret-scan evidence requirement for release approval.

### O4 — Telemetry/audit signal evidence

**Status: Collector, dashboard provisioning, and retention tracked and deployed;
live data, alert delivery, and retention configuration unverified.**

- [`observability-and-monitoring.md`](../06-operations/observability-and-monitoring.md)
  documents the telemetry/audit signals and how to investigate an
  audit-volume drop.
- S30.6/S30.7 telemetry outputs: structured audit events, fail-closed redaction,
  bounded diagnostic cardinality, configurable sampling, and tests proving audit
  logging survives 0% diagnostic sampling.
- **2026-09-02 local outcome — PASS for artifact/config review:** the
  Prometheus-backed dashboard JSON parsed successfully and includes request,
  latency, 5xx, 429, audit, redaction, sampling, cardinality, and app/PostgreSQL
  health views; alert expressions and the telemetry runbook use the same
  collector-derived metric contract.
- **Production telemetry configuration is tracked and deployed.** The collector
  derives every documented collector-boundary metric from spans, metrics, and
  logs in [`ops/observability/collector/config.yaml`](../../ops/observability/collector/config.yaml);
  the release workflow publishes `OTEL_EXPORTER_OTLP_ENDPOINT` to all three
  hosts so the exporter is actually enabled in production. Grafana provisions
  the tracked dashboard from `docs/observability/dashboards/`, and Prometheus
  retention is set from `OBSERVABILITY_RETENTION` (30d default).
- **Production verification checklist:** the required production observability
  platform (OTLP collector, dashboard service, alert evaluator, retention
  policy) and the exact verification steps to close this gate are documented in
  the W7/O4 section of
  [`observability-and-monitoring.md`](../06-operations/observability-and-monitoring.md).
  Live dashboard data, alert delivery/recovery, and retention configuration
  remain unverified and must be attached by executing that checklist against the
  production deployment.

### O7 — Restore/load drills

**Status: Closed — restore, migration-ownership, media-volume, and load evidence attached (2026-08-29 UTC).**

- Restore/migration/readiness/media-volume drill procedures are documented in
  [`runbooks.md`](../06-operations/runbooks.md) and
  [`deployment-guide.md`](../06-operations/deployment-guide.md).
- S30.5 was executed against the Docker Compose PostgreSQL 17 deployment on
  2026-08-29 UTC. The public run recorded HTTP p95 **23.574 ms**, payload p95
  **9,030 bytes**, 0.00% failed requests, and 100.00% checks. The admin run
  recorded HTTP p95 **21.660 ms**, payload p95 **9,256 bytes**, 0.00% failed
  requests, 100.00% checks, and zero 5xx responses.
- Raw K6 summaries (archived): [`s30.5-k6-public-20260829.json`](../history/s30.5-k6-public-20260829.json)
  and [`s30.5-k6-admin-20260829.json`](../history/s30.5-k6-admin-20260829.json).
- Restore drill: `pg_dump -Fc` created a 26,130-byte backup from the Compose
  PostgreSQL 17 `papyrus` database; `pg_restore --exit-on-error` restored it into
  a disposable `papyrus_restore` database. Verification found four applied EF
  migration-history rows and seven public tables in the restored database.
- Migration-ownership drill: the solution build passed with 0 warnings/errors;
  the two `MigrationOwnershipTests` passed; source inspection found the sole
  production `MigrateAsync()` call in Infrastructure and the sole Admin startup
  initialisation call in `Papyrus.Api.Admin`. No applied migration was edited or
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

**Status: Evidence attached (2026-09-03 UTC production-like re-measurement).**

- [`performance-budget.md`](performance-budget.md) records the measured
  baseline and explicit latency/error/payload/upload/allocation/GC budgets:
  public feed DB p95 below 0.1 ms, four public queries, two admin collection
  queries, HTTP p95 ≤ 2000 ms (target ≤ 500 ms), public payload ≤ 256 KiB,
  admin payload ≤ 128 KiB, upload ≤ 5 MiB, allocations ≤ 2 MiB/request public
  and ≤ 1 MiB/request admin, and Gen2 collections = 0 during smoke.
- PostgreSQL query-count budget tests (`PerformanceBudgetTests`) enforce the
  public `≤ 4` and admin `≤ 2` command budgets.
- **2026-09-03 production-like re-measurement — PASS:** the documented K6
  workload was re-run against a production-like dataset (8 categories, 12 tags,
  200 projects, 2,000 blog posts, and their tag links) seeded by
  [`seed-production-like.sql`](../../tests/load/seed-production-like.sql), with
  `Site__PublicBaseUrl` set to the production-like origins. The public run
  recorded HTTP p95 **32.79 ms**, payload p95 **16,887 bytes**, 0.00% failed
  requests, and 100.00% checks. The admin run recorded HTTP p95 **19.35 ms**,
  payload p95 **19,827 bytes**, 0.00% failed requests, 100.00% checks, and zero
  5xx responses. All values are within the explicit budgets (p95 ≤ 1000 ms,
  errors < 1%, public payload ≤ 256 KiB, admin payload ≤ 128 KiB).
- **Allocation/GC evidence:** `dotnet-counters` captured the `System.Runtime`
  counters during the admin workload. Total allocated was ~8.95 MB across the
  ~70 s run (~66 KiB/request, well under the ≤ 1 MiB/request admin budget);
  Gen2 collections = 0 during the smoke (matching the budget); total GC pause
  time was 0.117 s; max heap size was ~8.3 MiB.
- Raw artifacts (archived): [`s30.9-k6-public-20260903.json`](../history/s30.9-k6-public-20260903.json),
  [`s30.9-k6-admin-20260903.json`](../history/s30.9-k6-admin-20260903.json),
  and [`s30.9-admin-counters-20260903.json`](../history/s30.9-admin-counters-20260903.json).
  The reproducible workflow is [`perf-baseline.yml`](../../.github/workflows/perf-baseline.yml).

### O9 — Release artifacts and sanitized publication evidence

**Status: Evidence attached (2026-09-03 UTC gitleaks secret scan).**

O9 is a defined release gate (see the actual-progress section of
[`Papyrus_Roadmap.md`](../Papyrus_Roadmap.md)): release artifacts are documented
and sanitized publication evidence is a release gate. Its evidence overlaps with
W8 (release documentation and CI) and is recorded there:

- [`deployment-guide.md`](../06-operations/deployment-guide.md) and the
  CI/release workflows ([`ci.yml`](../../.github/workflows/ci.yml),
  [`release.yml`](../../.github/workflows/release.yml)) document the release
  path, image scanning, SBOM generation, and the dedicated `migrate` job.
- **2026-09-02 sanitized artifact review — PASS:** the committed showcase asset
  set contains only `admin.png`, `aspire.png`, `blog.png`, `health.png`, and
  `portfolio.png`; the tracked-file name check found no committed `.env`,
  private-key, or certificate files. No secrets, private URLs, or personal data
  are recorded.
- **2026-09-03 gitleaks secret scan — PASS:** the repository was scanned with
  `gitleaks detect` (SARIF report format, `--redact`) via the official
  `zricethezav/gitleaks` container. The scan covered 8 commits and
  ~23.08 MB, completed with **no leaks found**, and produced a valid SARIF
  report (v2.1.0, 1 run, 0 results). The redacted SARIF artifact is maintained
  locally and is not included in this repository. This satisfies the sanitized
  publication secret-scan evidence requirement.

### O10 — Architecture evidence

**Status: Evidence attached (S30.0 architecture tests).**

- S30.0 architecture tests (NetArchTest) verify modular-monolith boundaries,
  layer rules, and migration ownership.
- The modular-monolith decision is recorded in
  [`adr-0001-adopt-a-modular-monolith-over-microservices.md`](../02-architecture/decisions/adr-0001-adopt-a-modular-monolith-over-microservices.md)
  and the boundaries are described in
  [`architecture-overview.md`](../02-architecture/architecture-overview.md).
- **Remaining:** record a new ADR only if measured bottlenecks require
  distributed components.
