namespace NSubstitute.Community.Diagnostics.Logging
{
    internal class IndentationLogger : IDiagnosticsLogger
    {

        private readonly IDiagnosticsLogger _next;
 
        public IndentationLogger(IDiagnosticsLogger next)
        {
            _next = next;
        }

        public DiagnosticsLogLevel Level => _next.Level;

        public void WriteLine(string line) => _next.WriteLine(LoggingScope.GetIndentation() + line);
    }
}