using DLSS_Swapper.Data;
using DLSS_Swapper_Manifest_Builder.Downloaders.NVIDIA;
using DLSS_Swapper_Manifest_Builder.Downloaders.NVIDIA_RTX;
using DLSS_Swapper_Manifest_Builder.Processors.DLLSets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace DLSS_Swapper_Manifest_Builder.Processors;

internal class DLSSNRProcessor : DLLProcessor
{
    public override string NamePath => "dlss_nr";
    public override string ExpectedDLLName => "nvngx_dlssnr.dll";
    public override string[] ValidFileDescriptions => new string[]
    {
        "NVIDIA DLSS-NR - DVS PRODUCTION",
    };
    public override string[] ExpectedPrefix => new string[]
    {
        "Windows_x86_64/rel/", // used for DLSS SDK
		"bin/x64/",  // used for Streamline SDK
        "/",
    };
    public override string[] ExpectedDevPrefix => new string[]
    {
        "Windows_x86_64/dev/", // used for DLSS SDK
        "bin/x64/development/",  // used for Streamline SDK
    };
    public override Dictionary<string, string> CustomAdditionalLabels => new Dictionary<string, string>()
    {
    };
    public override Dictionary<string, string> DllSource => new Dictionary<string, string>()
    {

    };

    public override string[] DownloadedFilesPaths => [
        Path.Combine(Storage.DownloadedFilesPath, DLSSDownloader.DownloadPathName),
        Path.Combine(Storage.DownloadedFilesPath, StreamlineDownloader.DownloadPathName),
    ];

    public override List<DLLRecord> ProcessLocalFiles(IReadOnlyList<DLLRecord> existingRecords)
    {
        var modelPath = @"C:\ProgramData\NVIDIA\NGX\models\dlssg\versions\";
        var binFiles = Directory.GetFiles(modelPath, "*.bin", SearchOption.AllDirectories);

        foreach (var binFile in binFiles)
        {
            var md5Hash = string.Empty;
            using (var fileStream = File.OpenRead(binFile))
            {
                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(fileStream);
                    md5Hash = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();

                    // Check if the file is an exact match.
                    if (existingRecords.Any(x => x.MD5Hash.Equals(md5Hash, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        // If exact match, skip it.
                        continue;
                    }
                }
            }

            // Check if we have an existing DLL of the same version.
            var fileInfo = new FileInfo(binFile);
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(binFile);
            var productVersion = fileVersionInfo.ProductVersion?.Replace(',', '.') ?? string.Empty;

            if (string.IsNullOrWhiteSpace(productVersion))
            {
                // We should never get here.
                Debugger.Break();
                continue;
            }

            // Even though the DLL is different we don't want 50 copies of the same v1.2.3.4
            if (existingRecords.Any(x => x.Version.Equals(productVersion, StringComparison.InvariantCultureIgnoreCase)))
            {
                continue;
            }

            Log.Information($"dlss_nr - {productVersion} - {md5Hash}");

            // TODO: Handle new files.
        }

        var processedFiles = base.ProcessLocalFiles(existingRecords);
        return processedFiles;
    }

    public override GameAssetType GameAssetType => GameAssetType.DLSS_NR;
    public override DLLSetType DLLSetType => DLLSetType.DLSS;

    public DLSSNRProcessor(List<DLLRecord> manifestDllRecords) : base(manifestDllRecords)
    {
    }
}
