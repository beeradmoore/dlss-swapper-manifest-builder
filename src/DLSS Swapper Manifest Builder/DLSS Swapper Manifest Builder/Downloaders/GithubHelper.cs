using DLSS_Swapper_Manifest_Builder.GitHub;
using MonkeyCache.FileStore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace DLSS_Swapper_Manifest_Builder.Downloaders;


internal class GithubHelper
{
	//public static GitHubClient GithubClient { get; } = new GitHubClient(new ProductHeaderValue("dlss-swapper-manifest-builder"));


	static GithubHelper()
	{

	}

    public void Authenticate()
	{

		// For public repos, no auth is strictly required.
		// For higher rate limits, uncomment:
		// client.Credentials = new Credentials("GITHUB_TOKEN");
	}


    async Task<string> GetStringFromGitHubAsync(string url)
    {
        return await DownloadHelper.GetAsync(url, new Dictionary<string, string>()
        {
            { "Accept", "application/vnd.github+json" },
            { "X-GitHub-Api-Version", "2026-03-10" },
        });
    }

	public async Task<IReadOnlyList<Release>> GetReleasesAsync(string owner, string repo)
	{
		try
		{
			var releasesString = await GetStringFromGitHubAsync($"https://api.github.com/repos/{owner}/{repo}/releases");
			
			var releases = JsonSerializer.Deserialize<IReadOnlyList<Release>>(releasesString);
			if (releases is null)
			{
				return [];
			}
            
			return releases;
        }
		catch (Exception ex)
		{
			Log.Error(ex, $"Could not run GetReleasesAsync on {owner}/{repo}");
			return [];
		}
	}

	public bool VerifyDigest(ReleaseAsset asset, string path)
	{
		if (File.Exists(path) == false)
		{
			return false;
		}

		using (var fileStream = File.OpenRead(path))
		{
			if (asset.Digest is null)
			{
				Log.Warning($"ReleaseAsset digest is null for {asset.Name}. Falling back to size comparison.");
				return (asset.Size == fileStream.Length);
			}


			if (asset.Digest.StartsWith("sha256", StringComparison.OrdinalIgnoreCase))
			{
				var sha256Hash = Storage.GetSHA256(fileStream);
				return asset.Digest.Equals($"sha256:{sha256Hash}", StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				Debugger.Break();
				return false;
			}
		}
	}

	public async Task<bool> AttemptDownloadReleaseAssetAsync(ReleaseAsset asset, string outputFile)
	{
        if (File.Exists(outputFile))
        {
            var isHashValid = VerifyDigest(asset, outputFile);
            if (isHashValid)
            {
				return true;
            }
        }

        Log.Information($"Downloading {asset.Name}");
        var tempFile = Path.GetTempFileName();

		try
		{
			using (var fileStream = File.OpenWrite(tempFile))
			{
				using (var stream = await DownloadHelper.HttpClient.GetStreamAsync(asset.BrowserDownloadUrl))
				{
					await stream.CopyToAsync(fileStream);
				}
			}

			var isHashValid = VerifyDigest(asset, tempFile);
			if (isHashValid)
			{
                File.Move(tempFile, outputFile, true);
				return true;
            }
			else
			{
				Log.Error($"AttemptDownloadReleaseAssetAsync: Downloaded asset {asset.Name} invalid digest.");
				return false;
			}

		}
		catch (Exception ex)
		{
			Log.Error(ex, $"AttemptDownloadReleaseAssetAsync: Could not download {asset.Name}.");
			return false;
		}
    }

    public async Task<(bool Success, string DownloadedFilePath)> AttemptDownloadReleaseZipAsync(string owner, string repo, Release release, string outputFile)
    {
        if (File.Exists(outputFile))
        {
			Log.Warning($"No way to validate {release.Name}");
            return (true, outputFile);    
        }

        Log.Information($"Downloading {release.Name}");
        var tempFile = Path.GetTempFileName();

        try
        {
            var downloadedFilePath = outputFile;

            using (var fileStream = File.OpenWrite(tempFile))
            {
                //

                //				release.TagName
                // release.ZipballUrl
                var downloadUrl = $"https://github.com/{owner}/{repo}/archive/refs/tags/{release.TagName}.zip";
					

                var suggestedFilename = await DownloadHelper.FollowRedirectsAndDownload(downloadUrl, fileStream);

				if (string.IsNullOrWhiteSpace(suggestedFilename))
				{
					Debugger.Break();
				}
                var directory = Path.GetDirectoryName(outputFile) ?? string.Empty;
                downloadedFilePath = Path.Combine(directory, suggestedFilename);
            }

			File.Move(tempFile, downloadedFilePath, true);
			return (true, downloadedFilePath);
          
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"AttemptDownloadReleaseZipAsync: Could not download {release.Name}.");
            return (false, string.Empty);
        }
    }
}