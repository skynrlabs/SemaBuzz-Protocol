# Security Policy

## Supported Versions

| Version | Supported |
|---|---|
| Latest release on `main` | ✅ |
| Older releases | ❌ — please update |

---

## Reporting a Vulnerability

**Do not open a public GitHub issue for security vulnerabilities.**

Please report vulnerabilities privately via GitHub's built-in security advisory system:

1. Go to the [Security tab](https://github.com/skynrlabs/SemaBuzz-Protocol/security/advisories) of this repository.
2. Click **"Report a vulnerability"**.
3. Fill in the details — include steps to reproduce, affected component, and potential impact.

You will receive an acknowledgement within **5 business days**. We aim to triage and respond with a remediation plan within **14 days** of receiving a valid report.

---

## Scope

The following are in scope for security reports:

- **Encryption** — ECDH P-256 key exchange, HKDF derivation, AES-256-GCM packet integrity
- **Wire protocol** — packet framing, token handling, replay attacks
- **Relay client** — WebSocket connection handling, relay fallback logic

The following are **out of scope**:

- Vulnerabilities in third-party dependencies (report those upstream)
- Issues requiring physical access to the device

---

## Security Design

SemaBuzz Protocol is designed with the following guarantees:

- **End-to-end encryption.** All payloads are encrypted on-device with ephemeral ECDH P-256 key exchange and AES-256-GCM before transmission. An honest relay cannot read message content.
- **Key verification.** The key exchange is not authenticated by itself: a relay that is dishonest, or an attacker on an unencrypted `ws://` connection, could substitute its own keys (a man-in-the-middle attack). Each session therefore has a 20-digit safety code (`SemaBuzzSafetyCode`) made from both public keys. If both people compare it and it matches, no keys were substituted. The code is about 66 bits so it can't be brute-forced by generating keys during the handshake; a future version could add a commitment step to allow a shorter code.
- **Ephemeral keys.** Keys are generated fresh for every session and never persisted.
- **Blind relay.** The relay server is a pass-through — it never reads, logs, or stores message content.
