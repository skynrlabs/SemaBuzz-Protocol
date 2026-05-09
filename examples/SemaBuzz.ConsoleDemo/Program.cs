using System;
using System.Threading.Tasks;
using SemaBuzz.Protocol;

namespace SemaBuzz.ConsoleDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== SemaBuzz Protocol Demo ===");
            Console.WriteLine("This demo uses a public relay for easy local testing.");
            
            // For testing, you'd typically run your own local relay (e.g. ws://localhost:7171/relay)
            // from the open-source SemaBuzz Relay project.
            string relayUri = "wss://relay.semabuzz.me/relay";
            string token = "DEMO99";

            Console.WriteLine("Press 'H' to Host, or 'C' to Connect as Client:");
            var key = Console.ReadKey(intercept: true).Key;
            Console.WriteLine();

            if (key == ConsoleKey.H)
            {
                await RunHostAsync(relayUri, token);
            }
            else if (key == ConsoleKey.C)
            {
                await RunClientAsync(relayUri, token);
            }
            else
            {
                Console.WriteLine("Exiting.");
            }
        }

        static async Task RunHostAsync(string relayUri, string token)
        {
            Console.WriteLine($"Starting Host. Waiting for peer in room '{token}'...");
            using var listener = new SemaBuzzListener();
            
            listener.WireStateChanged += (s, e) => Console.WriteLine($"\n-> Host State: {e.State} ({e.Message})");
            listener.PacketReceived += (s, e) => Console.Write(e.Packet.Character);

            // Accept all connections
            listener.ConnectionApprovalCallback = (peer) => Task.FromResult(true);

            _ = listener.ListenViaRelayAsync(relayUri, token);

            Console.WriteLine("Type messages below. Press Enter for newline. Press ESC to quit.");
            var streamer = new SemaBuzzStreamer();
            
            // Link local typing to outbound wire
            streamer.PacketReady += async (s, e) => 
            {
                if (listener.State == SemaBuzzWireState.Secured)
                {
                    await listener.SendAsync(e.Packet);
                }
            };

            await HandleTypingLoop(streamer);
        }

        static async Task RunClientAsync(string relayUri, string token)
        {
            Console.WriteLine($"Starting Client. Dialing room '{token}'...");
            using var client = new SemaBuzzClient();
            
            client.WireStateChanged += (s, e) => Console.WriteLine($"\n-> Client State: {e.State} ({e.Message})");
            client.PacketReceived += (s, e) => Console.Write(e.Packet.Character);

            _ = client.ConnectViaRelayAsync(relayUri, token);

            Console.WriteLine("Type messages below. Press Enter for newline. Press ESC to quit.");
            var streamer = new SemaBuzzStreamer();
            
            streamer.PacketReady += async (s, e) => 
            {
                if (client.State == SemaBuzzWireState.Secured)
                {
                    await client.SendAsync(e.Packet);
                }
            };

            await HandleTypingLoop(streamer);
        }

        static async Task HandleTypingLoop(SemaBuzzStreamer streamer)
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(intercept: true);
                    if (k.Key == ConsoleKey.Escape) break;
                    
                    if (k.Key == ConsoleKey.Enter) 
                    {
                        Console.WriteLine();
                        streamer.Feed('\n');
                    }
                    else if (k.Key == ConsoleKey.Backspace)
                    {
                        Console.Write(" \b"); // terminal visual fix
                        streamer.Feed('\b');
                    }
                    else if (!char.IsControl(k.KeyChar))
                    {
                        Console.Write(k.KeyChar);
                        streamer.Feed(k.KeyChar);
                    }
                }
                await Task.Delay(10);
            }
        }
    }
}