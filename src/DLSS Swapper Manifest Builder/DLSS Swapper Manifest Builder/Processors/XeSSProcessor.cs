using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace DLSS_Swapper_Manifest_Builder.Processors;


public class XeSSProcessor : DLLProcessor
{
    public override string NamePath => "xess";
    public override string ExpectedDLLName => "libxess.dll";
    public override string[] ValidFileDescriptions => new string[]
    {
        "XeSS SDK"
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
        { "C9FB6BB716A087FD537790BEBE710D4C", "Forza Horizon 5" }, // v1.1.0.18
        { "367BE8343B288372FF8523438F53B62D", "Marvel Rivals" }, // v2.0.0.18
    };
}
