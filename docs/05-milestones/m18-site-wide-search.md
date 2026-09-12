# M18 — Site-Wide Search

## Objective
Provide visitors a way to find relevant posts and projects directly.

## Background
No search capability exists today.

## Scope

### In scope
- Full-text search across posts/projects using native PostgreSQL search.
- Locale-aware indexing (post-M11).
- Search results page/component on both sites.

### Out of scope
- Third-party hosted search services.

## Domain changes
- No new aggregate; a full-text search index added via migration.

## API changes
- New public search endpoint.

## UI/Presentation changes
- Search input component (paired with the M24 command palette) and results page.

## Non-functional requirements
- Queries must return within the performance budget as content volume grows.

## Risks & mitigations
- Native search relevance may be limited.
- Mitigation: acceptable at expected content volume; revisit only if issues are observed.

## Definition of Done
- [ ] A representative query returns relevant published content only.
- [ ] Response time stays within budget.

## Test plan
- Integration test verifying ranking for known queries.
- Test verifying draft content is excluded.
