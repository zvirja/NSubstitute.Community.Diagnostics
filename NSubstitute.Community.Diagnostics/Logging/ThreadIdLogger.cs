using System.Threading;

namespace NSubstitute.Community.Diagnostics.Logging
{
    internal class ThreadIdLogger : IDiagnosticsLogger
    {
        private readonly IDiagnosticsLogger _impl;

        public ThreadIdLogger(IDiagnosticsLogger impl)
        {
            _impl = impl;
        }

        public DiagnosticsLogLevel Level => _impl.Level;

        public void WriteLine(string line)
        {
            _impl.WriteLine($"[TID:{Thread.CurrentThread.ManagedThreadId}]{line}");
        }
    }
}