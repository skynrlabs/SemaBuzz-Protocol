# SemaBuzz Protocol

The **SemaBuzz Protocol** is a self-contained, pure .NET 9 library that implements a robust peer-to-peer encrypted communication wire. It serves as the core networking, cryptographic, and state management engine for any application.

## Features

- **End-to-End Encryption:** Every session generates ephemeral keys using ECDH P-256 key exchange, with all payloads authenticated and encrypted via AES-256-GCM. 
- **True Peer-to-Peer:** Built-in STUN discovery (RFC 5389) and concurrent UDP hole-punching to establish lowest-latency direct connections even behind restrictive NATs.
- **Live-Typing Streaming:** Transmits individual keystrokes along with calculated "intensity" (typing velocity) to provide dynamic, tactile visual feedback without needing discrete message bubbles.
- **Relay Fallback:** Seamlessly graceful fallback to WebSocket-based blind relays when direct UDP connections are impossible.
- **Rich Data Sync:** First-class protocol support for:
  - Real-time whiteboard drawing and stroke syncing (`SemaBuzzDraw`)
  - File transfers up to 10MB (SHA-256 verified)
  - Profile metadata exchange (handles, avatar PNGs, statuses)
  - URL "card" pushing

## Architecture

The protocol is designed as a standalone wrapper over base .NET networking/crypto primitives. It has **zero dependencies** on UI frameworks like WPF or WinUI, making it highly portable to cross-platform environments (macOS, Linux, mobile, or headless bots).

### Key Classes

- `SemaBuzzClient` / `SemaBuzzListener`: Outbound and inbound entry points.
- `SemaBuzzShield`: Handles ECDH key generation, shared secret derivation (HKDF-SHA256), and per-packet AES encryption.
- `SemaBuzzStreamer`: The typing engine that converts text inputs into variable-intensity stream packets.
- `SemaBuzzPunchThrough`: Orchestrates NAT hole-punch probes to build direct UDP connections.

## Building Locally

```bash
git clone https://github.com/skynrlabs/SemaBuzz-Protocol.git
cd SemaBuzz-Protocol
dotnet build
dotnet test
```

## Contributing

We welcome contributions to the SemaBuzz Protocol engine! Before submitting a pull request, please review our [Contributing Guidelines](CONTRIBUTING.md).

## License

**SemaBuzz Protocol** is open-source software licensed under the **GNU AGPL v3.0**. 

By using, modifying, or distributing this software, you agree to the terms of the AGPL v3.0, which (among other things) requires that any modified versions and services running the modified software must also be open-sourced under the same license. See the `LICENSE` file for full details.