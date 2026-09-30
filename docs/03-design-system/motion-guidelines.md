# Motion Guidelines

## Defined patterns

| Pattern | Where | behaviour |
|---|---|---|
| Hero entrance | Portfolio, once per session | Horizontal scroll-unroll reveal |
| Section reveal | Portfolio, on scroll | Fade + rise via IntersectionObserver |
| Accent shimmer | Buttons, active nav | Slow gradient sweep on hover |
| Card lift | Project cards, on hover | 4-8px translate with gold-tinted shadow |
| Page transition | Route changes | Cross-fade, 150-250ms |

## Exclusions
No 3D tilt, parallax-heavy, or scroll-jacking effects. No animation on the Blog reading surface beyond the shared transition. No animation on Admin beyond standard UI feedback.

## Direction-awareness
All directional motion (slide-in reveals, card-lift shadow direction) is authored using logical start/end semantics, not literal left/right, so it mirrors correctly under a right-to-left locale without a separate RTL animation set.

## Performance discipline
Every animation uses only `transform`/`opacity` and is validated against the performance budget before merge.
