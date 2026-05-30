using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DLSS_Swapper_Manifest_Builder.Downloaders.Intel;

internal class XeSSDownloader : ReleaseDownloader
{
    public new const string DownloadPathName = "XeSS";
    public override string DownloadPath => Path.Combine(Storage.DownloadedFilesPath, DownloadPathName);

    public XeSSDownloader()
    {

    }

    public override async Task DownloadAsync()
    {
        Log.Information("Checking downloads for XeSS");

        var githubHelper = new GithubHelper();
        var releases = await githubHelper.GetReleasesAsync("intel", "xess");

        foreach (var release in releases)
        {
            foreach (var asset in release.Assets)
            {
                if ((asset.Name.StartsWith("XeSS_SDK_") || asset.Name.StartsWith("XeSS_SDK-") || asset.Name.StartsWith("XeSS.SDK.")) 
                    && asset.Name.EndsWith(".zip"))
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
