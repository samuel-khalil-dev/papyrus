# ADR-0009: Reverse proxy and edge layer: Caddy behind Cloudflare

**Status:** Accepted

## Context
Multiple subdomains (Portfolio, Blog, Admin) must route to distinct containers with TLS termination, and the VPS (4 CPU, 8GB RAM) benefits from offloading static-asset caching and bot/DDoS mitigation rather than handling it application-side.

## Decision
A lightweight reverse proxy (Caddy, chosen for automatic HTTPS with minimal configuration) terminates TLS and routes by host to the correct container. Cloudflare sits in front of it for CDN caching, DDoS protection, and Turnstile-based bot mitigation on the contact form and comments.

## Consequences
Positive: reduced VPS load, automatic certificate management, better spam/bot protection than a custom solution, and reuse of a tool the owner already operates. Negative: an additional layer to configure and monitor; accepted as standard, low-maintenance infrastructure once set up.
