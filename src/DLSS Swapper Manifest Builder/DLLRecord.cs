using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder;

public class DLLRecord
{
    [JsonIgnore]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("version_number")]
    public ulong VersionNumber { get; set; } = 0;

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

    [JsonIgnore]
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

    public DLLRecord()
    {

    }

    public static DLLRecord FromFile(string filename)
    {
        var dllRecord = new DLLRecord();
        dllRecord.Filename = filename;
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

        return dllRecord;
    }

    
}
