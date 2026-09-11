# Layout Direction (LTR / RTL)

## Principle
Papyrus is designed to present identically under left-to-right and right-to-left locales - RTL is a mirrored presentation of the same system, not a separate design, per ADR-0008.

## Implementation rules
- All spacing, alignment, and positioning in shared components use CSS logical properties (`margin-inline-start`, `padding-inline-end`, `inset-inline-start`, etc.) instead of physical `left`/`right` properties.
- The root `dir` attribute is set from the resolved `Locale.TextDirection` value at render time, server-side, so there is no flash of incorrectly-directed layout.
- Iconography with inherent directionality (arrows, the command-palette caret) is mirrored automatically based on `dir`, using CSS `transform: scaleX(-1)` or direction-aware icon variants where mirroring would misrepresent meaning (e.g. a play button icon is not mirrored).
- The command palette, navigation, and card layouts are explicitly tested under both directions before any RTL locale is marked ready for content.

## Scope boundary
This document defines layout mechanics. Actual Arabic content, translation, and Arabic-specific typography validation are scoped to the milestone that ships Arabic as a live locale, not to the foundational RTL groundwork in M11.
