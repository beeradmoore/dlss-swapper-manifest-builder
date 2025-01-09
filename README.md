# dlss-swapper-manifest-builder
Builds manifest file used by DLSS Swapper


### SDKs

#### NVIDIA Streamline
While [Streamline](https://github.com/NVIDIAGameWorks/Streamline) can be used, the `nvngx_dlssg.dll` dlls it produces are marked as `NOT FOR PRODUCTION`. Using these files can sometimes display a message on screen.

The [NVIDIAGAmeWorks/Streamline](https://github.com/NVIDIAGameWorks/Streamline) repo also mentions

> As of SL 2.0.0, it is now possible to recompile all of SL from source, with the exception of the DLSS-G plugin. The DLSS-G plugin is provided as prebuilt DLLs only.