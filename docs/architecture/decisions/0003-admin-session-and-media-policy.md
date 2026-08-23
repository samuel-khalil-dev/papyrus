# ADR 0003: Harden Admin Sessions and Media

## Status

Superseded for admin authentication by [ADR 0006](0006-retire-admin-razor-pages.md).
The media portions of this decision remain active.

## Decision

Use API keys with constant-time comparison for admin authentication. Reject SVG by
default unless a reviewed sanitizer is introduced; validate decoded content and
serve uploads as non-executable media.

## Consequences

The admin surface has one authentication contract and no server-side browser
session store. Uploads may lose broad format compatibility in exchange for a
clear, supportable threat boundary.
