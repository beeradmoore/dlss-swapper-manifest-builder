# dlss-swapper-manifest-builder

This repository is used to maintain tools to ingest DLLs and maintain `manifest.json` file used by [DLSS Swapper](https://github.com/beeradmoore/dlss-swapper).

> [!NOTE]  
> Tools in this repository are not meant to run as standalone applications, but instead run from within Visual Studio 2022 directly.

Because `NewDLLHandler` generates files that `DLSS Swapper Manifest Builder` uses they should be run in that order. Running `NewDLLHandler` without `DLSS Swapper Manifest Builder` has no impact on DLSS Swapper and its data.

## DLSS Swapper Manifest Builder 

Takes in DLLs from various input paths (SDKs and manually zipped DLLs) and generates the structured output and `manifest.json` used by DLSS Swapper.

Running this tool creates a `generated_files` folder with three directories in it. `input_files`, `output_files`, and `temp_files`.

### input_files
These are files used to ingest new data to create manifests

#### input_files/base/{dlss/fsr/xess/etc.}/
Where all existing records are downloaded to.

#### input_files/import/{dlss/fsr/xess/etc.}/
Where zip files are placed that each contain a single dll in their root. These will be processed and added to the manifest.

#### input_files/sdks/

TODO: CREATE NEW SUB DIRECTORIES FOR SDK

The SDKs currently used are:

- [NVIDIA/DLSS](https://github.com/NVIDIA/DLSS)
- [NVIDIAGameWorks/Streamline](https://github.com/NVIDIAGameWorks/Streamline)
- [GPUOpen-LibrariesAndSDKs/FidelityFX-SDK](https://github.com/GPUOpen-LibrariesAndSDKs/FidelityFX-SDK)
- [intel/xess](https://github.com/intel/xess)

#### input_files/manifest.json

This is a copy of the `manifest.json` on the current file storage.

### output_files

This is a directory that is synced to the online Cloudflare R2 hosting with the following command.

```
aws s3 sync . s3://{bucket_name}/ --profile {profile_name} --endpoint-url https://{account_id}.r2.cloudflarestorage.com
```

Zip files are only put in here if they are new. Existing files in `input_files/base/{dlss/fsr/xess/etc.}/` are not copied into here.
`manifest.json` will always be written here on successful execution and then copied to the root of this repository. This `manifest.json` in the root of the repository is not loaded by DLSS Swapper, it is kept for diff reference of new to old `manifest.json` when running this application.

### temp_files

Temp directory for creating/unarchiving zips.

## NewDLLHandler 

Takes issues created in this repository and sorts through them to build a list of unknown DLLs and their sources. This also produces several files in the root of this reposotory.

#### known_dll_hashes.txt

A list of all known DLL hashes. This is copied into the main `manifest.json` when the DLSS Swapper Manifest Builder  application is run.


#### known_dll_sources.json

List of all DLLs that have been found and submitted by DLSS Swapper users.

#### known_dll_sources_missing.json

The same as `known_dll_sources.json`, but it is only DLLs not added to DLSS Swapper yet.

