# M6 — SEO & Discoverability

## Objective
Ensure Portfolio and Blog are fully discoverable and produce correct rich previews when shared.

## Background
No sitemap, structured data, or Open Graph metadata exists today.

## Scope

### In scope
- Generate sitemap.xml/robots.txt from published content.
- Add Open Graph/Twitter Card metadata per post/project, server-rendered.
- Add JSON-LD (Person on Portfolio, BlogPosting on Blog).
- Add canonical URLs.
- Explicitly exclude the Admin subdomain from indexing via robots.txt disallow and a page-level noindex on every Admin route.

### Out of scope
- Localized hreflang tags (deferred until after M11).

## Domain changes
- No new aggregates.

## API changes
- No API contract changes.

## UI/Presentation changes
- Shared Razor layout updated to emit metadata blocks and the Admin noindex header.

## Non-functional requirements
- Metadata must be present in the initial server response, not injected client-side.

## Risks & mitigations
- Incomplete metadata across pages, or an accidentally indexable Admin route.
- Mitigation: centralized metadata generation, plus an automated test specifically asserting the Admin subdomain returns noindex on every route.

## Definition of Done
- [ ] sitemap.xml lists every published item and nothing else.
- [ ] Every page includes correct OG/Twitter/canonical metadata.
- [ ] JSON-LD validates against schema.org.
- [ ] Every Admin route returns noindex and is disallowed in robots.txt.
- [ ] Lighthouse SEO score >= 95 on public sites.

## Test plan
- Automated test for sitemap correctness.
- Automated test asserting noindex on all Admin routes.
- Manual rich-preview verification.
