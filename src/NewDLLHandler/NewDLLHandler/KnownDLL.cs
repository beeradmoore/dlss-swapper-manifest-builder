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

    Version? _version;
    [JsonIgnore]
    public Version VersionObject => _version ??= new Version(Version);


    public bool Equals(KnownDLL? other)
    {
        if (other is null)
        {
            return false;
        }

        return Hash.Equals(other.Hash, StringComparison.InvariantCultureIgnoreCase);
    }

#if !NEW_DLL_HANDLER_TOOL
    public DLSS_Swapper_Manifest_Builder.HashedKnownDLL ToHashedKnownDLL()
    {
        var toReturn = new DLSS_Swapper_Manifest_Builder.HashedKnownDLL()
        {
            Hash = Hash,
            Version = Version,
        };

        foreach (var library in Sources.Keys)
        {   
            foreach (var game in Sources[library])
            {
                if (toReturn.Sources.ContainsKey(library) == false)
                {
                    toReturn.Sources[library] = new List<string>();
                }

                var gameBytes = Encoding.UTF8.GetBytes(game);

                // We Base64 encode the game title
                toReturn.Sources[library].Add(Convert.ToBase64String(gameBytes));                
            }
        }

        return toReturn;
    }
#endif
}
