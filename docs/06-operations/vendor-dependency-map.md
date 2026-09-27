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

## Declared but unused packages

Package-level dependencies in `src/Papyrus.Admin.UI/package.json` that are
declared but have no import anywhere in `src/`. They install and are scanned but
carry no runtime or build behaviour. The Admin UI primitives are hand-rolled
rather than shadcn-generated, which is why the shadcn scaffolding packages are
still declared.

| Package | Why it is unneeded |
|---|---|
| `@radix-ui/react-label` | `components/ui/label.tsx` renders a plain `<label>`; it does not wrap the Radix primitive. |
| `@radix-ui/react-select` | `components/ui/select.tsx` renders a native `<select>` on purpose, because the form contract requires `multiple` selection and native mobile controls. |
| `@radix-ui/react-separator` | `components/ui/separator.tsx` renders a plain `<div role="separator">`. |
| `@radix-ui/react-slot` | `components/ui/button.tsx` applies variants by conditional class rather than through `Slot` composition. |
| `@radix-ui/react-toast` | Toasts are served by `sonner`; `components/ui/toast.tsx` re-exports it. |
| `class-variance-authority` | No import. Button variant styling is a conditional-class switch in `components/ui/button.tsx`. |

Three local components are also unreferenced and can be deleted or adopted on
demand: `components/ui/badge.tsx` (the status badge in use is
`components/shared/status-badge.tsx`), `components/ui/table.tsx`, and
`components/ui/skeleton.tsx`.

Removing these is a dependency-hygiene change, not a functional one. Verify with
`npm ci`, `npm run build`, and `npm run lint` before and after.

## Review cadence
This map is reviewed whenever a new external dependency is introduced by a milestone - the milestone's author adds a row here as part of that milestone's Definition of Done. The declared-but-unused list is re-verified when an Admin UI dependency changes.
