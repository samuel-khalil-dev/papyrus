# Performance Budget

## Load testing
k6 defines p95 latency thresholds for key public routes, revisited as real traffic data becomes available.

## Front-end performance
Motion restricted to `transform`/`opacity`. **Planned (M13):** static assets served via the Cloudflare edge cache to reduce origin load on the VPS.

## Infrastructure headroom

**Current:** the 4-core/8GB VPS hosts the application containers and PostgreSQL directly via Docker Compose. **Planned (M2, M5, M13):** a reverse proxy, a lightweight monitoring agent, and the Cloudflare edge layer will add to this footprint — per-container resource limits (M5) are scoped to prevent any of these additions from starving PostgreSQL once they exist.

## Regression policy
A performance regression is a release-blocking defect.
