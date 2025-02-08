using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewDLLHandler;

internal class KnownDLL
{
    public string DLLType { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public Dictionary<string, List<string>> Sources { get; set; } = new Dictionary<string, List<string>>();
}
