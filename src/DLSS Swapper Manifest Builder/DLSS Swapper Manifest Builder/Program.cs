using DLSS_Swapper.Data;
using DLSS_Swapper_Manifest_Builder;
using DLSS_Swapper_Manifest_Builder.Downloaders;
using DLSS_Swapper_Manifest_Builder.Downloaders.AMD;
using DLSS_Swapper_Manifest_Builder.Downloaders.Intel;
using DLSS_Swapper_Manifest_Builder.Downloaders.Microsoft;
using DLSS_Swapper_Manifest_Builder.Downloaders.NVIDIA;
using DLSS_Swapper_Manifest_Builder.Downloaders.NVIDIA_RTX;
using DLSS_Swapper_Manifest_Builder.Processors;
using DLSS_Swapper_Manifest_Builder.Processors.DirectStorage;
using DLSS_Swapper_Manifest_Builder.Processors.FidelityFX_SDK1;
using DLSS_Swapper_Manifest_Builder.Processors.FidelityFX_SDK2;
using NewDLLHandler;
using Serilog;
using System.Diagnostics;
using System.Text.Json;


Log.Logger = new LoggerConfiguration()
	.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
	.WriteTo.Console()
	.CreateLogger();

Log.Information("Starting processing");
//Log.Debug(

Storage.CreateDirectories();

var manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(Storage.InputManifestPath));
if (manifest == null)
{
    Log.Information($"Could not load {Storage.InputManifestPath}.");
    return 1;
}

var downloaders = new List<ReleaseDownloader>();
//downloaders.Add(new StreamlineDownloader());
//downloaders.Add(new DLSSDownloader());
//downloaders.Add(new XeSSDownloader());
//downloaders.Add(new DirectStorageDownloader());
//downloaders.Add(new FidelityFXDownloader());


foreach (var downloader in downloaders)
{
	await downloader.DownloadAsync();
}


var dllProcessors = new List<DLLProcessor>();

// DLSS
//dllProcessors.Add(new DLSSProcessor(manifest.DLSS));
//dllProcessors.Add(new DLSSGProcessor(manifest.DLSS_G));
//dllProcessors.Add(new DLSSDProcessor(manifest.DLSS_D));

// FSR 3.1 / Fidelity SDK 1
//dllProcessors.Add(new FSR31DX12Processor(manifest.FSR_31_DX12));
//dllProcessors.Add(new FSR31VKProcessor(manifest.FSR_31_VK));

// FSR 4 / Fidelity SDK 2 
dllProcessors.Add(new FidelityFX2_Denoiser_DX12_Processor(manifest.FSR_31_DX12));
dllProcessors.Add(new FidelityFX2_FrameGeneration_DX12_Processor(manifest.FSR_31_DX12));
dllProcessors.Add(new FidelityFX2_Loader_DX12_Processor(manifest.FSR_31_DX12));
dllProcessors.Add(new FidelityFX2_RadianceCache_DX12_Processor(manifest.FSR_31_DX12));
dllProcessors.Add(new FidelityFX2_Upscaler_DX12_Processor(manifest.FSR_31_DX12));

// XeSS
//dllProcessors.Add(new XeSSProcessor(manifest.XeSS));
//dllProcessors.Add(new XeLLProcessor(manifest.XeLL));
//dllProcessors.Add(new XeSSFGProcessor(manifest.XeSS_FG));
//dllProcessors.Add(new XeSSDX11Processor(manifest.XeSS_DX11));

// Direct Storage
// dllProcessors.Add(new DirectStorageProcessor(manifest.DirectStorage));
//dllProcessors.Add(new DirectStorageCoreProcessor(manifest.DirectStorageCore));


foreach (var dllProcessor in dllProcessors)
{
	//await dllProcessor.DownloadExistingRecordsAsync(dllProcessor.ManifestDllRecords);

	var newDllRecords = dllProcessor.ProcessLocalFiles(dllProcessor.ManifestDllRecords);
	dllProcessor.ManifestDllRecords.Clear();
	dllProcessor.ManifestDllRecords.AddRange(newDllRecords);

    //dllProcessor.PostProcessRecords(dllProcessor.ManifestDllRecords, dllProcessor.GameAssetType);

    //dllProcessor.MoveToCorrectLocations(dllProcessor.ManifestDllRecords, dllProcessor.GameAssetType);

    //dllProcessor.MoveOldToNew(dllProcessor.ManifestDllRecords, dllProcessor.GameAssetType);

    //dllProcessor.ManifestDllRecords.Clear();

    //dllProcessor.ManifestDllRecords = dllProcessor.ProcessLocalFiles(dllProcessor.ManifestDllRecords);
}

