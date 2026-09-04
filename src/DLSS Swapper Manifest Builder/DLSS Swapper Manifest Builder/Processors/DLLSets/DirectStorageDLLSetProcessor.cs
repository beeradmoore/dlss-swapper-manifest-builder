using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace DLSS_Swapper_Manifest_Builder.Processors.DLLSets;

internal class DirectStorageDLLSetProcessor : DLLSetProcessor
{
    public override DLLSetType DLLSetType => DLLSetType.DirectStorage;

    readonly static Regex _dllSetNameMatcher = new Regex(@"Microsoft\.Direct3D\.DirectStorage\.(?<version>.*)\.nupkg", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    readonly static Regex _dllSetVersionMatcher = new Regex(@"(\d*)\.(\d*)\.(\d*)\.(\d*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);


    public DirectStorageDLLSetProcessor(List<DLLSet> dllSets) : base(dllSets)
    {
    }

    internal override string GetDLLSetNameFromSource(string dllSetSource)
    {
        var match = _dllSetNameMatcher.Match(dllSetSource);

        if (match.Success)
        {
            return $"DirectStorage v{match.Groups["version"].ValueSpan}";
        }
        else
        {
            match = _dllSetVersionMatcher.Match(dllSetSource);
            
            if (match.Success)
            {
                return $"DirectStorage v{dllSetSource}";
            }

            Log.Error($"Could not find DLL Set name from source {dllSetSource}");

            throw new Exception($"Could not find DLL Set name from source {dllSetSource} in GetDLLSetNameFromSource");
        }
    }
}
