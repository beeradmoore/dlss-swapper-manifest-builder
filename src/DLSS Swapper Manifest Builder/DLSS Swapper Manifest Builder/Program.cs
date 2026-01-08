using System.Diagnostics;
using System.Text.Json;
using DLSS_Swapper_Manifest_Builder;
using DLSS_Swapper_Manifest_Builder.Processors;
using NewDLLHandler;
using Serilog;

Log.Logger = new LoggerConfiguration()
	.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
	.WriteTo.Console()
	.CreateLogger();

Log.Information("Starting processing");
//Log.Debug(

// Deleting directory is not instant, moving it is :|
if (Directory.Exists(DLLProcessor.OutputFilesPath))
{
    var newPath = Path.Combine(Path.GetDirectoryName(DLLProcessor.OutputFilesPath) ?? string.Empty, Path.GetRandomFileName());
    Directory.Move(DLLProcessor.OutputFilesPath, newPath);
    Directory.Delete(newPath, true);
}

if (Directory.Exists(DLLProcessor.TempFilesPath))
{
    var newPath = Path.Combine(Path.GetDirectoryName(DLLProcessor.TempFilesPath) ?? string.Empty, Path.GetRandomFileName());
    Directory.Move(DLLProcessor.TempFilesPath, newPath);
    Directory.Delete(newPath, true);
}

if (Directory.Exists(DLLProcessor.InputFilesPath) == false)
{
    Directory.CreateDirectory(DLLProcessor.InputFilesPath);
}

if (Directory.Exists(DLLProcessor.OutputFilesPath) == false)
{
    Directory.CreateDirectory(DLLProcessor.OutputFilesPath);
}

if (Directory.Exists(DLLProcessor.InputSDKsFilesPath) == false)
{
    Directory.CreateDirectory(DLLProcessor.InputSDKsFilesPath);
}


var httpClient = new HttpClient();

var shouldUpdateManifest = true;
if (File.Exists(DLLProcessor.InputManifestPath) == true)
{
    var fileInfo = new FileInfo(DLLProcessor.InputManifestPath);
    var lastModified = (DateTime.Now - fileInfo.LastWriteTime).TotalMinutes;
    if (lastModified < 60)
    {
        shouldUpdateManifest = false;
    }
}

if (shouldUpdateManifest)
{
    var manifestData = await httpClient.GetStringAsync("https://beeradmoore.github.io/dlss-swapper/manifest.json");
    File.WriteAllText(DLLProcessor.InputManifestPath, manifestData);
}


var manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(DLLProcessor.InputManifestPath));
if (manifest == null)
{
    Log.Information($"Could not load {DLLProcessor.InputManifestPath}.");
    return 1;
}

var dlssProcessor = new DLSSProcessor();
var dlssgProcessor = new DLSSGProcessor();
var dlssdProcessor = new DLSSDProcessor();
var fsr31dx12Processor = new FSR31DX12Processor();
var fsr31vkProcessor = new FSR31VKProcessor();
var xessProcessor = new XeSSProcessor();
var xellProcessor = new XeLLProcessor();
var xessfgProcessor = new XeSSFGProcessor();
var xessDX11Processor = new XeSSDX11Processor();


await dlssProcessor.DownloadExistingRecordsAsync(manifest.DLSS);
await dlssgProcessor.DownloadExistingRecordsAsync(manifest.DLSS_G);
await dlssdProcessor.DownloadExistingRecordsAsync(manifest.DLSS_D);
await fsr31dx12Processor.DownloadExistingRecordsAsync(manifest.FSR_31_DX12);
await fsr31vkProcessor.DownloadExistingRecordsAsync(manifest.FSR_31_VK);
await xessProcessor.DownloadExistingRecordsAsync(manifest.XeSS);
await xellProcessor.DownloadExistingRecordsAsync(manifest.XeLL);
await xessfgProcessor.DownloadExistingRecordsAsync(manifest.XeSS_FG);
await xessDX11Processor.DownloadExistingRecordsAsync(manifest.XeSS_DX11);

