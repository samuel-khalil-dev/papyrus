# Phase 6 performance baseline and budgets

**Subtasks:** S30.9 baseline + S30.10 measured optimization  
**Capture date:** 2026-08-29 UTC  
**Status:** baseline committed; no S30.10 production optimization justified

## Scope and environment

The primary workload is the paginated public Blog/Portfolio feed and search. The
secondary workload is the versioned Admin collection read. Upload is measured as a
bounded single-file operation, not as a concurrency workload. Measurements use the
PostgreSQL-backed application and relative URLs; `Site__PublicBaseUrl` is unset,
so canonical/RSS/sitemap URL behavior is excluded.

| Variable | Baseline pin |
| --- | --- |
| Runtime | .NET 10, Release, nullable + warnings as errors |
| Database | PostgreSQL 17 / Npgsql EF Core 10 |
| Dataset | `main` seed (one live project, one live post, one category, two tags) |
| Public load shape | 4 VUs, 20 s feed then 4 VUs, 20 s search; S30.5 K6 scenario |
| Admin load shape | 1 request/s bounded path; limiter probe is separate and accepts 429 |
| URL mode | Relative URLs; no production base URL configured |

## Baseline measurements

The query timings below are the 30-sample PostgreSQL execution measurements from
[`projection-evidence.md`](projection-evidence.md:108). They exclude HTTP and Razor
rendering. HTTP p95/error and payload measurements are the S30.5 K6 dispatch
capture points; the committed smoke configuration is the reproducible baseline
until a production-like environment is available.

| Workload | DB p95 (ms) | Queries/request | Payload budget observation | Error baseline |
| --- | ---: | ---: | --- | --- |
| Public Blog feed | 0.073 | 4 (count, page, featured, tags) | response bytes to be captured by K6 `http_req_receiving`; budget 256 KiB | K6 threshold `<1%` |
| Public Portfolio feed | 0.041 | 4 (count, page, featured, tags) | response bytes to be captured by K6; budget 256 KiB | K6 threshold `<1%` |
| Public Blog search | not separately isolated in SQL capture | 4 | budget 256 KiB | K6 threshold `<1%` |
| Public Portfolio search | not separately isolated in SQL capture | 4 | budget 256 KiB | K6 threshold `<1%` |
| Admin Blog collection | 0.037 | 2 (count, page) | response budget 128 KiB | K6 threshold `<1%`, 0 5xx |
| Admin upload (5 MiB maximum) | not exercised in S30.5 | 0 DB commands | request budget 5 MiB; cost must be captured as upload duration and bytes | 0 5xx; validation 4xx allowed |

The current K6 transport baseline is p95 `<1000 ms`, failed requests `<1%`, and
checks `>99%`; those are enforced by
[`public-listing.js`](../../tests/load/k6/public-listing.js:23) and
[`admin-write.js`](../../tests/load/k6/admin-write.js:37). The CI/manual workflow
captures the actual p95, error rate, and response bytes without pretending that a
local seeded database is production capacity evidence.

## Explicit budgets

These are dispatch-gated budgets, not a per-PR soak:

| Signal | Budget | Enforcement |
| --- | --- | --- |
| Public/admin HTTP p95 | ≤ 1000 ms; target ≤ 500 ms | K6 threshold |
| HTTP error rate | < 1%; admin 5xx = 0 | K6 thresholds |
| Public page queries | ≤ 4 commands/request | `PerformanceBudgetTests` |
| Admin collection queries | ≤ 2 commands/request | `PerformanceBudgetTests` |
| Public payload | ≤ 256 KiB compressed response | K6 capture/review |
| Admin collection payload | ≤ 128 KiB compressed response | K6 capture/review |
| Upload | ≤ 5 MiB input; no unbounded buffering; record elapsed cost | media limit + dispatch capture |
| Managed allocations | ≤ 2 MiB/request public feed; ≤ 1 MiB/request admin read | `dotnet-counters` dispatch review |
| GC | Gen2 collections = 0 during smoke; Gen0/Gen1 trend recorded | `dotnet-counters` dispatch review |

The allocation and GC limits are operational budgets for a stable Release smoke
run, not reasons to add `Span<T>` or pooling. A failure requires a trace and
profile before code changes.

## S30.10 measured optimization decision

No production optimization was applied. The baseline already proves that public
reads are asynchronous, no-tracking, projected, bounded, and execute four bounded
queries; admin collections are projected and execute the expected count-plus-page
pair. PostgreSQL execution p95 is below 0.1 ms for the captured feed/index paths,
and the existing projection evidence reports no sequential scan or N+1 behavior.

Consequently, changing query shape, adding an index migration, or applying an
allocation micro-optimization would be speculative. Re-measure with a
production-like dataset before considering an S30.10 change.

## Reproduction and evidence handling

1. Start PostgreSQL 17 and the three Release web hosts using the S30.5 setup.
2. Run the manual [`perf-baseline.yml`](../../.github/workflows/perf-baseline.yml)
   workflow or the two K6 scripts in [`tests/load/README.md`](../../tests/load/README.md:39).
3. Record K6 summary output, response bytes, upload duration/bytes, and
   `dotnet-counters monitor --counters System.Runtime` output as a dated artifact.
4. Run the PostgreSQL integration budget tests. A query-count regression fails the
   test immediately.
5. Repeat with production `Site__PublicBaseUrl` set before using this baseline as
   release evidence for canonical/RSS/sitemap routes.
