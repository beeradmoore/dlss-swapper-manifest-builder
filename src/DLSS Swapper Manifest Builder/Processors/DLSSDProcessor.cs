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
        { "3ED68C9456DC83BDF66B13D1A9C66F18", "33284283" }, // DLSS D 3.5 CL 33284283
        { "625907DE06A912414FDB8444C91B262C", "33367307" }, // DLSS D 3.5 CL 33367307
    };
    public override Dictionary<string, string> DllSource => new Dictionary<string, string>()
    {

    };
}
