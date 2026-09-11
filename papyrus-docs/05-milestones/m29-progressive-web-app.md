# M29 — Progressive Web App

## Objective
Allow visitors to install the Blog and read previously visited content offline.

## Background
Neither public site currently offers installability or offline capability.

## Scope

### In scope
- Add a web app manifest and icon set matching the active theme.
- Implement a service worker for offline access to visited posts.
- Ensure manifest theme-color reflects the active theme.

### Out of scope
- Offline authoring or offline Admin functionality.

## Domain changes
- No new aggregates.

## API changes
- No new API surface.

## UI/Presentation changes
- Manifest, icon assets, service-worker registration.

## Non-functional requirements
- Caching must not serve stale content indefinitely.

## Risks & mitigations
- Aggressive caching could show outdated content unnoticed.
- Mitigation: a defined invalidation strategy tied to content updates.

## Definition of Done
- [ ] The Blog is installable and a previously visited post remains readable offline.

## Test plan
- Automated Lighthouse PWA audit.
- Manual offline-reading verification.
