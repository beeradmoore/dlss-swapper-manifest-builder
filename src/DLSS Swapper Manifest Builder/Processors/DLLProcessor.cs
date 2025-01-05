using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder.Processors;

public abstract class DLLProcessor
{
#if DEBUG
    // In debug mode, don't place these in the built directories as they will be removed when cleaned
    public static string InputFilesPath => Path.Combine("..", "..", "..", "..", "generated_files", "input_files");
    public static string OutputFilesPath => Path.Combine("..", "..", "..", "..", "generated_files", "output_files");
    public static string TempFilesPath => Path.Combine("..", "..", "..", "..", "generated_files", "temp_files");
#else
    public static string BaseInputPath => "input_files";
    public static string BaseOutputPath => "output_files";
    public static string TempFilesPath => "temp_files";
#endif
    public static string InputManifestPath => Path.Combine(InputFilesPath, "manifest.json");
    public static string OutputManifestPath => Path.Combine(OutputFilesPath, "manifest.json");

    public abstract string NamePath { get; }
    public abstract string ExpectedDLLName { get; }
    public abstract string[] ValidFileDescriptions { get; }

    public string BaseInputPath => Path.Combine(InputFilesPath, "base", NamePath);
    public string ImportPath => Path.Combine(InputFilesPath, "import", NamePath);
    public string OutputZipPath => Path.Combine(OutputFilesPath, NamePath);
    public string OutputDllPath => Path.Combine(TempFilesPath, NamePath);

    public DLLProcessor()
    {
        // Create directories if they don't exist.
        if (Directory.Exists(BaseInputPath) == false)
        {
            Directory.CreateDirectory(BaseInputPath);
        }

        if (Directory.Exists(ImportPath) == false)
        {
            Directory.CreateDirectory(ImportPath);
        }
        
        if (Directory.Exists(OutputZipPath) == false)
        {
            Directory.CreateDirectory(OutputZipPath);
        }

        if (Directory.Exists(OutputDllPath) == false)
        {
            Directory.CreateDirectory(OutputDllPath);
        }
    }

