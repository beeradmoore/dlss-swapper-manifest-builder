using DLSS_Swapper.Data;
using DLSS_Swapper_Manifest_Builder.Downloaders.AMD;
using System;
using System.Collections.Generic;
using System.Text;

namespace DLSS_Swapper_Manifest_Builder.Processors.FidelityFX_SDK2;

internal class FidelityFX2_FrameGeneration_DX12_Processor : DLLProcessor
{
    public override GameAssetType GameAssetType => GameAssetType.FidelityFX_SDK2_FrameGeneration_DX12;

    public override string NamePath => "fidelityfx_sdk2_framegeneration_dx12";

    public override string ExpectedDLLName => "amd_fidelityfx_framegeneration_dx12.dll";

    public override string[] ValidFileDescriptions => new string[]
    {
        "FFXDLL_FRAMEGENERATION_VERSION_API AMD FidelityFX Frame Generation Library",
        "AMD FidelityFX (DirectX) Frame Generation Library",
    };

    public override string[] ExpectedPrefix => new string[]
    {
        "Kits/FidelityFX/signedbin/",
    };

    public override string[] ExpectedDevPrefix => new string[]
    {

    };

    public override Dictionary<string, string> CustomAdditionalLabels => new Dictionary<string, string>()
    {

    };

    public override Dictionary<string, string> DllSource => new Dictionary<string, string>()
    {

    };

    public override string[] DownloadedFilesPaths => [
        Path.Combine(Storage.DownloadedFilesPath, FidelityFXDownloader.DownloadPathName),
    ];

    public FidelityFX2_FrameGeneration_DX12_Processor(List<DLLRecord> manifestDllRecords) : base(manifestDllRecords)
    {
    }
}
