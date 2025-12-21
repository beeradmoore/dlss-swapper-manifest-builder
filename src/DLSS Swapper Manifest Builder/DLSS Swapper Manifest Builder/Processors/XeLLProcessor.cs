using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder.Processors;

internal class XeLLProcessor : DLLProcessor
{
    public override string NamePath => "xell";
    public override string ExpectedDLLName => "libxell.dll";
    public override string[] ValidFileDescriptions => new string[]
    {
        "XeLL SDK"
    };
    public override string[] ExpectedPrefix => new string[]
    {
        "bin/",
        "/",
        "Binaries/ThirdParty/Win64/",
	};
    public override string[] ExpectedDevPrefix => new string[]
    {

    };
    public override Dictionary<string, string> CustomAdditionalLabels => new Dictionary<string, string>();
    public override Dictionary<string, string> DllSource => new Dictionary<string, string>()
    {
        { "D469069D3935032F907106331161E78A", "Marvel Rivals" }, // v1.0.0.139
    };
}
