using DLSS_Swapper.Data;
#if !NEW_DLL_HANDLER_TOOL
using DLSS_Swapper_Manifest_Builder.FSR31;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder;

public class DLLRecord : IComparable<DLLRecord>
{
    [JsonIgnore]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("version_number")]
    public ulong VersionNumber { get; set; } = 0;

    [JsonPropertyName("internal_name")]
    public string InternalName { get; set; } = string.Empty;

    [JsonPropertyName("internal_name_extra")]
    public string InternalNameExtra { get; set; } = string.Empty;

    [JsonPropertyName("additional_label")]
    public string AdditionalLabel { get; set; } = string.Empty;

    [JsonPropertyName("md5_hash")]
    public string MD5Hash { get; set; } = string.Empty;

    [JsonPropertyName("zip_md5_hash")]
    public string ZipMD5Hash { get; set; } = string.Empty;

    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("file_description")]
    public string FileDescription { get; set; } = string.Empty;

    [JsonPropertyName("signed_datetime")]
    public DateTime SignedDateTime { get; set; } = DateTime.MinValue;

    [JsonPropertyName("is_signature_valid")]
    public bool IsSignatureValid { get; set; } = false;

    [JsonPropertyName("is_dev_file")]
    public bool IsDevFile { get; set; } = false;

    [JsonPropertyName("file_size")]
    public long FileSize { get; set; } = 0;

    [JsonPropertyName("zip_file_size")]
    public long ZipFileSize { get; set; } = 0;

    [JsonPropertyName("dll_source")]
    public string DllSource { get; set; } = string.Empty;

    [JsonIgnore]
    public GameAssetType AssetType { get; set; } = GameAssetType.Unknown;

    public DLLRecord()
    {

    }

    [JsonIgnore]
    readonly static Regex _dlssCLMatcher = new Regex(@"^(?<CL>CL(\s*)(\d*))(\s*)(?<extra>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

#if !NEW_DLL_HANDLER_TOOL
    public static DLLRecord FromFile(string filename, string expectedDLLName)
    {
        var dllRecord = new DLLRecord();
        dllRecord.Filename = filename;

        dllRecord.AssetType = expectedDLLName switch
        {
            "nvngx_dlss.dll" => GameAssetType.DLSS,
            "nvngx_dlssg.dll" => GameAssetType.DLSS_G,
            "nvngx_dlssd.dll" => GameAssetType.DLSS_D,
            "amd_fidelityfx_dx12.dll" => GameAssetType.FSR_31_DX12,
            "amd_fidelityfx_vk.dll" => GameAssetType.FSR_31_VK,
            "libxess.dll" => GameAssetType.XeSS,
            "libxell.dll" => GameAssetType.XeLL,
            "libxess_fg.dll" => GameAssetType.XeSS_FG,
            _ => GameAssetType.Unknown,
        };

        if (dllRecord.AssetType == GameAssetType.Unknown)
        {
            throw new Exception($"Error processing dll: Unknown asset type, {filename}");
        }


        
        dllRecord.IsSignatureValid = WinTrust.VerifyEmbeddedSignature(filename);
        //if (ignoreInvalid == false && IsSignatureValid == false)
        if (dllRecord.IsSignatureValid == false)
        {
            if (filename.EndsWith("nvngx_dlss_1.0.11.0\\nvngx_dlss.dll", StringComparison.OrdinalIgnoreCase) ||
                filename.EndsWith("nvngx_dlss_1.0.13.0\\nvngx_dlss.dll", StringComparison.OrdinalIgnoreCase))
            {
                // NO-OP
            }
            else
            {
             //   throw new Exception($"Error processing dll: Invalid signature found, {filename}");
            }
        }

        var fileInfo = new FileInfo(filename);
        dllRecord.FileSize = fileInfo.Length;

        dllRecord.SignedDateTime = WinCrypt.GetSignedDateTime(filename);



        var versionInfo = FileVersionInfo.GetVersionInfo(filename);

        dllRecord.Version = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}";
        dllRecord.FileDescription = versionInfo.FileDescription ?? string.Empty;


        // VersionNumber is used for ordering dlls in the case where 2.1.18.0 would order below 2.1.2.0.
        // VersionNumber is calculated by putting the each part into a 16bit section of a 64bit number
        // VersionNumber = [AAAAAAAAAAAAAAAA][BBBBBBBBBBBBBBBB][CCCCCCCCCCCCCCCC][DDDDDDDDDDDDDDDD]
        // where AAAAAAAAAAAAAAAA = FileMajorPart
        //       BBBBBBBBBBBBBBBB = FileMinorPart
        //       CCCCCCCCCCCCCCCC = FileBuildPart
        //       DDDDDDDDDDDDDDDD = FilePrivatePart
        // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.fileversioninfo?view=net-5.0#remarks
        dllRecord.VersionNumber = ((ulong)versionInfo.FileMajorPart << 48) +
                     ((ulong)versionInfo.FileMinorPart << 32) +
                     ((ulong)versionInfo.FileBuildPart << 16) +
                     ((ulong)versionInfo.FilePrivatePart);

        // MD5 should never be used to check if a file has been tampered with.
        // We are simply using it to check the integrity of the downloaded/extracted file.
        using (var stream = File.OpenRead(filename))
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(stream);
                dllRecord.MD5Hash = BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            }
        }


        // Internal name stuff here

        if (dllRecord.AssetType == GameAssetType.DLSS)
        {
            if (versionInfo.OriginalFilename == "FineTune")
            {
                dllRecord.InternalName = "FineTune";
            }
            else if (string.IsNullOrEmpty(versionInfo.OriginalFilename) == false)
            {
                var match = _dlssCLMatcher.Match(versionInfo.OriginalFilename);
                if (match.Success)
                {
                    dllRecord.InternalName = match.Groups["CL"].Value.Trim();
                    dllRecord.InternalNameExtra = match.Groups["extra"].Value.Trim();
                }
                else
                {
                    Debugger.Break();
                }
            }
            else
            {
                Debugger.Break();
            }
        }
        else if (dllRecord.AssetType == GameAssetType.DLSS_G)
        {
            if (string.IsNullOrEmpty(versionInfo.OriginalFilename) == false)
            {
                if (versionInfo.OriginalFilename.StartsWith("SHA:"))
                {
                    // NOOP
                }
                else
                {
                    var match = _dlssCLMatcher.Match(versionInfo.OriginalFilename);
                    if (match.Success)
                    {
                        dllRecord.InternalName = match.Groups["CL"].Value.Trim();
                        dllRecord.InternalNameExtra = match.Groups["extra"].Value.Trim();
                    }
                    else
                    {
                        Debugger.Break();
                    }
                }
            }
            else
            {
                Debugger.Break();
            }
        }
        else if (dllRecord.AssetType == GameAssetType.DLSS_D)
        {
            if (string.IsNullOrEmpty(versionInfo.OriginalFilename) == false)
            {
                var match = _dlssCLMatcher.Match(versionInfo.OriginalFilename);
                if (match.Success)
                {
                    dllRecord.InternalName = match.Groups["CL"].Value.Trim();
                    dllRecord.InternalNameExtra = match.Groups["extra"].Value.Trim();
                }
                else
                {
                    Debugger.Break();
                }
            }
            else
            {
                Debugger.Break();
            }
        }
        else if (dllRecord.AssetType == GameAssetType.FSR_31_DX12 || dllRecord.AssetType == GameAssetType.FSR_31_VK)
        {
            var fsr31Helper = new FSR31Helper();
            var versions = fsr31Helper.GetVersions(dllRecord.Filename);

            if (versions.Count == 2)
            {
                dllRecord.InternalName = versions[0] ?? string.Empty;
                dllRecord.InternalNameExtra = versions[1] ?? string.Empty;
            }
            else
            {
                Debugger.Break();
            }
        }
        else if (dllRecord.AssetType == GameAssetType.XeSS)
        {
            // NOOP
        }
        else if (dllRecord.AssetType == GameAssetType.XeLL)
        {
            // NOOP
        }
        else if (dllRecord.AssetType == GameAssetType.XeSS_FG)
        {
            // NOOP
        }
        else
        {
            Debugger.Break();
        }
        
        return dllRecord;
    }
#endif

    public int CompareTo(DLLRecord? other)
    {
        if (other is null)
        {
            return 1;
        }

        if (VersionNumber == other.VersionNumber)
        {
            if (IsDevFile == other.IsDevFile)
            {
                return AdditionalLabel.CompareTo(other.AdditionalLabel);
            }

            return IsDevFile.CompareTo(other.IsDevFile);
        }

        return VersionNumber.CompareTo(other.VersionNumber);
    }

    internal string GetRecordSimpleType()
    {
        return AssetType switch
        {
            GameAssetType.DLSS => "dlss",
            GameAssetType.DLSS_G => "dlss_g",
            GameAssetType.DLSS_D => "dlss_d",
            GameAssetType.FSR_31_DX12 => "fsr_31_dx12",
            GameAssetType.FSR_31_VK => "fsr_31_vk",
            GameAssetType.XeSS => "xess",
            GameAssetType.XeLL => "xell",
            GameAssetType.XeSS_FG => "xess_fg",
            _ => string.Empty,
        };
    }
}
