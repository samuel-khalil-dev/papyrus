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

## Review cadence
This map is reviewed whenever a new external dependency is introduced by a milestone - the milestone's author adds a row here as part of that milestone's Definition of Done.
