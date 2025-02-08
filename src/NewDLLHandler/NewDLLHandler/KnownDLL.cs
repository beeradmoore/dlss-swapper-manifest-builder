using System;
using System.Collections.Generic;
using System.Linq;
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
}
