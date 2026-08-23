# Papyrus focused K6 load tests

These scripts implement Phase 6 subtask S30.5. They are short, thresholded
smoke tests for a Postgres-backed Papyrus deployment, not soak tests. The
scenarios complete in about 64 seconds and are intended to remain below the
plan's two-minute PR-sized slot.

## Scenarios and thresholds

[`k6/public-listing.js`](k6/public-listing.js) is the primary load target:

- `public_feed`: four VUs for 20 seconds, alternating paginated Blog and
  Portfolio index requests.
- `public_search`: four VUs for 20 seconds, starting after the feed workload,
  alternating paginated `q=papyrus` requests on both public sites.
- `http_req_duration`: p95 below 1000 ms.
- `http_req_failed`: below 1%; all checks must pass at least 99% of the time.

[`k6/admin-write.js`](k6/admin-write.js) covers the authenticated admin paths:

- `admin_bounded`: one request per second for 40 seconds, alternating the
  paginated blog-post read and blog-post create endpoints.
- `admin_limiter_probe`: four requests per second for 22 seconds to exercise
  the fixed-window limiter without treating an expected 429 as a server error.
- p95 below 1000 ms, failed requests below 1%, checks above 99%, and zero 5xx
  responses. `admin_rate_limited` records expected 429 responses.

The admin workload deliberately uses one API key and one tenant. Authentication
is process-local and the limiter is keyed by source IP; it does not model
multiple keys, operators, or replicas. The write workload creates unpublished
test posts and should be run against a disposable or reset database.

`Site__PublicBaseUrl` is not required by these requests and is intentionally not
set by the CI smoke job. Consequently, this test measures relative page/feed
request performance only; it does not establish production absolute canonical,
RSS, or sitemap URL behavior. Re-run with the production deployment setting
when measuring those URLs.

## Local run

Start the Admin, Blog, and Portfolio applications against PostgreSQL 17, then
run each script separately from the repository root. K6 is available from
<https://grafana.com/docs/k6/latest/get-started/installation/>.

```powershell
k6 run -e PAPYRUS_LOAD_BLOG_URL=http://127.0.0.1:5075 -e PAPYRUS_LOAD_PORTFOLIO_URL=http://127.0.0.1:5024 tests/load/k6/public-listing.js
k6 run -e PAPYRUS_LOAD_ADMIN_URL=http://127.0.0.1:5205 -e PAPYRUS_LOAD_ADMIN_KEY=dev-admin-key -e PAPYRUS_LOAD_TENANT_ID=main tests/load/k6/admin-write.js
```

The defaults match the local CI-style ports. Override URLs and the API key for
Aspire or another Postgres-backed environment; never commit a real key.

## CI

The `load-smoke` job in [`.github/workflows/ci.yml`](../../.github/workflows/ci.yml)
starts the three web hosts against PostgreSQL 17, installs K6 using the pinned
Grafana action, and runs both scripts. It is a short thresholded smoke job and
does not run a soak or multi-replica test.
