# Changelog

**Current:** this file records the release and phase history of the implemented system (the legacy `docs/project/` changelog was retired during the documentation consolidation).

All notable changes to Papyrus will be documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); entries describe repository changes and do not imply that deferred operational gates have passed.

Release sections are named for the tags cut from `main`. The first four
releases were cut within a single day while the deployment path was being
brought up, and they are thin by design: most of the substantive work they
carry is described in full under the release that actually reached production,
with the earlier tag recording only its own delta. `0.1.2` is that release.

## [Unreleased]

- **The Grafana datasource binding now has the regression guard it was recorded
  as still owing.** `PipelineWiringTests` gained
  `Dashboard_panels_bind_to_the_provisioned_prometheus_datasource`, which reads
  both `ops/observability/grafana/provisioning/datasources/prometheus.yml` and
  `docs/observability/dashboards/papyrus-dashboard.json` and asserts that the
  uid the provisioning declares is the uid every Prometheus datasource block in
  the dashboard names. It is mutation-checked against four distinct failures —
  the `uid` removed from the provisioning, a dashboard block reverted to
  `${DS_PROMETHEUS}`, the two uids diverging from each other, and every
  Prometheus datasource removed from the dashboard — so none of them can pass by
  accident. Reading both files is still the constraint: a test that read only
  the dashboard would pass while the provisioning half regressed. The documented
  claim that the live dashboard still returns data is unchanged by this; binding
  is not connectivity, and that remains an O4 checklist item.
- **The measured test baseline moved from 456 to 457 non-integration cases** and
  the total from 478 to 479, with the encoding (280), behavioural (176),
  behavioural-including-integration (198), Playwright (10), and 22 integration
  figures unchanged. The new case is an architectural wiring test, so only those
  two figures move. The canonical `<!-- test-baseline: ... -->` marker in
  [`00-overview/feature-matrix.md`](feature-matrix.md) and every restatement in
  `docs/README.md`, `00-overview/limitations.md`, and `Papyrus_Roadmap.md` now
  agree with the run.

### Documentation reconciliation after the first four releases - 2026-09-29

- **Three documents restated a test baseline that had already moved.** The
  canonical `<!-- test-baseline: ... -->` marker in
  [`00-overview/feature-matrix.md`](feature-matrix.md) read 456 non-integration
  / 176 behavioural / 478 unique, which is what CI measured. `docs/README.md`,
  `00-overview/limitations.md`, and `Papyrus_Roadmap.md` still published
  454 / 174 / 476, and the roadmap also still carried 196 for the
  behavioural-including-integration figure where the measured value is 198.
  All four files now agree.
- **The release status was flatly contradicted by the repository.**
  `docs/README.md` stated "**No release has been tagged.**" and both it and the
  roadmap recorded Phase 7 item 4, the release tag, as "intentionally open".
  Four tags exist: `v0.1.0`, `v0.1.1`, `v0.1.2`, and `v0.1.3`, cut 2026-09-29 to
  2026-09-30, each deployed to production by the release workflow. Phases 0–7
  are now recorded as complete, with the two still-open operational gates
  tracked in the release-gates document rather than in the phase status.
- **Tagging did not close the gates, and the documents now say so.** W7 and O4
  were open when `v0.1.3` shipped. The gates record production evidence that is
  still owed, not a claim that a release was unsafe to cut, and the largest
  remaining gap is unchanged: no Alertmanager is configured, so a firing alert
  notifies nobody.
- **The changelog had no correspondence with the tags it described.** It carried
  an `[Unreleased]` heading above entries that were in fact shipped, and then
  jumped to `0.5.0`. The `0.1.0`–`0.1.3` sections are now present, ordered
  newest first. `0.1.0` and `0.1.1` record that their content was carried to
  production by `0.1.2`, rather than implying they delivered nothing.
- **The W7 and O4 summaries were narrower than the evidence they sat beside.**
  Both still described production verification as wholly pending while the same
  documents recorded that the platform is tracked in `ops/observability/` and
  deployed by the pipeline. They now separate what is deployed from what is
  unverified, and name the Alertmanager gap and the missing alert
  fire-and-recover drill.
- **`docs/README.md` claimed a currency date two days stale.** The
  "reconciled on 2026-09-27" line is corrected, and the consolidation date is
  distinguished from the most recent reconciliation.
- **The CI guard that would have caught the drift could not detect it.** The
  `verify` job pins the expected figures as string literals inside
  `.github/workflows/ci.yml` and only asserts that each literal appears
  somewhere in each document. When the suite moved 454 → 456 the pinned `454`
  was still present in the prose, so the check passed while three documents
  disagreed with the run and with the marker — and the roadmap's 196 for the
  behavioural-including-integration figure was never checked at all. The
  expected values are now taken from the marker instead, and each document
  declares only *which* figures it must quote, so the numbers live in one
  place. A figure must also appear as a standalone number: `17` is not found
  inside `176`. The guard's own limitation is recorded in the workflow and in
  the feature matrix — it asks whether a correct figure is present, not whether
  every mention of it is correct.
- **Verification.** The rewritten guard was extracted from the workflow and run
  **unmodified** against real TRX output from a local run of both test steps —
  456 non-integration and 22 integration at the time, the two files the step
  requires. It reported
  `non-integration=456, integration=22, total=478, encoding=280,
  behavioural=176, behaviour-total=198, playwright=10` and exited 0. Reverting
  the roadmap's 198 to 196 with the same TRX files in place made it exit 1 and
  name the stale figure, so the failure path was proven as well as the success
  path. This closes the gap that the guard's own rewrite had introduced: it had
  previously only been exercised against a stubbed count, which would have
  hidden a mistake in the TRX parsing. **Those figures are the ones measured at
  that moment, not the current baseline** — the counts below were superseded
  when the dashboard test was added later in the same session.
- **Verified on Linux, not only on Windows.** The verification above ran the
  extracted guard on this machine, which cannot rule out a difference between
  what a Linux runner reads and what a Windows working tree holds. The
  repository's `.gitattributes` pins `*.md` and `*.yml` to `eol=lf`, but
  `core.autocrlf=true` is set here, so the claim was checked rather than
  assumed: staged into a `python:3.12-slim` Linux container, the guard —
  extracted from the workflow at build time and run unmodified against the same
  real TRX files — reported the same figures and exited 0, and reverting the
  roadmap's 198 to 196 failed the same step. The container also confirmed zero
  `CR` bytes in the workflow and the three documents, so the runner reads the
  same line endings that were written. One incidental finding: the repository's
  `.dockerignore` excludes `.github/` and `docs/`, so a container build over
  the repository root cannot see the very files this check reads. That is
  correct for building application images and was worked around only in the
  throwaway simulation, not in the repository.
- **`.aspire/` is missing from the encoding guard's exclusion list, and that
  cost 280 test cases.** `SourceEncodingTests` enumerates the working tree and
  excludes a fixed list of directories; `.aspire/` is gitignored but not on that
  list. Staging the Linux simulation under `.aspire/ci-linux-sim/` therefore
  raised the local run from 456 to 736 cases — exactly 280 more, matching the
  staged tree's `.cs/.csproj/.json/.props/.targets/.md` count. The guard's own
  guidance anticipates this class of movement for *tracked* files, and the
  working-tree enumeration is deliberate so the guard fails on the machine that
  made the mistake, which `git ls-files` could not do. A gitignored scratch
  directory is neither tracked nor listed, so it inflates the count invisibly.
  This was not fixed here: the staging tree has been removed, the count is back
  to 456, and adding `.aspire/` would be correct but is a change to the test
  suite, not to documentation. It is recorded so the next person who stages
  anything locally knows why the number moved.
- **The guard's limitation paragraph was itself a hazard, in the one document
  the guard reads.** `feature-matrix.md` explained the standalone-number rule by
  citing `17` inside `176` and by illustrating an inconsistent document as "456
  in one place and 454 in another". Because the marker lives in that file, a
  future run that happened to measure 454 would have satisfied the check with an
  illustration rather than a real restatement. The paragraph no longer states
  any live figure, and says why it must not. The same words in `ci.yml` and in
  this changelog are harmless, because neither file is read by the guard.
- **Adding `playwright` to the guarded set exposed a wording the guard cannot
  check, and the first account of it was wrong.** The new table required each
  guarded document to quote the Playwright figure as a number, and `README.md`
  had always written it as the word "Ten". The prose now says `10`. The earlier
  version of this entry claimed the guard "immediately broke the build" and
  presented the correction as proof the check works. **That claim did not hold
  up when it was tested.** The guard asks whether a figure appears *anywhere* in
  a document, not whether it appears in the sentence that states it, so the
  words-only version passed: `10` occurred exactly once in `README.md`, in the
  Playwright sentence itself, and the guard found that occurrence while the
  sentence still said "Ten". A later review reproduced the false negative
  directly — a document saying "Ten Playwright" alongside an unrelated `10`
  passes — and the changelog, the feature matrix, and the workflow comment have
  all been corrected to describe the real limitation. The `playwright` figure is
  the most exposed of the seven for exactly this reason: it is small, so it is
  the one most likely to appear incidentally for an unrelated reason. The other
  six are three-digit figures that are unlikely to collide.
