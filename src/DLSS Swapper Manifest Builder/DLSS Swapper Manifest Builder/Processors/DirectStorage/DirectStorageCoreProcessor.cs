using DLSS_Swapper.Data;
using DLSS_Swapper_Manifest_Builder.Downloaders.Microsoft;
using System;
using System.Collections.Generic;
using System.Text;

namespace DLSS_Swapper_Manifest_Builder.Processors.DirectStorage;

internal class DirectStorageCoreProcessor : DLLProcessor
{
    public override string NamePath => "directstorage";

    public override string ExpectedDLLName => "dstoragecore.dll";

    public override string[] ValidFileDescriptions => new string[]
    {
        "dstoragecore.dll",
    };

    public override string[] ExpectedPrefix => new string[]
    {
        "/native/bin/x64/",
        "/bin/x64/"
    };

    public override string[] ExpectedDevPrefix => new string[0];

    public override Dictionary<string, string> CustomAdditionalLabels => new Dictionary<string, string>();

    public override Dictionary<string, string> DllSource => new Dictionary<string, string>();

    public override string[] DownloadedFilesPaths => [
        Path.Combine(Storage.DownloadedFilesPath, DirectStorageDownloader.DownloadPathName)
    ];

    public override GameAssetType GameAssetType => GameAssetType.DirectStorageCore;
    public DirectStorageCoreProcessor(List<DLLRecord> manifestDllRecords) : base(manifestDllRecords)
    {
    }
}
