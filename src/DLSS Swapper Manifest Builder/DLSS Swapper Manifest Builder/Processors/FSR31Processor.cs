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
        "/",
    };
    public override string[] ExpectedDevPrefix => new string[]
    {

    };
    public override Dictionary<string, string> DllSource => new Dictionary<string, string>()
    {
        { "46F7049B30404D8C4EF685A13EF0BC43", "Horizon Zero Dawn Remastered" }, // DX12 v1.0.0.36752 
        { "CFCD38F47665DDFF195B80C62E3B57E2", "F1 2024" }, // DX12 v1.0.0.36208
    };
}
