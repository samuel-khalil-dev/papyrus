# Performance Budget

## Load testing
k6 defines p95 latency thresholds for key public routes, revisited as real traffic data becomes available.

### Shared-runner caveat
The `load-smoke` CI job runs on `ubuntu-latest`, so its latency figures are
measured on shared, contended runners rather than on dedicated hardware, and the
job has no retry. A `p(95)` threshold breach is therefore not by itself evidence
of a code regression. Both scenarios assert `http_req_duration p(95)<2000` while
the recorded baselines are 21–33 ms — roughly two orders of magnitude of
headroom — so a breach far above that margin indicates runner or host contention
and should be re-measured on a self-hosted runner before being treated as a
regression. The query-count and allocation budgets are unaffected by this caveat:
they are measured in-process, not on the runner's network path.

## Front-end performance
Motion restricted to `transform`/`opacity`. **Planned (M13):** static assets served via the Cloudflare edge cache to reduce origin load on the VPS.

## Infrastructure headroom

**Current:** the 4-core/8GB VPS hosts the application containers and PostgreSQL directly via Docker Compose. **Planned (M2, M5, M13):** a reverse proxy, a lightweight monitoring agent, and the Cloudflare edge layer will add to this footprint — per-container resource limits (M5) are scoped to prevent any of these additions from starving PostgreSQL once they exist.

## Current Budget Ceilings

The measured ceilings below are the dispatch-gated budgets captured for the
Phase 6 baseline (S30.9) and enforced by the CI/manual workflow; they are a
per-release baseline, not a per-PR soak. "Regression = release-blocker" applies:
a performance regression is a release-blocking defect.

### Explicit budgets

| Signal | Budget | Enforcement |
| --- | --- | --- |
| Public/admin HTTP p95 | ≤ 2000 ms; target ≤ 500 ms | K6 threshold |
| HTTP error rate | < 1%; admin 5xx = 0 | K6 thresholds |
| Public page queries | ≤ 4 commands/request | `PerformanceBudgetTests` |
| Admin collection queries | ≤ 2 commands/request | `PerformanceBudgetTests` |
| Public payload | ≤ 256 KiB compressed response | K6 capture/review |
| Admin collection payload | ≤ 128 KiB compressed response | K6 capture/review |
| Upload | ≤ 5 MiB input; no unbounded buffering; record elapsed cost | media limit + dispatch capture |
| Managed allocations | ≤ 2 MiB/request public feed; ≤ 1 MiB/request admin read | `dotnet-counters` dispatch review |
| GC | Gen2 collections = 0 during smoke; Gen0/Gen1 trend recorded | `dotnet-counters` dispatch review |

### Measured baseline (archived evidence)

The query timings are 30-sample PostgreSQL execution measurements; HTTP p95/
payload figures are the K6 capture points. Raw artifacts are archived under
`docs/history/` (`projection-evidence.md`, `s30.5-k6-*-20260829.json`,
`s30.9-k6-*-20260903.json`, `s30.9-admin-counters-20260903.json`).

| Workload | DB p95 (ms) | Queries/request | HTTP p95 (ms) | Payload p95 (bytes) | Error/check baseline |
| --- | ---: | ---: | ---: | ---: | --- |
| Public Blog/Portfolio feed (S30.5, 2026-08-29) | 0.073 / 0.041 | 4 | 23.574 | 9,030 | 0.00% failed; 100.00% checks |
| Admin Blog collection (S30.5, 2026-08-29) | 0.037 | 2 | 21.660 | 9,256 | 0.00% failed; 100.00% checks; 0 5xx |
| Public feed + search (S30.9 production-like, 2026-09-03) | — | 4 | 32.79 | 16,887 | 0.00% failed; 100.00% checks |
| Admin collection + write (S30.9 production-like, 2026-09-03) | — | 2 | 19.35 | 19,827 | 0.00% failed; 100.00% checks; 0 5xx |

All values are within the explicit budgets. `dotnet-counters` during the S30.9
admin workload recorded ~66 KiB/request (well under the ≤ 1 MiB admin budget),
Gen2 collections = 0, total GC pause 0.117 s, and max heap ~8.3 MiB.

### S30.10 measured-optimization decision

No production optimization was applied: public reads are asynchronous,
no-tracking, projected, bounded, and execute four bounded queries; admin
collections execute the expected count-plus-page pair; PostgreSQL execution p95
is below 0.1 ms for the captured paths with no sequential scan or N+1. Changing
query shape, adding an index migration, or applying allocation
micro-optimization would be speculative. Re-measure with a production-like
dataset before considering an S30.10 change.

## Regression policy
A performance regression is a release-blocking defect.
