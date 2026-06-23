using DLSS_Swapper.Data;
using DLSS_Swapper_Manifest_Builder.Downloaders.NVIDIA_RTX;
using System;
using System.Collections.Generic;
using System.Text;

namespace DLSS_Swapper_Manifest_Builder.Processors.Streamline;

internal class Streamline_DLSS_D_Processor : DLLProcessor
{
    public override GameAssetType GameAssetType => GameAssetType.Streamline_DLSS_D;

    public override string NamePath => "sl_dlss_d";

    public override string ExpectedDLLName => "sl.dlss_d.dll";

    public override string[] ValidFileDescriptions => new string[]
    {
        "SL.DLSS_RR PLUGIN - PRODUCTION",
        "SL.DLSS_RR PLUGIN - NOT FOR PRODUCTION",
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

    public Streamline_DLSS_D_Processor(List<DLLRecord> manifestDllRecords) : base(manifestDllRecords)
    {
    }
}