/*
dlssProcessor.PostProcessRecords(manifest.DLSS, DLSS_Swapper.Data.GameAssetType.DLSS);
dlssgProcessor.PostProcessRecords(manifest.DLSS_G, DLSS_Swapper.Data.GameAssetType.DLSS_G);
dlssdProcessor.PostProcessRecords(manifest.DLSS_D, DLSS_Swapper.Data.GameAssetType.DLSS_D);
fsr31dx12Processor.PostProcessRecords(manifest.FSR_31_DX12, DLSS_Swapper.Data.GameAssetType.FSR_31_DX12);
fsr31vkProcessor.PostProcessRecords(manifest.FSR_31_VK, DLSS_Swapper.Data.GameAssetType.FSR_31_VK);
xessProcessor.PostProcessRecords(manifest.XeSS, DLSS_Swapper.Data.GameAssetType.XeSS);
xellProcessor.PostProcessRecords(manifest.XeLL, DLSS_Swapper.Data.GameAssetType.XeLL);
xessfgProcessor.PostProcessRecords(manifest.XeSS_FG, DLSS_Swapper.Data.GameAssetType.XeSS_FG);
xessDX11Processor.PostProcessRecords(manifest.XeSS_DX11, DLSS_Swapper.Data.GameAssetType.XeSS_DX11);
*/

/*
dlssProcessor.MoveToCorrectLocations(manifest.DLSS, DLSS_Swapper.Data.GameAssetType.DLSS);
dlssgProcessor.MoveToCorrectLocations(manifest.DLSS_G, DLSS_Swapper.Data.GameAssetType.DLSS_G);
dlssdProcessor.MoveToCorrectLocations(manifest.DLSS_D, DLSS_Swapper.Data.GameAssetType.DLSS_D);
fsr31dx12Processor.MoveToCorrectLocations(manifest.FSR_31_DX12, DLSS_Swapper.Data.GameAssetType.FSR_31_DX12);
fsr31vkProcessor.MoveToCorrectLocations(manifest.FSR_31_VK, DLSS_Swapper.Data.GameAssetType.FSR_31_VK);
xessProcessor.MoveToCorrectLocations(manifest.XeSS, DLSS_Swapper.Data.GameAssetType.XeSS);
xellProcessor.MoveToCorrectLocations(manifest.XeLL, DLSS_Swapper.Data.GameAssetType.XeLL);
xessfgProcessor.MoveToCorrectLocations(manifest.XeSS_FG, DLSS_Swapper.Data.GameAssetType.XeSS_FG);
xessDX11Processor.MoveToCorrectLocations(manifest.XeSS_DX11, DLSS_Swapper.Data.GameAssetType.XeSS_DX11);
*/

/*
dlssProcessor.MoveOldToNew(manifest.DLSS, DLSS_Swapper.Data.GameAssetType.DLSS);
dlssgProcessor.MoveOldToNew(manifest.DLSS_G, DLSS_Swapper.Data.GameAssetType.DLSS_G);
dlssdProcessor.MoveOldToNew(manifest.DLSS_D, DLSS_Swapper.Data.GameAssetType.DLSS_D);
fsr31dx12Processor.MoveOldToNew(manifest.FSR_31_DX12, DLSS_Swapper.Data.GameAssetType.FSR_31_DX12);
fsr31vkProcessor.MoveOldToNew(manifest.FSR_31_VK, DLSS_Swapper.Data.GameAssetType.FSR_31_VK);
xessProcessor.MoveOldToNew(manifest.XeSS, DLSS_Swapper.Data.GameAssetType.XeSS);
xellProcessor.MoveOldToNew(manifest.XeLL, DLSS_Swapper.Data.GameAssetType.XeLL);
xessfgProcessor.MoveOldToNew(manifest.XeSS_FG, DLSS_Swapper.Data.GameAssetType.XeSS_FG);
xessDX11Processor.MoveOldToNew(manifest.XeSS_DX11, DLSS_Swapper.Data.GameAssetType.XeSS_DX11);

manifest.DLSS.Clear();
manifest.DLSS_G.Clear();
manifest.DLSS_D.Clear();
manifest.FSR_31_DX12.Clear();
manifest.FSR_31_VK.Clear();
manifest.XeSS.Clear();
manifest.XeLL.Clear();
manifest.XeSS_FG.Clear();
manifest.XeSS_DX11.Clear();
*/


