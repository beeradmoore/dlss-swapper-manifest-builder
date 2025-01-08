using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder.Processors;

internal class DLSSGProcessor : DLLProcessor
{
    public override string NamePath => "dlss_g";
    public override string ExpectedDLLName => "nvngx_dlssg.dll";
    public override string[] ValidFileDescriptions => new string[]
    {
        "NVIDIA DLSS-G - DVS PRODUCTION",
        "NVIDIA DLSS-G - NOT FOR PRODUCTION",
        "NVIDIA DLSS-G - PRODUCTION",
    };
    public override string[] ExpectedPrefix => new string[]
    {
        "bin/x64/",
        "/",
    };
    public override string[] ExpectedDevPrefix => new string[]
    {
        "bin/x64/development/",
    };
    public override Dictionary<string, string> CustomAdditionalLabels => new Dictionary<string, string>()
    {
        { "DE3479E49E53A8AB4950F8C72A415239", "v2" }, // DLSS G 1.0.4 v2
        { "8363E2AC2E3E512AC5AB2D364AA4C245", "v2" }, // DLSS G 3.1.30 v2
    };
    public override Dictionary<string, string> DllSource => new Dictionary<string, string>()
    {

    };
}
