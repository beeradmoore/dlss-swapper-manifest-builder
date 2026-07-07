
namespace DLSS_Swapper_Manifest_Builder.Processors.DirectStorage;

internal abstract class DirectStorageBaseProcessor : DLLProcessor
{
    public DirectStorageBaseProcessor(List<DLLRecord> manifestDllRecords) : base(manifestDllRecords)
    {
    }

    public override List<DLLRecord> ProcessLocalFiles(IReadOnlyList<DLLRecord> existingRecords)
    {
        var processedFiles = base.ProcessLocalFiles(existingRecords);

        foreach (var processedFile in processedFiles)
        {
            // Mark preview DLLs as dev files. In theory they should have a name like "1.4.0-preview2-2606.904" so checking for "-" should be fine
            if (processedFile.DllSource.Contains('-'))
            {
                processedFile.IsDevFile = true;
            }
        }

        return processedFiles;
    }
}
