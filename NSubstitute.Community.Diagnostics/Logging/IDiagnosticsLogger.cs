using System.Threading;

namespace NSubstitute.Community.Diagnostics.Logging
{
    internal interface IDiagnosticsLogger
    {
        void WriteLine(string line);
        
        DiagnosticsLogLevel Level { get; }
    }

    internal static class DiagnosticsTracerExtensions
    {
        public static void TraceWithTID(this IDiagnosticsLogger logger, string message)
        {
            if (logger.Level == DiagnosticsLogLevel.Tracing)
            {
                logger.WriteLineWithTID(message);
            }
        }
 
        public static void LogWithTID(this IDiagnosticsLogger logger, string message)
        {
            if (logger.Level == DiagnosticsLogLevel.Logging)
            {
                logger.WriteLineWithTID(message);
            }
        }
        
        public static void WriteLineWithTID(this IDiagnosticsLogger logger, string message)
        {
            logger.WriteLine($"[TID:{Thread.CurrentThread.ManagedThreadId}]{message}");
        }
    }
}