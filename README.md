# SemaBuzz Protocol

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![Contributions: Read Only](https://img.shields.io/badge/Contributions-Read_Only-red.svg)](CONTRIBUTING.md)

The **SemaBuzz Protocol** is a self-contained, pure .NET 9 library that implements a robust peer-to-peer encrypted communication wire. It serves as the core networking, cryptographic, and state management engine for any application.

## ✨ Features

- 🔒 **End-to-End Encryption:** Every session generates ephemeral keys using ECDH P-256 key exchange, with all payloads authenticated and encrypted via AES-256-GCM. 
- ⚡ **True Peer-to-Peer:** Built-in STUN discovery (RFC 5389) and concurrent UDP hole-punching to establish lowest-latency direct connections even behind restrictive NATs.
- ⌨️ **Live-Typing Streaming:** Transmits individual keystrokes along with calculated "intensity" (typing velocity) to provide dynamic, tactile visual feedback without needing discrete message bubbles.
- 🛜 **Relay Fallback:** Seamlessly graceful fallback to WebSocket-based blind relays when direct UDP connections are impossible.
- 📦 **Rich Data Sync:** First-class protocol support for:
  - 🎨 Real-time whiteboard drawing and stroke syncing (SemaBuzzDraw)
  - 📁 File transfers up to 10MB (SHA-256 verified)
  - 👤 Profile metadata exchange (handles, avatar PNGs, statuses)
  - 🔗 URL "card" pushing

## 📖 Core Concepts

Before building with SemaBuzz Protocol, it's helpful to understand two core concepts:

### 1. The Handshake Lifecycle
Connections progress through distinct states explicitly tracked by the protocol (SemaBuzzWireState):
- \Cold\: Network initialized, ready to dial.
- \Warming\: Contacting the peer or negotiating over the relay. STUN and UDP hole-punching happen concurrently here.
- \Secured\: ECDH P-256 key exchange has finished, AES-256-GCM context is established, and encrypted streaming can begin.
- \Dead\: The connection closed or timed out.

### 2. Live-Typing "Intensity"
SemaBuzz does not send whole messages or "user is typing..." indicators. Instead, it streams keystrokes live, one at a time. The \SemaBuzzStreamer\ calculates the velocity of keystrokes locally and attaches an \Intensity\ property (0-255) to every packet on the wire. This allows UI applications to render a tactile "filament" or vibrating response purely based on how fast the end user is typing.

## ⚡ Quick Start

SemaBuzz provides two primary classes: \SemaBuzzListener\ (Host) and \SemaBuzzClient\ (Dialer). Below is a minimal example using a WebSocket Relay fallback to establish a secured connection between two peers.

### 1. The Host (Listener)
`csharp
using var listener = new SemaBuzzListener();

// Auto-accept all incoming connection requests
listener.ConnectionApprovalCallback = (peer) => Task.FromResult(true);

// Bind to event before listening
listener.PacketReceived += (s, e) => Console.Write(e.Packet.Character);

// Start listening passively in a relay room "HELLOW"
_ = listener.ListenViaRelayAsync("wss://your-relay-host/relay", "HELLOW");
`

### 2. The Client (Dialer)
`csharp
using var client = new SemaBuzzClient();

// Connect out to the room "HELLOW"
_ = client.ConnectViaRelayAsync("wss://your-relay-host/relay", "HELLOW");

// Wait for state to become Secured (event: WireStateChanged)
`

### 3. Send Data (Both)
Both the Host and Client can send data symmetrically once the connection is \Secured\:
`csharp
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
`

## 🏗️ Architecture

The protocol is designed as a standalone wrapper over base .NET networking/crypto primitives. It has **zero dependencies** on UI frameworks like WPF or WinUI, making it highly portable to cross-platform environments (macOS, Linux, mobile, or headless bots).

### 🔑 Key Classes

- SemaBuzzClient / SemaBuzzListener: Outbound and inbound entry points.
- SemaBuzzShield: Handles ECDH key generation, shared secret derivation (HKDF-SHA256), and per-packet AES encryption.
- SemaBuzzStreamer: The typing engine that converts text inputs into variable-intensity stream packets.
- SemaBuzzPunchThrough: Orchestrates NAT hole-punch probes to build direct UDP connections.

## 🚀 Building Locally

`ash
git clone https://github.com/skynrlabs/SemaBuzz-Protocol.git
cd SemaBuzz-Protocol
dotnet build
dotnet test
`

> **Tip:** You can run the examples/SemaBuzz.ConsoleDemo project out-of-the-box to test passing live text locally!

## 🤝 Contributing

Since this is a read-only release, we are **not** accepting upstream contributions (Issues or Pull Requests) at this time. However, you are completely free to fork the project and build upon it under the terms of the AGPL v3.0 license. Please review our [Contributing Guidelines](CONTRIBUTING.md).

## ⚖️ License

**SemaBuzz Protocol** is open-source software licensed under the **GNU AGPL v3.0**. 

By using, modifying, or distributing this software, you agree to the terms of the AGPL v3.0, which (among other things) requires that any modified versions and services running the modified software must also be open-sourced under the same license. See the LICENSE file for full details.
