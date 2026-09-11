# M13 — Edge & CDN Layer (Cloudflare)

## Objective
Reduce origin VPS load and improve resilience by placing Cloudflare in front of the reverse proxy.

## Background
The reverse proxy (M2) handles TLS/routing but nothing yet caches static assets at the edge or mitigates DDoS/bot traffic before it reaches the VPS.

## Scope

### In scope
- Configure Cloudflare in front of the M2 reverse proxy for all public hosts.
- Enable edge caching for static assets.
- Enable Cloudflare Turnstile for the contact form (M7) and comment widget (M16) as the primary bot-mitigation layer.
- Confirm Admin remains excluded from aggressive edge caching that could serve stale authenticated content.

### Out of scope
- Cloudflare Workers or edge-compute logic (not needed at this platform's scale).

## Domain changes
- No new aggregates.

## API changes
- No API changes.

## UI/Presentation changes
- No visible UI change beyond faster static asset delivery and the Turnstile widget on protected forms.

## Non-functional requirements
- Admin responses must never be edge-cached in a way that could leak one session's content to another.

## Risks & mitigations
- Misconfigured caching rules could serve stale or incorrect content, especially for Admin.
- Mitigation: explicit cache-bypass rules for the Admin host, verified by test.

## Definition of Done
- [ ] Static assets are served from Cloudflare's edge with correct cache headers.
- [ ] Admin responses are confirmed never edge-cached.
- [ ] Turnstile is active on the contact form and comment widget.

## Test plan
- Automated verification that Admin responses carry no-cache headers and are not served from edge cache.
- Manual verification of Turnstile challenge behavior.
