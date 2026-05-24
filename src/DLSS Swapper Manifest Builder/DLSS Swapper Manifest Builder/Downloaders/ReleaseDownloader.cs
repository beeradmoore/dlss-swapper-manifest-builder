using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DLSS_Swapper_Manifest_Builder.Downloaders;

public abstract class ReleaseDownloader
{
    public abstract string DownloadPath { get; }

    public Dictionary<string, string> TagToFileName { get; } = new Dictionary<string, string>();

    string _tagToFileNameFile;

    public ReleaseDownloader()
    {
        Directory.CreateDirectory(DownloadPath);
        
        _tagToFileNameFile = Path.Combine(DownloadPath, "tag_to_filename.json");
        if (File.Exists(_tagToFileNameFile))
        {
            LoadTagToFileName();
        }
    }

    public abstract Task DownloadAsync();

    protected void LoadTagToFileName()
    {
        if (File.Exists(_tagToFileNameFile))
        {
            using (var fileStream = File.OpenRead(_tagToFileNameFile))
            {
                var tagToFileName = JsonSerializer.Deserialize<Dictionary<string, string>>(fileStream);
                if (tagToFileName is not null)
                {
                    TagToFileName.Clear();

                    foreach (var entry in tagToFileName)
                    {
                        TagToFileName[entry.Key] = entry.Value;
                    }
                }
            }
        }
    }

    protected void SaveTagToFileName()
    {
        var json = JsonSerializer.Serialize(TagToFileName, new JsonSerializerOptions() {  WriteIndented = true });
        File.WriteAllText(_tagToFileNameFile, json);
    }
}