    public async Task DownloadExistingRecordsAsync(IReadOnlyList<DLLRecord> dllRecords)
    {
        Console.WriteLine($"Downloading existing records: {NamePath}");
        foreach (var dllRecord in dllRecords)
        {
            if (string.IsNullOrEmpty(dllRecord.DownloadUrl))
            {
                continue;
            }

            dllRecord.Filename = Path.GetFileName(dllRecord.DownloadUrl);
            var expectedZipPath = Path.Combine(BaseInputPath, dllRecord.Filename);

            if (File.Exists(expectedZipPath) == false)
            {
                var httpClient = new HttpClient();
                Console.WriteLine($"Downloading {dllRecord.Filename}");
                using (var localStream = File.Create(expectedZipPath))
                {
                    using (var remoteStream = await httpClient.GetStreamAsync(dllRecord.DownloadUrl))
                    {
                        await remoteStream.CopyToAsync(localStream);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Skipping {dllRecord.Filename}");
            }

            using (var localStream = File.OpenRead(expectedZipPath))
            {
                var localMD5 = GetMD5Hash(localStream);
                if (localMD5 != dllRecord.ZipMD5Hash)
                {
                    throw new Exception($"Local file {dllRecord.Filename} does not match the hash in the manifest.");
                }
            }
        }
    }

    public string[] GetAllLocalRecords()
    {
        var files = new List<string>();
        files.AddRange(Directory.GetFiles(BaseInputPath, "*.zip", SearchOption.TopDirectoryOnly));
        files.AddRange(Directory.GetFiles(ImportPath, "*.zip", SearchOption.TopDirectoryOnly));
        return files.ToArray();
    }

    public virtual List<DLLRecord> ProcessLocalFiles(IReadOnlyList<DLLRecord> existingRecords)
    {
        var files = GetAllLocalRecords();

        var dllRecords = new Dictionary<string, DLLRecord>();



        var customAdditionalLabels = new Dictionary<string, string>();
        customAdditionalLabels.Add("DE3479E49E53A8AB4950F8C72A415239", "v2"); // DLSS G 1.0.4 v2
        customAdditionalLabels.Add("8363E2AC2E3E512AC5AB2D364AA4C245", "v2"); // DLSS G 3.1.30 v2
        customAdditionalLabels.Add("3ED68C9456DC83BDF66B13D1A9C66F18", "33284283"); // DLSS D 3.5 CL 33284283
        customAdditionalLabels.Add("625907DE06A912414FDB8444C91B262C", "33367307"); // DLSS D 3.5 CL 33367307

        foreach (var file in files)
        {
            // First check if the found file is one from our manifest list
            using (var fileStream = File.OpenRead(file))
            {
                var md5 = GetMD5Hash(fileStream);

                var existingRecord = existingRecords.Where(x => x.ZipMD5Hash == md5).SingleOrDefault();
                if (existingRecord is not null)
                {
                    dllRecords.Add(existingRecord.MD5Hash, existingRecord);
                    // Don't copy file, otherwise we have to re-upload it
                    continue;
                }
            }

            var fileName = Path.GetFileName(file);

            // Create a path to extract the dll to
            var dllExtractPath = Path.Combine(OutputDllPath, Path.GetFileNameWithoutExtension(fileName));
            Directory.CreateDirectory(dllExtractPath);

            using (var archive = ZipFile.OpenRead(file))
            {
                var dllEntry = archive.Entries.Single(x => x.Name == ExpectedDLLName);
                
                var dllExtractFilename = Path.Combine(dllExtractPath, $"{dllEntry.Crc32}_{ExpectedDLLName}");
                Console.WriteLine(Path.GetFileName(file) + " - " + dllEntry.Crc32);
                dllEntry.ExtractToFile(dllExtractFilename, false);

                var dllRecord = DLLRecord.FromFile(dllExtractFilename);

                if (customAdditionalLabels.ContainsKey(dllRecord.MD5Hash) == true)
                {
                    dllRecord.AdditionalLabel = customAdditionalLabels[dllRecord.MD5Hash];
                }

                if (dllRecords.ContainsKey(dllRecord.MD5Hash) == false)
                {
                    ValidateAndProcessDLLRecord(dllRecord);

                    dllRecords.Add(dllRecord.MD5Hash, dllRecord);
                }
                else
                {
                    Console.WriteLine($"Skipping {dllRecord.Version}, hash already exists. File: {file}, Dll: {dllExtractFilename}");
                }
            }
        }
        
        var dllRecordsList = dllRecords.Values.ToList();
        dllRecordsList.Sort((x, y) => x.VersionNumber.CompareTo(y.VersionNumber));
        return dllRecordsList;
    }

    public string GetMD5Hash(Stream stream)
    {
        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }
    }

    protected void CreateZipFromRecord(DLLRecord dllRecord)
    {
        // Create zip output
        var newZipFilename = $"{NamePath}_v{dllRecord.Version}";

        if (string.IsNullOrEmpty(dllRecord.AdditionalLabel) == false)
        {
            newZipFilename += "_" + dllRecord.AdditionalLabel.Replace(" ", "_");
        }

        if (dllRecord.IsDevFile)
        {
            newZipFilename += "_dev";
        }

        newZipFilename += ".zip";
        
        var zipOutputFile = Path.Combine(OutputZipPath, newZipFilename);

        Console.WriteLine($"Creating zip: {zipOutputFile}");
        using (var fileStream = File.Create(zipOutputFile))
        {
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
            {
                archive.CreateEntryFromFile(dllRecord.Filename, ExpectedDLLName);
            }

            fileStream.Flush();

            fileStream.Position = 0;

            dllRecord.ZipMD5Hash = GetMD5Hash(fileStream);
            dllRecord.ZipFileSize = fileStream.Length;
            dllRecord.DownloadUrl = $"https://downloads.dlss-swapper.beeradmoore.com/{NamePath}/{newZipFilename}";
        }

        // Do this after we close the zip
        /*
        using (var fileStream = File.OpenRead(zipOutputFile))
        {
            dllRecord.ZipMD5Hash = GetMD5Hash(fileStream);
            dllRecord.ZipFileSize = fileStream.Length;
            dllRecord.DownloadUrl = $"https://downloads.dlss-swapper.beeradmoore.com/{NamePath}/{newZipFilename}";
        }
        */
    }

    protected void ValidateAndProcessDLLRecord(DLLRecord dllRecord)
    {
        if (ValidFileDescriptions.Contains(dllRecord.FileDescription) == false)
        {
            throw new Exception($"Invalid file description: {dllRecord.FileDescription}");
        }

        CreateZipFromRecord(dllRecord);
    }
}
