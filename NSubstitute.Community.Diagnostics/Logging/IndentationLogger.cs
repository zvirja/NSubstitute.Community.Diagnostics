namespace NSubstitute.Community.Diagnostics.Logging
{
    internal class IndentationLogger : IDiagnosticsLogger
    {

        private readonly IDiagnosticsLogger _impl;
 
        public IndentationLogger(IDiagnosticsLogger impl)
        {
            _impl = impl;
        }

        public DiagnosticsLogLevel Level => _impl.Level;

        public void WriteLine(string line) => _impl.WriteLine(LoggingScope.GetIndentation() + line);
    }
}