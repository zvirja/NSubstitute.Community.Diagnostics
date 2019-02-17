using System;

namespace NSubstitute.Community.Diagnostics
{
    internal class LoggingFunctionDiagnosticsTracer : IDiagnosticsTracer
    {
        private readonly Action<string> _log;

        public LoggingFunctionDiagnosticsTracer(Action<string> log)
        {
            _log = log;
        }
        
        public void WriteLine(string line) => _log(line);
    }
}