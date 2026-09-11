# Environments

| Environment | Purpose | Orchestration |
|---|---|---|
| Local development | Day-to-day development | .NET Aspire AppHost |
| Containerized verification | Pre-release verification | docker-compose |
| Production | Live public deployment | Containers via Docker Compose today; Caddy/Cloudflare planned (M2/M13); self-hosted runner |

## Host topology

**Current:** direct Docker Compose networking, no reverse proxy, no CDN.
**Planned (M2, M13):** Caddy (reverse proxy/TLS) behind Cloudflare (CDN/DDoS/Turnstile), with identical topology shape intended across local, containerized-verification, and production environments once both milestones ship.

## Data
Local/verification environments use disposable, synthetic seed data. Production uses a persistent, backed-up PostgreSQL instance. Seed content is never promoted to production.
