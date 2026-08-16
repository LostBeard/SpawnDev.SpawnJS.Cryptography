#if NET6_0_OR_GREATER
using SpawnDev.SpawnJS.JSObjects;
using System.Runtime.Versioning;

namespace SpawnDev.SpawnJS.Cryptography
{
    /// <summary>
    /// Cross platform cryptography tools.<br/>
    /// BrowserWASMCrypto uses the web browser's SubtleCrypto API. Requires IJInProcessSRuntime and supports only webassembly rendering.<br/>
    /// </summary>
    public partial class BrowserWASMCrypto : PortableCrypto
    {
        SpawnJSRuntime JS { get; set; }
        Lazy<SubtleCrypto> _SubtleCrypto;
        SubtleCrypto SubtleCrypto => _SubtleCrypto.Value;
        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="js"></param>
        [SupportedOSPlatform("browser")]
        public BrowserWASMCrypto(SpawnJSRuntime js)
        {
            JS = js;
            _SubtleCrypto = new Lazy<SubtleCrypto>(() => JS.Get<SubtleCrypto>("crypto.subtle"));
        }
    }
}
#endif
