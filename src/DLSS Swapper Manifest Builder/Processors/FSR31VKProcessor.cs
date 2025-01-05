using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder.Processors;

internal class FSR31VKProcessor : DLLProcessor
{
    public override string NamePath => "fsr_31_vk";
    public override string ExpectedDLLName => "amd_fidelityfx_vk.dll";
    public override string[] ValidFileDescriptions => new string[]
    {
        //"XeSS SDK"
    };
}
