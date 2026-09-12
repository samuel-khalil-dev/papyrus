# ADR-0008: Include right-to-left layout support in the localization foundation from inception

**Status:** Accepted

## Context
Arabic is an intended future locale with direct relevance to the platform's Egyptian/Pharaonic identity and the owner's own background, not a hypothetical possibility. Retrofitting RTL after the design system and all public-facing components ship LTR-only would require touching nearly every component a second time.

## Decision
The `Locale` aggregate carries a text-direction attribute (LTR/RTL) from M11 onward. The design system and all shared layout components (navigation, cards, the command palette) are built using logical CSS properties (start/end rather than left/right) from the start, so a locale's direction attribute can mirror the layout without component-level rework later.

## Consequences
Positive: Arabic (or any future RTL locale) can be added later as a content/translation exercise, not a redesign. Negative: marginally more disciplined CSS authorship required from the first line of the design system; accepted as cheap insurance against an expensive retrofit.
