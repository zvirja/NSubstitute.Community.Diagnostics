using System.Threading;

namespace NSubstitute.Community.Diagnostics.Logging
{
    internal class ThreadIdLogger : IDiagnosticsLogger
    {
        private readonly IDiagnosticsLogger _next;

        public ThreadIdLogger(IDiagnosticsLogger next)
        {
            _next = next;
        }

        public DiagnosticsLogLevel Level => _next.Level;

        public void WriteLine(string line) =>
            _next.WriteLine($"[TID:{Thread.CurrentThread.ManagedThreadId}]{line}");
    }
}