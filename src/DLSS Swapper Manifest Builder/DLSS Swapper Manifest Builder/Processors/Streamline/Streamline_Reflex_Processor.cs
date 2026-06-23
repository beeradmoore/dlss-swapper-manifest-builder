using DLSS_Swapper.Data;
using DLSS_Swapper_Manifest_Builder.Downloaders.NVIDIA_RTX;
using System;
using System.Collections.Generic;
using System.Text;

namespace DLSS_Swapper_Manifest_Builder.Processors.Streamline;

internal class Streamline_Reflex_Processor : DLLProcessor
{
    public override GameAssetType GameAssetType => GameAssetType.Streamline_Reflex;

    public override string NamePath => "sl_reflex";

    public override string ExpectedDLLName => "sl.reflex.dll";

    public override string[] ValidFileDescriptions => new string[]
    {
        "SL.REFLEX PLUGIN - PRODUCTION",
        "SL.REFLEX PLUGIN - NOT FOR PRODUCTION",
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

    public Streamline_Reflex_Processor(List<DLLRecord> manifestDllRecords) : base(manifestDllRecords)
    {
    }
}