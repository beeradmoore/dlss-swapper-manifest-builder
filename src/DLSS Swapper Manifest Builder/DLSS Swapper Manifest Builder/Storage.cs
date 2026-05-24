using DLSS_Swapper_Manifest_Builder.Processors;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace DLSS_Swapper_Manifest_Builder;

internal class Storage
{
    // In debug mode, don't place these in the built directories as they will be removed when cleaned
#if DEBUG
    public static string BaseStoragePath => Path.Combine("..", "..", "..", "..");
#else
    public static string BaseStoragePath => string.Empty;
#endif

    public static string GeneratedFilesPath => Path.Combine(BaseStoragePath, "generated_files");
    public static string InputFilesPath => Path.Combine(GeneratedFilesPath, "input_files");
    public static string OutputFilesPath => Path.Combine(GeneratedFilesPath, "output_files");
    public static string DownloadedFilesPath => Path.Combine(GeneratedFilesPath, "downloaded_files");
    public static string CacheFilesPath => Path.Combine(GeneratedFilesPath, "cache");

#if DEBUG
    public static string TempFilesPath => Path.Combine(Path.GetTempPath(), "dlss_swapper_manifest_builder");
#else
    public static string InputFilesPath => "input_files";
    public static string OutputFilesPath => "output_files";
    public static string TempFilesPath => "temp_files";
#endif


    public static string InputManifestPath => Path.Combine(BaseStoragePath, "..", "..", "manifest.json");
    public static string OutputManifestPath => Path.Combine(OutputFilesPath, "manifest.json");
    public static string InputSDKsFilesPath => Path.Combine(InputFilesPath, "sdks");


    internal static void CreateDirectories()
    {
        // Deleting directory is not instant, moving it is :|
        if (Directory.Exists(OutputFilesPath))
        {
            var newPath = Path.Combine(Path.GetDirectoryName(OutputFilesPath) ?? string.Empty, Path.GetRandomFileName());
            Directory.Move(OutputFilesPath, newPath);
            Directory.Delete(newPath, true);
        }

        if (Directory.Exists(Storage.TempFilesPath))
        {
            var newPath = Path.Combine(Path.GetDirectoryName(Storage.TempFilesPath) ?? string.Empty, Path.GetRandomFileName());
            Directory.Move(Storage.TempFilesPath, newPath);
            Directory.Delete(newPath, true);
        }


        Directory.CreateDirectory(InputFilesPath);
        Directory.CreateDirectory(OutputFilesPath);
        Directory.CreateDirectory(TempFilesPath);
        Directory.CreateDirectory(DownloadedFilesPath);
        Directory.CreateDirectory(CacheFilesPath);
        Directory.CreateDirectory(InputSDKsFilesPath);

    }

    public static string GetSHA256(string path)
    {
        if (File.Exists(path) == false)
        {
            return string.Empty;
        }

        using (var fileStream = File.OpenRead(path))
        {
            return GetSHA256(fileStream);
        }
    }

    public static string GetSHA256(FileStream fileStream)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(fileStream);
            return Convert.ToHexString(hash);
        }
    }
}

