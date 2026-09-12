# Phase 4 PostgreSQL Projection Evidence

**Evidence date:** 2026-08-25 22:01 UTC  
**Scope:** public Blog/Portfolio projections and versioned Admin collection endpoints  
**Tenant used for generated SQL:** `main`  
**Provider:** PostgreSQL via Npgsql / EF Core 10

## Capture method

The SQL was generated with `IQueryable.ToQueryString()` using the same EF model,
tenant context, filters, ordering, pagination, and projection expressions as the
application. The raw capture is retained in
[`projection-sql.txt`](projection-sql.txt). It is SQL
generation evidence, not database execution evidence.

The application query sources are [`PublicContentQueryHandlers`](../../src/Papyrus.Application/Content/PublicContentQueries.cs:8)
and [`AdminApiEndpoints`](../../src/Papyrus.Api.Admin/AdminApiEndpoints.cs:14).

## Captured SQL evidence

The capture contains the following query families:

| Query | Generated SQL characteristics |
| --- | --- |
| Blog index page | `BlogPosts` projection with tenant/soft-delete/live filters, `ORDER BY COALESCE(PublishedAt, CreatedAt) DESC`, `LIMIT @p2 OFFSET @p`, and one SQL statement containing category/project/tag joins for the DTO projection. |
| Blog featured card | Same live filters, `ORDER BY IsFeatured DESC, COALESCE(PublishedAt, CreatedAt) DESC`, `LIMIT @p`. |
| Blog detail | Projected `BlogPosts` row constrained by tenant, soft-delete, live publication, and slug. |
| Blog public tags | Distinct tag pair subquery, tenant-filtered joins to live posts and non-deleted tags, ordered by name, `LIMIT @p`. |
| Portfolio index page | `Projects` projection with tenant/soft-delete/live filters, `ORDER BY COALESCE(PublishedAt, CreatedAt) DESC`, `LIMIT @p2 OFFSET @p`, and category/tag joins in one SQL statement. |
| Portfolio featured card | Same live filters, featured/date ordering, `LIMIT @p`. |
| Portfolio detail | Projected `Projects` row constrained by tenant, soft-delete, live publication, and slug. |
| Portfolio public tags | Distinct tag pair subquery, tenant-filtered joins to live projects and non-deleted tags, ordered by name, `LIMIT @p`. |
| Admin categories/tags | Narrow DTO projections with tenant/soft-delete filters, deterministic name ordering, `LIMIT @p1 OFFSET @p`. |
| Admin projects/blog posts | Narrow root DTO projections with tenant/soft-delete filters, deterministic featured/date ordering, `LIMIT @p1 OFFSET @p`. |

Representative generated SQL from the raw capture:

```sql
SELECT b0."Id", b0."Slug", b0."Title", b0."Summary", b0."CoverImageUrl",
       b0."IsFeatured", b0."PublishedAt", b0."CreatedAt", ...
FROM (
    SELECT b."TenantId", b."Id", b."CategoryId", b."CoverImageUrl",
           b."CreatedAt", b."IsFeatured", b."ProjectId", b."PublishedAt",
           b."Slug", b."Summary", b."Title",
           COALESCE(b."PublishedAt", b."CreatedAt") AS c
    FROM "BlogPosts" AS b
    WHERE b."TenantId" = @ef_filter__TenantId
      AND NOT (b."IsDeleted")
      AND b."IsPublished"
      AND (b."PublishedAt" IS NULL OR b."PublishedAt" <= @now)
    ORDER BY COALESCE(b."PublishedAt", b."CreatedAt") DESC
    LIMIT @p2 OFFSET @p
) AS b0
LEFT JOIN (...) AS c0 ON b0."TenantId" = c0."TenantId"
                       AND b0."CategoryId" = c0."Id"
LEFT JOIN (...) AS p0 ON b0."TenantId" = p0."TenantId"
                       AND b0."ProjectId" = p0."Id"
LEFT JOIN (...) AS s ON b0."TenantId" = s."TenantId"
                     AND b0."Id" = s."BlogPostId"
ORDER BY b0.c DESC, b0."TenantId", b0."Id", ...;
```

