using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DLSS_Swapper_Manifest_Builder.Downloaders.AMD;

internal class FidelityFXDownloader : ReleaseDownloader
{
    public override string DownloadPath => Path.Combine(Storage.DownloadedFilesPath, "FidelityFX-SDK");

    public FidelityFXDownloader()
    {

    }

    public override async Task DownloadAsync()
    {
        Log.Information("Checking downloads for FidelityFX");

        var githubHelper = new GithubHelper();
        var releases = await githubHelper.GetReleasesAsync("GPUOpen-LibrariesAndSDKs", "FidelityFX-SDK");
        
        foreach (var release in releases)
        {
            var outputFile = Path.Combine(DownloadPath, $"{release.TagName}.zip");

            if (TagToFileName.ContainsKey(release.TagName))
            {
                outputFile = Path.Combine(DownloadPath, TagToFileName[release.TagName]);
            }

            var result = await githubHelper.AttemptDownloadReleaseZipAsync("GPUOpen-LibrariesAndSDKs", "FidelityFX-SDK", release, outputFile);
            
            if (result.Success)
            {
                TagToFileName[release.TagName] = Path.GetFileName(result.DownloadedFilePath);
                SaveTagToFileName();
            }
        }

    }
}
