using DLSS_Swapper_Manifest_Builder;
using DLSS_Swapper_Manifest_Builder.Processors;
using NewDLLHandler;
using System.Diagnostics;
using System.Text.Json;

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

//if (File.Exists(DLLProcessor.InputManifestPath) == false)
//{
    var manifestData = await httpClient.GetStringAsync("https://dlss-swapper-downloads.beeradmoore.com/manifest.json");
    File.WriteAllText(DLLProcessor.InputManifestPath, manifestData);
//}


var manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(DLLProcessor.InputManifestPath));
if (manifest == null)
{
    Console.WriteLine($"Could not load {DLLProcessor.InputManifestPath}.");
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

await dlssProcessor.DownloadExistingRecordsAsync(manifest.DLSS);
manifest.DLSS = dlssProcessor.ProcessLocalFiles(manifest.DLSS);

await dlssgProcessor.DownloadExistingRecordsAsync(manifest.DLSS_G);
manifest.DLSS_G = dlssgProcessor.ProcessLocalFiles(manifest.DLSS_G);

await dlssdProcessor.DownloadExistingRecordsAsync(manifest.DLSS_D);
manifest.DLSS_D = dlssdProcessor.ProcessLocalFiles(manifest.DLSS_D);

await fsr31dx12Processor.DownloadExistingRecordsAsync(manifest.FSR_31_DX12);
manifest.FSR_31_DX12 = fsr31dx12Processor.ProcessLocalFiles(manifest.FSR_31_DX12);

await fsr31vkProcessor.DownloadExistingRecordsAsync(manifest.FSR_31_VK);
manifest.FSR_31_VK = fsr31vkProcessor.ProcessLocalFiles(manifest.FSR_31_VK);

await xessProcessor.DownloadExistingRecordsAsync(manifest.XeSS);
manifest.XeSS = xessProcessor.ProcessLocalFiles(manifest.XeSS);

await xellProcessor.DownloadExistingRecordsAsync(manifest.XeLL);
manifest.XeLL = xellProcessor.ProcessLocalFiles(manifest.XeLL);

await xessfgProcessor.DownloadExistingRecordsAsync(manifest.XeSS_FG);
manifest.XeSS_FG = xessfgProcessor.ProcessLocalFiles(manifest.XeSS_FG);


var knownDLLSourcesMissingPath = Path.Combine("..", "..", "..", "..", "..", "..", "known_dll_sources_missing.json");
using (var stream = File.OpenRead(knownDLLSourcesMissingPath))
{
    var knownDLLSourcesMissing = await JsonSerializer.DeserializeAsync<Dictionary<string, List<KnownDLL>>>(stream);
    if (knownDLLSourcesMissing is null)
    {
        Debugger.Break();
        return 0;
    }

    manifest.KnownDLLs.DLSS = knownDLLSourcesMissing["DLSS"].Select(x => x.ToHashedKnownDLL()).ToList();
    manifest.KnownDLLs.DLSS_G = knownDLLSourcesMissing["DLSS_G"].Select(x => x.ToHashedKnownDLL()).ToList();
    manifest.KnownDLLs.DLSS_D = knownDLLSourcesMissing["DLSS_D"].Select(x => x.ToHashedKnownDLL()).ToList();
    manifest.KnownDLLs.FSR_31_DX12 = knownDLLSourcesMissing["FSR_31_DX12"].Select(x => x.ToHashedKnownDLL()).ToList();
    manifest.KnownDLLs.FSR_31_VK = knownDLLSourcesMissing["FSR_31_VK"].Select(x => x.ToHashedKnownDLL()).ToList();
    manifest.KnownDLLs.XeSS = knownDLLSourcesMissing["XeSS"].Select(x => x.ToHashedKnownDLL()).ToList();
    manifest.KnownDLLs.XeLL = knownDLLSourcesMissing["XeLL"].Select(x => x.ToHashedKnownDLL()).ToList();
    manifest.KnownDLLs.XeSS_FG = knownDLLSourcesMissing["XeSS_FG"].Select(x => x.ToHashedKnownDLL()).ToList();
}

var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions() { WriteIndented = true });

File.WriteAllText(DLLProcessor.OutputManifestPath, manifestJson);

// Copy to root of the repo
var repoRootManifestPath = Path.Combine("..", "..", "..", "..", "..", "..", "manifest.json");
File.Copy(DLLProcessor.OutputManifestPath, repoRootManifestPath, true);

return 1;