The exact expanded SQL, including all selected DTO columns and relationship
subqueries, is in [`projection-sql.txt`](projection-sql.txt:7).

## Verification results

- **No N+1 in public projections:** public cards use explicit DTO `Select`
  expressions. EF emits one relational command per projection query and folds
  category/project/tag data into joins/subqueries; it does not materialize
  entities and lazily traverse navigations.
- **No N+1 in versioned admin collections:** the collection endpoints now use
  explicit DTO projections and execute one `CountAsync` plus one page query.
  The page query carries `LIMIT/OFFSET`; related data is selected in the DTO
  projection rather than loaded with an `Include` graph.
- **Pagination:** versioned Admin requests clamp page size to 1–100, calculate
  totals server-side, and apply `Skip/Take` before materialization. Legacy `/api`
  behavior remains unchanged.
- **Tenant filtering:** every root query includes the model-level
  `TenantId = @ef_filter__TenantId` predicate. Relationship subqueries in the
  public capture also include the same tenant parameter, so cross-tenant rows
  are not joined.
- **Read-only behavior:** all reviewed read paths use `AsNoTracking()`.
- **Projection translation issue fixed:** the original public tag query applied
  `Distinct()` to a DTO record and then ordered it. PostgreSQL translation could
  not translate that shape. It now distincts an anonymous scalar pair before
  ordering and constructing [`PublicTag`](../../src/Papyrus.Contracts/Content/PublicContentContracts.cs:5).

## Index observations

The applied Phase 3 schema has useful tenant/live/relationship indexes:

- `BlogPosts(TenantId, IsPublished, PublishedAt)` and
  `Projects(TenantId, IsPublished, IsFeatured, PublishedAt)` filtered on
  non-deleted rows;
- tenant + slug unique indexes for detail lookups;
- tenant + category/project indexes;
- tenant + parent/tag indexes for both join tables.

The generated list ordering uses `COALESCE(PublishedAt, CreatedAt)`, which is
not the leading order expression of the current indexes. PostgreSQL may
therefore perform a sort after filtering for larger tenants. Search predicates
Search is now bounded and uses PostgreSQL `ILIKE`; the applied
`20260825220000_SecuritySearchTrigramIndexes` migration adds the corresponding
trigram indexes. Revisit query plans after production-like data volume is
available.

## Latency and query-plan evidence

PostgreSQL was started locally as Docker container `papyrus-postgres` using the
persistent named volume `papyrus-postgres-data`, published on `localhost:5432`,
with database `papyrus` and the documented development credentials. The five
current migrations were applied, and the admin startup seed verified the
`main` tenant with one live project, one live post, one category, and two tags.

The latency harness executed each representative query 30 times inside
PostgreSQL using `clock_timestamp()` around the same tenant, soft-delete, live
publication, ordering, projection-root, and pagination predicates used by the
handlers. These are server-side query execution timings, excluding HTTP and
Razor rendering overhead:

| Query family | p50 (ms) | p95 (ms) | max (ms) | Samples |
| --- | ---: | ---: | ---: | ---: |
| Blog index | 0.009 | 0.073 | 1.983 | 30 |
| Blog detail | 0.008 | 0.049 | 0.641 | 30 |
| Portfolio index | 0.009 | 0.041 | 1.162 | 30 |
| Portfolio detail | 0.007 | 0.016 | 0.336 | 30 |
| Admin categories | 0.008 | 0.022 | 1.117 | 30 |
| Admin tags | 0.009 | 0.115 | 0.763 | 30 |
| Admin projects | 0.009 | 0.020 | 0.815 | 30 |
| Admin blog posts | 0.008 | 0.037 | 0.370 | 30 |

The live HTTP smoke run returned 200 for all public index/detail pages and the
first seven admin collection measurements. The admin API rate limiter returned
429 while the final collection was being driven at 30 consecutive requests;
the PostgreSQL harness above avoids that transport-level limit and contains the
required 30 samples for every query family.

