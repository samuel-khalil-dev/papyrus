# Accessibility Standards

## Baseline
WCAG 2.1 Level AA across Portfolio and Blog; Admin held to the same contrast standards.

## Contrast and motion
All token combinations verified against WCAG AA ratios. Every animation has a static equivalent under `prefers-reduced-motion: reduce`.

## Keyboard and screen-reader operability
All interactive elements are keyboard-operable with visible focus states in the brand accent. All images require alt text as a mandatory field.

## Accessibility statement
A published Accessibility Statement page (part of M7) states the standard targeted, known limitations, and a contact path for reporting an issue - aligned with EU accessibility-directive expectations for a publicly operated site.

## Verification
Automated accessibility checks (axe-core) run in the Playwright suite; manual keyboard-only and screen-reader passes occur before any milestone introducing new interactive UI is marked done.
