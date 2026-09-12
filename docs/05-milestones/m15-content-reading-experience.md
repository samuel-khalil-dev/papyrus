# M15 — Content Reading Experience

## Objective
Improve the technical reading experience on the Blog.

## Background
Blog posts render as plain markdown-to-HTML without highlighting, navigation aids, or discovery.

## Scope

### In scope
- Server-rendered syntax highlighting for code blocks.
- Auto-generated table of contents.
- Estimated reading time.
- Related content by shared tags.

### Out of scope
- Client-side-only highlighting (avoided to prevent a flash of unhighlighted code).

## Domain changes
- No new aggregates; related-content queries existing ContentTag relationships.

## API changes
- No API contract changes.

## UI/Presentation changes
- Blog post template updated with TOC, reading time, related content.

## Non-functional requirements
- Highlighting must render server-side.

## Risks & mitigations
- Highlighting library choice affects render performance.
- Mitigation: benchmark and cache rendered HTML per post.

## Definition of Done
- [ ] Code blocks render highlighted with zero client-side flash.
- [ ] Related content shares at least one tag.

## Test plan
- Automated test for highlighting markup in the response.
- Unit test for tag-matching logic.
