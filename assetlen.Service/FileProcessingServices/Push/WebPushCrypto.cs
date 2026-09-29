using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace assetlen.Service.FileProcessingServices.Push;

/// <summary>
/// Web Push message encryption (RFC 8291, <c>aes128gcm</c>) and VAPID signing
/// (RFC 8292), on the framework's own P-256, HKDF and AES-GCM. A push service
/// only ever sees ciphertext; only the subscribed browser can read it.
/// </summary>
public static class WebPushCrypto
{
    private const int RecordSize = 4096;

    public static string B64Url(ReadOnlySpan<byte> bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static byte[] FromB64Url(string s)
    {
        var t = s.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(t.PadRight(t.Length + (4 - t.Length % 4) % 4, '='));
    }

    /// <summary>The uncompressed point (0x04 ‖ X ‖ Y) browsers exchange keys as.</summary>
    public static byte[] PublicPoint(ECParameters p)
    {
        var point = new byte[65];
        point[0] = 0x04;
        p.Q.X!.CopyTo(point, 1);
        p.Q.Y!.CopyTo(point, 33);
        return point;
    }

    public static ECParameters FromPoint(byte[] point) => new()
    {
        Curve = ECCurve.NamedCurves.nistP256,
        Q = new ECPoint { X = point[1..33], Y = point[33..65] }
    };

    /// <summary>Encrypt a payload for one subscription. Returns the whole request body.</summary>
    public static byte[] Encrypt(byte[] plaintext, byte[] uaPublic, byte[] authSecret)
    {
        using var asKey = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var asPublic = PublicPoint(asKey.ExportParameters(false));

        using var ua = ECDiffieHellman.Create(FromPoint(uaPublic));
        var ecdhSecret = asKey.DeriveRawSecretAgreement(ua.PublicKey);

        var salt = RandomNumberGenerator.GetBytes(16);
        var (cek, nonce) = DeriveKeys(ecdhSecret, authSecret, uaPublic, asPublic, salt);

        // One record: the payload, then the 0x02 last-record delimiter.
        var padded = new byte[plaintext.Length + 1];
        plaintext.CopyTo(padded, 0);
        padded[^1] = 0x02;

        var cipher = new byte[padded.Length];
        var tag = new byte[16];
        using (var gcm = new AesGcm(cek, 16)) gcm.Encrypt(nonce, padded, cipher, tag);

        var body = new byte[16 + 4 + 1 + asPublic.Length + cipher.Length + tag.Length];
        var o = 0;
        salt.CopyTo(body, o); o += 16;
        BinaryPrimitives.WriteUInt32BigEndian(body.AsSpan(o, 4), RecordSize); o += 4;
        body[o++] = (byte)asPublic.Length;
        asPublic.CopyTo(body, o); o += asPublic.Length;
        cipher.CopyTo(body, o); o += cipher.Length;
        tag.CopyTo(body, o);
        return body;
    }

    /// <summary>The browser's half. Used by the development sink to prove a round trip.</summary>
    public static byte[] Decrypt(byte[] body, ECDiffieHellman uaKey, byte[] authSecret)
    {
        var salt = body[..16];
        var idLen = body[20];
        var asPublic = body[21..(21 + idLen)];
        var rest = body[(21 + idLen)..];
        var cipher = rest[..^16];
        var tag = rest[^16..];

        var uaPublic = PublicPoint(uaKey.ExportParameters(false));
        using var asKey = ECDiffieHellman.Create(FromPoint(asPublic));
        var ecdhSecret = uaKey.DeriveRawSecretAgreement(asKey.PublicKey);
        var (cek, nonce) = DeriveKeys(ecdhSecret, authSecret, uaPublic, asPublic, salt);

        var padded = new byte[cipher.Length];
        using (var gcm = new AesGcm(cek, 16)) gcm.Decrypt(nonce, cipher, tag, padded);
        var end = Array.LastIndexOf(padded, (byte)0x02);
        return padded[..end];
    }

    private static (byte[] Cek, byte[] Nonce) DeriveKeys(byte[] ecdhSecret, byte[] authSecret, byte[] uaPublic, byte[] asPublic, byte[] salt)
    {
        var keyInfo = Concat(Encoding.ASCII.GetBytes("WebPush: info\0"), uaPublic, asPublic);
        var ikm = HKDF.DeriveKey(HashAlgorithmName.SHA256, ecdhSecret, 32, authSecret, keyInfo);
        var cek = HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, 16, salt, Encoding.ASCII.GetBytes("Content-Encoding: aes128gcm\0"));
        var nonce = HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, 12, salt, Encoding.ASCII.GetBytes("Content-Encoding: nonce\0"));
        return (cek, nonce);
    }

    /// <summary>The <c>Authorization: vapid t=…, k=…</c> header for one push service origin.</summary>
    public static string VapidHeader(string endpoint, ECDsa signingKey, string publicKeyB64, string subject)
    {
        var uri = new Uri(endpoint);
        var audience = $"{uri.Scheme}://{uri.Authority}";
        var header = B64Url(JsonSerializer.SerializeToUtf8Bytes(new { typ = "JWT", alg = "ES256" }));
        var claims = B64Url(JsonSerializer.SerializeToUtf8Bytes(new
        {
            aud = audience,
            exp = DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeSeconds(),
            sub = subject
        }));
        var signed = $"{header}.{claims}";
        var signature = signingKey.SignData(Encoding.ASCII.GetBytes(signed), HashAlgorithmName.SHA256);
        return $"vapid t={signed}.{B64Url(signature)}, k={publicKeyB64}";
    }

    private static byte[] Concat(params byte[][] parts)
    {
        var result = new byte[parts.Sum(p => p.Length)];
        var o = 0;
        foreach (var p in parts) { p.CopyTo(result, o); o += p.Length; }
        return result;
    }
}
