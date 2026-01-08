
using DLSS_Swapper_Manifest_Builder;
using NewDLLHandler;
using Serilog;
using System.Diagnostics;
using System.Text.Json;

Log.Logger = new LoggerConfiguration()
	.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
	.WriteTo.Console()
	.CreateLogger();

Log.Information("Starting processing");


Dictionary<string, List<KnownDLL>>? missingKnownDLLs;
Manifest? manifest;

using (var fileStream = File.OpenRead("../../../../../../known_dll_sources_missing.json"))
{
    var tempMissingKnownDLLs = JsonSerializer.Deserialize<Dictionary<string, List<KnownDLL>>>(fileStream);
    if (tempMissingKnownDLLs is null)
    {
        Log.Error("Failed to deserialize known missing DLLs from json file.");
        return 1;
    }

    missingKnownDLLs = tempMissingKnownDLLs;
}

using (var fileStream = File.OpenRead("../../../../../../manifest.json"))
{
    var tempManifest = JsonSerializer.Deserialize<Manifest>(fileStream);
    if (tempManifest is null)
    {
		Log.Error("Failed to deserialize manifest json file.");
        return 1;
    }

    manifest = tempManifest;
}

ProcessData("DLSS", manifest.DLSS, missingKnownDLLs["DLSS"]);
ProcessData("DLSS_G", manifest.DLSS_G, missingKnownDLLs["DLSS_G"]);
ProcessData("DLSS_D", manifest.DLSS_D, missingKnownDLLs["DLSS_D"]);
ProcessData("FSR_31_DX12", manifest.FSR_31_DX12, missingKnownDLLs["FSR_31_DX12"]);
ProcessData("FSR_31_VK", manifest.FSR_31_VK, missingKnownDLLs["FSR_31_VK"]);
ProcessData("XeSS", manifest.XeSS, missingKnownDLLs["XeSS"]);
ProcessData("XeSS_FG", manifest.XeSS_FG, missingKnownDLLs["XeSS_FG"]);
ProcessData("XeSS_DX11", manifest.XeSS_DX11, missingKnownDLLs["XeSS_DX11"]);
ProcessData("XeLL", manifest.XeLL, missingKnownDLLs["XeLL"]);

void ProcessData(string upscaler, List<DLLRecord> manifestDLLRecords, List<KnownDLL> missingKnownDLLList)
{
    var latestVersion = new Version(0, 0);

    Log.Information(upscaler);

    var knownDLLsToFind = new Dictionary<Version, List<KnownDLL>>();
    foreach (var missingKnownDLL in missingKnownDLLList)
    {
        if (missingKnownDLL.Version == "0.0.0.0" || missingKnownDLL.Version == "5.1.100.0")
        {
            continue;
        }

        if (missingKnownDLL.VersionObject > latestVersion)
        {
            latestVersion = missingKnownDLL.VersionObject;
        }
        
        if (manifestDLLRecords.Any(x => x.Version == missingKnownDLL.Version) == false)
        {
            if (knownDLLsToFind.ContainsKey(missingKnownDLL.VersionObject) == false)
            {
                knownDLLsToFind[missingKnownDLL.VersionObject] = new List<KnownDLL>();
            }
            knownDLLsToFind[missingKnownDLL.VersionObject].Add(missingKnownDLL);
        }
    }

    if (knownDLLsToFind.Count == 0)
    {
		Log.Information("No unknown DLLs found.");
    }
    else
    {
        var versions = knownDLLsToFind.Keys.ToList();
        versions.Sort((a, b) => b.CompareTo(a));

        foreach (var version in versions)
        {
            var latestString = (version == latestVersion) ? " LATEST" : string.Empty;
            var knownDLLs = knownDLLsToFind[version];
			Log.Information($"- {version}{latestString}");
			Log.Information($"- Variants: {knownDLLs.Count}");

            var sources = new Dictionary<string, List<string>>();

            foreach (var knownDLL in knownDLLs)
            {
                foreach (var source in knownDLL.Sources)
                {
                    if (sources.ContainsKey(source.Key) == false)
                    {
                        sources[source.Key] = new List<string>();
                    }

                    foreach (var game in source.Value)
                    {
                        if (sources[source.Key].Contains(game) == false)
                        {
                            sources[source.Key].Add(game);
                        }
                    }
                }
            }

            foreach (var source in sources)
            {
                Log.Information($"  - {source.Key}: {source.Value.Count} games");
                foreach (var game in source.Value)
                {
                    Log.Information($"    - {game}");
                }
            }

            Log.Information(string.Empty);
        }

        Log.Information(string.Empty);
    }

    Log.Information(string.Empty);
    Log.Information(string.Empty);
}


return 0;
