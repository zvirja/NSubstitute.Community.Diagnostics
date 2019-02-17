using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute.Core;
using NSubstitute.Core.Arguments;
using NSubstitute.Routing;

namespace NSubstitute.Community.Diagnostics
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
            Log($"SetLastCallRouter(callRouter: {callRouter.DiagName(_ctx)})");
            _impl.SetLastCallRouter(_ctx.MapToDiagRouter(callRouter));
        }

        public void ClearLastCallRouter()
        {
            Log("ClearLastCallRouter()");
            _impl.ClearLastCallRouter();
        }

        public ConfiguredCall LastCallShouldReturn(IReturn value, MatchArgs matchArgs)
        {
            Log($"LastCallShouldReturn(value: {value.DiagName(_ctx)}, matchArgs: {matchArgs.DiagName()})");
            return _impl.LastCallShouldReturn(value, matchArgs);
        }

        public void SetNextRoute(ICallRouter callRouter, Func<ISubstituteState, IRoute> nextRouteFactory)
        {
            Log(
                $"SetNextRoute(callRouter: {callRouter.DiagName(_ctx)}, nextRouteFactory: {nextRouteFactory.DiagName()})");
            _impl.SetNextRoute(_ctx.MapToDiagRouter(callRouter), nextRouteFactory);
        }

        public Func<ISubstituteState, IRoute> UseNextRoute(ICallRouter callRouter)
        {
            var result = _impl.UseNextRoute(_ctx.MapToDiagRouter(callRouter));
            Log($"UseNextRoute(callRouter: {callRouter.DiagName(_ctx)}) => {result.DiagName()}");
            return result;
        }

        public void EnqueueArgumentSpecification(IArgumentSpecification spec)
        {
            Log($"EnqueueArgumentSpecification(spec: {spec.DiagName()})");
            _impl.EnqueueArgumentSpecification(spec);
        }

        public IList<IArgumentSpecification> DequeueAllArgumentSpecifications()
        {
            var result = _impl.DequeueAllArgumentSpecifications();
            Log($"DequeueAllArgumentSpecifications() => {result.Print(x => x.DiagName())}");
            return result;
        }

        public void SetPendingRaisingEventArgumentsFactory(Func<ICall, object[]> getArguments)
        {
            Log($"SetPendingRaisingEventArgumentsFactory(getArguments: {getArguments.DiagName()})");
            _impl.SetPendingRaisingEventArgumentsFactory(getArguments);
        }

        public Func<ICall, object[]> UsePendingRaisingEventArgumentsFactory()
        {
            var result = _impl.UsePendingRaisingEventArgumentsFactory();
            Log($"UsePendingRaisingEventArgumentsFactory() => {result.DiagName()}");
            return result;
        }

        public void RunInQueryContext(Action calls, IQuery query)
        {
            Log($"BEGIN RunInQueryContext()");
            _impl.RunInQueryContext(calls, query);
            Log($"END RunInQueryContext()");
        }

        public void RegisterInContextQuery(ICall call)
        {
            Log($"RegisterInContextQuery(call: {call.DiagName(_ctx)})");
            _impl.RegisterInContextQuery(call);
        }

        public IPendingSpecification PendingSpecification { get; }

        public bool IsQuerying => _impl.IsQuerying;

        private void Log(string message) => _ctx.Tracer.WriteLineWithTID($"[ThreadLocalContext] {message}");
    }
}