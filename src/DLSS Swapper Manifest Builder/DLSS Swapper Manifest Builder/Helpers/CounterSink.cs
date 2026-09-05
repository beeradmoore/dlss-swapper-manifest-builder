using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace DLSS_Swapper_Manifest_Builder.Helpers;

internal class CounterSink : ILogEventSink
{
    int _verboseCount = 0;
    int _debugCount = 0;
    int _informationCount = 0;
    int _warningCount = 0;
    int _errorCount = 0;
    int _fatalCount = 0;



    public int VerboseCount => _verboseCount;
    public int DebugCount => _debugCount;
    public int InformationCount => _informationCount;
    public int WarningCount => _warningCount;
    public int ErrorCount => _errorCount;
    public int FatalCount => _fatalCount;

    public void Emit(LogEvent logEvent)
    {
        switch (logEvent.Level)
        {
            case LogEventLevel.Verbose:
                Interlocked.Increment(ref _verboseCount);
                break;

            case LogEventLevel.Debug:
                Interlocked.Increment(ref _debugCount);
                break;

            case LogEventLevel.Information:
                Interlocked.Increment(ref _informationCount);
                break;

            case LogEventLevel.Warning:
                Interlocked.Increment(ref _warningCount);
                break;

            case LogEventLevel.Error:
                Interlocked.Increment(ref _errorCount);
                break;

            case LogEventLevel.Fatal:
                Interlocked.Increment(ref _fatalCount);
                break;
        }
    }
}
