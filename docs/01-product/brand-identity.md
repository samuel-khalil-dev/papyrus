# Brand Identity

## Narrative
The Pharaonic identity is expressed through restraint and material cues. Black and gold represent permanence and value; the papyrus scroll represents durable authorship.

## Identity principles
1. Restraint over decoration - gold never exceeds roughly 15% of any viewport.
2. Legibility over ornament - ornamental serif type confined to headings and the logo lockup.
3. Motion with purpose - every animation communicates state or hierarchy.
4. One identity, two moods - a fixed admin-controlled base theme, with an optional visitor-facing Day/Night variation referencing Ra and Nut.
5. One identity, multiple directions - the same identity holds in both left-to-right and right-to-left layouts; RTL is a mirrored presentation of the same system, not a separate design.

## Typeface licensing

The intended posture is self-hosted typefaces rather than a third-party font CDN,
so that a visitor's IP address is not transmitted to an external party without
consent — a real GDPR exposure for EU-based sites that dynamically load fonts
from a vendor's servers.

**That posture is not implemented.** No `@font-face` rule and no font file is
tracked anywhere in the repository. Cinzel, Inter, and JetBrains Mono are loaded
at runtime from `fonts.googleapis.com` by the shared public layout and by the
Admin UI shell, and `fonts.gstatic.com` and `fonts.googleapis.com` are
allow-listed in the Content-Security-Policy for exactly that purpose. The
typefaces are used under their SIL Open Font License terms.

Self-hosting the three families, together with the milestone's third-party
font-request check, is tracked as open work under
[`05-milestones/m8-brand-and-motion-implementation.md`](../05-milestones/m8-brand-and-motion-implementation.md).
Until it lands, the CDN load is the actual and documented runtime dependency, and
`06-operations/vendor-dependency-map.md` is the record of it.

## Logo and iconography
Primary lockup: wordmark in the display serif, gold on black. Icon accents drawn from hieroglyphic silhouettes are used only as small decorative marks, never as primary navigational icons.

### Asset sources and required copies

The brand SVGs exist as byte-identical copies in three places because they are
served from three roots that cannot reference each other: the documentation, the
Admin SPA bundle, and the public hosts' shared static web assets. The copies are
structural, not accidental, but they have no sync mechanism, so a brand change is
currently a three-file edit per asset.

| Asset | Canonical source | Required copies | Consumed by |
|---|---|---|---|
| `Papyrus_Logo.svg` | `src/Papyrus.Web.Shared/wwwroot/images/Papyrus_Logo.svg` | `docs/assets/`, `src/Papyrus.Admin.UI/public/` | The shared layout's `<link rel="icon">` and hero mark; the Admin UI shell's favicon; the documentation |
| `Papyrus_Background.svg` | `src/Papyrus.Web.Shared/wwwroot/images/Papyrus_Background.svg` | `docs/assets/`, `src/Papyrus.Admin.UI/src/assets/` | `papyrus.css` and `index.css` `body::before`, which reference the copy in their own project, not the canonical file |
| `favicon.ico` | `src/Papyrus.Web.Blog/wwwroot/favicon.ico` | `src/Papyrus.Web.Portfolio/wwwroot/favicon.ico` | The shared layout's alternate-icon link on both public hosts |

The Admin UI copy of the logo lives in `public/` because Vite copies that
directory to `dist/` verbatim, whereas the background lives in `src/assets/`
because the bundler must fingerprint and inline it through `index.css`.

Verify that a brand change is complete by confirming the copies still match:
the three logo copies and the three background copies are identical git blobs,
so `git ls-files -s` reports the same object id for each set.
