namespace NSubstitute.Community.Diagnostics.Logging
{
    internal interface IDiagnosticsLogger
    {
        void WriteLine(string line);
        
        DiagnosticsLogLevel Level { get; }
    }

    internal static class DiagnosticsTracerExtensions
    {
        public static void Trace(this IDiagnosticsLogger logger, string message)
        {
            if (logger.Level == DiagnosticsLogLevel.Tracing)
            {
                logger.WriteLine(message);
            }
        }
 
        public static void Log(this IDiagnosticsLogger logger, string message)
        {
            if (logger.Level == DiagnosticsLogLevel.Logging)
            {
                logger.WriteLine(message);
            }
        }
    }
}