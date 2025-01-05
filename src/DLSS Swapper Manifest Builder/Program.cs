

using DLSS_Swapper_Manifest_Builder;
using DLSS_Swapper_Manifest_Builder.Processors;
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

var httpClient = new HttpClient();

if (File.Exists(DLLProcessor.InputManifestPath) == false)
{
    var manifestData = await httpClient.GetStringAsync("https://downloads.dlss-swapper.beeradmoore.com/manifest.json");
    File.WriteAllText(DLLProcessor.InputManifestPath, manifestData);
}

var manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(DLLProcessor.InputManifestPath));

if (manifest == null)
{
    manifest = new Manifest();

    Console.WriteLine("Could not find manifest.json.");
    return 1;
}



var dlssProcessor = new DLSSProcessor();
var dlssgProcessor = new DLSSGProcessor();
var dlssdProcessor = new DLSSDProcessor();
var xessProcessor = new XeSSProcessor();
var fsr31dxProcessor = new FSR31DX12Processor();
var fsr31vkProcessor = new FSR31VKProcessor();

/*
await dlssProcessor.DownloadExistingRecordsAsync(manifest.DLSS);
manifest.DLSS = dlssProcessor.ProcessLocalFiles(manifest.DLSS);
*/

/*
await dlssgProcessor.DownloadExistingRecordsAsync(manifest.DLSS_G);
manifest.DLSS_G = dlssgProcessor.ProcessLocalFiles(manifest.DLSS_G);
*/

/*
await dlssdProcessor.DownloadExistingRecordsAsync(manifest.DLSS_D);
manifest.DLSS_D = dlssdProcessor.ProcessLocalFiles(manifest.DLSS_D);
*/

/*
await xessProcessor.DownloadExistingRecordsAsync(manifest.XeSS);
manifest.XeSS = xessProcessor.ProcessLocalFiles(manifest.XeSS);
*/

/*
await fsr31dxProcessor.DownloadExistingRecordsAsync(manifest.FSR_31_DX12);
manifest.FSR_31_DX12 = fsr31dxProcessor.ProcessLocalFiles(manifest.FSR_31_DX12);
*/

/*
await fsr31vkProcessor.DownloadExistingRecordsAsync(manifest.FSR_31_VK);
manifest.FSR_31_VK = fsr31vkProcessor.ProcessLocalFiles(manifest.FSR_31_VK);
*/


var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions() { WriteIndented = true });
File.WriteAllText(DLLProcessor.OutputManifestPath, manifestJson);

// Copy to root of the repo
var repoRootManifestPath = Path.Combine("..", "..", "..", "..", "..", "manifest.json");
File.Copy(DLLProcessor.OutputManifestPath, repoRootManifestPath, true);

return 1;