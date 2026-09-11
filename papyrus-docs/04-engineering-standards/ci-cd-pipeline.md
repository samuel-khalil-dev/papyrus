# CI/CD Pipeline

## Workflows
- `ci.yml` - build, tests, browser-smoke (hard gate).
- `openapi-schema.yml` - contract drift check.
- `perf-baseline.yml` - k6 load-test baseline.
- `release.yml` - builds/pushes GHCR images and deploys via a self-hosted runner with a post-deploy health check. **Planned (M2, M13):** deployment sits behind the reverse-proxy/Cloudflare edge layer.

## Supply-chain hygiene
Docker base images and Actions pinned to immutable digests/SHAs. Dependabot grouped weekly across NuGet/npm/Actions with major bumps blocked by default. Secret scanning (gitleaks) on every push/PR.

## Release flow
Pinned, digest-referenced images deployed via a self-hosted runner with scoped credentials, gated by a post-deploy health check.
