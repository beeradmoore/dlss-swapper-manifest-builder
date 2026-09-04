using DLSS_Swapper.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DLSS_Swapper_Manifest_Builder.Processors.DLLSets;

public enum DLLSetType
{
    DLSS,
    Streamline,
    DirectStorage,
    FidelityFX_SDK1,
    FidelityFX_SDK2,
    XeSS,
}

internal abstract class DLLSetProcessor
{
    public abstract DLLSetType DLLSetType { get; }

    public string DLLSetTypeString => GetDLLSetTypeString(this.DLLSetType);

    public List<DLLProcessor> DLLProcessors { get; } = new List<DLLProcessor>();

    public List<DLLSet> DLLSets { get; init; }

    public DLLSetProcessor(List<DLLSet> dllSets)
    {
        DLLSets = dllSets;
    }



    public void ProcessDLLSets()
    {
        var dictionary = new Dictionary<string, DLLSet>();

        foreach (var dllProcessor in DLLProcessors)
        {
            foreach (var dllRecord in dllProcessor.ManifestDllRecords)
            {
                // To see if a DLL should be in the same set see if it has the same DllSource OR
                // the same DLL version. Be careful not to mix and match dev DLLs though.
                
                var dllSetSource = string.IsNullOrWhiteSpace(dllRecord.DllSource) ? dllRecord.Version  : dllRecord.DllSource; 
                if (dllRecord.IsDevFile)
                {
                    dllSetSource += "_dev";
                }

                if (dictionary.ContainsKey(dllSetSource) == false)
                {
                    var dllSetName = GetDLLSetNameFromSource(dllSetSource);
                    dictionary.Add(dllSetSource, new DLLSet() { Name = dllSetName });
                }

                dictionary[dllSetSource].DLLRecords.Add(dllRecord.GetRecordSimpleType(), dllRecord.MD5Hash);
            }
        }

        var incomingDllSets = dictionary.Values.ToList();

        foreach (var incomingDllSet in incomingDllSets)
        {
            var dllSetMatch = DLLSets.FirstOrDefault(x => x.Name == incomingDllSet.Name);

            if (dllSetMatch is null)
            {
                // Add new DLL set
                DLLSets.Add(incomingDllSet);
            }
            else
            {
                // Update existing DLL set
                dllSetMatch.DLLRecords = incomingDllSet.DLLRecords;
            }
        }
    }

    // Allows per DLLSetProcessor to name DLL sets automatically
    internal abstract string GetDLLSetNameFromSource(string dllSetSource);


    public static string GetDLLSetTypeString(DLLSetType dllSetType)
    {
        // TODO: Can this be dynamic? 
        return dllSetType switch
        {
            DLLSetType.DLSS => DLLSet.DLSS,
            DLLSetType.Streamline => DLLSet.Streamline,
            DLLSetType.DirectStorage => DLLSet.DirectStorage,
            DLLSetType.FidelityFX_SDK1 => DLLSet.FidelityFX_SDK1,
            DLLSetType.FidelityFX_SDK2 => DLLSet.FidelityFX_SDK2,
            DLLSetType.XeSS => DLLSet.XeSS,
            _ => throw new Exception($"Unable to find DLLSetType {dllSetType} in GetDLLSetTypeString"),
        };
    }

    public static DLLSetProcessor FromDLLSetType(DLLSetType dllSetType, List<DLLSet> dllSets)
    {
        return dllSetType switch
        {
            DLLSetType.DirectStorage => new DirectStorageDLLSetProcessor(dllSets),
            _ => throw new Exception($"Unable to find DLLSetType {dllSetType} in FromDLLSetType"),
        };
    }
}
