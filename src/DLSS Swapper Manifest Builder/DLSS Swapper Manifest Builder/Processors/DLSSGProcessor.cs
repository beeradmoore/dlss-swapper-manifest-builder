using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder.Processors;

internal class DLSSGProcessor : DLLProcessor
{
    public override string NamePath => "dlss_g";
    public override string ExpectedDLLName => "nvngx_dlssg.dll";
    public override string[] ValidFileDescriptions => new string[]
    {
        "NVIDIA DLSS-G - DVS PRODUCTION",
        "NVIDIA DLSS-G - NOT FOR PRODUCTION",
        "NVIDIA DLSS-G - PRODUCTION",
        "NVIDIA DLSS-G MFGLW - DVS PRODUCTION",
    };
    public override string[] ExpectedPrefix => new string[]
    {
        //"bin/x64/", // used for streamline which is currently disabled
        "/",
    };
    public override string[] ExpectedDevPrefix => new string[]
    {
        //"bin/x64/development/", // used for streamline which is currently disabled
    };
    public override Dictionary<string, string> CustomAdditionalLabels => new Dictionary<string, string>()
    {
        /*
        { "E8E78491DC415315A2C3785916185CF1", "Streamline" },
        { "59E944602740351AAFA406F2115F1973", "Streamline Dev" },
        { "7A6E7A3DDD0BBBDB7F8236E379EA9C75", "Streamline" },
        { "94C0A7ACC38EA9EFF61061C8BBD971B3", "Streamline Dev" },
        { "DC3C3C2AD0EC4FD47FE65864AFD2B931", "Streamline" },
        { "2A129C87FE9617A1B8FA314D3A12AF9F", "Streamline Dev" },
        { "B6F797DA83B1911A941472517AD4E72E", "Streamline" },
        { "F828A5C6F08103A27E469A74E6BA7D4B", "Streamline Dev" },
        { "ED76C30C0778B797C8DD43F189152122", "Streamline" },
        { "081B836B0B452AFD9805DAE2A71AC04F", "Streamline Dev" },
        { "14860860199C29F2336A761CF5BAE182", "Streamline" },
        { "0C972BE1D5B7625EA656EA73A4C9A5D9", "Streamline Dev" },
        { "EE98CD63C16316882C3E5D56E51FC680", "Streamline" },
        { "B1649310FB9452DCE59C77C09B0B1CF1", "Streamline Dev" },
        { "69E9F9DC32D0AEF4E7C986E2339A0E52", "Streamline" },
        { "99EB5CB0720D58EA591B061EBD454EC9", "Streamline Dev" },
        */

        { "DE3479E49E53A8AB4950F8C72A415239", "v2" }, // DLSS G 1.0.4 v2
    };
    public override Dictionary<string, string> DllSource => new Dictionary<string, string>()
    {
        { "756C81DECD8AB626058E8DAAA08D9A19", "Microsoft Flight Simulator (via TechPowerUp)" }, // v1.0.1
        { "36931062A0B9227011885514914C7A3B", "Witcher 3 (via TechPowerUp)" }, // v1.0.2
        { "9A44C28A5E5A915F884E2ED7DFC5D4B4", "Spiderman Remastered (via TechPowerUp)" }, // 1.0.3
        { "9BFAC44640D2FA6659AC18369F9870F4", "Portal RTX (via TechPowerUp)" }, // v1.0.4
        { "DE3479E49E53A8AB4950F8C72A415239", "Portal RTX (via TechPowerUp)" }, // v1.0.4 v2
        { "4F39303B168462A4F2ECF04FEAD1A9E8", "Hitman 3 (via TechPowerUp)" }, // v1.0.5
        { "F869FBD1AD307071CF7997B66C1E1E76", "Forza Horizon 5 (via TechPowerUp)" }, // v1.0.6
        { "BBAEB66B55BEA5A97EF12B949AF52BC0", "The Witcher 3 (via TechPowerUp)" }, // v1.0.7
        { "3209DE901666436042F4CE59344DF47F", "Cyberpunk Overdrive (via TechPowerUp)" }, // v3.1.1
        { "2A3FEB039FFB9EA70DC690B21E918417", "Forza Horizon 5 (via TechPowerUp)" }, // v3.1.11
        { "76F8BE4E38A0789BCE0DCDE2CEA9A324", "Diablo IV Beta (via TechPowerUp)" }, // 3.1.12
        { "EF9B5FA50410A45A13014CB55AAE950D", "Diablo IV (via TechPowerUp)" }, // 3.1.13
        { "8363E2AC2E3E512AC5AB2D364AA4C245", "Desordre (via TechPowerUp)" }, // v3.1.30
        { "1B21DA1D91AB05B83625729D0B683797", "NVIDIA UE5 Plugin (via TechPowerUp)" }, // v3.5
        { "4DE45236C2AA62CD25F1D670E0C5DEAF", "NVIDIA UE5 Plugin (via TechPowerUp)" }, // v3.5.10
        { "67453779383C58139267A9B15839BB78", "Naraka Bladepoint (via TechPowerUp)" }, // v3.6
        { "EE98CD63C16316882C3E5D56E51FC680", "NVIDIA SDK (via TechPowerUp)" }, // 3.7 Streamline
        { "B16E50DD7C60D254AAB782278C2EF73C", "Senua's Saga: Hellblade II (via TechPowerUp)" }, // v3.7.1.0
        { "69E9F9DC32D0AEF4E7C986E2339A0E52", "Diablo IV (via TechPowerUp)" }, // 3.7.10, also from Streamline
        { "DB7B5C2C8686E1586CC56D5BDB64965C", "Diablo IV (via TechPowerUp)" }, // 3.8.1
        { "11ECC4BF7FE5CEFCF4A737E95B6752FF", "Cyberpunk 2077 2.21" }, // 310.1.0.0
        { "26C2EAFE046CDD403A1850F71186595C", "NVIDIA Driver" }, // v310.2.0.0

        // Not showing E8E78491DC415315A2C3785916185CF1 (1.10.0 , 3.1.10)
        // as it says NVIDIA CONFIDENTIAL - PROVIDED UNDER NDA - DO NOT DISTRIBUTE IN ANY WAY
    };
}
