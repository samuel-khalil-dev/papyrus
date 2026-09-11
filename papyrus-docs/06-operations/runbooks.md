# Runbooks

## Failed deployment / failed health check
1. Confirm the failure is genuine.
2. Halt promotion; roll back to the previous pinned image.
3. Investigate via CI and health-check logs before retrying.

## Backup restore drill
1. Provision a non-production target.
2. Restore the most recent backup and verify data integrity.
3. Document the outcome; repeat periodically, not only once.

## Infrastructure resource alert (M5)
1. Identify the resource (CPU/memory/disk) and the responsible container via the monitoring dashboard.
2. For disk-usage alerts specifically, check backup retention and media growth first - the most likely causes.
3. Scale down or clean up the responsible workload; escalate to a VPS upgrade only if the trend, not a one-time spike, justifies it, per ADR-0005.

## AI provider failure or budget cap reached (M25/M27)
1. Confirm whether the active provider profile failed its connection or the budget cap triggered the graceful disable.
2. For a provider failure, use the Admin's pre-configured fallback profile if one exists, after a test-connection pass.
3. For a budget cap, review usage by capability before simply raising the cap - identify what drove the spike.

## Scheduled-publish failure (M17)
1. Check the job's execution log.
2. Manually publish via Admin as an immediate override.
3. Fix the underlying failure before relying on scheduling again.

## Contact form or comment abuse (M7, M16)
1. Confirm Cloudflare Turnstile (M13) is active on the protected forms and correctly configured.
2. Review submission volume against rate limits; tighten if abuse is confirmed.

## GitHub API unavailability (M9, M22)
1. Confirm affected widgets degrade gracefully.
2. No emergency action needed if the fallback functions correctly.