- **Two changelog defects were introduced and then found in this same pass, and
  are recorded because the file is evidence.** Restructuring the release
  sections placed 23 historical entries under `[0.1.2]`, and the `[0.1.0]` entry
  described only one of the three changes that tag carried. Both were caught by
  checking the file's structure against the tags rather than by reading it, and
  both are corrected: the history now has its own section, `[0.1.0]` lists its
  three commits with their PR numbers, and the history entries are in strictly
  descending date order.

### Full `docs/` review - 2026-09-29

A review of all 82 markdown documents found five further defects, one of them
an operational fault rather than a documentation fault. A sixth reported finding
turned out to be an error in the review itself and is recorded as such.

- **The provisioned Grafana dashboard had no datasource to bind to, and nothing
  failed. Now fixed in the file; the deployed instance is not yet corrected.**
  The dashboard referenced `${DS_PROMETHEUS}` in every panel datasource block it
  declared. That is a Grafana *import-time* variable, substituted when
  a dashboard is imported through the UI. This dashboard is mounted by **file
  provisioning**
  (`ops/observability/grafana/provisioning/dashboards/papyrus.yml`, `type:
  file`), which does not perform import-time substitution, and the provisioned
  datasource set no `uid` for the reference to resolve to. Every panel was
  therefore bound to a datasource that did not exist.
  The failure mode is what makes this worth recording: the dashboard still
  rendered, so no check failed, no container restarted, and no alert fired. A
  green Prometheus, a healthy collector, and a dashboard of empty panels are
  each individually consistent with a correct deployment. This is the same shape
  as the blind-probe defect already recorded under `[0.1.3]` — a monitoring
  surface that looks healthy because the thing being monitored is not being
  checked.
  It survived because the 2026-09-02 verification established only that the
  dashboard *parsed* and used "the documented Prometheus datasource placeholder".
  Both statements were true and neither asked the operational question.
  The provisioned datasource now declares `uid: papyrus-prometheus` and the
  dashboard names that same uid in every datasource block it declares — 26 in
  all, counting the per-panel and per-target blocks and the `app` template
  variable's own block, with the built-in `Annotations & Alerts` entry bound to
  Grafana's internal `-- Grafana --` datasource rather than to Prometheus. (Two
  counts for this file circulate and both are true of different things: the
  `panels[]` array holds **15** entries, three of which are `row` containers
  that declare no datasource, leaving **12** data panels; those 12 panels carry
  25 datasource blocks between them — a panel-level block, one per `targets[]`
  entry, and additional nested blocks inside `fieldConfig` overrides — and the
  `app` template variable adds 1, for 26. Grafana's own read-back reported "15
  panels" because it counts the row containers alongside the panels that draw,
  and it renders some panels as more than one tile. Do not reconcile the two
  figures by picking one — they measure different layers.)
  **The fix was verified on a fresh volume, which is the only case that can
  succeed, and the upgrade path over existing data was not verified at all.**
  Grafana 12.1.0 was run from the tracked provisioning files and asked what it
  had actually stored rather than trusting that the files agreed: on a fresh
  volume the insert honours the declared `uid`, so before the fix the panels
  held the literal `${DS_PROMETHEUS}` against a generated datasource uid of
  `PBFA97CFB590B2093`, and after it every block reads `papyrus-prometheus`. The
  pre-fix run is the part that matters — it demonstrates the placeholder is
  genuinely unresolved and that Grafana's generated uid is arbitrary and
  volume-dependent, which is the whole failure mechanism. What that run does
  *not* establish is the production path. Grafana reads its datasource
  provisioning files once per process, and its data lives in the named volume
  `grafana-data`
  (`ops/observability/docker-compose.observability.yml`, `volumes:`), so on the
  existing deployment the provisioning is an *update* of a datasource row that
  already exists, and on that path Grafana preserves the **stored** uid rather
  than adopting the incoming one. The deployed instance therefore keeps whatever
  uid it generated originally, the dashboard's panels stay bound to that
  unresolved value, and they render empty. The verification exercised the one
  branch that could not have failed and skipped the one that will. A fresh
  volume is not a stand-in for the production volume, and re-running that check
  will not close this. A targeted `up --detach --force-recreate` limited to the
  Grafana service has since been added to the `deploy` job in
  [`.github/workflows/release.yml`](../../.github/workflows/release.yml), which
  is the right shape for it, but that step has not yet been run against the
  production volume and nothing recorded here rests on it.
  **A regression guard now exists.**
  [`PipelineWiringTests.Dashboard_panels_bind_to_the_provisioned_prometheus_datasource`](../../src/Papyrus.Tests/Architecture/PipelineWiringTests.cs)
  reads both files and asserts the two `uid` values agree, that no binding
  reverted to `${DS_PROMETHEUS}`, and that a Prometheus datasource is declared at
  all; it was mutation-checked against each of those failure modes. **Whether
  the panels return data is still unverified** and remains an O4 checklist
  item: binding is now correct in the file, but binding is not connectivity, so
  that check now tests Prometheus reachability rather than panel binding. The
  full description is in
  [`06-operations/observability-and-monitoring.md`](../06-operations/observability-and-monitoring.md).
- **An archived measurement claimed to be the current baseline.**
  `history/projection-evidence.md` recorded its 2026-08-25 result of 70
  non-integration and 14 integration tests, and then stated in a note that
  "the documented 70/70 result is the current local non-integration baseline".
  The figures current at the time of this review were 456 and 22. This
  directly contradicted
  `documentation-maintenance.md`, which requires that a figure not re-verified is
  not restated as current. The note now says the counts are what the suite
  measured on that date, points at the canonical marker, and records why this
  file is deliberately excluded from the machine-checked set: an archived
  measurement has to be allowed to keep the numbers it actually recorded.
- **The deployment guide's credential guidance had been overtaken.** Its "What
  has not been verified" section said the deploy job's pull path "is only
  exercised by a real tagged release" and instructed the reader to treat the
  first successful real release as the verification. Four have since been
  tagged and deployed, and `v0.1.2` and `v0.1.3` each pulled with the per-job
  token, so the path is verified. The section now records that, while keeping
  the underlying asymmetry — a dry run never contacts a registry — because it
  still governs anyone who next changes the credential path.
- **A `file:line` reference pointed past the end of its file.**
  `history/projection-evidence.md` cited `AdminApiEndpoints.cs:35` in a file of
  27 lines; the handlers it describes now live under
  `src/Papyrus.Api.Admin/Endpoints/`. The reference is split between the entry
  point that maps them and the support file that implements them. The other 31
  `file:line` references in `docs/` all resolve.
- **Terminology was mixed across the set.** British `behaviour` was the
  established norm, with 22 American `behavior`/`organization`/`normalizes`
  spellings scattered across 14 documents. All are now normalized. Technical
  identifiers were left alone: `theme-color` in the PWA milestone is a real web
  manifest key, not prose, and rewriting it would have made the document
  wrong.
- **A reported finding was wrong, and the review's own method was the cause.**
  Four anchors in `c4-diagrams.md` were reported as broken. They are not. The
  links use `#level-2--containers` and the headings are `## Level 2 —
  Containers`; the em dash is stripped by GitHub's slugger, leaving the two
  surrounding spaces to become two hyphens. The check that flagged them
  collapsed runs of whitespace first, which GitHub does not do. Re-run with the
  real slug algorithm, all 82 documents contain zero broken anchors. No change
  was made. **A link check that reports a false positive costs the same trust as
  one that misses a real break**, so the check itself was corrected rather than
  the links.
- Five documents were also missing a trailing newline and now have one.

## [0.1.3] - 2026-09-30

### The observability stack was deploying a second copy of the application, and the availability alerts were blind for two of three hosts - 2026-09-29

- **The observability step tried to start the entire application stack again.** The step layered the observability override on top of the platform compose file and ran `up` with no service list, so Compose reconciled every service in the *merged* result — including `db`, `admin-api`, `blog`, and `portfolio`. Because the override renames the project, that produced a duplicate of the whole stack under `papyrus-observability`, which collided with the database the deploy step had just started: `Bind for 127.0.0.1:5432 failed: port is already allocated`. The step failed *after* the application had already deployed, which is why `v0.1.2` reports failure while production is in fact running the new images. The services are now named explicitly. Passing `-p papyrus` would not have fixed it: the duplicates would still have been created, just under the right name, and would still have collided on the published ports.
- **The availability probes were blind for blog and portfolio, and looked healthy doing it.** `PapyrusApplicationDown` evaluates `up` from blackbox probes of each host's `/health`. Blog and portfolio returned 400 for the entire period, so `up` was a constant `0` and a genuine outage would have been indistinguishable from the steady state. The cause was `Bad Request - Invalid Hostname`: the public hosts derive their allowed Host list from the configured public base URL plus loopback, and blackbox sends the target's own hostname by default. Admin passed only because it does not filter its Host header, which is what made this read as a partial outage rather than a configuration fault. The application was correct and was not relaxed — the restriction is a Host-header-injection defence. Each public host is now probed by a module that sends the host it actually answers to.
- **The first attempt at that fix was wrong, and the test that now guards it is the interesting part.** The first version sourced the `Host` header from `BLOG_PROBE_HOST`/`PORTFOLIO_PROBE_HOST` environment variables, on the belief that blackbox-exporter expands `${VAR}` in its config. It does not: v0.27.0 has no `--config.expand-env` flag, so the substitution parsed as an empty scalar and the module sent no `Host` header at all. The variable-based version was also never wired into the deploy job's environment, so the stack would not have started. The mistake was reported as verified because the probe returned 200 — but only when aimed at the real host, which accepts loopback and masked the empty header. Re-tested against a target that returns 400 unless the `Host` is exactly right, the `${VAR}` module returns 400 and the literal returns 200. The headers are now literals, `PipelineWiringTests` asserts they match the configured public domains, and the test was mutation-checked: reintroducing the `${VAR}` form, renaming the domain, or dropping the module from the scrape config each makes it fail.
- **Alerts still do not notify anyone, and that is a deliberate deferral rather than a completed capability.** No Alertmanager is configured, so a firing alert produces no page and no issue; the dashboard and `/api/v1/alerts` are the only way a human learns of one. The rules evaluate and `up` is now trustworthy, but a green probe is evidence that a probe works, not that detection works. No alert has been observed firing and recovering against a real outage, and that drill is what the W7 gate still waits on.

