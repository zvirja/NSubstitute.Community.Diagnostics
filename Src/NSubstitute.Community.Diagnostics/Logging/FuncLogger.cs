using System;

namespace NSubstitute.Community.Diagnostics.Logging
{
    internal class FuncLogger : IDiagnosticsLogger
    {
        private readonly Action<string> _log;
        public DiagnosticsLogLevel Level { get; }
 
        public FuncLogger(Action<string> log, DiagnosticsLogLevel level)
        {
            _log = log;
            Level = level;
        }

        public void WriteLine(string line) => _log(line);
    }
}