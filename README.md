# dlss-swapper-manifest-builder

This repository is used to maintain tools to ingest DLLs and maintain `manifest.json` file used by DLSS Swapper.

> [!NOTE]  
> Tools in this repository are not meant to run as standalone applications, but instead run from within Visual Studio 2022 directly.


## DLSS Swapper Manifest Builder 

Takes in DLLs from various input paths (SDKs and manually zipped DLLs) and generates the structured output and `manifest.json` used by DLSS Swapper.

### SDKs

- [NVIDIA/DLSS](https://github.com/NVIDIA/DLSS)
- [NVIDIAGameWorks/Streamline](https://github.com/NVIDIAGameWorks/Streamline)
- [GPUOpen-LibrariesAndSDKs/FidelityFX-SDK](https://github.com/GPUOpen-LibrariesAndSDKs/FidelityFX-SDK)
- [intel/xess](https://github.com/intel/xess)

## NewDLLHandler 

Takes issues created in this repository and sorts through them to build a list of unknown DLLs and their sources. This also produces `known_dlls.json` list which is then use to create the `known_dlls` list in the main `manifest.json`

