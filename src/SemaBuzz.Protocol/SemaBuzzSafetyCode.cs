using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace SemaBuzz.Protocol;

/// <summary>
/// A short code both people can compare (out loud, on a call, or in person) to confirm their
/// connection isn't being intercepted.
///
/// Every session uses fresh ECDH keys, and the relay passes those keys along. A dishonest relay,
/// or anyone in the middle of an unencrypted ws:// connection, could swap in its own keys and
/// read everything while both people still see a "secure" connection. The safety code is made
/// from both public keys, so both people see the same code only if each received the other's
/// real key. If the codes differ, someone is in the middle.
///
/// The code is 20 digits (about 66 bits). There's no commitment step in the handshake, so an
/// attacker in the middle could keep generating keys looking for two that happen to give the
/// same code on both sides. At 66 bits that takes around 2^33 key generations per side, which
/// can't be done before the handshake times out.
///
/// The algorithm is part of the protocol: both peers must compute the code the same way, so
/// changing it needs a new <see cref="Version"/> and a way for both sides to agree on it.
/// </summary>
public static class SemaBuzzSafetyCode
{
    /// <summary>Version tag mixed into the hash; part of the wire contract.</summary>
    public const string Version = "SemaBuzz-safety-code-v1";

    /// <summary>Number of 5-digit groups in a code.</summary>
    public const int Groups = 4;

    /// <summary>Digits per group.</summary>
    public const int GroupDigits = 5;

    private const int BytesPerGroup = 5; // 40 bits per group, reduced to 5 digits

    /// <summary>
    /// The safety code for a session between two public keys (SubjectPublicKeyInfo bytes, as
    /// sent in the key exchange). The order of the keys doesn't matter: both peers get the same
    /// code. Formatted as four groups of five digits, e.g. "48213 93750 21661 08427".
    /// </summary>
    public static string Compute(ReadOnlySpan<byte> localPublicKey, ReadOnlySpan<byte> peerPublicKey)
    {
        if (localPublicKey.IsEmpty || peerPublicKey.IsEmpty)
            throw new ArgumentException("Both public keys are needed to compute a safety code.");

        // Sort the two keys so both sides hash them in the same order.
        ReadOnlySpan<byte> first = localPublicKey, second = peerPublicKey;
        if (localPublicKey.SequenceCompareTo(peerPublicKey) > 0)
        {
            first = peerPublicKey;
            second = localPublicKey;
        }

        var tag = Encoding.ASCII.GetBytes(Version);
        var input = new byte[tag.Length + 4 + first.Length + 4 + second.Length];
        var span = input.AsSpan();
        tag.CopyTo(span);
        span = span[tag.Length..];
        BinaryPrimitives.WriteInt32BigEndian(span, first.Length);
        first.CopyTo(span[4..]);
        span = span[(4 + first.Length)..];
        BinaryPrimitives.WriteInt32BigEndian(span, second.Length);
        second.CopyTo(span[4..]);

        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(input, hash);

        var sb = new StringBuilder(Groups * (GroupDigits + 1));
        for (int g = 0; g < Groups; g++)
        {
            ulong chunk = 0;
            for (int i = 0; i < BytesPerGroup; i++)
                chunk = (chunk << 8) | hash[g * BytesPerGroup + i];
            if (g > 0) sb.Append(' ');
            sb.Append((chunk % 100_000).ToString("D5"));
        }
        return sb.ToString();
    }

    /// <summary>
    /// True if two codes are the same, ignoring spaces (for an app that lets someone type the
    /// code the other person reads out).
    /// </summary>
    public static bool Matches(string? a, string? b)
    {
        if (a is null || b is null) return false;
        static string Digits(string s) => new(s.Where(char.IsAsciiDigit).ToArray());
        var da = Digits(a);
        return da.Length == Groups * GroupDigits && da == Digits(b);
    }
}
