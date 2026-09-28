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
There is no automated accessibility scanning today. `tests/e2e/package.json` declares only `@playwright/test` and `@types/node`, no spec imports axe-core, and no CI job runs an accessibility scan, so nothing in `browser-smoke` checks WCAG. Accessibility is verified by a manual keyboard-only and screen-reader pass, which must occur before any milestone introducing new interactive UI is marked done. Automated axe-core scanning in the Playwright suite is a committed M7 deliverable, tracked with the published Accessibility Statement above; until that lands, a WCAG regression is not caught by CI and this document must not be read as claiming otherwise.
