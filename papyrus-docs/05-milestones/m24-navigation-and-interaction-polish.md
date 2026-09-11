# M24 — Navigation & Interaction Polish

## Objective
Add a command-palette style quick-navigation pattern.

## Background
As content volume grows, conventional browsing becomes less efficient.

## Scope

### In scope
- Implement a keyboard-invoked command palette.
- Integrate M18 search as the palette's primary input.
- Style the palette to the brand identity, mirrored correctly under RTL per the layout-direction standard.

### Out of scope
- A general-purpose action system beyond navigation and search.

## Domain changes
- No new aggregates.

## API changes
- No new API surface beyond M18.

## UI/Presentation changes
- Command palette component on both public sites.

## Non-functional requirements
- Must be fully keyboard-operable without incorrect focus trapping; must mirror correctly under RTL.

## Risks & mitigations
- A poor focus trap could harm accessibility.
- Mitigation: explicit keyboard-only and screen-reader testing, plus an RTL rendering check.

## Definition of Done
- [ ] The palette opens/closes via shortcut, is fully keyboard-operable, and mirrors correctly under a synthetic RTL locale.

## Test plan
- End-to-end test for open/search/navigate.
- Automated accessibility and RTL-mirroring checks.
