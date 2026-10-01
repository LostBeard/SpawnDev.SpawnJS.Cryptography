using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpawnDev.SpawnJS.Cryptography;
using SpawnDev.SpawnJS.Cryptography.DotNet;

namespace PlaywrightTestRunner
{
    /// <summary>
    /// The .NET half of the browser's CrossPlatform_* tests (BrowserWasmDemo/Pages/WasmUnitTests.razor).
    /// Every endpoint runs DotNetCrypto, so a passing test proves keys, signatures, hashes and ciphertext
    /// interoperate between BrowserWASMCrypto (SubtleCrypto via SpawnJS) and System.Security.Cryptography.
    /// Served by <see cref="StaticFileServer"/> on the same origin as the WASM app.
    /// </summary>
    internal static class CryptographyTestApi
    {
        public static void MapCryptographyTestApi(this WebApplication app)
        {
            var crypto = new DotNetCrypto();
            // one long-lived server key of each kind, like a real server identity
            var serverKeys = new Lazy<Task<(PortableECDSAKey Ecdsa, PortableECDHKey Ecdh, PortableEd25519Key Ed25519)>>(async () =>
                (await crypto.GenerateECDSAKey(), await crypto.GenerateECDHKey(), await crypto.GenerateEd25519Key()));

            var api = app.MapGroup("/api/CryptographyTest");

            api.MapGet("/ecdh", async () =>
                await crypto.ExportPublicKeySpki((await serverKeys.Value).Ecdh));

            api.MapPost("/GetSharedSecret", async (GetSharedSecretArgs args) =>
            {
                using var browsersECDHKey = await crypto.ImportECDHKey(Convert.FromBase64String(args.SenderECDHPublicKeyB64));
                return await crypto.DeriveBits((await serverKeys.Value).Ecdh, browsersECDHKey);
            });

            api.MapPost("/SignData", async ([FromBody] byte[] data) =>
            {
                var key = (await serverKeys.Value).Ecdsa;
                return new CrossPlatformSignResult { Signature = await crypto.Sign(key, data, "SHA-512"), PublicKeySpki = await crypto.ExportPublicKeySpki(key) };
            });

            api.MapPost("/VerifySignature", async (CrossPlatformVerifyArgs args) =>
            {
                using var browserKey = await crypto.ImportECDSAKey(args.PublicKeySpki);
                return await crypto.Verify(browserKey, args.Data, args.Signature, "SHA-512");
            });

            api.MapPost("/Digest", async (CrossPlatformDigestArgs args) => await crypto.Digest(args.HashName, args.Data));

            api.MapPost("/AesGcmEncrypt", async (CrossPlatformAesGcmArgs args) =>
            {
                using var key = await crypto.GenerateAESGCMKey(args.Secret);
                return await crypto.Encrypt(key, args.Data);
            });

            api.MapPost("/AesGcmDecrypt", async (CrossPlatformAesGcmArgs args) =>
            {
                using var key = await crypto.GenerateAESGCMKey(args.Secret);
                return await crypto.Decrypt(key, args.Data);
            });

            api.MapPost("/AesCbcEncrypt", async (CrossPlatformAesCbcArgs args) =>
            {
                using var key = await crypto.ImportAESCBCKey(args.RawKey);
                return await crypto.Encrypt(key, args.Data);
            });

            api.MapPost("/AesCbcDecrypt", async (CrossPlatformAesCbcArgs args) =>
            {
                using var key = await crypto.ImportAESCBCKey(args.RawKey);
                return await crypto.Decrypt(key, args.Data);
            });

            api.MapPost("/Ed25519Sign", async ([FromBody] byte[] data) =>
            {
                var key = (await serverKeys.Value).Ed25519;
                return new CrossPlatformSignResult { Signature = await crypto.Sign(key, data), PublicKeySpki = await crypto.ExportPublicKeySpki(key) };
            });

            api.MapPost("/Ed25519Verify", async (CrossPlatformVerifyArgs args) =>
            {
                using var browserKey = await crypto.ImportEd25519Key(args.PublicKeySpki);
                return await crypto.Verify(browserKey, args.Data, args.Signature);
            });
        }
    }

    public class GetSharedSecretArgs
    {
        public string SenderECDHPublicKeyB64 { get; set; } = "";
    }

    public class CrossPlatformSignResult
    {
        public byte[] Signature { get; set; } = Array.Empty<byte>();
        public byte[] PublicKeySpki { get; set; } = Array.Empty<byte>();
    }

    public class CrossPlatformVerifyArgs
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public byte[] Signature { get; set; } = Array.Empty<byte>();
        public byte[] PublicKeySpki { get; set; } = Array.Empty<byte>();
    }

    public class CrossPlatformDigestArgs
    {
        public string HashName { get; set; } = "SHA-256";
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }

    public class CrossPlatformAesGcmArgs
    {
        public byte[] Secret { get; set; } = Array.Empty<byte>();
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }

    public class CrossPlatformAesCbcArgs
    {
        public byte[] RawKey { get; set; } = Array.Empty<byte>();
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
