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

Package-level dependencies in `src/Papyrus.Admin.UI/package.json` that are
declared but have no import anywhere in `src/`. They install and are scanned but
carry no runtime or build behaviour. The Admin UI primitives are hand-rolled
rather than shadcn-generated, which is why the shadcn scaffolding packages are
still declared.

| Package | Why it is unneeded |
|---|---|
| `@radix-ui/react-alert-dialog` | The wrapping `components/ui/alert-dialog.tsx` was unreferenced and has been deleted. |
| `@radix-ui/react-dropdown-menu` | The wrapping `components/ui/dropdown-menu.tsx` was unreferenced and has been deleted. |
| `@radix-ui/react-label` | Nothing imports it; labels are plain `<label>` elements. |
| `@radix-ui/react-select` | `components/ui/select.tsx` renders a native `<select>` on purpose, because the form contract requires `multiple` selection and native mobile controls. |
| `@radix-ui/react-separator` | Nothing imports it; separators are plain elements. |
| `@radix-ui/react-slot` | `components/ui/button.tsx` applies variants by conditional class rather than through `Slot` composition. |
| `@radix-ui/react-tabs` | The wrapping `components/ui/tabs.tsx` was unreferenced and has been deleted. |
| `@radix-ui/react-toast` | Toasts are served by `sonner`; `components/ui/toast.tsx` re-exports it. |
| `class-variance-authority` | No import. Button variant styling is a conditional-class switch in `components/ui/button.tsx`. |

`@radix-ui/react-dialog` is the only Radix package with a live import.

Removing any of these is a dependency-hygiene change, not a functional one, and
it requires a matching `package.json` and `package-lock.json` update. Verify with
`npm ci`, `npm run build`, and `npm run lint` before and after.

## Review cadence
This map is reviewed whenever a new external dependency is introduced by a milestone - the milestone's author adds a row here as part of that milestone's Definition of Done. The declared-but-unused list is re-verified when an Admin UI dependency changes.
