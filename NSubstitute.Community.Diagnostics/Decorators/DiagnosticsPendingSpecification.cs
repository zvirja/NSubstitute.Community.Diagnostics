using NSubstitute.Community.Diagnostics.Logging;
using NSubstitute.Community.Diagnostics.Utils;
using NSubstitute.Core;

namespace NSubstitute.Community.Diagnostics.Decorators
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
            Trace($"UseCallSpecInfo() => {result.DiagName(_ctx)}");
            return result;
        }

        public void SetCallSpecification(ICallSpecification callSpecification)
        {
            Trace($"SetCallSpecification(spec: {callSpecification.DiagName()})");
            _impl.SetCallSpecification(callSpecification);
        }

        public void SetLastCall(ICall lastCall)
        {
            Trace($"SetLastCall(call: {lastCall.DiagName(_ctx)})");
            _impl.SetLastCall(lastCall);
        }

        public void Clear()
        {
            Trace("Clear()");
            _impl.Clear();
        }

        public override int GetHashCode() => _impl.GetHashCode();

        private void Trace(string message) =>
            _ctx.Logger.Trace($"[ThreadLocalContext.PendingSpecification] {message}");
    }
}