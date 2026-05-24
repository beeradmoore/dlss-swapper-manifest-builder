using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace DLSS_Swapper_Manifest_Builder.Downloaders.Microsoft;

internal class DirectStorageDownloader : ReleaseDownloader
{
    public override string DownloadPath => Path.Combine(Storage.DownloadedFilesPath, "DirectStorage");


    public DirectStorageDownloader()
    {

    }


    public override async Task DownloadAsync()
    {
        Log.Information("Checking downloads for DirectStorage");

        var manifestUrl = $"https://api.nuget.org/v3-flatcontainer/Microsoft.Direct3D.DirectStorage/index.json";

        var versions = new List<string>();

        using (var stream = await DownloadHelper.HttpClient.GetStreamAsync(manifestUrl))
        {
            using (var jsonDocument = await JsonDocument.ParseAsync(stream))
            {
                var versionsElement = jsonDocument.RootElement.GetProperty("versions");
                
                for (var i = 0; i < versionsElement.GetArrayLength(); ++i)
                {
                    versions.Add(versionsElement[i].GetString() ?? string.Empty);
                }
            }
        }

        foreach (var version in versions)
        {
            var packageName = $"Microsoft.Direct3D.DirectStorage.{version}.nupkg";
            var outputPath = Path.Combine(DownloadPath, packageName);
            if (File.Exists(outputPath))
            {
                continue;
            }

            Log.Information($"Downloading {packageName}");
            var downloadUrl =   $"https://api.nuget.org/v3-flatcontainer/Microsoft.Direct3D.DirectStorage/{version}/Microsoft.Direct3D.DirectStorage.{version}.nupkg";
            var tempFile = Path.GetTempFileName();

            using (var fileStream = File.OpenWrite(tempFile))
            {
                using (var nupkgStream = await DownloadHelper.HttpClient.GetStreamAsync(downloadUrl))
                {
                    await nupkgStream.CopyToAsync(fileStream);
                }
            }

            File.Move(tempFile, outputPath);
        }
    }
}
