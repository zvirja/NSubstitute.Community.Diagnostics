using System.Threading;

namespace NSubstitute.Community.Diagnostics
{
    public interface IDiagnosticsTracer
    {
        void WriteLine(string line);
    }

    internal static class DiagnosticsTracerExtensions
    {
        public static void WriteLineWithTID(this IDiagnosticsTracer tracer, string message)
        {
            tracer.WriteLine($"[TID:{Thread.CurrentThread.ManagedThreadId}]{message}");
        }
    }
}