using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using Serilog;

namespace DLSS_Swapper_Manifest_Builder.Processors;

public class DLSSProcessor : DLLProcessor
{
    public override string NamePath => "dlss";
    public override string ExpectedDLLName => "nvngx_dlss.dll";
    public override string[] ValidFileDescriptions => new string[]
    {
        "NGX DLSS",
        "NGX DLSS - DVS PRODUCTION",
        "NGX DLSS - DVS VIRTUAL",
        "NVIDIA DLSSv2 - DVS PRODUCTION",
        "NVIDIA DLSSv3 - DVS PRODUCTION",
        "NVIDIA DLSSv2 - Beta - White Collie 1 - DVS PRODUCTION",
        "NVIDIA DLSSv2 - Beta - White Collie 2 - DVS PRODUCTION",
        "NVIDIA DLSS - DVS PRODUCTION",
    };
    public override string[] ExpectedPrefix => new string[]
    {
        "Windows_x86_64/rel/",
        "/",
        "bin/x64/",  // used for streamline SDK
    };
    public override string[] ExpectedDevPrefix => new string[]
    {
        "Windows_x86_64/dev/",
        "bin/x64/development/", // used for streamline SDK
    };
    public override Dictionary<string, string> CustomAdditionalLabels => new Dictionary<string, string>()
    {
        { "CA186073CE212602143FDEBD2DC80D33", "v2" }, // 2.3.11 v2
        { "68B0E070CADB1A48237B027549F6A216", "v2" }, // 2.4.2 v2
        { "0A71EFBA8DAFF9C284CE6010923C01F1", "v2" }, // 2.4.12 v2
        { "31BFD8F750F87E5040557D95C2345080", "v3" }, // 2.4.12 v3
        { "40D468487EA4E0F56595F8DE1AC8ED7C", "v2" }, // 3.1.1 v2
        { "ADB36F4684E64ADA9C8D82AD365E6D19", "v2" }, // 3.1.11 v2 (signed 4sec after previous)
        { "4DB1C0E9C79757BDA8FD9116D8137D2F", "v2" }, // 3.1.13 v2 (signed 3 days after previous)
        { "BF68025B3603C382FCA65B148B979682", "v2" }, // 3.5 v2
        { "B2B6FAE8936719CF81D6B5577F257C40", "Beta White Collie 1" },
        { "CD71EE48B994AC254DFF5DEC20828BE7", "Beta White Collie 2" },
        { "78EB8BFB301EAA9FBFE90DB9EDEB20AE", "v2" }, // 3.7.20 (signed 1.5months after previous)
    };
    public override Dictionary<string, string> DllSource => new Dictionary<string, string>()
    {
        { "65D2E2A86352D77244A73BEDD5837F50", "TechPowerUp" }, // 1.0.0.0
        { "667B23ED632FD0B9A5F2992ACE8C6B51", "3DMark DLSS Test (via TechPowerUp)" }, // 1.0.9.0
        { "41878C22B109427192788DD4FCE796C1", "Metro Exodus (via TechPowerUp)" }, // 1.0.11.0
        { "A8ED873E61FCB3A105D249824A0B0511", "TechPowerUp" }, // 1.0.13.0
        { "A435B45B0F2586402BCECA683DD9F1A6", "Shadow of the Tomb Raider (via TechPowerUp)" }, // 1.0.17.0
        { "5E7B70421ECE9DCCA09260A38E4E9172", "Anthem (via TechPowerUp)" }, // 1.1.6.0
        { "5EE9DE2AE9D76A32C727C1B6FF21E0FA", "Monster Hunter World (via TechPowerUp)" }, // 1.1.13.0
        { "899EBC3AC0637125D7578D87FEF42970", "Sword of Legends Online (via TechPowerUp)" }, // 1.2.14.0 
        { "B92191E7A0B994A297FF8B2012EBA529", "Control (via TechPowerUp)" }, // 1.3.2.0
        { "D30C27CA983F9792512C6F5874C60B2E", "Control (via TechPowerUp)" }, // 2.0.34.0
        { "ACB8B647BA19DCC638DD50FF621F7801", "F1 2020 (via TechPowerUp)" }, // 2.0.38.0
        { "1A30408F7AD1BDAEC0EF81B5E2313C4E", "Minecraft Bedrock Edition (via TechPowerUp)" }, // 2.1.16.0
        { "0A82A657E294219B6DC8875897A2CCED", "Death Stranding (via TechPowerUp)" }, // 2.1.19.0
        { "5AF4C12DDE0E9DFA54ED76FE9E4DB647", "TechPowerUp" }, // 2.1.24.0
        { "18B651A8DF80464512ED19FA9859B585", "Control (via TechPowerUp)" }, // 2.1.25.0
        { "F92355A0A6376447C7D0FC0A3CFC2F43", "Bright Memory Infinite Benchmark (via TechPowerUp)" }, // 2.1.28.0
        { "5F4B894692E9FD415731F0C0AA4A33AF", "3DMark DLSS Test v2 (via TechPowerUp)" }, // 2.1.29.0 
        { "3E68D270CEF16027DA3FF3BA3B974537", "Watch Dogs Legion (via TechPowerUp)" }, // 2.1.31.0 
        { "FB72213CF0F3CF103A16CFE7D930F707", "TechPowerUp" }, // 2.1.35.0
        { "6CBF57C2D08D775808AEF61C9671DB5B", "Cyberpunk 2077 (via TechPowerUp)" }, // 2.1.39.0
        { "8E3AC0A089ABE2B319E2926B38DB0FF9", "TechPowerUp" }, // 2.1.40.0
        { "6672C90B5AFEF2153259F84929364B2A", "System Shock Demo (via TechPowerUp)" }, // 2.1.50.0
        { "4950C49D5E337F3146BCD51A26AB8CC6", "F1 2020 (via TechPowerUp)" }, // 2.1.51.0
        { "430EE0D531F4B89A7CC65524CCC5D912", "Fortnite (via TechPowerUp)" }, // 2.1.52.0
        { "E696265C702CC90CBCF4974FAEEBB9F4", "TechPowerUp" }, // 2.1.53.0
        { "72C53FE8CF9114143680F3BC56CEA1A7", "Metro Exodus Enhanced (via TechPowerUp)" }, // 2.1.55.0
        { "2B1B8D32E4866E7D7734ACF2BCC830B1", "COD MW (via TechPowerUp)" }, // 2.1.58.0
        { "ECF487BC067ED07B90A3452DB0203655", "TechPowerUp" }, // 2.1.62.0
        { "262B728F1692E2550CF1CC9A41681A59", "No Man's Sky (via TechPowerUp)" }, // 2.1.63.0
        { "1A81316E57ABA6C9C46FAE53936C3933", "Doom Eternal (via TechPowerUp)" }, // 2.1.66.0 
        { "52219034574426FEF16B19D2495648C0", "Rainbow Six Siege (via TechPowerUp)" }, // 2.2.6.0
        { "5B9B917AF8FB72D63F073A0E0F621D94", "F1 2021 (via TechPowerUp)" }, // 2.2.9.0
        { "062215C828802B9202ADA4CA4D3619B4", "Rust (via TechPowerUp)" }, // 2.2.10.0
        { "65345301BA1A985AA9AC3EA6D671ABDF", "No Man's Sky (via TechPowerUp)" }, // 2.2.11.0
        { "110F5CA0453C6F3901FD148EA5CC7650", "Call of Duty Warzone (via TechPowerUp)" }, // 2.2.14.0
        { "C9DCDCD97E19529A7C89432693C8EC0C", "Marvel Avengers (via TechPowerUp)" }, // 2.2.15.0
        { "77A75B96DD2D36A4A291F3939D59C221", "UE5 (via TechPowerUp)" }, // 2.2.18.0
        { "B2B6FAE8936719CF81D6B5577F257C40", "NVIDIA - Experimental DLSS Models" }, // 2.2.18.0 Beta - White Collie 1
        { "CD71EE48B994AC254DFF5DEC20828BE7", "NVIDIA - Experimental DLSS Models" }, // 2.2.18.0 Beta - White Collie 2
        { "D62B6857F2F874E3892B30ABCE3DC603", "Sword and Fairy 7 (via TechPowerUp)" }, // 2.3.2.0
        { "229DD563C5A7CFE67A927C06333249DA", "Swords of Legends Online (via TechPowerUp)" }, // 2.3.3.0
        { "49B252E0F803A01C0CF6964ED4B16AD8", "Jurassic World Evolution 2 (via TechPowerUp)" }, // 2.3.4.0
        { "F59A89CA2150F107A1D9DE4CA8CA8DDC", "Horizon Zero Dawn (via TechPowerUp)" }, // 2.3.5.0
        { "0C1EDA5E9D60B4D66E075FE0CB672794", "Dying Light 2 (via TechPowerUp)" }, // 2.3.7.0
        { "F3938D96EB0600634CB16E01A4D6EA1F", "Chorus (via TechPowerUp)" }, // 2.3.9.0
        { "CA186073CE212602143FDEBD2DC80D33", "Loopmancer" }, // 2.3.11 v2
        { "FC7F487079B5132E132729EA046DD017", "Back4Blood (via TechPowerUp)" }, // 2.4.2
        { "68B0E070CADB1A48237B027549F6A216", "Baldur's Gate 3 (via TechPowerUp)" }, // 2.4.2 v2
        { "DEA5F1DABF86203C3210E21DF9CFCF07", "Evil Dead (via TechPowerUp)" }, // 2.4.3.0
        { "03A5A7BE0E4D83487DD68DFB88FD0036", "Microsoft Flight Simulator SU10 (via TechPowerUp)" }, // 2.4.6.0
        { "C1E931321A9818FA896ABE03823D4C1E", "Sackboy (via TechPowerUp)" }, // 2.4.11.0
        { "0AA6CF6B6779919088455E47FBBA0A5F", "Final Fantasy Origin (via TechPowerUp)" }, // 2.4.12.0
        { "0A71EFBA8DAFF9C284CE6010923C01F1", "A Plague Tale Requiem (via TechPowerUp)" }, // 2.4.12.0 v2
        { "31BFD8F750F87E5040557D95C2345080", "Spider-Man: Miles Morales (via TechPowerUp)" }, // 2.4.12.0 v3
        { "578D0A372C0D59A704513BB22A7DB234", "WRC Generations (via TechPowerUp)" }, // 2.4.13.0
        { "87A8231AC086AD8EE6B3C57140F25DCC", "Marvel's Midnight Suns Dec 10 Patch (via TechPowerUp)" }, // 2.5.0.0
        { "DFE932D0B3955E649C1F759382D4E0D4", "Portal RTX new patch (via TechPowerUp)" }, // 2.5.1.0        
        { "40D468487EA4E0F56595F8DE1AC8ED7C", "Atomic Heart (via TechPowerUp)" }, // 3.1.1.0 v2
        { "FCEDE5737D34413CE16CC0BE9A6203AF", "The Last of Us (via TechPowerUp)" }, // 3.1.2.0
        { "ADB36F4684E64ADA9C8D82AD365E6D19", "NVIDIA SDK (via TechPowerUp)" }, // 3.1.11.0
        { "22B2359CF7EC76F523FEEEC53FA81464", "NVIDIA SDK (via TechPowerUp)" }, // 3.1.13.0
        { "BF68025B3603C382FCA65B148B979682", "DLSS Demo" }, // 3.5.0.0 v2
        { "49CE45D36D0FE6E498F2AAE54D94BDEE", "Naraka Bladepoint (via TechPowerUp)" }, // 3.6.0.0
        { "53DBEC92BB250E667CBC532D5D782316", "NVIDIA Driver (DirectSR) (via TechPowerUp)" }, // 3.7.20.0
        { "AED94E7029846C356882DB166B824F5E", "NVIDIA Driver (DirectSR) (via TechPowerUp)" }, // 3.8.10.0
        { "117595D4839DCAB18501249CBEEE9B7A", "Cyberpunk 2077 2.21" }, // 310.1.0.0
        { "3A875F45C315D09E5F4548CC9288F178" , "NVIDIA Driver" }, // v310.2.0.0
    };

    public override List<DLLRecord> ProcessLocalFiles(IReadOnlyList<DLLRecord> existingRecords)
    {
        var modelPath = @"C:\ProgramData\NVIDIA\NGX\models\dlss\versions\";
        var binFiles = Directory.GetFiles(modelPath, "*.bin", SearchOption.AllDirectories);
		Log.Information($"Checking NVIDIA driver for DLSS, found {binFiles.Length} files");

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
                    var existingRecord = existingRecords.FirstOrDefault(x => x.MD5Hash.Equals(md5Hash, StringComparison.InvariantCultureIgnoreCase));
					if (existingRecord is not null)
					{
						Log.Information($"Skipping {Path.GetFileName(binFile)}, hash exists for DLSS {existingRecord.Version}.");
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

            Log.Information($"dlss - {productVersion} - {md5Hash}");

            // TODO: Handle new files.
        }

        var processedFiles = base.ProcessLocalFiles(existingRecords);
        return processedFiles;
    }
}
