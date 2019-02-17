using NSubstitute.Core;

namespace NSubstitute.Community.Diagnostics
{
    internal class DiagnosticsPendingSpecification : IPendingSpecification
    {
        private readonly IPendingSpecification _impl;
        private readonly DiagContextInternal _ctx;

        public DiagnosticsPendingSpecification(IPendingSpecification impl, DiagContextInternal ctx)
        {
            _impl = impl;
            _ctx = ctx;
        }

        public bool HasPendingCallSpecInfo() => _impl.HasPendingCallSpecInfo();

        public PendingSpecificationInfo UseCallSpecInfo()
        {
            var result = _impl.UseCallSpecInfo();
            Log($"UseCallSpecInfo() => {result.DiagName(_ctx)}");
            return result;
        }

        public void SetCallSpecification(ICallSpecification callSpecification)
        {
            Log($"SetCallSpecification(spec: {callSpecification.DiagName()})");
            _impl.SetCallSpecification(callSpecification);
        }

        public void SetLastCall(ICall lastCall)
        {
            Log($"SetLastCall(call: {lastCall.DiagName(_ctx)})");
            _impl.SetLastCall(lastCall);
        }

        public void Clear()
        {
            Log("Clear()");
            _impl.Clear();
        }

        public override int GetHashCode() => _impl.GetHashCode();

        private void Log(string message) =>
            _ctx.Tracer.WriteLineWithTID($"[ThreadLocalContext.PendingSpecification] {message}");
    }
}