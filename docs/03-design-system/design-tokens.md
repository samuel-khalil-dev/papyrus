# Design Tokens

## Brand assets

**Current:** the actual source-of-truth brand assets are the committed files in `../assets/`:

- [Papyrus logo](../assets/Papyrus_Logo.svg)
- [Papyrus background](../assets/Papyrus_Background.svg)

Use these real files rather than purely descriptive color-only framing; the color tokens below describe the palette system, not a substitute for the shipped brand artwork.

## Color tokens

| Token | Default | Role |
|---|---|---|
| `--papyrus-void` | `#0B0B0C` | Primary background |
| `--papyrus-obsidian` | `#16151A` | Elevated surface |
| `--papyrus-gold` | `#C9A227` | Primary accent |
| `--papyrus-gold-bright` | `#E8C766` | Hover/glow states |
| `--papyrus-gold-dim` | `#7A6320` | Disabled/secondary accents |
| `--papyrus-parchment` | `#EDE3C8` | Body text on dark |
| `--papyrus-lapis` | `#1F3A5F` | Rare secondary accent |

Gold tokens combined must not exceed roughly 15% of any viewport.

## Typeface delivery
All typefaces are self-hosted (bundled and served from Papyrus's own infrastructure), never loaded from a third-party font CDN, to avoid the GDPR exposure of transmitting visitor IPs to an external font host without consent.

## Spacing scale
An 8px base unit scale (4, 8, 16, 24, 32, 48, 64, 96), consistent across all three frontends.

## Elevation
Two levels only: base and raised, differentiated by background shift and a hairline border, not drop shadows.

## Token governance
Values are editable by the owner through the Admin theme editor within a bounded palette.
