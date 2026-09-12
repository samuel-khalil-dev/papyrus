# M16 — Community Layer

## Objective
Enable reader engagement with minimal backend investment.

## Background
No comment mechanism exists today.

## Scope

### In scope
- Integrate a GitHub Discussions-backed comment widget (giscus), protected by Cloudflare Turnstile at the surrounding page level (M13).

### Out of scope
- Custom-built moderation system.

## Domain changes
- No new aggregates; comments stored externally via GitHub Discussions.

## API changes
- No API changes; client-side embed.

## UI/Presentation changes
- Comment widget at the bottom of the post template, lazy-loaded.

## Non-functional requirements
- Must not block or delay post content rendering.

## Risks & mitigations
- A third-party embed introduces an external dependency.
- Mitigation: lazy-load below the fold; disclose in the privacy policy.

## Definition of Done
- [ ] A published post accepts and displays a comment via GitHub login.
- [ ] No performance degradation beyond budget.

## Test plan
- Manual verification of posting/displaying a comment.
- Automated check that the widget is lazy-loaded.
