using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace DLSS_Swapper_Manifest_Builder.Downloaders.NVIDIA_RTX;

internal class StreamlineDownloader : ReleaseDownloader
{
    public override string DownloadPath => Path.Combine(Storage.DownloadedFilesPath, "Streamline");


    public StreamlineDownloader()
    {

    }


    public override async Task DownloadAsync()
    {
        Log.Information("Checking downloads for Streamline");

        var githubHelper = new GithubHelper();
        var releases = await githubHelper.GetReleasesAsync("NVIDIA-RTX", "Streamline");

        foreach (var release in releases)
        {
            foreach (var asset in release.Assets)
            {
                if (asset.Name.StartsWith("streamline-sdk") && asset.Name.EndsWith(".zip"))
                {
                    var outputFile = Path.Combine(DownloadPath, asset.Name);
                    await githubHelper.AttemptDownloadReleaseAssetAsync(asset, outputFile);
                }
                else
                {
                    // Unknown asset found.
                    Debugger.Break();
                }
            }
        }
    }
}
