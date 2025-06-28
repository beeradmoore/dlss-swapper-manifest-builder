using DLSS_Swapper.Data;
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
    public static string TempFilesPath => Path.Combine(Path.GetTempPath(), "dlss_swapper_manifest_builder");
#else
    public static string BaseInputPath => "input_files";
    public static string BaseOutputPath => "output_files";
    public static string TempFilesPath => "temp_files";
#endif
    public static string InputManifestPath => Path.Combine(InputFilesPath, "manifest.json");
    public static string OutputManifestPath => Path.Combine(OutputFilesPath, "manifest.json");
    public static string InputSDKsFilesPath => Path.Combine(InputFilesPath, "sdks");

    public abstract string NamePath { get; }
    public abstract string ExpectedDLLName { get; }
    public abstract string[] ValidFileDescriptions { get; }
    public abstract string[] ExpectedPrefix { get; }
    public abstract string[] ExpectedDevPrefix { get; }
    public abstract Dictionary<string, string> CustomAdditionalLabels { get; }
    public abstract Dictionary<string, string> DllSource { get; }


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
                Console.WriteLine($"Skipping download of {dllRecord.Filename}");
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
        files.AddRange(Directory.GetFiles(InputSDKsFilesPath, "*.zip", SearchOption.AllDirectories));
        files.AddRange(Directory.GetFiles(ImportPath, "*.zip", SearchOption.TopDirectoryOnly));
        return files.ToArray();
    }

    public virtual List<DLLRecord> ProcessLocalFiles(IReadOnlyList<DLLRecord> existingRecords)
    {
        var files = GetAllLocalRecords();

        var dllRecords = new Dictionary<string, DLLRecord>();

        foreach (var file in files)
        {
            // First check if the found file is one from our manifest list
            using (var fileStream = File.OpenRead(file))
            {
                var md5 = GetMD5Hash(fileStream);

                var existingRecord = existingRecords.Where(x => x.ZipMD5Hash == md5).SingleOrDefault();
                if (existingRecord is not null)
                {
                    if (dllRecords.ContainsKey(existingRecord.MD5Hash) == false)
                    {
                        dllRecords.Add(existingRecord.MD5Hash, existingRecord);
                        // Don't copy file, otherwise we have to re-upload it
                    }
                    continue;
                }
            }

            var fileName = Path.GetFileName(file);

            // Create a path to extract the dll to
            var dllExtractPath = Path.Combine(OutputDllPath, Path.GetFileNameWithoutExtension(fileName));
            Directory.CreateDirectory(dllExtractPath);

            using (var archive = ZipFile.OpenRead(file))
            {
                foreach (var prefix in ExpectedPrefix)
                {
                    var dllEntries = prefix.StartsWith("/") switch
                    {
                        true => archive.Entries.Where(x => x.FullName.Equals($"{prefix.Substring(1)}{ExpectedDLLName}", StringComparison.OrdinalIgnoreCase)).ToList(),
                        _ => archive.Entries.Where(x => x.FullName.EndsWith($"{prefix}{ExpectedDLLName}", StringComparison.OrdinalIgnoreCase)).ToList(),
                    };

                    if (dllEntries.Count == 1)
                    {
                        var dllEntry = dllEntries[0];

                        var dllExtractFilename = Path.Combine(dllExtractPath, $"{dllEntry.Crc32}_{ExpectedDLLName}");
                        dllEntry.ExtractToFile(dllExtractFilename, false);

                        var dllRecord = DLLRecord.FromFile(dllExtractFilename, ExpectedDLLName);
                        // If the file is imported from SDKs, add its source name here.
                        if (file.Contains(InputSDKsFilesPath) == true)
                        {
                            dllRecord.DllSource = fileName;
                        }
                        else if (DllSource.ContainsKey(dllRecord.MD5Hash))
                        {
                            dllRecord.DllSource = DllSource[dllRecord.MD5Hash];
                        }
                        else
                        {
                            Console.WriteLine($"! {ExpectedDLLName} {dllRecord.Version} {dllRecord.MD5Hash} does not have a source attributed.");
                        }


                        if (CustomAdditionalLabels.ContainsKey(dllRecord.MD5Hash) == true)
                        {
                            dllRecord.AdditionalLabel = CustomAdditionalLabels[dllRecord.MD5Hash];
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
                    else if (dllEntries.Count == 0)
                    {
                        // NOOP
                    }
                    else
                    {
                        Debugger.Break();
                    }
                }

                foreach (var prefix in ExpectedDevPrefix)
                {
                    var dllEntries = prefix.StartsWith("/") switch
                    {
                        true => archive.Entries.Where(x => x.FullName.Equals($"{prefix.Substring(1)}{ExpectedDLLName}", StringComparison.OrdinalIgnoreCase)).ToList(),
                        _ => archive.Entries.Where(x => x.FullName.EndsWith($"{prefix}{ExpectedDLLName}", StringComparison.OrdinalIgnoreCase)).ToList(),
                    };

                    if (dllEntries.Count == 1)
                    {
                        var dllEntry = dllEntries[0];

                        var dllExtractFilename = Path.Combine(dllExtractPath, $"{dllEntry.Crc32}_{ExpectedDLLName}");
                        dllEntry.ExtractToFile(dllExtractFilename, false);

                        var dllRecord = DLLRecord.FromFile(dllExtractFilename, ExpectedDLLName);
                        dllRecord.IsDevFile = true;
                        // Default to the filename to make it easy to track SDK source

                        if (fileName.StartsWith("nvngx_") == false)
                        {
                            dllRecord.DllSource = fileName;
                        }

                        if (CustomAdditionalLabels.ContainsKey(dllRecord.MD5Hash) == true)
                        {
                            dllRecord.AdditionalLabel = CustomAdditionalLabels[dllRecord.MD5Hash];
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
                    else if (dllEntries.Count == 0)
                    {
                        // NOOP
                    }
                    else
                    {
                        Debugger.Break();
                    }
                }
            }
        }
        
        var dllRecordsList = dllRecords.Values.ToList();
        dllRecordsList.Sort();
        return dllRecordsList;
    }

    string GetMD5Hash(Stream stream)
    {
        // If you don't reset the position the hash will be invalid.
        stream.Position = 0;

        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }
    }

    protected void CreateZipFromRecord(DLLRecord dllRecord)
    {
        // Old zip filename. Moving to new name to support Streamline and DirectStorage
        // as well as match name in DLSS Swapper to make manual import easier.
        var newZipFilename = $"{dllRecord.GetRecordSimpleType()}_v{dllRecord.Version}_{dllRecord.MD5Hash}.zip";
        /*
        // Create zip output
        var newZipFilename = $"{Path.GetFileNameWithoutExtension(ExpectedDLLName)}_v{dllRecord.Version}";

        if (string.IsNullOrEmpty(dllRecord.AdditionalLabel) == false)
        {
            newZipFilename += "_" + dllRecord.AdditionalLabel.Replace(" ", "_");
        }

        if (dllRecord.IsDevFile)
        {
            newZipFilename += "_dev";
        }

        newZipFilename += ".zip";
        */

        var zipOutputFile = Path.Combine(OutputZipPath, newZipFilename);

        Console.WriteLine($"Creating zip: {zipOutputFile}");

        if (File.Exists(zipOutputFile))
        {
            Console.WriteLine($"File already exists {zipOutputFile}");
            Debugger.Break();
        }
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
            dllRecord.DownloadUrl = $"https://dlss-swapper-downloads.beeradmoore.com/{NamePath}/{newZipFilename}";
        }

        // Do this after we close the zip
        /*
        using (var fileStream = File.OpenRead(zipOutputFile))
        {
            dllRecord.ZipMD5Hash = GetMD5Hash(fileStream);
            dllRecord.ZipFileSize = fileStream.Length;
            dllRecord.DownloadUrl = $"https://dlss-swapper-downloads.beeradmoore.com/{NamePath}/{newZipFilename}";
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

    public void MoveToCorrectLocations(IReadOnlyList<DLLRecord> dllRecords, GameAssetType assetType)
    {
        foreach (var  dllRecord in dllRecords)
        {
            dllRecord.AssetType = assetType;

            var currentFileName = Path.GetFileName(dllRecord.DownloadUrl);
            var oldZipFilename = Path.Combine(BaseInputPath, currentFileName);
            if (File.Exists(oldZipFilename) == false)
            {
                Debugger.Break();
            }


            var newZipFilename = $"{dllRecord.GetRecordSimpleType()}_v{dllRecord.Version}_{dllRecord.MD5Hash}.zip";

            if (currentFileName != newZipFilename)
            {
                var zipOutputFile = Path.Combine(OutputZipPath, newZipFilename);
                File.Copy(oldZipFilename, zipOutputFile);
                dllRecord.DownloadUrl = $"https://dlss-swapper-downloads.beeradmoore.com/{NamePath}/{newZipFilename}";
            }
        }

    }

    public void MoveOldToNew(IReadOnlyList<DLLRecord> dllRecords, GameAssetType assetType)
    {
        //var oldFiles = Directory.GetFiles(BaseInputPath, "*.zip").Select(x => x.Replace(BaseInputPath + "\\", string.Empty)).ToList();
        foreach (var dllRecord in dllRecords)
        {
            dllRecord.AssetType = assetType;
            var oldZipFilename = Path.Combine(BaseInputPath, Path.GetFileName(dllRecord.DownloadUrl));
            if (File.Exists(oldZipFilename) == false)
            {
                Debugger.Break();
            }
            var newZipFilename = $"{dllRecord.GetRecordSimpleType()}_v{dllRecord.Version}_{dllRecord.MD5Hash}.zip";

            var zipOutputFile = Path.Combine(OutputZipPath, newZipFilename);

            using (var fileStream = File.OpenRead(oldZipFilename))
            {
                dllRecord.ZipFileSize = fileStream.Length;
                dllRecord.DownloadUrl = $"https://dlss-swapper-downloads.beeradmoore.com/{NamePath}/{newZipFilename}";

                // TODO: Extract data out of the zip

                var newZip = GetMD5Hash(fileStream);
                if (newZip != dllRecord.ZipMD5Hash)
                {
                    Debugger.Break();
                }
            }

            File.Copy(oldZipFilename, zipOutputFile);
        }
    }
}
