# M2 — Reverse Proxy & TLS Termination

## Objective
Stand up the actual internet-facing layer that terminates HTTPS and routes each host to its container, which M1 assumes exists but does not build.

## Background
Nothing in the platform yet defines how Portfolio, Blog, and Admin are actually reachable from the internet with valid TLS certificates.

## Scope

### In scope
- Deploy Caddy as a reverse proxy in front of the Admin, Blog, and Portfolio containers.
- Configure automatic HTTPS certificate issuance and renewal per host.
- Route each configured subdomain to its correct container per M1's host mapping.
- Document the proxy configuration in the deployment guide.

### Out of scope
- The Cloudflare edge layer in front of the proxy (see M13).

## Domain changes
- No new aggregates.

## API changes
- No API changes; this is infrastructure configuration.

## UI/Presentation changes
- No UI changes.

## Non-functional requirements
- Certificate renewal must be fully automatic; no manual renewal step should ever be required.

## Risks & mitigations
- A misconfigured proxy rule could route traffic to the wrong container or expose Admin without TLS.
- Mitigation: an automated smoke test hitting each public host and asserting both the correct backend responds and TLS is valid.

## Definition of Done
- [ ] All three hosts are reachable over HTTPS with valid, auto-renewing certificates.
- [ ] Requests route to the correct container per host.
- [ ] Admin is not reachable without TLS under any configuration.
- [ ] A **malformed** `Site:PublicBaseUrl` no longer silently disables host enforcement. A value
      such as `https//blog.example.com`, or one with no scheme, is currently indistinguishable
      from an unset value: `HostOf` returns `null` for both, the allowlist degrades to `"*"`,
      and no warning is logged — unlike a `Site:Kind` mismatch, which fails startup loudly.
      `docker-compose.yml`'s `${VAR:?...}` checks only that the variable is non-empty, so a
      one-character typo in a production secret turns off M1's entire host-allowlist control
      with no signal. The validator must distinguish *unset* from *unparseable* and fail
      loudly, or at minimum log a warning naming the offending key. This item is owned here
      because it is the same configuration surface and the same fail-loud principle; it was
      previously tracked as a "candidate follow-up" in
      [M1 routing truth](m1-routing-truth.md) with no milestone attached.

## Test plan
- Automated smoke test per host verifying TLS validity and correct backend routing.
- Manual verification of certificate auto-renewal behavior.
