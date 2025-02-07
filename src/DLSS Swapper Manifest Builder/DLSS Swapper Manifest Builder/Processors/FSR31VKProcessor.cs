using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder.Processors;

internal class FSR31VKProcessor : FSR31Processor
{
    public override string NamePath => "fsr_31_vk";
    public override string ExpectedDLLName => "amd_fidelityfx_vk.dll";
    public override string[] ValidFileDescriptions => new string[]
    {
        "VK AMD FidelityFX Library",
    };
    public override Dictionary<string, string> CustomAdditionalLabels => new Dictionary<string, string>()
    {
        { "A2BC75BC5455E870D71890C774541DA9", "3.1.0" }, // VK FidelityFX-SDK-1.1.zip
        { "1FC735751D2632E926016206398B8654", "3.1.1" }, // VK FidelityFX-SDK-1.1.1.zip
        { "0D0CDC8D5865027D100DAA35B605793F", "3.1.2" }, // VK FidelityFX-SDK-1.1.2.zip
        { "23A87C10A859D5756EDBE5A6D7F692B5", "3.1.3" }, // VK FidelityFX-SDK-1.1.3.zip
    };
}