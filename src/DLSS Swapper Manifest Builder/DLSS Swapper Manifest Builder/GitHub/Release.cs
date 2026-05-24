using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DLSS_Swapper_Manifest_Builder.GitHub;

public class Release
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("prerelease")]
    public bool PreRelease { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;

    [JsonPropertyName("published_at")]
    public string PublishedAt { get; set; } = string.Empty;
        
    [JsonPropertyName("assets")]
    public ReleaseAsset[] Assets { get; set; } = [];

    [JsonPropertyName("tarball_url")]
    public string TarballUrl { get; set; } = string.Empty;

    [JsonPropertyName("zipball_url")]
    public string ZipballUrl { get; set; } = string.Empty;
   
}
