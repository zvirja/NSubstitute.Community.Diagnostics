using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute.Core;
using NSubstitute.Routing;

namespace NSubstitute.Community.Diagnostics
{
    internal class DiagnosticsCallRouter : ICallRouter
    {
        private readonly ICallRouter _impl;
        private readonly DiagContextInternal _ctx;

        public DiagnosticsCallRouter(ICallRouter impl, DiagContextInternal ctx)
        {
            _impl = impl;
            _ctx = ctx;
        }

        public ConfiguredCall LastCallShouldReturn(IReturn returnValue, MatchArgs matchArgs,
            PendingSpecificationInfo pendingSpecInfo)
        {
            Log(
                $"LastCallShouldReturn(value: {returnValue.DiagName(_ctx)}, matchArgs: {matchArgs.DiagName()}, pendingSpecInfo: {pendingSpecInfo.DiagName(_ctx)})");
            return _impl.LastCallShouldReturn(returnValue, matchArgs, pendingSpecInfo);
        }

        public object Route(ICall call)
        {
            Log($"Route(call: {call.DiagName(_ctx)})");
            return _impl.Route(call);
        }

        public IEnumerable<ICall> ReceivedCalls()
        {
            var result = _impl.ReceivedCalls().ToArray();
            Log($"ReceivedCalls() => {result.Print(x => x.DiagName(_ctx))}");
            return result;
        }

        public void SetRoute(Func<ISubstituteState, IRoute> getRoute)
        {
            Log($"SetRoute(routeFactory: {getRoute.DiagName()})");
#pragma warning disable 618 - Just proxy it.
            _impl.SetRoute(getRoute);
#pragma warning restore 618
        }

        public void SetReturnForType(Type type, IReturn returnValue)
        {
            Log($"SetReturnForType(type: {type.DiagName()}, returnValue: {returnValue.DiagName(_ctx)})");
            _impl.SetReturnForType(type, returnValue);
        }

        public void RegisterCustomCallHandlerFactory(CallHandlerFactory factory)
        {
            Log($"RegisterCustomCallHandlerFactory(factory: {factory.DiagName()})");
            _impl.RegisterCustomCallHandlerFactory(factory);
        }

        public void Clear(ClearOptions clear)
        {
            Log($"Clear(clear: {clear.DiagName()})");
            _impl.Clear(clear);
        }

        public bool CallBaseByDefault
        {
            get => _impl.CallBaseByDefault;
            set
            {
                Log($"CallBaseByDefault = {value}");
                _impl.CallBaseByDefault = value;
            }
        }

        public override int GetHashCode() => _impl.GetHashCode();

        private void Log(string message) =>
            _ctx.Tracer.WriteLineWithTID($"[CallRouter] {message} [this: {this.DiagName(_ctx)}]");
    }
}