var knownDLLSourcesMissingPath = Path.Combine("..", "..", "..", "..", "..", "..", "known_dll_sources_missing.json");
using (var stream = File.OpenRead(knownDLLSourcesMissingPath))
{
    var knownDLLSourcesMissing = await JsonSerializer.DeserializeAsync<Dictionary<string, List<KnownDLL>>>(stream);
    if (knownDLLSourcesMissing is null)
    {
        Debugger.Break();
        return 0;
	}

	foreach (var gameAssetType in Enum.GetValues<GameAssetType>())
    {
		if (gameAssetType == GameAssetType.Unknown)
		{
			continue;
		}

        var gameAssetName = Enum.GetName<GameAssetType>(gameAssetType);

		if (string.IsNullOrWhiteSpace(gameAssetName) || gameAssetName.Contains("_BACKUP", StringComparison.OrdinalIgnoreCase))
		{
			continue;
		}

		var knownDLLsList = gameAssetType switch
        {
            GameAssetType.DLSS => manifest.KnownDLLs.DLSS,
            GameAssetType.DLSS_D => manifest.KnownDLLs.DLSS_D,
            GameAssetType.DLSS_G => manifest.KnownDLLs.DLSS_G,
            GameAssetType.FSR_31_DX12 => manifest.KnownDLLs.FSR_31_DX12,
            GameAssetType.FSR_31_VK => manifest.KnownDLLs.FSR_31_VK,
            GameAssetType.XeSS => manifest.KnownDLLs.XeSS,
            GameAssetType.XeLL => manifest.KnownDLLs.XeLL,
            GameAssetType.XeSS_FG => manifest.KnownDLLs.XeSS_FG,
            GameAssetType.XeSS_DX11 => manifest.KnownDLLs.XeSS_DX11,
            GameAssetType.DirectStorage => manifest.KnownDLLs.DirectStorage,
            GameAssetType.DirectStorageCore => manifest.KnownDLLs.DirectStorageCore,
            GameAssetType.FidelityFX_SDK2_Denoiser_DX12 => manifest.KnownDLLs.FidelityFX_SDK2_Denoiser_DX12,
            GameAssetType.FidelityFX_SDK2_FrameGeneration_DX12 => manifest.KnownDLLs.FidelityFX_SDK2_FrameGeneration_DX12,
            GameAssetType.FidelityFX_SDK2_Loader_DX12 => manifest.KnownDLLs.FidelityFX_SDK2_Loader_DX12,
            GameAssetType.FidelityFX_SDK2_RadianceCache_DX12 => manifest.KnownDLLs.FidelityFX_SDK2_RadianceCache_DX12,
            GameAssetType.FidelityFX_SDK2_Upscaler_DX12 => manifest.KnownDLLs.FidelityFX_SDK2_Upscaler_DX12,
            _ => null,
		};

		if (knownDLLsList is null)
		{
			Log.Error($"Unknown KnownDLL list for {gameAssetType}");
            Debugger.Break();
			continue;
		}


        if (knownDLLSourcesMissing.TryGetValue(gameAssetName, out var missingGameAssets))
        {
            knownDLLsList = missingGameAssets.Select(x => x.ToHashedKnownDLL()).ToList();
        }
    }
}

var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions() { WriteIndented = true });

File.WriteAllText(Storage.OutputManifestPath, manifestJson);

// Copy to root of the repo
var repoRootManifestPath = Path.Combine("..", "..", "..", "..", "..", "..", "manifest.json");
File.Copy(Storage.OutputManifestPath, repoRootManifestPath, true);

//Copy to DLSS Swapper docs if the folder is in a relative location.
var dlssSwapperRepoManifestPath = Path.Combine("..", "..", "..", "..", "..", "..", "..", "dlss-swapper", "docs", "manifest.json");
if (File.Exists(dlssSwapperRepoManifestPath))
{
	File.Copy(Storage.OutputManifestPath, dlssSwapperRepoManifestPath, true);
}

// Cleanup.

if (Directory.Exists(Storage.TempFilesPath))
{
    Directory.Delete(Storage.TempFilesPath, true);
}

return 1;