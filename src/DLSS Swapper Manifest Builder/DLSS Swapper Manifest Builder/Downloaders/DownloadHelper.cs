using DLSS_Swapper_Manifest_Builder.GitHub;
using MonkeyCache;
using MonkeyCache.FileStore;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using static System.Net.WebRequestMethods;

namespace DLSS_Swapper_Manifest_Builder.Downloaders;

internal class DownloadHelper
{
    public static HttpClient HttpClient { get; }

    static DownloadHelper()
    {
        var httpClientHandler = new HttpClientHandler()
        {
            AllowAutoRedirect = false,
        };

        HttpClient = new HttpClient(httpClientHandler);
        HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("dlss-swapper-maninfest-builder", "1.0"));
        
        BarrelUtils.SetBaseCachePath(Path.Combine(Storage.CacheFilesPath, "HttpClient"));
        Barrel.ApplicationId = "dlss-swapper-manifest-builder";
    }

    public static async Task<string> GetAsync(string url, Dictionary<string, string>? headers = null)
    {
        if (Barrel.Current.IsExpired(url) == false)
        {
            return Barrel.Current.Get<string>(url);
        }

        try
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                if (headers is not null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                var httpResponseMessage = await HttpClient.SendAsync(request);
                httpResponseMessage.EnsureSuccessStatusCode();

                var result = await httpResponseMessage.Content.ReadAsStringAsync();

                Barrel.Current.Add(url, result, TimeSpan.FromHours(1));

                return result;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Could not call GetAsync on {url}");
            return string.Empty;
        }
    }

    public static async Task<string> FollowRedirectsAndDownload(string url, FileStream fileStream, int redirectCount = 0)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new Exception("No URL given.");
        }

        if (redirectCount > 10)
        {
            throw new Exception("Too many redirects.");
        }

        using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url))
        {
            using (var httpResponseMessage = await HttpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead))
            {
                // Handle redirect.
                if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Redirect || 
                    httpResponseMessage.StatusCode == System.Net.HttpStatusCode.PermanentRedirect ||
                    httpResponseMessage.StatusCode == System.Net.HttpStatusCode.TemporaryRedirect)
                {
                    var newUrl = httpResponseMessage.Headers.Location?.AbsoluteUri ?? string.Empty;
                    return await FollowRedirectsAndDownload(newUrl, fileStream, redirectCount + 1);
                }

                httpResponseMessage.EnsureSuccessStatusCode();

                await httpResponseMessage.Content.CopyToAsync(fileStream);

                // If there is a filename provided by headers use it.
                if (string.IsNullOrEmpty(httpResponseMessage.Content.Headers.ContentDisposition?.FileName) == false)
                {
                    return httpResponseMessage.Content.Headers.ContentDisposition.FileName;
                }

                return string.Empty;
            }
        }
    }
}
