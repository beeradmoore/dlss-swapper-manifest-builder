using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder.Processors;

internal class XeSSFGProcessor : DLLProcessor
{
    public override string NamePath => "xess_fg";
    public override string ExpectedDLLName => "libxess_fg.dll";
    public override string[] ValidFileDescriptions => new string[]
    {
        "XeSS Frame Generation SDK"
    };
    public override string[] ExpectedPrefix => new string[]
    {
        "bin/",
        "/",
    };
    public override string[] ExpectedDevPrefix => new string[]
    {

    };
    public override Dictionary<string, string> CustomAdditionalLabels => new Dictionary<string, string>();
    public override Dictionary<string, string> DllSource => new Dictionary<string, string>()
    {
        { "BF976A1C2E466B1BE6FC4B17D76B50D5", "Marvel Rivals" }, // v1.0.0.16
    };
}
