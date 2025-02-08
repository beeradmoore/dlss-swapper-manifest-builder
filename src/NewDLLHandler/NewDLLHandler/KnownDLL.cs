using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NewDLLHandler;

internal class KnownDLL : IEquatable<KnownDLL>
{
    [JsonPropertyName("dll_type")]
    public string DLLType { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("sources")]
    public Dictionary<string, List<string>> Sources { get; set; } = new Dictionary<string, List<string>>();

    public bool Equals(KnownDLL? other)
    {
        if (other is null)
        {
            return false;
        }

        return Hash.Equals(other.Hash, StringComparison.InvariantCultureIgnoreCase);
    }

#if !NEW_DLL_HANDLER_TOOL
    public DLSS_Swapper_Manifest_Builder.CompactKnownDLL ToCompactKnownDLL()
    {
        var toReturn = new DLSS_Swapper_Manifest_Builder.CompactKnownDLL()
        {
            Hash = Hash,
        };

        foreach (var library in Sources.Keys)
        {   
            foreach (var game in Sources[library])
            {
                using (var md5 = MD5.Create())
                {
                    var inputBytes = Encoding.ASCII.GetBytes($"{library}-{game}");
                    var hashBytes = md5.ComputeHash(inputBytes);
                    toReturn.Sources.Add(BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant());
                }
            }
        }


        return toReturn;
    }
#endif
}
