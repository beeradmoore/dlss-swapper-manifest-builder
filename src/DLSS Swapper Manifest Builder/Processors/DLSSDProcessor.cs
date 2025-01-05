using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder.Processors;

internal class DLSSDProcessor : DLLProcessor
{
    public override string NamePath => "dlss_d";
    public override string ExpectedDLLName => "nvngx_dlssd.dll";
    public override string[] ValidFileDescriptions => new string[]
    {
        "NVIDIA DLSSv3 - DVS PRODUCTION",
    };
}
