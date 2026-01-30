using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace DLSS_Swapper_Manifest_Builder.Processors;

internal class DLSSDProcessor : DLLProcessor
{
    public override string NamePath => "dlss_d";
    public override string ExpectedDLLName => "nvngx_dlssd.dll";
    public override string[] ValidFileDescriptions => new string[]
    {
        "NVIDIA DLSSv3 - DVS PRODUCTION",
        "NVIDIA DLSS - DVS PRODUCTION",
    };
    public override string[] ExpectedPrefix => new string[]
	{
		"Windows_x86_64/rel/", // used for DLSS SDK
        "bin/x64/", // used for Streamline SDK
        "/",
    };
    public override string[] ExpectedDevPrefix => new string[]
    {
        "bin/x64/development/", // used for streamline SDK
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
        { "10C793F0B14EFEDC595838BB8A09FD28", "Cyberpunk 2077 2.21" }, // v310.1.0.0
        { "E6081B848EA68880DB8ADC83CDFB15DC", "NVIDIA Driver" }, // v310.2.0.0
        { "E5311D6C46C2920E2D5347C029F60CB7", "Half-Life 2 RTX" }, // v310.2.1.0
    };

    public override List<DLLRecord> ProcessLocalFiles(IReadOnlyList<DLLRecord> existingRecords)
    {
        var modelPath = @"C:\ProgramData\NVIDIA\NGX\models\dlssd\versions\";
        var binFiles = Directory.GetFiles(modelPath, "*.bin", SearchOption.AllDirectories);

        foreach (var binFile in binFiles)
        {
            var md5Hash = string.Empty;
            using (var fileStream = File.OpenRead(binFile))
            {
                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(fileStream);
                    md5Hash = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();

                    // Check if the file is an exact match.
                    if (existingRecords.Any(x => x.MD5Hash.Equals(md5Hash, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        // If exact match, skip it.
                        continue;
                    }
                }
            }

            // Check if we have an existing DLL of the same version.
            var fileInfo = new FileInfo(binFile);
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(binFile);
            var productVersion = fileVersionInfo.ProductVersion?.Replace(',', '.') ?? string.Empty;

            if (string.IsNullOrWhiteSpace(productVersion))
            {
                // We should never get here.
                Debugger.Break();
                continue;
            }

            // Even though the DLL is different we don't want 50 copies of the same v1.2.3.4
            if (existingRecords.Any(x => x.Version.Equals(productVersion, StringComparison.InvariantCultureIgnoreCase)))
            {
                continue;
            }

            Log.Information($"dlss_d - {productVersion} - {md5Hash}");

            // TODO: Handle new files.
        }

        var processedFiles = base.ProcessLocalFiles(existingRecords);
        
        return processedFiles;
    }
}