manifest.DLSS = dlssProcessor.ProcessLocalFiles(manifest.DLSS);
manifest.DLSS_G = dlssgProcessor.ProcessLocalFiles(manifest.DLSS_G);
manifest.DLSS_D = dlssdProcessor.ProcessLocalFiles(manifest.DLSS_D);
manifest.FSR_31_DX12 = fsr31dx12Processor.ProcessLocalFiles(manifest.FSR_31_DX12);
manifest.FSR_31_VK = fsr31vkProcessor.ProcessLocalFiles(manifest.FSR_31_VK);
manifest.XeSS = xessProcessor.ProcessLocalFiles(manifest.XeSS);
manifest.XeLL = xellProcessor.ProcessLocalFiles(manifest.XeLL);
manifest.XeSS_FG = xessfgProcessor.ProcessLocalFiles(manifest.XeSS_FG);
manifest.XeSS_DX11 = xessDX11Processor.ProcessLocalFiles(manifest.XeSS_DX11);

var knownDLLSourcesMissingPath = Path.Combine("..", "..", "..", "..", "..", "..", "known_dll_sources_missing.json");
using (var stream = File.OpenRead(knownDLLSourcesMissingPath))
{
    var knownDLLSourcesMissing = await JsonSerializer.DeserializeAsync<Dictionary<string, List<KnownDLL>>>(stream);
    if (knownDLLSourcesMissing is null)
    {
        Debugger.Break();
        return 0;
	}

	if (knownDLLSourcesMissing.TryGetValue("DLSS", out var DLSS))
	{
		manifest.KnownDLLs.DLSS = DLSS.Select(x => x.ToHashedKnownDLL()).ToList();
	}

	if (knownDLLSourcesMissing.TryGetValue("DLSS_G", out var DLSS_G))
	{	
		manifest.KnownDLLs.DLSS_G = DLSS_G.Select(x => x.ToHashedKnownDLL()).ToList();
	}

	if (knownDLLSourcesMissing.TryGetValue("DLSS_D", out var DLSS_D))
	{
		manifest.KnownDLLs.DLSS_D = DLSS_D.Select(x => x.ToHashedKnownDLL()).ToList();
	}

	if (knownDLLSourcesMissing.TryGetValue("FSR_31_DX12", out var FSR_31_DX12))
	{
		manifest.KnownDLLs.FSR_31_DX12 = FSR_31_DX12.Select(x => x.ToHashedKnownDLL()).ToList();
	}

	if (knownDLLSourcesMissing.TryGetValue("FSR_31_VK", out var FSR_31_VK))
	{
		manifest.KnownDLLs.FSR_31_VK = FSR_31_VK.Select(x => x.ToHashedKnownDLL()).ToList();
	}

	if (knownDLLSourcesMissing.TryGetValue("XeSS", out var XeSS))
	{
		manifest.KnownDLLs.XeSS = XeSS.Select(x => x.ToHashedKnownDLL()).ToList();
	}

	if (knownDLLSourcesMissing.TryGetValue("XeLL", out var XeLL))
	{
		manifest.KnownDLLs.XeLL = XeLL.Select(x => x.ToHashedKnownDLL()).ToList();
	}

	if (knownDLLSourcesMissing.TryGetValue("XeSS_FG", out var XeSS_FG))
	{
		manifest.KnownDLLs.XeSS_FG = XeSS_FG.Select(x => x.ToHashedKnownDLL()).ToList();
	}

	if (knownDLLSourcesMissing.TryGetValue("XeSS_DX11", out var XeSS_DX11))
	{
		manifest.KnownDLLs.XeSS_DX11 = XeSS_DX11.Select(x => x.ToHashedKnownDLL()).ToList();
	}
}

var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions() { WriteIndented = true });

File.WriteAllText(DLLProcessor.OutputManifestPath, manifestJson);

// Copy to root of the repo
var repoRootManifestPath = Path.Combine("..", "..", "..", "..", "..", "..", "manifest.json");
File.Copy(DLLProcessor.OutputManifestPath, repoRootManifestPath, true);


if (Directory.Exists(DLLProcessor.TempFilesPath))
{
    Directory.Delete(DLLProcessor.TempFilesPath, true);
}

return 1;