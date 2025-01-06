using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder.Processors;

public class FSR31DX12Processor : FSR31Processor
{
    public override string NamePath => "fsr_31_dx12";
    public override string ExpectedDLLName => "amd_fidelityfx_dx12.dll";
    public override string[] ValidFileDescriptions => new string[]
    {
        "DX12 AMD FidelityFX Library",
    };
}