## [0.1.2] - 2026-09-29

### The site survived a host reboot for the first time, and the registry credential finally expires - 2026-09-29

- **Production was offline for more than four hours and nothing detected it.** The application containers exited at `12:58:09Z` during a host reboot for a kernel upgrade and never returned. No service in `docker-compose.yml` declared a restart policy, so every one inherited Docker's default of `restart: no` and the Docker daemon had no instruction to restore them. The database came back only because the release dry run's migrate job happened to recreate it, which is luck rather than design. All four production services now declare `restart: unless-stopped`; the `test-db` profile is deliberately left without one. The signature to recognise next time is `Exited (255)` with an empty `State.Error` and `OOMKilled=false` and nothing but normal traffic in the logs: that is a host-initiated stop, not an application fault. The deployment guide now records that a host reboot is a normal event rather than an incident, provided the restart policy holds.
- **The release pipeline reached a green end-to-end run for the first time.** Five consecutive `migrate` job failures, each masked by the last, are resolved: the EF Core local-tool manifest, the missing restore, the uninstallable system-wide SDK, the unreachable `db` hostname, the unapplied loopback port binding, the unset compose variables, and finally the fact that Compose interpolates every service in a file before acting on the one named on the command line, which required reconciling the database from a database-only compose file. A dry run of all six jobs now passes.
- **The three public origins were never configured, so the pipeline could not have deployed.** `BLOG_PUBLIC_BASE_URL`, `PORTFOLIO_PUBLIC_BASE_URL`, and `MEDIA_BASE_URL` existed in no repository variable. They are set now, and the `deploy` job fails naming any that are missing rather than surfacing Compose's per-service interpolation error, which reads as a compose fault.
- **There is now no stored registry credential in use.** The deploy job pulled with a long-lived `GHCR_READ_TOKEN`/`GHCR_READ_USER` personal access token, which never expires and is not attributable to a single job. Replacing it with a GitHub App installation token was attempted and abandoned: it failed in two consecutive releases, both times inside the step that mints the token, before the registry was ever contacted. The custom App's token was therefore never successfully obtained, and nothing was ever proven about whether GHCR accepts it. The job's own per-job `GITHUB_TOKEN` with `packages: read` is what the deploy job uses now, matching the `images` job that pushes the images. Nothing is stored, nothing expires, and there is no private key to rotate. The PAT pair, the App's private key, and the App's `APP_ID`/`APP_INSTALLATION_ID`/`APP_SLUG` configuration are all deleted, and the `papyrus-contabo` App itself is removed.
- **Two claims made during that work were wrong, and the record should say so.** First, the claim that a self-hosted runner's per-job token "cannot pull from GHCR" was never true and is contradicted by the `images` job, which pushes with exactly that token. The whole PAT-to-App detour was solving a problem that did not exist. Second, the App path was attributed to a registry refusal that had never been observed; both failures were readable OpenSSL errors about a corrupt PEM. A separate proposed fix was caught in adversarial review *before* it was committed, after it would have printed an entire private key to the CI log: the diagnostic added to catch a collapsed key would have leaked that same key, because a collapsed PEM has no first line. And a stored key was shredded on the host before the copy in the secret store was verified, leaving the release blocked. These were process failures. Verify the actual claim before building for it, and review changes that touch credentials before committing them.
- **A four-and-a-half-hour outage still has no alerting.** Restarting the stack by hand is a recovery, not a detection. Nothing pages when the public origins stop answering, and the Prometheus alerts from the previous entry remain unverified against live data.

## [0.1.1] - 2026-09-29

- **The App JWT was signed from the key text, not a key file.** The abandoned
  GitHub App path left a signing step that read the private key as a string
  value, which is what produced the two corrupt-PEM failures recorded under
  `0.1.2` below. The step was corrected to read the key from a file before the
  App approach was abandoned, so the correction is recorded here rather than
  deleted with the App.

## [0.1.0] - 2026-09-29

The first tagged release. It carries three changes, each of which is described
in full under `0.1.2` below because that is the release that finally took them
to production:

