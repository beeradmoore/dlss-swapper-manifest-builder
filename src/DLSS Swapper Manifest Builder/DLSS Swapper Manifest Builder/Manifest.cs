using DLSS_Swapper.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder;

internal class Manifest
{
    [JsonPropertyName("dlss")]
    public List<DLLRecord> DLSS { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("dlss_d")]
    public List<DLLRecord> DLSS_D { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("dlss_g")]
    public List<DLLRecord> DLSS_G { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("fsr_31_dx12")]
    public List<DLLRecord> FSR_31_DX12 { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("fsr_31_vk")]
    public List<DLLRecord> FSR_31_VK { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("xess")]
    public List<DLLRecord> XeSS { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("xell")]
    public List<DLLRecord> XeLL { get; set; } = new List<DLLRecord>();

	[JsonPropertyName("xess_fg")]
	public List<DLLRecord> XeSS_FG { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("xess_dx11")]
    public List<DLLRecord> XeSS_DX11 { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("directstorage")]
    public List<DLLRecord> DirectStorage { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("directstorage_core")]
    public List<DLLRecord> DirectStorageCore { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("fidelityfx_sdk2_denoiser_dx12")]
    public List<DLLRecord> FidelityFX_SDK2_Denoiser_DX12 { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("fidelityfx_sdk2_framegeneration_dx12")]
    public List<DLLRecord> FidelityFX_SDK2_FrameGeneration_DX12 { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("fidelityfx_sdk2_loader_dx12")]
    public List<DLLRecord> FidelityFX_SDK2_Loader_DX12 { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("fidelityfx_sdk2_radiancecache_dx12")]
    public List<DLLRecord> FidelityFX_SDK2_RadianceCache_DX12 { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("fidelityfx_sdk2_upscaler_dx12")]
    public List<DLLRecord> FidelityFX_SDK2_Upscaler_DX12 { get; set; } = new List<DLLRecord>();

    [JsonPropertyName("known_dlls")]
    public KnownDLLs KnownDLLs { get; set; } = new KnownDLLs();
}
