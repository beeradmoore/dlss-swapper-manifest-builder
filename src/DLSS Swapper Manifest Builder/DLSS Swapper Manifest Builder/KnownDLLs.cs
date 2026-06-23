using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder;

public class KnownDLLs
{
    [JsonPropertyName("dlss")]
    public List<HashedKnownDLL> DLSS { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("dlss_d")]
    public List<HashedKnownDLL> DLSS_D { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("dlss_g")]
    public List<HashedKnownDLL> DLSS_G { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("fsr_31_dx12")]
    public List<HashedKnownDLL> FSR_31_DX12 { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("fsr_31_vk")]
    public List<HashedKnownDLL> FSR_31_VK { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("xess")]
    public List<HashedKnownDLL> XeSS { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("xell")]
    public List<HashedKnownDLL> XeLL { get; set; } = new List<HashedKnownDLL>();

	[JsonPropertyName("xess_fg")]
	public List<HashedKnownDLL> XeSS_FG { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("xess_dx11")]
    public List<HashedKnownDLL> XeSS_DX11 { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("directstorage")]
    public List<HashedKnownDLL> DirectStorage { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("directstorage_core")]
    public List<HashedKnownDLL> DirectStorageCore { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("fidelityfx_sdk2_denoiser_dx12")]
    public List<HashedKnownDLL> FidelityFX_SDK2_Denoiser_DX12 { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("fidelityfx_sdk2_framegeneration_dx12")]
    public List<HashedKnownDLL> FidelityFX_SDK2_FrameGeneration_DX12 { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("fidelityfx_sdk2_loader_dx12")]
    public List<HashedKnownDLL> FidelityFX_SDK2_Loader_DX12 { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("fidelityfx_sdk2_radiancecache_dx12")]
    public List<HashedKnownDLL> FidelityFX_SDK2_RadianceCache_DX12 { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("fidelityfx_sdk2_upscaler_dx12")]
    public List<HashedKnownDLL> FidelityFX_SDK2_Upscaler_DX12 { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("sl_reflex")]
    public List<HashedKnownDLL> Streamline_Reflex { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("sl_pcl")]
    public List<HashedKnownDLL> Streamline_PCL { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("sl_nvperf")]
    public List<HashedKnownDLL> Streamline_NvPerf { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("sl_nis")]
    public List<HashedKnownDLL> Streamline_NIS { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("sl_interposer")]
    public List<HashedKnownDLL> Streamline_Interposer { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("sl_dlss_g")]
    public List<HashedKnownDLL> Streamline_DLSS_G { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("sl_dlss_d")]
    public List<HashedKnownDLL> Streamline_DLSS_D { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("sl_dlss")]
    public List<HashedKnownDLL> Streamline_DLSS { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("sl_directsr")]
    public List<HashedKnownDLL> Streamline_DirectSR { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("sl_deepdvc")]
    public List<HashedKnownDLL> Streamline_DeepDVC { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("sl_common")]
    public List<HashedKnownDLL> Streamline_Common { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("deepdvc")]
    public List<HashedKnownDLL> DeepDVC { get; set; } = new List<HashedKnownDLL>();

    [JsonPropertyName("nvlowlatencyvk")]
    public List<HashedKnownDLL> NvLowLatencyVK { get; set; } = new List<HashedKnownDLL>();

}
