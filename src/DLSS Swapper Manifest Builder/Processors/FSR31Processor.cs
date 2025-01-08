using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder.Processors;

public abstract class FSR31Processor : DLLProcessor
{
    public override string[] ExpectedPrefix => new string[]
    {
        "/PrebuiltSignedDLL/",
    };
    public override string[] ExpectedDevPrefix => new string[]
    {

    };
    public override Dictionary<string, string> DllSource => new Dictionary<string, string>()
    {

    };
}
