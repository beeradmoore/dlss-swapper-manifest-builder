using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSS_Swapper_Manifest_Builder.Processors;

public abstract class FSR31Processor : DLLProcessor
{
    public override List<DLLRecord> ProcessLocalFiles(IReadOnlyList<DLLRecord> existingRecords)
    {
        var files = GetAllLocalRecords();
        var dllRecords = new Dictionary<string, DLLRecord>();

        var customAdditionalLabels = new Dictionary<string, string>();
        customAdditionalLabels.Add("071EB42E3CBD0989A12465E0E529BCAC", "3.1.0"); // DX12 FidelityFX-SDK-1.1.zip
        customAdditionalLabels.Add("2FCBE69A137DBA1FF45071A9B64C9581", "3.1.1"); // DX12 FidelityFX-SDK-1.1.1.zip
        customAdditionalLabels.Add("2DC40D2F183920624BE396B624466157", "3.1.2"); // DX12 FidelityFX-SDK-1.1.2.zip
        customAdditionalLabels.Add("3EF374F8F7EB44D211BCC0C063A520D9", "3.1.3"); // DX12 FidelityFX-SDK-1.1.3.zip
        customAdditionalLabels.Add("A2BC75BC5455E870D71890C774541DA9", "3.1.0"); // VK FidelityFX-SDK-1.1.zip
        customAdditionalLabels.Add("1FC735751D2632E926016206398B8654", "3.1.1"); // VK FidelityFX-SDK-1.1.1.zip
        customAdditionalLabels.Add("0D0CDC8D5865027D100DAA35B605793F", "3.1.2"); // VK FidelityFX-SDK-1.1.2.zip
        customAdditionalLabels.Add("23A87C10A859D5756EDBE5A6D7F692B5", "3.1.3"); // VK FidelityFX-SDK-1.1.3.zip

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

            // Get ready to create the dev path
            var devDLLExtractPath = Path.Combine(OutputDllPath, Path.GetFileNameWithoutExtension(fileName) + "_dev");
            Directory.CreateDirectory(devDLLExtractPath);

            using (var archive = ZipFile.OpenRead(file))
            {
                // If we are using the SDK distribution we should get it from the rel folder
                var releaseDll = archive.Entries.Where(x => x.FullName.EndsWith($"PrebuiltSignedDLL/{ExpectedDLLName}", StringComparison.InvariantCultureIgnoreCase)).ToList();
                if (releaseDll.Count == 1)
                {
                    var dllEntry = releaseDll.Single();
                    Console.WriteLine(Path.GetFileName(file) + " - " + dllEntry.Crc32);

                    var dllExtractFilename = Path.Combine(dllExtractPath, $"{dllEntry.Crc32}_{ExpectedDLLName}");
                    dllEntry.ExtractToFile(dllExtractFilename, false);

                    var dllRecord = DLLRecord.FromFile(dllExtractFilename);
                    Console.WriteLine(dllRecord.MD5Hash);

                    // Default to the filename to make it easy to track SDK source
                    dllRecord.DllSource = fileName;

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
                else if (releaseDll.Count > 0)
                {
                    throw new Exception($"Multiple DLLs found in {file}");
                }
                else
                {
                    var dllEntry = archive.Entries.Single(x => x.Name == ExpectedDLLName);
                    Console.WriteLine(Path.GetFileName(file) + " - " + dllEntry.Crc32);

                    var dllExtractFilename = Path.Combine(dllExtractPath, $"{dllEntry.Crc32}_{ExpectedDLLName}");
                    dllEntry.ExtractToFile(dllExtractFilename, false);

                    var dllRecord = DLLRecord.FromFile(dllExtractFilename);
                    Console.WriteLine(dllRecord.MD5Hash);

                    // Default to the filename to make it easy to track SDK source
                    dllRecord.DllSource = fileName;

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
                        Console.WriteLine($"Skipping {dllRecord.Version} from {file}.");
                    }
                }
            }
        }

        var dllRecordsList = dllRecords.Values.ToList();
        dllRecordsList.Sort((x, y) => x.VersionNumber.CompareTo(y.VersionNumber));
        return dllRecordsList;
    }
}
