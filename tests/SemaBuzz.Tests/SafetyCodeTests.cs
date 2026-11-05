using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using SemaBuzz.Protocol;
using Xunit;

namespace SemaBuzz.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// Safety codes: both people see the same 20 digits only if each got the other's
// real public key, so a relay or network attacker swapping keys is caught.
// ─────────────────────────────────────────────────────────────────────────────

public class SafetyCodeTests
{
    private static byte[] NewPublicKey()
    {
        using var ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        return ecdh.PublicKey.ExportSubjectPublicKeyInfo();
    }

    [Fact]
    public void BothSidesGetTheSameCode()
    {
        var alice = NewPublicKey();
        var bob = NewPublicKey();
        Assert.Equal(SemaBuzzSafetyCode.Compute(alice, bob), SemaBuzzSafetyCode.Compute(bob, alice));
    }

    [Fact]
    public void CodeIsFourGroupsOfFiveDigits()
    {
        var code = SemaBuzzSafetyCode.Compute(NewPublicKey(), NewPublicKey());
        var groups = code.Split(' ');
        Assert.Equal(SemaBuzzSafetyCode.Groups, groups.Length);
        Assert.All(groups, g =>
        {
            Assert.Equal(SemaBuzzSafetyCode.GroupDigits, g.Length);
            Assert.True(g.All(char.IsAsciiDigit), g);
        });
    }

    /// <summary>
    /// The algorithm is part of the protocol: peers on different versions must still agree.
    /// These values were computed independently; if this test fails, the code has changed and
    /// older peers would show different codes for the same session.
    /// </summary>
    [Fact]
    public void KnownAnswer_AlgorithmHasNotChanged()
    {
        var a = Enumerable.Range(1, 91).Select(i => (byte)i).ToArray();
        var b = Enumerable.Range(100, 91).Select(i => (byte)i).ToArray();
        Assert.Equal("80924 48152 88452 19814", SemaBuzzSafetyCode.Compute(a, b));
        Assert.Equal("80924 48152 88452 19814", SemaBuzzSafetyCode.Compute(b, a));

        // Keys differing only in their last byte still give unrelated codes.
        var c = Enumerable.Repeat((byte)0x30, 91).ToArray();
        var d = Enumerable.Repeat((byte)0x30, 90).Append((byte)0x31).ToArray();
        Assert.Equal("03962 17858 02194 63571", SemaBuzzSafetyCode.Compute(c, d));
    }

    [Fact]
    public void ManInTheMiddle_ShowsDifferentCodes()
    {
        // Alice and Bob each think they're talking to each other, but the relay (Mallory)
        // swapped both keys: Alice computes with Mallory's first key, Bob with Mallory's second.
        var alice = NewPublicKey();
        var bob = NewPublicKey();
        var mallory1 = NewPublicKey();
        var mallory2 = NewPublicKey();

        var aliceSees = SemaBuzzSafetyCode.Compute(alice, mallory1);
        var bobSees = SemaBuzzSafetyCode.Compute(bob, mallory2);
        Assert.NotEqual(aliceSees, bobSees);
        Assert.False(SemaBuzzSafetyCode.Matches(aliceSees, bobSees));

        // Even reusing one key for both sides doesn't help Mallory.
        Assert.NotEqual(SemaBuzzSafetyCode.Compute(alice, mallory1), SemaBuzzSafetyCode.Compute(bob, mallory1));
    }

    [Fact]
    public void Matches_IgnoresSpacingButNotDigits()
    {
        var code = SemaBuzzSafetyCode.Compute(NewPublicKey(), NewPublicKey());
        Assert.True(SemaBuzzSafetyCode.Matches(code, code.Replace(" ", "")));
        Assert.True(SemaBuzzSafetyCode.Matches(code, " " + code.Replace(" ", "  ") + " "));
        var wrong = (code[0] == '9' ? '0' : (char)(code[0] + 1)) + code[1..];
        Assert.False(SemaBuzzSafetyCode.Matches(code, wrong));
        Assert.False(SemaBuzzSafetyCode.Matches(code, code[..10]));
        Assert.False(SemaBuzzSafetyCode.Matches(null, code));
    }

    [Fact]
    public void EmptyKeyIsRejected()
    {
        Assert.Throws<ArgumentException>(() => SemaBuzzSafetyCode.Compute([], NewPublicKey()));
    }
}

/// <summary>
/// A real listener and client talking over UDP on this machine: both must end up showing the
/// same safety code once the wire is secured.
/// </summary>
public class SafetyCodeSessionTests
{
    private static int FreeUdpPort()
    {
        using var probe = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)probe.Client.LocalEndPoint!).Port;
    }

    private static async Task WaitForAsync(Func<bool> condition, TimeSpan timeout)
    {
        var until = DateTime.UtcNow + timeout;
        while (!condition())
        {
            if (DateTime.UtcNow > until) return;
            await Task.Delay(50);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DirectSession_BothEndsShowTheSameCode()
    {
        var port = FreeUdpPort();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var listener = new SemaBuzzListener();
        using var client = new SemaBuzzClient();

        Assert.Null(listener.SafetyCode);
        Assert.Null(client.SafetyCode);

        var listenTask = Task.Run(() => listener.ListenAsync(port, cts.Token));
        await WaitForAsync(() => listener.State == SemaBuzzWireState.Warming, TimeSpan.FromSeconds(5));
        var connectTask = Task.Run(() => client.ConnectAsync("127.0.0.1", port, cts.Token));

        static bool Up(SemaBuzzWireState s) => s is SemaBuzzWireState.Secured or SemaBuzzWireState.Live;
        await WaitForAsync(() => Up(listener.State) && Up(client.State)
                                 && listener.SafetyCode != null && client.SafetyCode != null,
                           TimeSpan.FromSeconds(15));

        Assert.True(Up(client.State), $"client state: {client.State}");
        Assert.True(Up(listener.State), $"listener state: {listener.State}");
        Assert.NotNull(client.SafetyCode);
        Assert.Equal(listener.SafetyCode, client.SafetyCode);
        Assert.True(SemaBuzzSafetyCode.Matches(listener.SafetyCode, client.SafetyCode));

        cts.Cancel();
        try { await Task.WhenAll(listenTask, connectTask).WaitAsync(TimeSpan.FromSeconds(10)); }
        catch (OperationCanceledException) { }
        catch (TimeoutException) { }
    }
}
