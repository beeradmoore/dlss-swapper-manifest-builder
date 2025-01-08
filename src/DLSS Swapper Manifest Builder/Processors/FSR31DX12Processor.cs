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
    public override Dictionary<string, string> CustomAdditionalLabels => new Dictionary<string, string>()
    {
        { "071EB42E3CBD0989A12465E0E529BCAC", "3.1.0" }, // DX12 FidelityFX-SDK-1.1.zip
        { "2FCBE69A137DBA1FF45071A9B64C9581", "3.1.1" }, // DX12 FidelityFX-SDK-1.1.1.zip
        { "2DC40D2F183920624BE396B624466157", "3.1.2" }, // DX12 FidelityFX-SDK-1.1.2.zip
        { "3EF374F8F7EB44D211BCC0C063A520D9", "3.1.3" }, // DX12 FidelityFX-SDK-1.1.3.zip
    };
}