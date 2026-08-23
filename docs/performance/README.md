# Performance evidence

- [Phase 6 baseline and budgets](phase6-baseline.md) — S30.9 budgets, query-count
  gates, reproducible K6 capture, and the S30.10 optimization decision.
- [Projection evidence](projection-evidence.md) — generated SQL, query behavior,
  latency samples, and `EXPLAIN` observations.
- [Captured projection SQL](projection-sql.txt) — raw EF Core/Npgsql output.

The evidence is scoped to the current seeded dataset and is not a substitute for
the production-like budgets planned for Phase 6.
