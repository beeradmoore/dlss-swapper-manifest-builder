using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DLSS_Swapper_Manifest_Builder.Downloaders.NVIDIA;

internal class DLSSDownloader : ReleaseDownloader
{
    public override string DownloadPath => Path.Combine(Storage.DownloadedFilesPath, "DLSS");

    public DLSSDownloader()
    {

    }


    public override async Task DownloadAsync()
    {
        Log.Information("Checking downloads for DLSS");

        var githubHelper = new GithubHelper();
        var releases = await githubHelper.GetReleasesAsync("NVIDIA", "DLSS");

        foreach (var release in releases)
        {

            var outputFile = Path.Combine(DownloadPath, $"{release.TagName}.zip");

            if (TagToFileName.ContainsKey(release.TagName))
            {
                outputFile = Path.Combine(DownloadPath, TagToFileName[release.TagName]);
            }

            var result = await githubHelper.AttemptDownloadReleaseZipAsync("NVIDIA", "DLSS", release, outputFile);

            if (result.Success)
            {
                TagToFileName[release.TagName] = Path.GetFileName(result.DownloadedFilePath);
                SaveTagToFileName();
            }
        }
    }
}


