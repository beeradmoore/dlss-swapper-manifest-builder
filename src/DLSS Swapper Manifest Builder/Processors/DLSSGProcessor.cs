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
}
