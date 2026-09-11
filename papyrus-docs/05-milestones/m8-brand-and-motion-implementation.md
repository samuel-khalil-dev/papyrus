# M8 — Brand & Motion Implementation

## Objective
Implement the black-and-gold identity and motion language on the Portfolio landing page using self-hosted assets.

## Background
The design system is specified but not yet implemented; fonts must be self-hosted, not loaded from a third-party CDN.

## Scope

### In scope
- Self-host all typefaces (no Google Fonts CDN or equivalent third-party font host).
- Apply design tokens as the default theme across Portfolio and Blog.
- Implement the hero scroll-unroll entrance and scroll-triggered section reveals.
- Implement accent shimmer and card-lift interactions using logical CSS properties throughout.
- Respect prefers-reduced-motion.

### Out of scope
- Visitor-facing theme toggle (M23).
- Admin-editable theme values (M10; this ships the fixed default only).

## Domain changes
- No new aggregates.

## API changes
- No API changes.

## UI/Presentation changes
- Portfolio fully restyled/animated; Blog restyled with the same tokens, without added motion.

## Non-functional requirements
- No third-party font requests should appear in network traffic.
- All animation limited to transform/opacity.

## Risks & mitigations
- Loading fonts from a third-party CDN by default would reintroduce the GDPR exposure this milestone exists to avoid.
- Mitigation: an automated check asserting no font requests leave the origin domain.

## Definition of Done
- [ ] Portfolio matches the design system spec.
- [ ] No network request loads a font from a third-party host.
- [ ] Animations respect prefers-reduced-motion.
- [ ] Lighthouse performance does not regress.

## Test plan
- Automated check for third-party font requests.
- Automated Lighthouse comparison.
- Automated accessibility check.
