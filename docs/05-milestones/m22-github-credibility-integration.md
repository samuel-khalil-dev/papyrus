# M22 — GitHub Credibility Integration

## Objective
Reinforce technical credibility by surfacing live GitHub activity and publishing a public API reference.

## Background
An active GitHub presence and a contract-first API are currently invisible to a Portfolio visitor.

## Scope

### In scope
- Display a live GitHub contribution heatmap.
- Display a pinned-repository widget.
- Publish a public, read-only API documentation page.

### Out of scope
- Public write access to the content API.

## Domain changes
- No new aggregates.

## API changes
- A public, documented, read-only API surface.

## UI/Presentation changes
- Portfolio components for the heatmap and repo widget.
- A public API reference page.

## Non-functional requirements
- GitHub fetches must be cached server-side.

## Risks & mitigations
- GitHub unavailability could break the widget.
- Mitigation: server-side caching with a graceful hide-widget fallback.

## Definition of Done
- [ ] Widgets render correctly and degrade gracefully if GitHub is unreachable.
- [ ] The API reference matches the current contract.

## Test plan
- Integration test for fetch-and-cache including the fallback path.
