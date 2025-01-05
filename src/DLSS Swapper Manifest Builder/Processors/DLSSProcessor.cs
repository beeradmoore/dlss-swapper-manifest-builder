using System.Diagnostics;
using System.IO.Compression;

namespace DLSS_Swapper_Manifest_Builder.Processors;

public class DLSSProcessor : DLLProcessor
{
    public override string NamePath => "dlss";
    public override string ExpectedDLLName => "nvngx_dlss.dll";

    public override string[] ValidFileDescriptions => new string[]
    {
        "NGX DLSS",
        "NGX DLSS - DVS PRODUCTION",
        "NGX DLSS - DVS VIRTUAL",
        "NVIDIA DLSSv2 - DVS PRODUCTION",
        "NVIDIA DLSSv3 - DVS PRODUCTION",
        "NVIDIA DLSSv2 - Beta - White Collie 1 - DVS PRODUCTION",
        "NVIDIA DLSSv2 - Beta - White Collie 2 - DVS PRODUCTION",
    };

    public DLSSProcessor() : base()
    {
    }

    public override List<DLLRecord> ProcessLocalFiles(IReadOnlyList<DLLRecord> existingRecords)
    {
        var files = GetAllLocalRecords();
        var dllRecords = new Dictionary<string, DLLRecord>();


        var customAdditionalLabels = new Dictionary<string, string>();
        customAdditionalLabels.Add("0A71EFBA8DAFF9C284CE6010923C01F1", "v2"); // 2.4.12 v2
        customAdditionalLabels.Add("31BFD8F750F87E5040557D95C2345080", "v3"); // 2.4.12 v3
        customAdditionalLabels.Add("40D468487EA4E0F56595F8DE1AC8ED7C", "v2"); // 3.1.1 v2
        customAdditionalLabels.Add("BF68025B3603C382FCA65B148B979682", "v2"); // 3.5 v2
        customAdditionalLabels.Add("B2B6FAE8936719CF81D6B5577F257C40", "Beta - White Collie 1");
        customAdditionalLabels.Add("CD71EE48B994AC254DFF5DEC20828BE7", "Beta - White Collie 2");


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
                var releaseDll = archive.Entries.Where(x => x.FullName.EndsWith($"Windows_x86_64/rel/{ExpectedDLLName}", StringComparison.InvariantCultureIgnoreCase)).ToList();
                if (releaseDll.Count == 1)
                {
                    var dllEntry = releaseDll.Single();
                    Console.WriteLine(Path.GetFileName(file) + " - " + dllEntry.Crc32);

                    var dllExtractFilename = Path.Combine(dllExtractPath, $"{dllEntry.Crc32}_{ExpectedDLLName}");
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

                    // If we have one from the rel folder we should also check the dev folder
                    var devDll = archive.Entries.Where(x => x.FullName.EndsWith($"Windows_x86_64/dev/{ExpectedDLLName}", StringComparison.InvariantCultureIgnoreCase)).ToList();
                    if (devDll.Count == 1)
                    {


                        var devDllEntry = devDll.Single();
                        Console.WriteLine(Path.GetFileName(file) + " - " + devDllEntry.Crc32);
                        var devDLLExtractFilename = Path.Combine(devDLLExtractPath, $"{devDllEntry.Crc32}_{ExpectedDLLName}");
                        devDllEntry.ExtractToFile(devDLLExtractFilename, false);

                        var devDllRecord = DLLRecord.FromFile(devDLLExtractFilename);

                        if (customAdditionalLabels.ContainsKey(devDllRecord.MD5Hash) == true)
                        {
                            devDllRecord.AdditionalLabel = customAdditionalLabels[devDllRecord.MD5Hash];
                        }

                        if (dllRecords.ContainsKey(devDllRecord.MD5Hash) == false)
                        {
                            devDllRecord.IsDevFile = true;
                            ValidateAndProcessDLLRecord(devDllRecord);
                            dllRecords.Add(devDllRecord.MD5Hash, devDllRecord);
                        }
                        else
                        {
                            Console.WriteLine($"Skipping {dllRecord.Version}, hash already exists. File: {file}, Dll: {dllExtractFilename}");
                        }
                    }
                    else if (devDll.Count > 0)
                    {
                        throw new Exception($"Multiple dev DLLs found in {file}");
                    }
                    else
                    {
                        throw new Exception($"Expected to find a dev DLL but did not find any in {file}");
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
