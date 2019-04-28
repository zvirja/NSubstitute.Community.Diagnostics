using System;
using System.Collections.Generic;
using NSubstitute.Community.Diagnostics.Logging;
using NSubstitute.Community.Diagnostics.Utils;
using NSubstitute.Core;
using NSubstitute.Core.Arguments;
using NSubstitute.Routing;

namespace NSubstitute.Community.Diagnostics.Decorators
{
    internal class DiagnosticsThreadLocalContext : IThreadLocalContext
    {
        private readonly IThreadLocalContext _impl;
        private readonly DiagContextInternal _ctx;

        public DiagnosticsThreadLocalContext(IThreadLocalContext impl, DiagContextInternal ctx)
        {
            _impl = impl;
            _ctx = ctx;
            PendingSpecification = new DiagnosticsPendingSpecification(impl.PendingSpecification, ctx);
        }

        public void SetLastCallRouter(ICallRouter callRouter)
        {
            Trace($"SetLastCallRouter(callRouter: {callRouter.DiagName(_ctx)})");
            _impl.SetLastCallRouter(_ctx.MapToDiagRouter(callRouter));
        }

        public void ClearLastCallRouter()
        {
            Trace("ClearLastCallRouter()");
            _impl.ClearLastCallRouter();
        }

        public ConfiguredCall LastCallShouldReturn(IReturn value, MatchArgs matchArgs)
        {
            Trace($"LastCallShouldReturn(value: {value.DiagName(_ctx)}, matchArgs: {matchArgs.DiagName()})");
            return _impl.LastCallShouldReturn(value, matchArgs);
        }

        public void SetNextRoute(ICallRouter callRouter, Func<ISubstituteState, IRoute> nextRouteFactory)
        {
            Trace(
                $"SetNextRoute(callRouter: {callRouter.DiagName(_ctx)}, nextRouteFactory: {nextRouteFactory.DiagName()})");
            _impl.SetNextRoute(_ctx.MapToDiagRouter(callRouter), nextRouteFactory);
        }

        public Func<ISubstituteState, IRoute> UseNextRoute(ICallRouter callRouter)
        {
            var result = _impl.UseNextRoute(_ctx.MapToDiagRouter(callRouter));
            Trace($"UseNextRoute(callRouter: {callRouter.DiagName(_ctx)}) => {result.DiagName()}");
            return result;
        }

        public void EnqueueArgumentSpecification(IArgumentSpecification spec)
        {
            Trace($"EnqueueArgumentSpecification(spec: {spec.DiagName()})");
            Log($"[Enqueue argument specification] Specification: {spec.DiagName()} ");
            _impl.EnqueueArgumentSpecification(spec);
        }

        public IList<IArgumentSpecification> DequeueAllArgumentSpecifications()
        {
            var result = _impl.DequeueAllArgumentSpecifications();
            Trace($"DequeueAllArgumentSpecifications() => {result.Print(x => x.DiagName())}");
            return result;
        }

        public void SetPendingRaisingEventArgumentsFactory(Func<ICall, object[]> getArguments)
        {
            Trace($"SetPendingRaisingEventArgumentsFactory(getArguments: {getArguments.DiagName()})");
            _impl.SetPendingRaisingEventArgumentsFactory(getArguments);
        }

        public Func<ICall, object[]> UsePendingRaisingEventArgumentsFactory()
        {
            var result = _impl.UsePendingRaisingEventArgumentsFactory();
            Trace($"UsePendingRaisingEventArgumentsFactory() => {result.DiagName()}");
            return result;
        }

        public void RunInQueryContext(Action calls, IQuery query)
        {
            Trace($"BEGIN RunInQueryContext()");
            using (new LoggingScope())
            {
                _impl.RunInQueryContext(calls, query);
            }
            Trace($"END RunInQueryContext()");
        }

        public void RegisterInContextQuery(ICall call)
        {
            Trace($"RegisterInContextQuery(call: {call.DiagName(_ctx)})");
            _impl.RegisterInContextQuery(call);
        }

        public IPendingSpecification PendingSpecification { get; }

        public bool IsQuerying => _impl.IsQuerying;

        private void Trace(string message) => _ctx.Logger.Trace($"[ThreadLocalContext] {message}");
        private void Log(string message) => _ctx.Logger.Log(message);
    }
}