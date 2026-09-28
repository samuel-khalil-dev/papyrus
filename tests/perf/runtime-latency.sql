DROP TABLE IF EXISTS phase4_latency;
CREATE TEMP TABLE phase4_latency (query_name text, sample_no integer, elapsed_ms double precision);

DO $$
DECLARE
    started timestamptz;
    finished timestamptz;
    i integer;
BEGIN
    FOR i IN 1..30 LOOP
        started := clock_timestamp();
        PERFORM 1 FROM "BlogPosts" b WHERE b."TenantId" = 'main' AND NOT b."IsDeleted" AND b."IsPublished" AND (b."PublishedAt" IS NULL OR b."PublishedAt" <= clock_timestamp()) ORDER BY COALESCE(b."PublishedAt", b."CreatedAt") DESC LIMIT 6;
        finished := clock_timestamp(); INSERT INTO phase4_latency VALUES ('blog-index', i, extract(epoch FROM finished - started) * 1000);
        started := clock_timestamp();
        PERFORM 1 FROM "BlogPosts" b WHERE b."TenantId" = 'main' AND NOT b."IsDeleted" AND b."IsPublished" AND (b."PublishedAt" IS NULL OR b."PublishedAt" <= clock_timestamp()) AND b."Slug" = 'building-papyrus-platform-foundation' LIMIT 1;
        finished := clock_timestamp(); INSERT INTO phase4_latency VALUES ('blog-detail', i, extract(epoch FROM finished - started) * 1000);
        started := clock_timestamp();
        PERFORM 1 FROM "Projects" p WHERE p."TenantId" = 'main' AND NOT p."IsDeleted" AND p."IsPublished" AND (p."PublishedAt" IS NULL OR p."PublishedAt" <= clock_timestamp()) ORDER BY COALESCE(p."PublishedAt", p."CreatedAt") DESC LIMIT 6;
        finished := clock_timestamp(); INSERT INTO phase4_latency VALUES ('portfolio-index', i, extract(epoch FROM finished - started) * 1000);
        started := clock_timestamp();
        PERFORM 1 FROM "Projects" p WHERE p."TenantId" = 'main' AND NOT p."IsDeleted" AND p."IsPublished" AND (p."PublishedAt" IS NULL OR p."PublishedAt" <= clock_timestamp()) AND p."Slug" = 'platform-foundation' LIMIT 1;
        finished := clock_timestamp(); INSERT INTO phase4_latency VALUES ('portfolio-detail', i, extract(epoch FROM finished - started) * 1000);
        started := clock_timestamp();
        PERFORM 1 FROM "ContentCategories" c WHERE c."TenantId" = 'main' AND NOT c."IsDeleted" ORDER BY c."Name" LIMIT 10 OFFSET 0;
        finished := clock_timestamp(); INSERT INTO phase4_latency VALUES ('admin-categories', i, extract(epoch FROM finished - started) * 1000);
        started := clock_timestamp();
        PERFORM 1 FROM "ContentTags" t WHERE t."TenantId" = 'main' AND NOT t."IsDeleted" ORDER BY t."Name" LIMIT 10 OFFSET 0;
        finished := clock_timestamp(); INSERT INTO phase4_latency VALUES ('admin-tags', i, extract(epoch FROM finished - started) * 1000);
        started := clock_timestamp();
        PERFORM 1 FROM "Projects" p WHERE p."TenantId" = 'main' AND NOT p."IsDeleted" ORDER BY p."IsFeatured" DESC, COALESCE(p."PublishedAt", p."CreatedAt") DESC LIMIT 10 OFFSET 0;
        finished := clock_timestamp(); INSERT INTO phase4_latency VALUES ('admin-projects', i, extract(epoch FROM finished - started) * 1000);
        started := clock_timestamp();
        PERFORM 1 FROM "BlogPosts" b WHERE b."TenantId" = 'main' AND NOT b."IsDeleted" ORDER BY COALESCE(b."PublishedAt", b."CreatedAt") DESC LIMIT 10 OFFSET 0;
        finished := clock_timestamp(); INSERT INTO phase4_latency VALUES ('admin-blog-posts', i, extract(epoch FROM finished - started) * 1000);
    END LOOP;