- **Every service gained a restart policy** (`fix(deploy)`, #86) — the
  `restart: unless-stopped` change recorded in the outage entry below.
- **A short-lived App token was introduced for the registry**
  (`fix(release)`, #85) — the first attempt at replacing the personal access
  token, which failed and was abandoned two releases later.
- **The outage, the green pipeline, and the credential change were written
  down** (`docs`, #87).

## Development history before 0.1.0

The entries below predate the first tagged release and are recorded by date
rather than by release, because no tag separated them at the time. The one
exception is `0.5.0`, which carries a version but sits inside this history
rather than among the `0.1.x` tags above: nothing was ever tagged `0.5.0`, and it
is recorded here as the work that was already in the tree when versioning began.
It is placed last, after the 2026-09-03 to 2026-08-25 entries, because its own
`2026-08-25` date is the same day as the "Security and verification" entry
immediately above it, which records the verification work carried out alongside
it.

### The observability stack is now tracked and deployed, and three defects that stopped it producing data are fixed - 2026-09-28

- **The production observability stack was not in the repository.** The OTLP collector, Prometheus, Grafana, and blackbox-exporter configuration existed only as files hand-placed on the VPS, and no release step started it. W7 and O4 claimed a platform that nothing in the repository described. The stack now lives in `ops/observability/`, which the `.gitignore` had been excluding wholesale, and the `deploy` job starts it with the same tracked, reviewed compose path as the applications.
- **The collector would have received nothing.** `docker-compose.yml` never published `OTEL_EXPORTER_OTLP_ENDPOINT`, which is the variable that gates the OTLP exporter. The stack could have been started, healthy, and completely empty. The endpoint is now published to all three hosts; leaving it empty keeps the exporter off and structured audit logging intact.
- **The collector's transform processor was wrong twice.** It renamed a *span* rather than a metric, and used a metric context that does not accept `metric.attributes`. The documented contract groups request metrics by `papyrus_service`, so the resource service name is now copied onto each data point, which is what the Prometheus exporter actually turns into a label.
- **The availability alerts had nothing to alert on.** `up{job=...}` was scraped from targets that served no Prometheus text. The two availability alerts are now satisfied by blackbox probes of each host's `/health` and of Postgres over TCP.
- **The audit and diagnostic metrics were permanently empty.** A count connector now derives every metric the alert rules and dashboard query. `PapyrusAuditSignalLost` was added, because the audit-volume alert cannot fire when the audit signal itself disappears, a failure mode that was previously invisible.
- **Test baseline moved 449 to 454 non-integration** (169 to 174 behavioural, 471 to 476 unique) from three new `PipelineWiringTests` cases. Each was mutation-checked: reintroducing the defect makes the test fail. The suite itself is clean; only the documented counts were behind.
- **W7 and O4 remain open.** Configuration work does not close a gate. Live redaction-output inspection, cross-host trace correlation, alert firing and recovery, the audit-volume-drop drill, dashboard data, alert delivery, and retention verification all still require executing the production checklist against the live deployment.

### The documented test counts were measured wrong, and most of them were not behaviours — 2026-09-28

- **The published baseline overstated what CI actually ran.** Five documents claimed 436 non-integration / 16 integration / 452 unique .NET tests, but the suite really runs 449 / 22 / 471. The 436 was `433 + 3`, and the 3 came from `SourceEncodingTests`: it enumerates the *working tree* with a hard-coded exclusion list that omitted `.vs/`, so a developer machine contributed two Visual Studio JSON files and `tests/e2e/test-results/` one. In a clean checkout — which is what CI sees — none of those exist. `release-gates.md` was therefore describing a count the `verify` job never produced.
- **63% of the non-integration suite asserted a property of a file, not of the system.** 280 of the 449 cases are the encoding guard, one per source file, all exercising a single code path. They are a real guard against a real incident, but reading "436 non-integration tests" as evidence of behaviour coverage was wrong by roughly 2.7×. The honest behavioural baseline is 169 non-integration and 191 including integration.
- **The guard's file set is now reproducible.** `.vs/`, `.idea/`, `.history/`, `TestResults/`, `playwright-report/`, and `test-results/` are excluded alongside the existing entries, so the case count no longer depends on what a particular machine has written to disk. The enumeration is deliberately still the working tree rather than `git ls-files`: the point of the guard is to fail on the machine that made the mistake, which `git ls-files` cannot do. `TrackedSourceFiles` was renamed to `CandidateSourceFiles` because it was never reading tracked files.
- **The count now lives in one place.** The `feature-matrix.md` "Automated test baseline" row is canonical, and the other four documents link to it rather than restating it. Six independent restatements is why a three-case drift survived review. `release-gates.md` no longer carries a count at all: a gate is that the job runs and nothing fails, not an integer.
- **Three further claims were corrected in the same pass.** The Playwright browser suite has 10 tests, not 5, in `README.md` and `release-gates.md`. `accessibility-standards.md` and `testing-strategy.md` described automated axe-core checks that do not exist — `tests/e2e/package.json` has no axe dependency and no spec imports it — so both now state that accessibility is a manual pass today, with automated scanning tracked under M7. `adr-0004` no longer cites a live "86-test CI baseline" that was 5× stale; the counts were removed rather than rewritten, because a point-in-time record must not be falsified by rewriting it to a number that was not true when the decision was made.

### M1 routing truth: the root/subdomain decision is now enforced — 2026-09-28

- **The "Portfolio is the root, Blog is the subdomain" decision was documented but not enforced anywhere.** Both public `appsettings.json` files already carried a complete `Site` block, and `docker-compose.yml` mapped `BLOG_PUBLIC_BASE_URL` / `PORTFOLIO_PUBLIC_BASE_URL` per host, so the *configuration* existed. But `AllowedHosts` was `"*"` in both public hosts, so the host-filtering middleware that `ConfigureWebHostDefaults` had already registered accepted every Host header, and `Site:Kind` was read only by two views to choose a label. A request to the Blog container carrying the Portfolio's host was served, and a deployment where both hosts advertised the same canonical host was not detectable.
- **`SiteHostValidator` now supplies the enforcement**, as a pure static type in `Papyrus.Web.Shared` so the rules are testable without booting a host: a public host's `Site:PublicBaseUrl` must name the same host as its matching role base URL, and `Site:Kind` must match the site the process actually serves. Both fail startup with a message naming the offending key, mirroring the Admin API key check and `MediaRootResolver` rather than degrading into a silently wrong site.
- **The allowlist is derived, not hand-maintained.** It is the bare host of `Site:PublicBaseUrl` — with any port stripped, because `HostFilteringOptions.AllowedHosts` does not consider ports and a value carrying one never matches — plus the loopback literals `localhost`, `127.0.0.1`, and `[::1]`. Loopback is unconditionally permitted and that is load-bearing: `ci.yml` (two jobs), `perf-baseline.yml`, and `release.yml` all start the hosts on `ASPNETCORE_URLS=http://127.0.0.1:<port>` and poll `127.0.0.1`, as do the Aspire AppHost health checks and the local `dotnet run` path, all while the configured public base URLs are real domains. Without the exemption every one of those probes would have been refused with a 400. When no public base URL is configured at all the allowlist degrades to `"*"`, preserving the pre-milestone behaviour that `SiteUrl.ResolvePublicBase`'s relative-URL fallback depends on.
- **The loopback exemption is a real trade-off, not a free win.** `deployment-guide.md` records that Papyrus runs under Docker Compose with no reverse proxy or CDN in front of it, so any co-located process or peer on the compose network can present a loopback Host header and be served. The exposure is bounded — the public hosts are read-only, hold no secrets, and canonical URLs are never derived from `Request.Host`, so the host-header poisoning sink is absent — but the allowlist is not a complete defence in the current topology. Scoping the exemption to an explicit flag is tracked with the **M2** milestone, alongside the T-06a rate-limiter keying decision. A *malformed* public base URL also silently disables enforcement, because Compose's `:?` checks only that the variable is non-empty; that is documented in `limitations.md` rather than treated as a safe default.
- **The 400 is proven by real requests, not a source-text assertion.** `PublicHostFilterTests` boots the Blog pipeline against `TestServer` and sends requests carrying `evil.example.com`, `example.com`, `blog.example.com.evil.test`, and `notblog.example.com`, each of which must be refused with 400. A 404 would not be sufficient evidence, since it would also indicate a host that passed the filter and merely missed a route. The canonical host, an uppercase variant, and both loopback literals are required *not* to be rejected, which is what pins the loopback exemption in place. Against a locally started host, `Host: evil.example.com` returned 400 while loopback and the canonical host reached the application, and all three CI probe URLs returned 200.
- **The one rule that cannot be enforced in-process is named rather than glossed.** A process cannot see the other host's configuration, so cross-host distinctness is asserted by a test over both shipped `appsettings.json` files and recorded as a genuine limit of the approach, not as a covered startup case. A wiring assertion over both public hosts remains as a second layer for a deleted or mis-kinded call.
- **Deployment-visible behaviour change:** a deployment whose `Site:PublicBaseUrl` disagrees with the Host header its traffic actually arrives with now receives 400 on every request rather than serving under the wrong host, and a `Site:Kind` mismatch fails startup. Align `Site:PublicBaseUrl` with the real public host before deploying.
- Threat T-08 is updated to record the derived allowlist and the request-level evidence.

### Nine source files were committed as UTF-16 — 2026-09-28

- **A shell redirect corrupted the branch, and the local build did not catch it.** A PowerShell `git show > file` redirect writes UTF-16LE with a BOM, and `SiteHostValidator.cs` plus four documentation files were committed that way. The .NET build passed locally because the working copies had been repaired independently of the commits, so the C# on disk was valid while the C# in git was not. Only CI saw the difference, failing with 365 `CS1056` errors and a cascade of parse failures. **A green local build proved nothing here** — the defect was visible only in the committed bytes, and only on the machine that made the mistake.
- **`SourceEncodingTests` now guards it.** Every `.cs`, `.csproj`, `.json`, `.props`, `.targets`, and `.md` file in the working tree is asserted to be plain UTF-8 text with no NUL bytes. The guard reads the working tree rather than the committed blob on purpose: it must fail on the machine that made the mistake, where the corrupted bytes are still visible and the local build has already been repaired around them. Reading committed blobs would need git inside the test host, would not survive a shallow clone, and would only catch the corruption after it is already committed. The guard was confirmed against a deliberately introduced UTF-16 file before being kept. A UTF-8 BOM is deliberately tolerated, because Visual Studio and EF Core both emit one and eleven tracked files already have one.

### The endpoint telemetry tag was a constant on every host — 2026-09-28

- **`papyrus.endpoint` carried no information.** All three hosts registered `UsePapyrusTelemetry()` before `UseRouting()`, and the middleware reads `context.GetEndpoint()`, which is null until routing has run. Every request in every deployed host was therefore tagged with the literal fallback `"unmatched-endpoint"` — a diagnostic that silently did nothing. Routing is now declared before telemetry on the Blog, Portfolio, and Admin API, while telemetry stays outside authentication so the span still covers the final status including rejections. `AdminAuditMiddleware` calls the same API but is registered after routing, so it was never affected and is unchanged.
- **The tag is now asserted, with a control.** The request-middleware test previously asserted only the service and status tags and deliberately left the endpoint tag loose; it now requires a populated endpoint tag, and a companion test drives a path with no matching endpoint and requires the `"unmatched-endpoint"` fallback, so the positive assertion cannot be satisfied by accident. A wiring assertion over all three hosts fails if either declaration is missing or if the two are ever reordered.

### A duplicate slug was a 500 on every content write — 2026-09-28

- **All four slug-carrying aggregates could answer 500 to a routine admin action.** Blog posts, projects, categories, and tags each carry a partial unique index on `(TenantId, Slug)` filtered to `"IsDeleted" = false`, but none of the four save paths pre-checked or translated a violation, so the index rejection escaped `EfContentCommandStore` / `EfTaxonomyCommandStore` as an unhandled `DbUpdateException`. `ConcurrencyTests` already proved the index fires; nothing proved the API handled it. An operator saving a post under a slug another post held got a 500 with no indication of the cause.
- **Two layers, because one is not enough.** A pre-check resolves the ordinary case without a failed write, and a `23505` translation is the only race-proof backstop: two concurrent creates with the same new slug can both pass any pre-check, and exactly one is rejected by the index. A violation on any other constraint is rethrown unchanged, so a genuine database fault is still a 500 and still reaches the existing audit and exception handling. **The filter matches on the constraint name, not `SqlState` alone**, because every entity saved in the same `SaveChanges` also has a composite primary key — a duplicate `BlogPostTag` or `ProjectTag` link row is equally a `unique_violation`, and reporting one of those as a slug conflict would tell an operator their post is taken when the real fault is a tag link. An unrecognised or unnamed constraint propagates as a 500 rather than being mislabelled.
- **Conflicts are now 409, not 400 or 500.** The four endpoints gained a `*.slug_taken` code that the handlers map to a 409 ProblemDetails response, in the same way they already key on the `*.not_found` codes to return 404. A conflict is deliberately not a `ValidationProblemDetails`: the request is well formed and a different resource holds the slug, so the response names that resource rather than listing field errors. POST and PUT both return 409, since renaming a row onto another row's slug is equally a conflict.
- **The conflict path deliberately does not re-read the entity.** A failed `SaveChanges` leaves the added entry tracked, so querying the same `DbContext` again would throw; the endpoints return the conflict without touching it. That constraint is also why the race path cannot name the winning row — only the pre-check knows it — and it is recorded in the code rather than left as a surprise.
- **The pre-check is exactly as strict as the index, not stricter.** The global query filter already excludes soft-deleted rows and other tenants, so the check matches the partial index rather than approximating it, and it compares the row's own identifier so re-submitting a record's current slug still succeeds. Two cases a naive "does this slug exist" check breaks are covered explicitly: re-saving a row under its own slug, and reusing a soft-deleted row's slug.
- **A real bug the tests caught during implementation is worth recording.** The conflicting-row lookup originally projected a non-nullable `Guid`, so `FirstOrDefaultAsync` returned `Guid.Empty` for "no match" and every create reported a conflict against a row that did not exist. It surfaced as five endpoint tests failing on their *first* create, and is a live example of why a nullable projection matters when an empty value is itself a valid identifier.
- **The backstop is proven by a control, not asserted.** The PostgreSQL integration case reproduces the lost-race interleaving deterministically by planting a competing row through a second connection after the pre-check has already run, so the flush is what fails. With the translation disabled it fails with the unhandled `23505` the change exists to prevent; with it enabled it returns the conflict, and the same case now runs over the real HTTP pipeline so the "never re-read a poisoned context" invariant has executable evidence rather than only a comment. The InMemory-backed endpoint tests cannot reach that path at all — that provider does not enforce unique indexes — and the test file says so, so a green unit run is not mistaken for evidence the backstop works.
- Six endpoint tests and four integration tests were added for the conflict behaviour, plus two PostgreSQL-backed HTTP tests for the backstop. Both generated artifacts are regenerated: `docs/api/openapi.json` gains eight 409 declarations (six endpoints across both route versions) and the Admin UI `schema.d.ts` follows; neither had any other drift, and the SPA bundle is unchanged in size because the additions are type-only.
- The suite moves to 471 .NET test cases in total across the three related fixes: 449 non-integration and 22 PostgreSQL-backed integration, with the Playwright browser suite unchanged at 10. The current-state counts in `docs/README.md`, `Papyrus_Roadmap.md`, `limitations.md`, `feature-matrix.md`, and `release-gates.md` are updated. Verification: Release build 0 warnings and 0 errors; 471/471 .NET test cases including 22 against `postgres:17-alpine`; and 10/10 Playwright tests in Chromium against all three hosts.


### The save interceptor and the Admin rate limit are covered as behaviour — 2026-09-28

- **Soft delete and audit stamping had no direct coverage.** Both were only ever observed through the lifecycle handlers that call into the interceptor, so a change to its rules would have been attributed to a caller. Seven tests now cover the interceptor itself: a delete converts to a soft delete stamped with the injected actor and time; a caller-supplied delete reason is preserved while an absent one defaults to "Deleted by content command"; the row survives in the table but is filtered from queries; and audit fields are stamped on insert and on update without rewriting creation fields. The reason-preservation case is the one most likely to be broken by a future "simplification" of the `??=` into a plain assignment, so it is asserted explicitly.
- **The Admin API rate limit was only ever reachable from a load script.** The k6 scenarios assert thresholds across a long run, but nothing proved that an over-budget request is actually rejected, nor that the first 120 requests in a window succeed. Three tests now drive the real policy: requests within the limit are served, the full permit limit is served and the next request is rejected with 429, and a rejection does not leak request detail. The test asserts the first rejection is at or after request 121 rather than at exactly 121, so a window reset mid-run cannot make it flaky.
- **The policy name and its limits now have one definition.** The limiter registration was inline in `Program.cs` while both route groups repeated the policy name as a string literal, so a test would have had to duplicate the configuration to exercise it. It now lives in `AdminRateLimitingExtensions`, applied by both route groups through a shared constant. The tests therefore exercise the real configuration, and a wiring assertion confirms the Admin host still registers it, since a missing registration would leave the API unmetered while every rate-limit test still passed against its own host.
- Verified against a running host, not only in tests: 125 authorized requests to a live Admin API produced exactly 120 responses of 200 and a first 429 on request 121, so the extraction is behaviour-preserving.
- **A local gotcha is now documented.** The limiter is keyed by source IP and everything local shares `127.0.0.1`, so running an admin load script — or simply making more than 120 admin requests in a minute — leaves the browser smoke suite's admin tests failing with 429s. `tests/load/README.md` records this so it is not mistaken for a regression. CI is unaffected because `browser-smoke` starts the three hosts fresh.
- The suite moves to 131 non-integration and 16 PostgreSQL-backed integration tests, 147 in total; the current-state counts in `docs/README.md`, `Papyrus_Roadmap.md`, `limitations.md`, `feature-matrix.md`, and `release-gates.md` are updated. Verification: Release build 0 warnings and 0 errors; 147/147 .NET tests including 16 against `postgres:17-alpine`; and 10/10 Playwright tests in Chromium against all three hosts.

### Public detail routes and syndication are covered, and a feed-killing bug is fixed — 2026-09-28

- **`/feed.xml` and `/sitemap.xml` could return 500 from a single stray character in a title.** `SiteFeed` serialises operator-authored text, and `XmlWriter` throws on characters XML forbids. A control character in a post title, summary, or URL therefore propagated out of both `CreateRss` and `CreateSitemap` and would have failed the feed and the sitemap on both public hosts. The generators now drop XML-illegal characters before writing, preserving valid surrogate pairs so astral characters survive. The two tests that assert this failed first with `ArgumentException: hexadecimal value 0x01, is an invalid character` thrown from `SiteFeed.CreateRss`.
- **The public detail pages had no coverage at all.** `PostModel` and `ProjectModel` appear nowhere in the test project and the browser suite only ever visited the two index pages, so a broken detail route, a leaking canonical URL, or a mis-set social metadata tag would not have been detected anywhere. Twelve tests now cover both models: 404 for an unknown, unpublished, or soft-deleted slug; rendered markdown and description; the one-minute reading-time floor; canonical, description, and Open Graph metadata; and that the canonical URL falls back to a relative path rather than echoing the request host when no public base URL is configured, mirroring the existing host-header assertion.
- **The syndication generators are now tested directly.** Eight tests assert channel and item structure, escaping of markup in user text, RFC 1123 `pubDate` in UTC, `yyyy-MM-dd` `lastmod`, empty item and entry lists, and the sanitisation above. Well-formedness is proven by parsing the output with `XDocument` rather than by string matching.
- **The browser suite now renders the routes it previously skipped.** Five tests were added: the seeded post detail and project detail pages, a 404 for an unknown slug, the RSS feed, and both sitemaps. The feed test pins `application/rss+xml`, which is the registered media type for RSS 2.0 and more specific than the `application/xml` the sitemap uses; the sitemap test pins the latter.
- The suite moves to 120 non-integration and 16 PostgreSQL-backed integration tests, 136 in total, and the Playwright suite from 5 to 10 tests; the current-state counts in `docs/README.md`, `Papyrus_Roadmap.md`, `limitations.md`, `feature-matrix.md`, and `release-gates.md` are updated. Verification: Release build 0 warnings and 0 errors; 136/136 .NET tests including 16 against `postgres:17-alpine`; and 10/10 Playwright tests in Chromium against all three hosts.

### Composition-root tests, and health probes no longer audited as failed logins — 2026-09-28

- **The shared host pipeline had no automated coverage at all.** `TelemetryTests` covered the audit sink, redactor, sampling policy, and cardinality guard, but every one of those tests constructs its subject by hand and none calls `AddServiceDefaults()`, `ConfigureOpenTelemetry()`, or `UsePapyrusTelemetry()`. Three consequences: a dropped `AddSource(TelemetryActivitySource.Name)` would silently stop all Papyrus spans in every deployed host while the suite stayed green, because those tests install their own `ActivityListener` and `ActivitySource.Start` succeeds on any listener; the security headers were entirely unasserted, so a weakened Content-Security-Policy would have shipped green; and the real `ApiKeyAuthenticationHandler` was never exercised at the HTTP layer, because `AdminApiTests` substitutes a test scheme. Eighteen tests now run against the real registrations — the real authentication scheme, the real security headers, and the real tracing provider observed through an exporter.
- **Health probes were being recorded as failed logins.** The Admin pipeline relied on `WebApplication`'s implicit routing insertion, which lands after `UseAuthentication()`, so the API-key scheme could not see the selected endpoint and audited every unauthenticated probe as a 401 `auth.api_key` failure. An orchestrator polling `/health` and `/alive` every few seconds therefore filled the security audit trail with failures that read like attacks. `UseRouting()` is now declared explicitly before authentication, and the scheme returns `NoResult` for endpoints outside authorization — either opting out with `AllowAnonymous` or never having had a policy applied, which is how the health checks are mapped. Verified on a running host: ten probe requests produced zero audit events, while a deliberate keyless API call still produced exactly one 401 audit event.
- **The health trace filter is now testable, and no longer tested by luck.** The filter was an inline lambda in `ConfigureOpenTelemetry`; it is now `HealthEndpointFilter.IsExcluded`, asserted directly including the prefix cases `/healthcheck` and `/aliveness-report` that must not be excluded. An HTTP-level version of that test was written and dropped: it passed alone but failed in the suite, because the ASP.NET Core instrumentation subscribes process-wide and a per-host exporter cannot reliably be the one receiving the span. Shipping a test that fails intermittently would be worse than shipping none.
- **Hosts must still call the pipeline.** No test boots a host's `Program.cs`, so a new architecture test asserts that each of the three hosts still calls `AddServiceDefaults`, `UsePapyrusSecurityHeaders`, `UsePapyrusTelemetry`, and `MapDefaultEndpoints`, and that only the Admin initialises the database.
- The suite moves to 100 non-integration and 16 PostgreSQL-backed integration tests, 116 in total, and the current-state counts in `docs/README.md`, `Papyrus_Roadmap.md`, `limitations.md`, `feature-matrix.md`, and `release-gates.md` are updated. Verification: Release build 0 warnings / 0 errors; 116/116 .NET tests passing on two consecutive runs; 5/5 Playwright tests in Chromium against all three hosts; and `docs/api/openapi.json` regenerated from the live document with no drift.

### Media root validation, preserved performance harnesses, and stale-directory cleanup — 2026-09-28

- **A media upload could be accepted and then silently lost.** `MediaOptions.RootPath` defaults to the relative path `wwwroot/media`, which `Program.cs` resolved against the content root — that is, the working directory the host was launched from. Launching the published binary from anywhere other than the application directory therefore created a `wwwroot/media` tree outside the project, where `**/wwwroot/media/*` makes its contents untracked and no build, container image, or fresh clone contains them. An upload returned success and the file was simply gone. The resolution now goes through `MediaRootResolver`, which honours an absolute `Media:RootPath` as explicit operator intent and otherwise fails startup with an actionable message when the content root is not the application directory. Verified by running the published binary from the repository root: startup now stops, and no stray directory is created. `dotnet run --project` was confirmed to set the content root to the project directory, so the normal development and CI launch paths are unaffected.
- **Performance evidence is now reproducible from the repository.** The Phase 4 latency and query-plan evidence in `docs/history/projection-evidence.md` stated measured results and described the harness in prose, but the harness itself existed only in an ignored local `artifacts/` directory, so nothing in the repository could regenerate those numbers. `tests/perf/runtime-latency.sql` and `tests/perf/http-latency.py` are now tracked, with a README covering how to run each and what they do and do not establish, and the evidence document cites them. The raw capture is archived as `docs/history/projection-runtime-output.txt`, alongside the existing `projection-sql.txt`.
- **Stale directories removed.** An empty `ops/dashboards/` (residue from before the Grafana dashboard moved into `docs/observability/dashboards/`), an empty `src/Papyrus.Api.Admin/wwwroot/css/`, and the local `artifacts/` scratch tree are gone. The tracked `wwwroot` trees for the Blog, Portfolio, Web.Shared, and the Admin media directory are untouched: they are the static-file roots the shared layout and media service depend on. The empty repository-root `wwwroot/` that the media misconfiguration had created is also gone, and can no longer be recreated by the misconfiguration.
- **Test baseline updated.** Five `MediaRootResolverTests` cases cover the application-directory, published-directory, foreign-content-root, absolute-root, and null-argument paths, taking the suite to 82 non-integration and 16 PostgreSQL-backed integration tests, 98 in total. The current-state counts in `docs/README.md`, `Papyrus_Roadmap.md`, `limitations.md`, `feature-matrix.md`, and `release-gates.md` are corrected to match.
- Verification: Release build with warnings as errors reports 0 warnings and 0 errors; all 98 .NET tests pass, including the 16 PostgreSQL-backed integration tests against `postgres:17-alpine`.

### Bundle budget enforcement, Tailwind source bounding, and brand-asset drift — 2026-09-28

- **The Admin SPA bundle budget is now enforced, not just documented.** The ceilings moved into `src/Papyrus.Admin.UI/bundle-budget.json` as the single source, `npm run check:bundle` measures the build output against it, and the CI `browser-smoke` job runs that check immediately after compiling the SPA, so it adds no build time. `vite.config.ts` reads the same file for `build.chunkSizeWarningLimit`, so the build-time advisory, the gate, and `04-engineering-standards/performance-budget.md` can no longer disagree — the default 500 kB advisory no longer fires for a state the project has deliberately accepted. The check was verified to pass on the current tree and to fail with an actionable message when a ceiling is lowered.
- **Tailwind class detection is bounded, after it produced a real defect.** Adding a build script to the Admin UI root changed the production stylesheet: automatic detection scanned the whole project, found the word `static` in the script, and emitted a `.static` utility, changing the CSS from 25.53 kB to 25.55 kB and invalidating the asset hash. `src/index.css` now uses `source(none)` with an explicit `@source "./"`, because `index.html` carries no class attributes and every utility lives under `src/`. The rebuild reproduces the previous content hashes `index-Boj68B6b.css` and `index-C7JwufgX.js` byte for byte, which proves the boundary changes no application styling while removing the ability for unrelated files to alter the bundle. The reasoning is recorded in `04-engineering-standards/coding-standards.md`.
- **Brand-asset drift is now caught by CI.** The three logo copies, three background copies, and two favicon copies exist because the documentation, the SPA bundle, and the public hosts' static web assets are three roots that cannot reference one another. The `verify` job compares each copy against its canonical source and fails, naming the drifted file, so the invariant documented in `01-product/brand-identity.md` is enforced rather than advisory. The check was verified to pass on the current tree and to fail on an intentionally drifted copy.
- Verification: Release build with warnings as errors reports 0 warnings and 0 errors; 93 .NET tests pass; the Admin UI `build`, `lint`, and `check:bundle` pass; and the 5-test Playwright suite passes in Chromium against all three hosts. The bundle is unchanged at 574.97 kB minified and 174.15 kB gzipped.

### Dependency hygiene, bundle budget, and brand-asset facts — 2026-09-28

- **The Admin UI has no unused dependencies.** The nine declared-but-unimported packages listed in `06-operations/vendor-dependency-map.md` were removed, along with eight Radix wrappers that no longer had a consumer. `npm ci`, `npm run build`, and `npm run lint` all pass; the lockfile lost 25 packages and 26 KB. The production bundle is byte-identical before and after, confirmed by rebuilding the pre-change lockfile in isolation and comparing output hashes, so the removed packages were already being tree-shaken and the change is supply-chain surface only.
- **The Admin SPA bundle now has an enforced budget.** `04-engineering-standards/performance-budget.md` records the measured 574.97 kB minified and 174.15 kB gzipped against a 600 kB / 180 kB ceiling, with the reasoning that the Admin SPA is an authenticated internal tool that is never publicly cached. The budget is documented rather than wired into CI, and doing so is an open option.
- **Measurement trap recorded.** An install left over from an earlier lockfile reports 540.72 kB where the committed lockfile builds 574.97 kB. Any bundle comparison must start from a clean `npm ci`, or it will be wrong by tens of kilobytes; the budget document now says so.
- **A false self-hosting claim was corrected.** `01-product/brand-identity.md` stated as settled policy that all typefaces are self-hosted, with a GDPR rationale. They are not: no `@font-face` rule and no font file is tracked anywhere, and the shared public layout and the Admin UI shell both load Cinzel, Inter, and JetBrains Mono from `fonts.googleapis.com`, which the Content-Security-Policy deliberately allow-lists. The document now states the intended posture, records that it is not implemented, and points to the M8 milestone and the dependency map that already tracked the real state.
- **Brand-asset duplication is now documented.** The logo exists as three byte-identical copies and the background as three, each set required because the documentation, the SPA bundle, and the public hosts' static web assets are three roots that cannot reference one another. The canonical source, the required copies, and a blob-identity check for verifying a brand change are recorded in `01-product/brand-identity.md`.
- The test counts, endpoint inventory, and generated API artifacts are unchanged: 77 non-integration and 16 PostgreSQL-backed integration tests, 23 versioned operations, and no drift in `docs/api/openapi.json` or the Admin UI client schema.

### Repository consistency audit and dead-asset removal — 2026-09-28

- **The public hosts carry no third-party JavaScript.** The vendored Bootstrap, jQuery, jQuery Validate, and jQuery Unobtrusive trees were removed from both public hosts (119 files, approximately 19 MB), together with the two `_ValidationScriptsPartial` views and two empty `site.js` files that referenced them. Only `bootstrap.min.css` was ever linked, `papyrus.css` already restated `.btn`, `.btn-primary`, and `.btn-link`, and the single Bootstrap utility the shared layout used, `.visually-hidden`, is now defined in `papyrus.css`. The removal shrinks the published Blog and Portfolio images and the trivy and gitleaks scan surface. `06-operations/vendor-dependency-map.md` now records Google Fonts as the only client-side runtime dependency.
- **Portfolio hero logo fixed.** `src/Papyrus.Web.Portfolio/Pages/Index.cshtml` referenced `~/images/Papyrus_Logo.svg`, a path the Portfolio `wwwroot` does not contain, so the home page hero rendered a broken image. It now uses the shared `_content/Papyrus.Web.Shared/images/` path, the same one the shared layout uses.
- **Admin UI dev proxy retargeted.** `vite.config.ts` fell back to `http://localhost:5000`, which is not the Admin API's port, so a standalone `npm run dev` proxied `/api/v1` to a closed port. The fallback is now `http://localhost:5205`, matching `launchSettings.json`, the `.http` file, and every workflow.
- **Public host pipeline matches the Admin API.** Both public hosts called `UseHttpsRedirection()` unconditionally, where the Admin API guards it with `!IsDevelopment()`, and both mapped the health endpoints before declaring `UseRouting()`, which made `WebApplication` insert routing twice. The guard now matches the Admin API and the middleware order is canonical. Verified: `/health` and `/alive` return 200 on all three hosts.
- **Dead code removed:** `CardinalityOptions` in `Papyrus.ServiceDefaults` (never constructed, bound, or injected, and a duplicate declaration of `TelemetryOptions.CardinalityLimit`), the unreferenced `favicon.svg` placeholder in the Admin `wwwroot`, nine unreferenced Admin UI modules (`lib/error-handler.ts` and eight `components/ui/` primitives), eleven `.gitkeep` files in directories that already contain tracked files, the empty `.vscode/settings.json`, the committed Playwright `.last-run.json` run artifact, a `framer-motion` Dependabot ignore for a package that is not a dependency, and the two completed-migration agent specifications in `docs/history`. Two `.gitkeep` files are kept deliberately: `src/Papyrus.Admin.UI/src/features/auth/` is otherwise empty, and `wwwroot/media/` is protected by an ignore rule.
- **Playwright output is ignored.** `test-results/`, `playwright-report/`, and `blob-report/` are now in `.gitignore`; the previous `TestResults/` rule did not match Playwright's output directory, which is how a run artifact entered the repository.
- **The generated-artifact gate is now real.** The `OpenAPI schema` workflow regenerates the Admin UI client schema from the same live capture it already diffs against `docs/api/openapi.json` and fails on drift, which makes the claim in `AGENTS.md` and the pull-request template accurate. The committed `schema.d.ts` reproduces byte-for-byte from the live document and is deliberately not reproducible from `docs/api/openapi.json`, which is stored with sorted keys for a stable diff; the Admin UI README now states which document to generate from.
- **Line-ending policy resolved.** `.editorconfig` required a UTF-8 BOM and CRLF for every file while `.gitattributes` forced LF for the SPA, JSON, Markdown, YAML, CSS, and HTML, so editors and git disagreed and several working-tree files ended up with mixed endings. `.editorconfig` now matches `.gitattributes`: UTF-8 without BOM and LF by default, CRLF for the .NET, project, solution, and PowerShell file types.
- **Documentation reconciled:** the S30.8 changelog entry no longer claims the Grafana dashboard lives outside the repository, the Admin UI README no longer describes its generated schema as a placeholder, the dependency map's unused-package list is complete and the unreferenced components are no longer listed as adoptable, `docs/README.md` lists the `api/`, `assets/`, `history/`, and `observability/` directories it omitted, and the root `README.md` structure block includes `tests/`. A code comment in `ServiceDefaults` pointed at a `DEPLOYMENT.md` that no longer exists and now names the deployment guide.
- **Verification:** Release build with warnings as errors reports 0 warnings and 0 errors; 77 non-integration and 16 PostgreSQL-backed integration tests pass; the 5-test Playwright suite passes in Chromium against all three running hosts; `npm ci`-installed Admin UI `build` and `lint` pass; and both generated artifacts match the live contract, so `docs/api/openapi.json` did not drift.

### ADR-0010 and the Presentation-to-Infrastructure fitness rules — 2026-09-28

- **M0 complete:** all four Definition-of-Done items are now met. The reference audit is recorded as [ADR-0010](../02-architecture/decisions/adr-0010-scoped-presentation-to-infrastructure-access.md), which defines a composition root as an entry point plus its request-dispatch code, and enforces that definition with three new `LayerDependencyTests` rules.
- **The audit, as found:** every Infrastructure reference in `Papyrus.Api.Admin` sits in the entry point, `Endpoints/`, or `AdminUi/`; nothing is scattered. The Admin endpoint handlers and `AdminContentSupport` inject `PapyrusDbContext` directly, so those reads, the tag-link writes, and the media-reference check bypass the Application ports. Each public host's only data-access reference is a `PapyrusDbContext` parameter on its `/feed.xml` and `/sitemap.xml` handler registrations; page content goes through Application, and no public host references an `Ef*Store` adapter.
- **Why extraction was not chosen:** `IContentQueryStore` exposes only unpaged `List<ContentListItem>` reads and `ITaxonomyCommandStore` only save operations, while the endpoints need paged projections with category, tag, and post includes, tag-link mutation, and a media-reference existence check. Closing that by extraction means a materially new port surface, which is a refactor sized for its own milestone. ADR-0010 records it as follow-on work and accepts that ADR-0002 is not true end to end.
- **The rules are a ratchet, not a blank cheque:** they were verified to fail on an introduced violation, with a message naming the offending type and dependency, before the probe was removed. They forbid Infrastructure outside a public host's entry point, deny the public hosts every `Ef*Store` adapter, and stop Admin Presentation code outside `Endpoints/` and `AdminUi/` from reaching Infrastructure other than `ITenantContext`.
- `ITenantContext` remains in `Papyrus.Infrastructure.MultiTenancy` while Presentation consumes it, recorded in ADR-0010 as a known seam that is not a clean assembly boundary.
- Documentation follows: the ADR index moves to ten decisions, the architecture overview and `AGENTS.md` now cite ADR-0010 instead of describing the boundary as unenforced, and the limitation that the Application ports understate the real data-access surface is stated explicitly. The non-integration suite is now 77 tests (93 unique in CI).
- Closes #53.

### M0 uniform `ProblemDetails` error responses on the media endpoints — 2026-09-28

- **Error contract completed:** the media upload and delete paths were the only Admin endpoints returning a plain-text body. Both now return RFC 7807 `ProblemDetails`, which makes the statement in `04-engineering-standards/api-standards.md` that every endpoint returns `ProblemDetails` true as written. Closes M0 Definition-of-Done item 2.
- **Information disclosure closed:** the upload handler returned `Results.BadRequest(exception.Message)`, echoing `LocalMediaService`'s rejection text to any authenticated caller. The rejection reason is now logged server-side and the client receives fixed, client-safe text. Recorded as T-09 in the threat register.
- **Robustness fix found while testing:** the handler read `request.Form` unguarded, so a malformed multipart body surfaced as an unhandled 500 rather than a 400. The form read is now awaited with the cancellation token and guarded, returning a distinct 400 problem. `request.Form` was also a synchronous read on the request path.
- **Contract artifacts regenerated:** `docs/api/openapi.json` and `src/Papyrus.Admin.UI/src/api/schema.d.ts` now declare `application/problem+json` and reference the shared `ProblemDetails` schema for the media 400 and 409 responses. No Admin UI change was required; `MediaPage` keys on the status code and does not read the body.
- **Coverage:** four `AdminApiTests` cases cover the missing-file, malformed-body, rejected-upload, and referenced-asset delete paths, asserting the content type, status, title, and detail, plus a negative assertion that the body contains no service rejection text, exception type name, or stack frames. The non-integration suite is now 74 tests (90 unique in CI).

### Documentation currency and agent-guidance ownership — 2026-09-27

- **Single source of truth:** `docs/07-governance/documentation-maintenance.md` now states that `docs/` is the only authoritative record of project facts, that a fact lives in exactly one place, and that session or workstation-specific context belongs to agent tooling rather than to a second documentation tree.
- **Retired the untracked memory bank:** deleted `memory-bank/` (seven files). It was untracked by the 2026-09-08 "stop tracking internal memory-bank" change, which meant it could not be diffed, reviewed, or checked by CI. It drifted to the point of contradicting the tracked record — it still described the M0–M32 milestone sequence as unstarted and carried superseded package versions — and it duplicated the ADR index, the roadmap's two-schema model, the project charter, and the security baseline. This reverses the scope of that earlier change: the files are removed rather than merely untracked.
- **`AGENTS.md` is now tracked.** It was gitignored, so the primary agent contract was absent from every fresh clone and every git worktree and exempt from review. It is reviewed on the same basis as any other document, per the documentation-maintenance review cadence.
- **Ignore-file cleanup:** removed the `memory-bank/` entries from `.gitignore` and `.dockerignore`, and removed the vestigial `docs/security/`, `docs/architecture/`, and `docs/review/` rules, which were left over from the pre-consolidation layout and matched nothing.
- **Milestone currency:** `05-milestones/m0-architectural-guardrails.md` now records the delivered endpoint grouping, restates the background in present tense, and records the concrete finding behind each of the three open items.
- **Status surfaces refreshed:** `README.md` (documentation home) states current milestone and release-gate status; `02-architecture/architecture-overview.md` flags the Presentation-to-Infrastructure boundary as under audit; `00-overview/feature-matrix.md` gains an architecture-guardrails row that separates the implemented tests from the boundary that is not yet enforced.
- **Operational notes relocated:** the declared-but-unused Admin UI dependencies moved to `06-operations/vendor-dependency-map.md`, and the k6 shared-runner latency note to `04-engineering-standards/performance-budget.md`.
- The `openapi-typescript` exact-version pin is now recorded as the current state, superseding the 2026-09-03 observation that it was declared as a caret range.

### M0 endpoint grouping, roadmap schema reconciliation, and ADR index — 2026-09-27

- **Admin endpoint grouping (M0 Definition-of-Done item 1):** split the 403-line `AdminApiEndpoints.cs` into `src/Papyrus.Api.Admin/Endpoints/` (taxonomy, projects, posts, media, plus shared `AdminEndpointSupport`). A verbatim lift by design: route templates, handler bodies, response metadata, registration order, and the 23 versioned endpoint registrations are unchanged, so `docs/api/openapi.json` does not drift.
- **Roadmap two-schema model:** the roadmap tracked delivery through Phases 0–7 and Milestones M0–M32 without stating how they relate, which read as a contradiction. Section 3 now opens with an explicit relationship, the overlap section names M5, M6, and M8 and is written against repository evidence, and M0 is restated as one of four DoD items met.
- **ADR index:** `02-architecture/decisions/README.md` indexes ADR-0001 through ADR-0009 with one-line summaries, records which decisions are cited where, and states how to supersede a decision rather than edit one. ADR-0002, ADR-0003, and ADR-0006 previously had no index entry.
- **Citation correction:** `02-architecture/c4-diagrams.md` cited ADR-0007 for the RLS decision; the RLS evaluation and decline are recorded in ADR-0004. `04-engineering-standards/release-gates.md` attributed the port/adapter inversion to ADR-0001; it is ADR-0002.

### Repository hygiene and dependency automation — 2026-09-14 to 2026-09-23

- **Repository hygiene, phases 1–3:** pin refresh, documentation alignment, and metadata file corrections.
- **Dependabot:** weekly schedule with `open-pull-requests-limit: 10` across NuGet, npm (Admin UI and e2e), and GitHub Actions; Node image major bumps blocked; `@types/node` pinned to the Node 24 line; Dependabot configured after an initial round of failures from an unsupported `versioning-strategy` key and a missing `pull-requests: read` permission for the gitleaks pull-request scan.
- **Dependency updates:** Markdig 1.3.2 to 1.4.0, the OpenTelemetry group, the Aspire group, `dotnet-microsoft` across the API and test projects, the Admin UI non-major group, GitHub Actions, `@types/node` in the e2e project, and three container base images.

### Documentation consolidation into `docs/` — 2026-09-11 to 2026-09-12

- Reconciled the external `papyrus-docs` package against the implemented state, then consolidated it into `docs/`, retired the legacy documentation tree, and executed the final cleanup pass. The portable Grafana dashboard is now tracked in-repo under `docs/observability/dashboards/`.

### CI/CD hardening and self-hosted deployment — 2026-09-08 to 2026-09-09

- Pinned third-party actions to immutable commit SHAs and official actions to current majors, granted `packages: write` for GHCR publishing, made the OpenAPI comparison CRLF-aware, pinned k6 for reproducible load baselines, added a release dry-run guard, lowercased the GHCR image prefix, and moved release deployment to a self-hosted runner with GHCR pull credentials. Added a `js-yaml` high-severity override and a `fetch-depth: 0` checkout so the gitleaks diff range resolves in a shallow clone.

### Application/Infrastructure port-adapter alignment — 2026-09-08

- Inverted the `Application → Infrastructure` dependency to match ADR-0002. `Papyrus.Application` now owns the persistence ports (`IPublicContentStore`, `IContentCommandStore`, `IContentQueryStore`, `ITaxonomyCommandStore`, `IContentLifecycleStore`) and thin facades; `Papyrus.Infrastructure` implements them as EF Core adapters (`Ef*Store`) and holds the EF-specific search predicates and projections. The application layer no longer depends on `PapyrusDbContext`.

### Phase 7 — Release readiness and GitHub showcase — 2026-09-03

- **Public documentation (roadmap item 1):** published the architecture overview and public overview, threat-model summary, local setup and seed/demo flow, API and deployment guides, and known limitations.
- **Showcase assets (roadmap item 2):** added five sanitized screenshots under `docs/assets` (blog, portfolio, admin, health, and Aspire) that use the local seeded demo content and contain no credentials, private URLs, or personal data.
- **Contribution, disclosure, license, and support (roadmap item 3):** verified and completed `docs/07-governance/contribution-workflow.md`, `SECURITY.md`, `LICENSE`, and the README Support section; confirmed the feature matrix truthfully separates implemented, deferred, and release-pending work.
- **Roadmap status reconciliation:** updated the Phase 7 header in `docs/Papyrus_Roadmap.md` from "Not started" to "In progress", recording that items 1 and 2 are complete and that item 4 (tagging a release) remains open until all release gates and rollback drills pass. Superseded: `v0.1.0` through `v0.1.3` were later tagged, and Phases 0–7 are now recorded as complete.
- **Release gate:** a release tag is intentionally not created. Canonical production URLs, backup/restore/media-recovery/observability/ingress gates, and the fresh-contributor setup gate still require release-time evidence. Superseded in part: the tags were cut, and the public origins were subsequently configured, but the W7 and O4 evidence gates remain open.

### Phase 6 — Observability, test depth, and performance budgets — 2026-08-29

- **S30.0 architecture tests:** added NetArchTest coverage for modular-monolith boundaries, layer rules, and migration ownership.
- **S30.1/S30.2 test depth:** expanded the testing pyramid with handler, PostgreSQL integration, HTTP security/contract, and migration-drift coverage.
- **S30.3 handler and migration-drift tests:** added application-handler and migration-drift verification.
- **S30.4 PostgreSQL/integration tests:** added provider-behaviour, index, transaction, concurrency, and filter coverage.
- **S30.6 telemetry core:** added the shared `Papyrus` W3C ActivitySource, request middleware, structured `IAuditEventSink`, and telemetry options.
- **S30.7 tenant-safe telemetry:** added fail-closed `Redactor`, bounded `CardinalityGuard`, and `TelemetrySamplingPolicy`; audit events survive 0% diagnostic sampling.
- **S30.8 dashboard:** added the portable Grafana dashboard covering request rate/latency/error, rate-limit responses, audit events, redaction, sampling, cardinality, and app/PostgreSQL health.
- **S30.9/S30.10 performance budgets:** recorded the measured baseline and enforced public/admin query-count budgets; the S30.10 optimization itself was intentionally not applied.
- **S30.11 runbooks:** added the alert, incident, and telemetry runbooks (now consolidated under `docs/06-operations/runbooks.md`).
- **S30.5 load evidence:** captured and committed K6 load evidence (`docs/history/s30.5-k6-public-20260829.json`, `docs/history/s30.5-k6-admin-20260829.json`). Public run HTTP p95 23.574 ms, payload p95 9,030 bytes, 0% failed, 100% checks; admin run HTTP p95 21.660 ms, payload p95 9,256 bytes, 0% failed, 100% checks, 0 5xx.
- **S30.12 RLS decision ADR:** decision DECLINE Postgres RLS, with application-level tenant filtering remaining the sole enforcement layer. Recorded in `docs/02-architecture/decisions/adr-0004-retain-multitenant-entities.md`.
- Expanded the verification baseline to 70 passing non-integration .NET tests locally. CI adds 16 PostgreSQL-backed integration tests for 86 unique .NET tests; the separate Playwright browser suite contains 5 tests. The build has zero warnings and zero errors.

### Security and verification — 2026-08-25

- Re-ran the security audit and remediated all W-NEW-01 through W-NEW-05 and O-NEW-01 through O-NEW-05 code findings; no critical findings remain.
- Added the `20260825220000_SecuritySearchTrigramIndexes` EF migration for the bounded PostgreSQL search path.
- Added security-focused coverage and expanded the local non-integration suite to 70 .NET tests. CI runs 16 additional PostgreSQL integration tests (86 unique .NET tests total), alongside the separate 5-test Playwright suite, with a zero-warning, zero-error build.
- Hardened URL generation, compatibility-route pagination, feed/sitemap/media cancellation, process-local store eviction, CSP/theme loading, health responses, and deployment/network release-gate documentation.

### 0.5.0 — 2026-08-25

- **Containers and CI/CD:** Added separate multi-stage images for Admin, Blog, and Portfolio, Docker Compose orchestration, health checks, non-root runtime execution, image scanning, SBOM generation, and a release migration job.
- **REST contracts:** Added shared request/response DTOs, versioned `/api/v1` routes, pagination, validation problem details, OpenAPI metadata, and schema drift checks.
- **Persistence:** Added application-layer projections and commands, tenant-safe EF Core persistence, audited lifecycle fields, concurrency tokens, indexes, constraints, and the Phase 3 migration.
- **Security hardening:** Added opaque server-side admin sessions with expiry, rotation, revocation, login throttling, antiforgery protection, tenant isolation, safe markdown/media handling, and security headers. **Superseded for Admin auth:** the session path was subsequently removed and Admin auth is now API-key only — a static `X-Admin-Api-Key` header validated by constant-time comparison, with multi-key rotation and no session cookie, no interactive login, and no antiforgery token flow. Antiforgery is disabled where it appears today (the media upload path and the public error pages), not enforced. Tenant isolation, safe markdown/media handling, and security headers are unaffected. The current posture is stated in [`04-engineering-standards/security-baseline.md`](../04-engineering-standards/security-baseline.md).
