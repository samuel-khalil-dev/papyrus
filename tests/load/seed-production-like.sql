-- Papyrus production-like performance seed.
--
-- Purpose: O8 release-gate re-measurement requires a production-like dataset
-- rather than the single-post application seed. This script inserts a bounded
-- but representative volume of live content into the `main` tenant so the K6
-- public feed/search and admin collection workloads exercise real pagination,
-- projection, and trigram search paths.
--
-- Usage (CI perf-baseline job, after migrations have been applied by the Admin
-- startup seed):
--   psql "$PAPYRUS_TEST_CONNECTION_STRING" -f tests/load/seed-production-like.sql
--
-- The script is idempotent: it deletes only the rows it inserts (identified by
-- the `perf-seed-` slug prefix) and re-inserts them, so re-running does not
-- accumulate duplicates. It never touches the application's own seed rows.
--
-- No secrets, private URLs, or personal data are present. All content is
-- synthetic and sanitized.

BEGIN;

-- ---------------------------------------------------------------------------
-- 1. Remove any prior perf-seed rows (idempotent re-run).
-- ---------------------------------------------------------------------------
DELETE FROM "BlogPostTags"
 WHERE "BlogPostId" IN (SELECT "Id" FROM "BlogPosts" WHERE "Slug" LIKE 'perf-seed-%');

DELETE FROM "ProjectTags"
 WHERE "ProjectId" IN (SELECT "Id" FROM "Projects" WHERE "Slug" LIKE 'perf-seed-%');

DELETE FROM "BlogPosts" WHERE "Slug" LIKE 'perf-seed-%';
DELETE FROM "Projects"  WHERE "Slug" LIKE 'perf-seed-%';
DELETE FROM "ContentTags"      WHERE "Slug" LIKE 'perf-seed-%';
DELETE FROM "ContentCategories" WHERE "Slug" LIKE 'perf-seed-%';

-- ---------------------------------------------------------------------------
-- 2. Categories (bounded taxonomy).
-- ---------------------------------------------------------------------------
INSERT INTO "ContentCategories" ("Id", "TenantId", "Slug", "Name", "Description", "IsPublished", "CreatedAt")
SELECT gen_random_uuid(), 'main', 'perf-seed-cat-' || n, 'Perf Category ' || n,
       'Synthetic category for performance re-measurement.',
       true, now()
FROM generate_series(1, 8) AS n;

-- ---------------------------------------------------------------------------
-- 3. Tags (bounded taxonomy).
-- ---------------------------------------------------------------------------
INSERT INTO "ContentTags" ("Id", "TenantId", "Slug", "Name", "CreatedAt")
SELECT gen_random_uuid(), 'main', 'perf-seed-tag-' || n, 'Perf Tag ' || n, now()
FROM generate_series(1, 12) AS n;

-- ---------------------------------------------------------------------------
-- 4. Projects (live, featured subset).
-- ---------------------------------------------------------------------------
INSERT INTO "Projects"
    ("Id", "TenantId", "CategoryId", "Slug", "Title", "Summary", "Description",
     "RepositoryUrl", "LiveUrl", "IsFeatured", "IsPublished", "PublishedAt", "CreatedAt")
SELECT gen_random_uuid(), 'main',
       (SELECT "Id" FROM "ContentCategories" WHERE "Slug" = 'perf-seed-cat-' || (1 + (n % 8))),
       'perf-seed-project-' || n,
       'Perf Project ' || n,
       'Synthetic project summary for performance re-measurement.',
       'Synthetic project description body used to exercise projection and rendering.',
       'https://example.invalid/repo/' || n,
       'https://example.invalid/project/' || n,
       (n % 5 = 0),
       true,
       now() - make_interval(days => n),
       now() - make_interval(days => n)
FROM generate_series(1, 200) AS n;

-- ---------------------------------------------------------------------------
-- 5. Blog posts (live, featured subset, searchable body).
-- ---------------------------------------------------------------------------
INSERT INTO "BlogPosts"
    ("Id", "TenantId", "ProjectId", "CategoryId", "Slug", "Title", "Summary",
     "ContentMarkdown", "CoverImageUrl", "IsFeatured", "IsPublished", "PublishedAt",
     "CreatedAt", "CreatedBy")
SELECT gen_random_uuid(), 'main',
       (SELECT "Id" FROM "Projects" WHERE "Slug" = 'perf-seed-project-' || (1 + (n % 200))),
       (SELECT "Id" FROM "ContentCategories" WHERE "Slug" = 'perf-seed-cat-' || (1 + (n % 8))),
       'perf-seed-post-' || n,
       'Perf Post ' || n,
       'Synthetic post summary for performance re-measurement.',
       '# Perf Post ' || n || E'\n\n' ||
       'This is a synthetic body used to exercise the trigram search and ' ||
       'markdown rendering paths. It repeats the keyword papyrus and other ' ||
       'common terms so the public search workload has real matches.',
       NULL,
       (n % 5 = 0),
       true,
       now() - make_interval(days => n),
       now() - make_interval(days => n),
       'perf-seed'
FROM generate_series(1, 2000) AS n;

-- ---------------------------------------------------------------------------
-- 6. Project <-> tag links (bounded fan-out).
-- ---------------------------------------------------------------------------
INSERT INTO "ProjectTags" ("ProjectId", "TagId", "TenantId")
SELECT p."Id", t."Id", 'main'
FROM "Projects" p
JOIN "ContentTags" t ON t."Slug" = 'perf-seed-tag-' || (1 + (abs(hashtext(p."Id"::text)) % 12))
WHERE p."Slug" LIKE 'perf-seed-%';

-- ---------------------------------------------------------------------------
-- 7. Blog post <-> tag links (bounded fan-out).
-- ---------------------------------------------------------------------------
INSERT INTO "BlogPostTags" ("BlogPostId", "TagId", "TenantId")
SELECT b."Id", t."Id", 'main'
FROM "BlogPosts" b
JOIN "ContentTags" t ON t."Slug" = 'perf-seed-tag-' || (1 + (abs(hashtext(b."Id"::text)) % 12))
WHERE b."Slug" LIKE 'perf-seed-%';

COMMIT;