END $$;

SELECT query_name, round(min(elapsed_ms)::numeric, 3) AS min_ms,
       round((percentile_cont(0.5) WITHIN GROUP (ORDER BY elapsed_ms))::numeric, 3) AS p50_ms,
       round((percentile_cont(0.95) WITHIN GROUP (ORDER BY elapsed_ms))::numeric, 3) AS p95_ms,
       round(max(elapsed_ms)::numeric, 3) AS max_ms, count(*) AS samples
FROM phase4_latency GROUP BY query_name ORDER BY query_name;

EXPLAIN (ANALYZE, BUFFERS)
SELECT b."Id", b."Slug", b."Title", b."Summary", b."CoverImageUrl", b."IsFeatured", b."PublishedAt", b."CreatedAt"
FROM "BlogPosts" b
WHERE b."TenantId" = 'main' AND NOT b."IsDeleted" AND b."IsPublished" AND (b."PublishedAt" IS NULL OR b."PublishedAt" <= clock_timestamp())
ORDER BY COALESCE(b."PublishedAt", b."CreatedAt") DESC LIMIT 6 OFFSET 0;

EXPLAIN (ANALYZE, BUFFERS)
SELECT p."Id", p."Slug", p."Title", p."Summary", p."CoverImageUrl", p."IsFeatured", p."PublishedAt", p."CreatedAt"
FROM "Projects" p
WHERE p."TenantId" = 'main' AND NOT p."IsDeleted" AND p."IsPublished" AND (p."PublishedAt" IS NULL OR p."PublishedAt" <= clock_timestamp())
ORDER BY COALESCE(p."PublishedAt", p."CreatedAt") DESC LIMIT 6 OFFSET 0;

EXPLAIN (ANALYZE, BUFFERS)
SELECT b."Id", b."Slug", b."Title", b."Summary", b."ContentMarkdown"
FROM "BlogPosts" b
WHERE b."TenantId" = 'main' AND NOT b."IsDeleted" AND b."IsPublished" AND (b."PublishedAt" IS NULL OR b."PublishedAt" <= clock_timestamp()) AND b."Slug" = 'building-papyrus-platform-foundation' LIMIT 1;

EXPLAIN (ANALYZE, BUFFERS)
SELECT p."Id", p."Slug", p."Title", p."Summary", p."Description"
FROM "Projects" p
WHERE p."TenantId" = 'main' AND NOT p."IsDeleted" AND p."IsPublished" AND (p."PublishedAt" IS NULL OR p."PublishedAt" <= clock_timestamp()) AND p."Slug" = 'platform-foundation' LIMIT 1;

EXPLAIN (ANALYZE, BUFFERS)
SELECT c."Id", c."Slug", c."Name", c."Description", c."IsPublished", c."CreatedAt", c."UpdatedAt"
FROM "ContentCategories" c WHERE c."TenantId" = 'main' AND NOT c."IsDeleted" ORDER BY c."Name" LIMIT 10 OFFSET 0;

EXPLAIN (ANALYZE, BUFFERS)
SELECT t."Id", t."Slug", t."Name", t."CreatedAt", t."UpdatedAt"
FROM "ContentTags" t WHERE t."TenantId" = 'main' AND NOT t."IsDeleted" ORDER BY t."Name" LIMIT 10 OFFSET 0;

EXPLAIN (ANALYZE, BUFFERS)
SELECT p."Id", p."Slug", p."Title", p."Summary", p."Description"
FROM "Projects" p WHERE p."TenantId" = 'main' AND NOT p."IsDeleted"
ORDER BY p."IsFeatured" DESC, COALESCE(p."PublishedAt", p."CreatedAt") DESC LIMIT 10 OFFSET 0;

EXPLAIN (ANALYZE, BUFFERS)
SELECT b."Id", b."Slug", b."Title", b."Summary", b."ContentMarkdown"
FROM "BlogPosts" b WHERE b."TenantId" = 'main' AND NOT b."IsDeleted"
ORDER BY COALESCE(b."PublishedAt", b."CreatedAt") DESC LIMIT 10 OFFSET 0;
