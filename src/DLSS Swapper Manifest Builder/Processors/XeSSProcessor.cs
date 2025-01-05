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
}
