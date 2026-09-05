using DLSS_Swapper.Data;
using DLSS_Swapper_Manifest_Builder;
using DLSS_Swapper_Manifest_Builder.Downloaders;
using DLSS_Swapper_Manifest_Builder.Downloaders.AMD;
using DLSS_Swapper_Manifest_Builder.Downloaders.Intel;
using DLSS_Swapper_Manifest_Builder.Downloaders.Microsoft;
using DLSS_Swapper_Manifest_Builder.Downloaders.NVIDIA;
using DLSS_Swapper_Manifest_Builder.Downloaders.NVIDIA_RTX;
using DLSS_Swapper_Manifest_Builder.Helpers;
using DLSS_Swapper_Manifest_Builder.Processors;
using DLSS_Swapper_Manifest_Builder.Processors.DirectStorage;
using DLSS_Swapper_Manifest_Builder.Processors.DLLSets;
using DLSS_Swapper_Manifest_Builder.Processors.FidelityFX_SDK1;
using DLSS_Swapper_Manifest_Builder.Processors.FidelityFX_SDK2;
using DLSS_Swapper_Manifest_Builder.Processors.Streamline;
using NewDLLHandler;
using Serilog;
using System.Diagnostics;
using System.Text.Json;

// Reset console to prevent text overriding previous text
Console.Clear();

var counterSink = new CounterSink();


Log.Logger = new LoggerConfiguration()
	.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
	.WriteTo.Console()
    .WriteTo.Sink(counterSink)
	.CreateLogger();

Log.Information("Starting processing");
Log.Information(string.Empty);

// Create input/output folder structure
Storage.CreateDirectories();

// Validate GameAssetType 
Log.Information("Validating GameAssetTypes");
var gameAssetTypeErrors = new List<string>();
var gameAssetTypes = Enum.GetNames(typeof(GameAssetType));
foreach (var gameAssetType in gameAssetTypes)
{
    if (gameAssetType == nameof(GameAssetType.Unknown))
    {
        continue;
    }

    if (gameAssetType.Contains("_BACKUP"))
    {
        var gameAssetTypeWithoutBackup = gameAssetType.Substring(0, gameAssetType.Length - "_BACKUP".Length);
        if (gameAssetTypes.Contains(gameAssetTypeWithoutBackup) == false)
        {
            gameAssetTypeErrors.Add($"{gameAssetType} exists but there is no {gameAssetTypeWithoutBackup}");
        }
    }
    else
    {
        var gameAssetTypeWithBackup = $"{gameAssetType}_BACKUP";
        if (gameAssetTypes.Contains(gameAssetTypeWithBackup) == false)
        {
            gameAssetTypeErrors.Add($"{gameAssetType} exists but there is no {gameAssetTypeWithBackup}");
        }
    }
}

if (gameAssetTypeErrors.Count > 0)
{
    Log.Error($"Found {gameAssetTypeErrors.Count} issues.");
    foreach (var gameAssetTypeError in gameAssetTypeErrors)
    {
        Log.Error(gameAssetTypeError);
    }

    Debugger.Break();
}
else
{
    Log.Information("No GameAssetType issues found.");
}

Log.Information(string.Empty);

var manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(Storage.InputManifestPath));
if (manifest == null)
{
    Log.Information($"Could not load {Storage.InputManifestPath}.");
    Log.CloseAndFlush();
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
//dllProcessors.Add(new DLSSNRProcessor(manifest.DLSS_NR));


// FSR 3.1 / Fidelity SDK 1
//dllProcessors.Add(new FSR31DX12Processor(manifest.FSR_31_DX12));
//dllProcessors.Add(new FSR31VKProcessor(manifest.FSR_31_VK));

// FSR 4 / Fidelity SDK 2 
//dllProcessors.Add(new FidelityFX2_Denoiser_DX12_Processor(manifest.FSR_31_DX12));
//dllProcessors.Add(new FidelityFX2_FrameGeneration_DX12_Processor(manifest.FSR_31_DX12));
//dllProcessors.Add(new FidelityFX2_Loader_DX12_Processor(manifest.FSR_31_DX12));
//dllProcessors.Add(new FidelityFX2_RadianceCache_DX12_Processor(manifest.FSR_31_DX12));
//dllProcessors.Add(new FidelityFX2_Upscaler_DX12_Processor(manifest.FSR_31_DX12));

// XeSS
//dllProcessors.Add(new XeSSProcessor(manifest.XeSS));
//dllProcessors.Add(new XeLLProcessor(manifest.XeLL));
//dllProcessors.Add(new XeSSFGProcessor(manifest.XeSS_FG));
//dllProcessors.Add(new XeSSDX11Processor(manifest.XeSS_DX11));

// Direct Storage
dllProcessors.Add(new DirectStorageProcessor(manifest.DirectStorage));
dllProcessors.Add(new DirectStorageCoreProcessor(manifest.DirectStorageCore));

// Sreamline
//dllProcessors.Add(new Streamline_Reflex_Processor(manifest.Streamline_Reflex));
//dllProcessors.Add(new Streamline_PCL_Processor(manifest.Streamline_PCL));
//dllProcessors.Add(new Streamline_NvPerf_Processor(manifest.Streamline_NvPerf));
//dllProcessors.Add(new Streamline_NIS_Processor(manifest.Streamline_NIS));
//dllProcessors.Add(new Streamline_Interposer_Processor(manifest.Streamline_Interposer));
//dllProcessors.Add(new Streamline_DLSS_G_Processor(manifest.Streamline_DLSS_G));
//dllProcessors.Add(new Streamline_DLSS_D_Processor(manifest.Streamline_DLSS_D));
//dllProcessors.Add(new Streamline_DLSS_Processor(manifest.Streamline_DLSS));
//dllProcessors.Add(new Streamline_DirectSR_Processor(manifest.Streamline_DirectSR));
//dllProcessors.Add(new Streamline_DeepDVC_Processor(manifest.Streamline_DeepDVC));
//dllProcessors.Add(new Streamline_Common_Processor(manifest.Streamline_Common));
//dllProcessors.Add(new DeepDVC_Processor(manifest.DeepDVC));
//dllProcessors.Add(new NvLowLatencyVK_Processor(manifest.NvLowLatencyVK));


foreach (var dllProcessor in dllProcessors)
{
	//await dllProcessor.DownloadExistingRecordsAsync(dllProcessor.ManifestDllRecords);

	var newDllRecords = dllProcessor.ProcessLocalFiles(dllProcessor.ManifestDllRecords);
	dllProcessor.ManifestDllRecords.Clear();
	dllProcessor.ManifestDllRecords.AddRange(newDllRecords);

    // Old things we don't do anymore.

    //dllProcessor.PostProcessRecords(dllProcessor.ManifestDllRecords, dllProcessor.GameAssetType);

    //dllProcessor.MoveToCorrectLocations(dllProcessor.ManifestDllRecords, dllProcessor.GameAssetType);

    //dllProcessor.MoveOldToNew(dllProcessor.ManifestDllRecords, dllProcessor.GameAssetType);

    //dllProcessor.ManifestDllRecords.Clear();

    //dllProcessor.ManifestDllRecords = dllProcessor.ProcessLocalFiles(dllProcessor.ManifestDllRecords);
}

// Rebuild DLL sets
#region Rebuild DLL Sets

var dllSetProcessors = new Dictionary<DLLSetType, DLLSetProcessor>();

foreach (var dllProcessor in dllProcessors)
{
    if (dllSetProcessors.ContainsKey(dllProcessor.DLLSetType) == false)
    {
        var dllSetTypeString = DLLSetProcessor.GetDLLSetTypeString(dllProcessor.DLLSetType);   
        var dllSetList = ObjectPropertyGrabber.GetPropertyByJsonName<List<DLLSet>>(manifest.DLLSets, dllSetTypeString);
        var dllSetProcessor = DLLSetProcessor.FromDLLSetType(dllProcessor.DLLSetType, dllSetList);
        dllSetProcessors.Add(dllProcessor.DLLSetType, dllSetProcessor);
    }

    dllSetProcessors[dllProcessor.DLLSetType].DLLProcessors.Add(dllProcessor);
}

foreach ((DLLSetType dllSetType,  DLLSetProcessor dllSetProcessor) in dllSetProcessors)
{
    dllSetProcessor.ProcessDLLSets();
}

#endregion


var knownDLLSourcesMissingPath = Path.Combine("..", "..", "..", "..", "..", "..", "known_dll_sources_missing.json");
using (var stream = File.OpenRead(knownDLLSourcesMissingPath))
{
    var knownDLLSourcesMissing = await JsonSerializer.DeserializeAsync<Dictionary<string, List<KnownDLL>>>(stream);
    if (knownDLLSourcesMissing is null)
    {
        Debugger.Break();

        Log.CloseAndFlush();
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

        // TODO: Can this be dynamic?
		var knownDLLsList = gameAssetType switch
        {
            GameAssetType.DLSS => manifest.KnownDLLs.DLSS,
            GameAssetType.DLSS_D => manifest.KnownDLLs.DLSS_D,
            GameAssetType.DLSS_G => manifest.KnownDLLs.DLSS_G,
            GameAssetType.DLSS_NR => manifest.KnownDLLs.DLSS_NR,
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
            GameAssetType.Streamline_Reflex => manifest.KnownDLLs.Streamline_Reflex,
            GameAssetType.Streamline_PCL => manifest.KnownDLLs.Streamline_PCL,
            GameAssetType.Streamline_NvPerf => manifest.KnownDLLs.Streamline_NvPerf,
            GameAssetType.Streamline_NIS => manifest.KnownDLLs.Streamline_NIS,
            GameAssetType.Streamline_Interposer => manifest.KnownDLLs.Streamline_Interposer,
            GameAssetType.Streamline_DLSS_G => manifest.KnownDLLs.Streamline_DLSS_G,
            GameAssetType.Streamline_DLSS_D => manifest.KnownDLLs.Streamline_DLSS_D,
            GameAssetType.Streamline_DLSS => manifest.KnownDLLs.Streamline_DLSS,
            GameAssetType.Streamline_DirectSR => manifest.KnownDLLs.Streamline_DirectSR,
            GameAssetType.Streamline_DeepDVC => manifest.KnownDLLs.Streamline_DeepDVC,
            GameAssetType.Streamline_Common => manifest.KnownDLLs.Streamline_Common,
            GameAssetType.DeepDVC => manifest.KnownDLLs.DeepDVC,
            GameAssetType.NvLowLatencyVK => manifest.KnownDLLs.NvLowLatencyVK,
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

//Copy to DLSS Swapper static assets file to ensure new installs have the latest version.
var dlssSwapperRepoStaticAssetsPath = Path.Combine("..", "..", "..", "..", "..", "..", "..", "dlss-swapper", "src", "Assets", "static_manifest.json");
if (File.Exists(dlssSwapperRepoStaticAssetsPath))
{
    File.Copy(Storage.OutputManifestPath, dlssSwapperRepoStaticAssetsPath, true);
}


// Cleanup.

if (Directory.Exists(Storage.TempFilesPath))
{
    Directory.Delete(Storage.TempFilesPath, true);
}


Log.CloseAndFlush();

Console.WriteLine();
Console.WriteLine($"Output Summary");
Console.WriteLine($"{"Verbose:", -15} {counterSink.VerboseCount}");
Console.WriteLine($"{"Debug:", -15} {counterSink.DebugCount}");
Console.WriteLine($"{"Information:", -15} {counterSink.InformationCount}");
Console.WriteLine($"{"Warning:", -15} {counterSink.WarningCount}");
Console.WriteLine($"{"Error:", -15} {counterSink.ErrorCount}");
Console.WriteLine($"{"Fatal:", -15} {counterSink.FatalCount}");
Console.WriteLine();
return 1;