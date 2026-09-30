# Vendor & External Dependency Map

A single reference for every external dependency this platform relies on, what breaks if it becomes unavailable, and the corresponding recovery path.

| Dependency | Role | Impact if unavailable | Recovery path |
|---|---|---|---|
| Contabo VPS | Hosts all containers and the database | Total outage | Restore from off-VPS backups (M4) to a new VPS; DNS repoint |
| Cloudflare | CDN, DDoS mitigation, Turnstile, DNS | Degraded performance, exposed origin, forms lose spam protection | Once M2 ships, Caddy still terminates TLS directly; origin remains reachable, just unprotected until restored |
| Domain registrar | Domain ownership/renewal | Site unreachable at expiry | Renewal reminder tracked outside the platform (calendar), not automated by Papyrus itself |
| OpenRouter (or configured AI provider) | Powers M25/M26 AI features | AI-assist features disable gracefully (per IAiProviderResolver's fallback) | Switch to a pre-configured fallback profile after a test-connection pass |
| Transactional email provider (M14) | Contact form and newsletter delivery | Contact/newsletter features fail silently without monitoring | Deliverability and send failures are logged; a provider swap is a configuration change behind the shared `IEmailSender` port |
| GitHub API | Repository metadata, contribution heatmap, comments (giscus) | Widgets degrade gracefully; comments become temporarily unavailable | No action required unless the fallback itself fails |
| GHCR | Container image registry for releases | New releases cannot be deployed | Existing running containers are unaffected; investigate before the next release only |

## Client-side runtime dependencies

The public Blog and Portfolio hosts serve **no third-party JavaScript**. The
shared design system (`Papyrus.Web.Shared/wwwroot`) is the only stylesheet, and
the only client-side code is the repository's own `papyrus.js` and
`papyrus-theme.js`. Vendored Bootstrap, jQuery, jQuery Validate, and jQuery
Unobtrusive were removed on 2026-09-28; nothing referenced them, and
`papyrus.css` already restated the only classes in use.

| Dependency | Role | Impact if unavailable | Recovery path |
|---|---|---|---|
| Google Fonts (Cinzel, Inter, JetBrains Mono) | Display, body, and mono typefaces, loaded from the CDN in the shared layout and the Admin UI shell | Degraded typography only; the `font-display`, `font-sans`, and `font-mono` token stacks fall back to system fonts and the sites stay fully readable | Self-host the three families under `wwwroot/fonts` and change the `@font-face` declarations in `papyrus.css` and `src/index.css`; no application code changes |

The Admin UI has no runtime CDN dependency: it loads only its own bundle and
`/Papyrus_Logo.svg`.

## Declared but unused packages

**None.** Every package-level dependency in `src/Papyrus.Admin.UI/package.json`
is imported somewhere in `src/`. This was verified on 2026-09-28 by removing the
nine that were not and re-running `npm ci`, `npm run build`, and `npm run lint`;
the production bundle was byte-identical before and after, confirming they were
already tree-shaken rather than merely unreferenced. The lockfile lost 25
packages and 26 KB.

`@radix-ui/react-dialog` is the only Radix package retained, because
`components/ui/dialog.tsx` uses it for focus trapping, Escape handling, and modal
ARIA behaviour, which are correctness requirements rather than styling. The
remaining hand-rolled primitives are built on native elements on purpose:
`components/ui/select.tsx` renders a native `<select>` because the form contract
requires `multiple` selection and native mobile controls, and
`components/ui/button.tsx` applies variants by conditional class rather than
through `Slot` composition.

When a dependency is removed, `package.json` and `package-lock.json` must change
together. Verify with `npm ci --prefix src/Papyrus.Admin.UI`, then
`npm run build` and `npm run lint`, and re-measure the bundle against
`04-engineering-standards/performance-budget.md`. A new package with no import
in `src/` is a defect, not a pending decision.

## Review cadence
This map is reviewed whenever a new external dependency is introduced by a milestone - the milestone's author adds a row here as part of that milestone's Definition of Done. The declared-but-unused check is re-run whenever an Admin UI dependency changes.
