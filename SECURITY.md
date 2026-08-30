# Security Policy

Papyrus takes security reports seriously. Please do **not** disclose a suspected
vulnerability in a public issue, pull request, discussion, or other public
channel.

## Reporting a vulnerability

Use the repository's **Security** tab to submit a private vulnerability report
through GitHub Security Advisories when that feature is available. If private
reporting is not enabled, contact the maintainer through a private GitHub
channel before sharing technical details. The project does not publish a
security-reporting email address in this repository.

Include only the information needed to reproduce and assess the issue:

- affected version, commit, component, or deployment surface;
- a concise description of the security impact;
- minimal reproduction steps or a proof of concept; and
- any suggested mitigation, if known.

Never include API keys, passwords, connection strings, private URLs, personal
data, or production data in a report. If sensitive data was included
accidentally, notify the maintainer privately so it can be removed.

## Response and disclosure

The maintainer will acknowledge a report when practical, assess its severity and
affected versions, and coordinate a fix or mitigation with the reporter. Please
allow time for validation and release preparation before making a report public.
Disclosure timing is coordinated case by case; there is no guaranteed response
or remediation SLA.

For the project's security posture and known risks, see the
[security documentation](docs/security/README.md), the
[threat-model summary](docs/security/threat-model-summary.md), and the
[known limitations](docs/project/limitations.md).
