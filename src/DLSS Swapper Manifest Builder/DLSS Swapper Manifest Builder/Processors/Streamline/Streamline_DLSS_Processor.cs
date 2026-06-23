using DLSS_Swapper.Data;
using DLSS_Swapper_Manifest_Builder.Downloaders.NVIDIA_RTX;
using System;
using System.Collections.Generic;
using System.Text;

namespace DLSS_Swapper_Manifest_Builder.Processors.Streamline;

internal class Streamline_DLSS_Processor : DLLProcessor
{
    public override GameAssetType GameAssetType => GameAssetType.Streamline_DLSS;

    public override string NamePath => "sl_dlss";

    public override string ExpectedDLLName => "sl.dlss.dll";

    public override string[] ValidFileDescriptions => new string[]
    {
        "SL.DLSS PLUGIN - PRODUCTION",
        "SL.DLSS PLUGIN - NOT FOR PRODUCTION",
    };

    public override string[] ExpectedPrefix => new string[]
    {
        "bin/x64/",
    };

    public override string[] ExpectedDevPrefix => new string[]
    {
        "bin/x64/development/",
    };

    public override Dictionary<string, string> CustomAdditionalLabels => new Dictionary<string, string>()
    {

    };

    public override Dictionary<string, string> DllSource => new Dictionary<string, string>()
    {

    };

    public override string[] DownloadedFilesPaths => [
        Path.Combine(Storage.DownloadedFilesPath, StreamlineDownloader.DownloadPathName),
    ];

    public Streamline_DLSS_Processor(List<DLLRecord> manifestDllRecords) : base(manifestDllRecords)
    {
    }
}