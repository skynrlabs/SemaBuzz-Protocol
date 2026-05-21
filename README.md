# ⚡ SemaBuzz Protocol

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)](LICENSE)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![Contributions Welcome](https://img.shields.io/badge/Contributions-Welcome-brightgreen.svg)](CONTRIBUTING.md)
[![Discord](https://img.shields.io/badge/Discord-Join%20Community-5865F2?logo=discord&logoColor=white)](https://discord.gg/rJMQ2cfN)

> **Encrypted. Direct. Alive.**  
> A pure .NET 9 library that gives your app a real-time, end-to-end encrypted communication wire — complete with live keystroke streaming, NAT hole-punching, file transfers, and a shared whiteboard. No servers required beyond a lightweight relay for matchmaking.

## ✨ What it does

- 🔒 **End-to-end encryption** — ephemeral ECDH P-256 key exchange + AES-256-GCM on every packet. The relay never sees plaintext. Ever.
- ⚡ **Goes direct when it can** — built-in STUN (RFC 5389) + concurrent UDP hole-punching punches through most NATs for the lowest possible latency.
- ⌨️ **Streams keystrokes, not messages** — characters fly across the wire one at a time with a calculated typing *intensity* (0–255), so the other side can render a live, tactile response as you type.
- 🛜 **Relay fallback** — when UDP is completely blocked, the connection falls back gracefully to WebSocket relay mode. Transparent to your app.
- 📦 **Rich data sync out of the box:**
  - 🎨 Real-time whiteboard strokes (SemaBuzzDraw)
  - 📁 File transfers up to 10 MB (SHA-256 verified on receipt)
  - 👤 Profile metadata — handles, avatars, statuses
  - 🔗 URL card pushing

## 📖 Two things worth knowing first

### 1. The connection lifecycle
Every connection moves through four states (`SemaBuzzWireState`):

| State | What's happening |
|---|---|
| `Cold` | Initialized, ready to dial |
| `Warming` | STUN + hole-punch probes flying, relay negotiation in progress |
| `Secured` | Keys exchanged, AES context live — encrypted streaming begins |
| `Dead` | Connection closed or timed out |

### 2. Intensity — not "user is typing…"
SemaBuzz doesn't send typing indicators. It streams every individual keystroke. The `SemaBuzzStreamer` measures the velocity of each keypress and stamps an `Intensity` byte (0–255) onto the wire packet. Your UI can use that number to drive a glowing filament, a vibrating bar, or anything else — purely from how fast the other person is hammering their keyboard.

## 📦 Install

```
dotnet add package SemaBuzz.Protocol
```

or via the NuGet Package Manager:

```
Install-Package SemaBuzz.Protocol
```

## ⚡ Quick start

Two classes. One connection. Let's go.

> You'll need a relay running locally first — grab [SemaBuzz Relay](https://github.com/skynrlabs/SemaBuzz-Relay) and run it on port 7171.

### The Host (waits for someone to join)
```csharp
using var listener = new SemaBuzzListener();

// Auto-accept all incoming connection requests
listener.ConnectionApprovalCallback = (peer) => Task.FromResult(true);

// Bind to event before listening
listener.PacketReceived += (s, e) => Console.Write(e.Packet.Character);

// Start listening passively in a relay room "HELLO"
_ = listener.ListenViaRelayAsync("ws://localhost:7171/relay", "HELLO");
```

### The Client (dials in)
```csharp
using var client = new SemaBuzzClient();

// Connect out to the room "HELLO"
_ = client.ConnectViaRelayAsync("ws://localhost:7171/relay", "HELLO");

// Wait for state to become Secured (event: WireStateChanged)
```

### Send data (works on both sides once `Secured`)
```csharp
var streamer = new SemaBuzzStreamer();

// Every keystroke generates an encrypted packet on the wire
streamer.PacketReady += async (s, e) => 
{
    if (client.State == SemaBuzzWireState.Secured)
    {
        await client.SendAsync(e.Packet);
    }
};

// Feed raw characters as the user types
streamer.Feed('h');
streamer.Feed('e');
streamer.Feed('l');
```

## 🏗️ Architecture

Zero UI dependencies. Zero framework lock-in. The library wraps base .NET networking and crypto primitives — no NuGet bloat. Drop it into WPF, MAUI, a headless bot, or a terminal app and it just works.

### Key classes

| Class | Role |
|---|---|
| `SemaBuzzClient` / `SemaBuzzListener` | Outbound dialer and inbound listener |
| `SemaBuzzShield` | ECDH key generation, HKDF-SHA256 derivation, per-packet AES-256-GCM |
| `SemaBuzzStreamer` | Converts keystrokes into intensity-stamped wire packets |
| `SemaBuzzPunchThrough` | Orchestrates concurrent UDP hole-punch probes |

## 🚀 Build it yourself

```bash
git clone https://github.com/skynrlabs/SemaBuzz-Protocol.git
cd SemaBuzz-Protocol
dotnet build
dotnet test
```

Then fire up `examples/SemaBuzz.ConsoleDemo` to see live keystroke streaming in action between two local processes.

## 💬 Community

Join the [SemaBuzz Discord](https://discord.gg/rJMQ2cfN) to ask questions, share what you're building, and follow development.

## 🤝 Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for the branch model, coding standards, and PR process.

## ⚖️ License

**SemaBuzz Protocol** is licensed under the [GNU Affero General Public License v3.0](LICENSE). You are free to use, modify, and distribute it under those terms. Any application or service that uses this library must also be released under the AGPL-3.0.
