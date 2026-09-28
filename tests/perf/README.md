# Papyrus performance measurement harnesses

These scripts produced the latency and query-plan evidence recorded in
[`docs/history/projection-evidence.md`](../../docs/history/projection-evidence.md).
They are kept in the repository so that the evidence is reproducible: the
document states measured results, and these are the harnesses that generated
them. They are measurement tools, not gates — nothing in CI runs them, and they
make no pass/fail assertion. For thresholded, repeatable load testing use
[`tests/load`](../load/README.md) instead, which is what the `load-smoke` CI job
runs.

## `runtime-latency.sql` — server-side query latency and plans

Runs each representative query family 30 times inside PostgreSQL, timing with
`clock_timestamp()` around the same tenant, soft-delete, live-publication,
ordering, projection-root, and pagination predicates the handlers use, then
prints p50/p95/max per family. It follows that with `EXPLAIN (ANALYZE, BUFFERS)`
for each family.

Driving the queries in the database rather than over HTTP avoids the admin
fixed-window rate limiter, which otherwise returns 429 before 30 samples can be
collected for every family.

```powershell
psql -h localhost -U <user> -d <database> -f tests/perf/runtime-latency.sql
```

The output is archived as
[`docs/history/projection-runtime-output.txt`](../../docs/history/projection-runtime-output.txt).
Re-running it produces a different `EXPLAIN` plan text and different timings;
compare against the archived file rather than overwriting it.

## `http-latency.py` — end-to-end request latency

Measures cold and warm latency for the same eight surfaces over HTTP: the Blog
and Portfolio index and detail pages, and the four paginated admin collection
endpoints. The admin requests carry the development API key.

```powershell
python tests/perf/http-latency.py
```

Ports are the local CI-style defaults (Blog 5075, Portfolio 5024, Admin 5205)
and the URLs are hard-coded, so this harness suits a local run against the three
development hosts. Use `tests/load` for a repeatable, thresholded measurement
with configurable endpoints.

## Scope and limits

Both harnesses were run against a small seeded tenant, so the plans use
tenant/slug indexes with in-memory sorts and the timings are sub-millisecond.
They are evidence that the projections and filters do not introduce a sequential
scan, a buffer read, an N+1, or a cross-tenant join at that dataset size. They
are not evidence about larger tenants, and re-measurement with
production-like data volume is the trigger for revisiting the index strategy
described in `04-engineering-standards/performance-budget.md`.
