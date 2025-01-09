using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder.Processors;

internal class DLSSDProcessor : DLLProcessor
{
    public override string NamePath => "dlss_d";
    public override string ExpectedDLLName => "nvngx_dlssd.dll";
    public override string[] ValidFileDescriptions => new string[]
    {
        "NVIDIA DLSSv3 - DVS PRODUCTION",
    };
    public override string[] ExpectedPrefix => new string[]
    {
        "bin/x64/",
        "/",
    };
    public override string[] ExpectedDevPrefix => new string[]
    {
        "bin/x64/development/",
    };
    public override Dictionary<string, string> CustomAdditionalLabels => new Dictionary<string, string>()
    {
        { "3ED68C9456DC83BDF66B13D1A9C66F18", "33284283" }, // DLSS D 3.5 CL 33284283
        { "625907DE06A912414FDB8444C91B262C", "33367307" }, // DLSS D 3.5 CL 33367307
    };
    public override Dictionary<string, string> DllSource => new Dictionary<string, string>()
    {
        { "CD523B592B9471B43D0E85D9629A6AE4", "Cyberpunk 2077 Phantom Liberty (via TechPowerUp)" }, // v3.5
        { "3ED68C9456DC83BDF66B13D1A9C66F18", "Cyberpunk 2077 Phantom Liberty 2.01 (via TechPowerUp)" }, // v3.5 33284283
        { "625907DE06A912414FDB8444C91B262C", "Alan Wake 2 (via TechPowerUp)" }, // v3.5 33367307
        { "CE8B65654872A30DC6B771AAE8CA98AD", "Alan Wake  (via TechPowerUp)2" }, // v3.5.10
        { "622FCB76D37A2D73811756E891CC80E8", "Portal with RTX (via TechPowerUp)" }, // v3.7
        { "FDFAC845AB72D509A24EA2C16A1619C4", "The First Descendant (via TechPowerUp)" }, // v3.7.10
        { "F2F968B15CD295D13D571D0D18170E10", "Star Wars Outlaws (via TechPowerUp)" }, // v3.7.20
    };
}