Representative `EXPLAIN (ANALYZE, BUFFERS)` output from the live database:

```text
Blog index: Index Scan using "IX_BlogPosts_TenantId_Slug"; Sort Method: quicksort, Memory: 25kB; Buffers: shared hit=2; Planning Time: 0.167 ms; Execution Time: 0.043 ms
Portfolio index: Index Scan using "IX_Projects_TenantId_Slug"; Sort Method: quicksort, Memory: 25kB; Buffers: shared hit=2; Planning Time: 0.293 ms; Execution Time: 0.065 ms
Blog detail: Index Scan using "IX_BlogPosts_TenantId_Slug" with TenantId = 'main' and Slug = 'building-papyrus-platform-foundation'; Buffers: shared hit=2; Planning Time: 0.114 ms; Execution Time: 0.031 ms
Portfolio detail: Index Scan using "IX_Projects_TenantId_Slug" with TenantId = 'main' and Slug = 'platform-foundation'; Buffers: shared hit=2; Planning Time: 0.089 ms; Execution Time: 0.024 ms
Admin categories: Index Scan using "IX_ContentCategories_TenantId_Slug", Sort by Name; Buffers: shared hit=2; Planning Time: 0.076 ms; Execution Time: 0.030 ms
Admin tags: Index Scan using "IX_ContentTags_TenantId_Slug", Sort by Name; Buffers: shared hit=2; Planning Time: 0.059 ms; Execution Time: 0.031 ms
Admin projects: Index Scan using "IX_Projects_TenantId_Slug", Sort by IsFeatured DESC and COALESCE(...); Buffers: shared hit=2; Planning Time: 0.085 ms; Execution Time: 0.031 ms
Admin blog posts: Index Scan using "IX_BlogPosts_TenantId_Slug", Sort by COALESCE(...); Buffers: shared hit=2; Planning Time: 0.087 ms; Execution Time: 0.025 ms
```

The plans use tenant/slug indexes for the small seeded tenant and perform
in-memory quicksorts for the `COALESCE(PublishedAt, CreatedAt)` and name
ordering expressions. No sequential scan, buffer read, N+1 behavior, or
unexpected cross-tenant join was observed. Larger tenants may benefit from
expression/order-specific indexes and functional/trigram search indexes; no
migration was added because the current plans do not justify a schema change at
this dataset size.

## Change made

The versioned Admin category, tag, project, and blog-post collection handlers
were changed from load-all-then-page behavior to server-side count plus
projected page queries in [`AdminApiEndpoints`](../../src/Papyrus.Api.Admin/AdminApiEndpoints.cs:35).
The public Blog and Portfolio hosts were also corrected to register the
application query handlers in [`Program.cs`](../../src/Papyrus.Web.Blog/Program.cs:1)
and [`Program.cs`](../../src/Papyrus.Web.Portfolio/Program.cs:1), which was
required for the live projection pages to resolve their dependencies. No
migration files, connection strings, secrets, or environment files were
changed.

## Verification status

- [`dotnet build Papyrus.sln`](../../Papyrus.sln) — passed, 0 warnings, 0 errors.
- Filtered [`dotnet test Papyrus.sln`](../../Papyrus.sln) — passed, 70/70
  non-integration tests locally. CI adds the 14 PostgreSQL integration tests
  for 84 unique .NET tests total.

> **Note on date and test count:** this evidence is dated 2026-08-25. The
> documented 70/70 result is the current local non-integration baseline; CI
> separately adds 14 PostgreSQL integration tests.
- PostgreSQL SQL translation harness — passed and produced
  [`projection-sql.txt`](projection-sql.txt:1).
- Runtime PostgreSQL latency and `EXPLAIN (ANALYZE, BUFFERS)` capture — passed
  for all eight query families with 30 server-side samples each.
- Phase 4 projection evidence release gate — **complete for the PostgreSQL
  latency/plan blocker**. Remaining Phase 4 items are outside this scoped task.
