using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute.Community.Diagnostics.Logging;
using NSubstitute.Community.Diagnostics.Utils;
using NSubstitute.Core;
using NSubstitute.Routing;

namespace NSubstitute.Community.Diagnostics.Decorators
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
            Trace($"LastCallShouldReturn(value: {returnValue.DiagName(_ctx)}, " +
                 $"matchArgs: {matchArgs.DiagName()}, pendingSpecInfo: {pendingSpecInfo.DiagName(_ctx)})");
            Log($"[Configure last call] " +
                $"CallSpecification: {pendingSpecInfo.DiagName(_ctx)} " +
                $"Value: {returnValue.DiagName(_ctx)} " +
                $"Match: {matchArgs.DiagName()}");
 
            return _impl.LastCallShouldReturn(returnValue, matchArgs, pendingSpecInfo);
        }

        public object Route(ICall call)
        {
            Trace($"Route(call: {call.DiagName(_ctx)})");
            Log($"[Received call] " +
                $"Substitute: {call.Target().SubstituteId(_ctx)} " +
                $"Call: {call.FormatArgs(_ctx)} " +
                $"Signature: {call.GetMethodInfo().DiagName()} " +
                $"Pending specs: {call.GetArgumentSpecifications().Print(s => s.DiagName())}");
            
            using (_ctx.Logger.Scope())
            {
                return _impl.Route(call);
            }
        }

        public IEnumerable<ICall> ReceivedCalls()
        {
            var result = _impl.ReceivedCalls().ToArray();
            Trace($"ReceivedCalls() => {result.Print(x => x.DiagName(_ctx))}");
            return result;
        }

        public void SetRoute(Func<ISubstituteState, IRoute> getRoute)
        {
            Trace($"SetRoute(routeFactory: {getRoute.DiagName()})");
#pragma warning disable 618 - Just proxy it.
            _impl.SetRoute(getRoute);
#pragma warning restore 618
        }

        public void SetReturnForType(Type type, IReturn returnValue)
        {
            Trace($"SetReturnForType(type: {type.DiagName()}, returnValue: {returnValue.DiagName(_ctx)})");
            _impl.SetReturnForType(type, returnValue);
        }

        public void RegisterCustomCallHandlerFactory(CallHandlerFactory factory)
        {
            Trace($"RegisterCustomCallHandlerFactory(factory: {factory.DiagName()})");
            _impl.RegisterCustomCallHandlerFactory(factory);
        }

        public void Clear(ClearOptions clear)
        {
            Trace($"Clear(clear: {clear.DiagName()})");
            _impl.Clear(clear);
        }

        public bool CallBaseByDefault
        {
            get => _impl.CallBaseByDefault;
            set
            {
                Trace($"CallBaseByDefault = {value}");
                _impl.CallBaseByDefault = value;
            }
        }

        public override int GetHashCode() => _impl.GetHashCode();

        private void Trace(string message) =>
            _ctx.Logger.TraceWithTID($"[CallRouter] {message} [this: {this.DiagName(_ctx)}]");
 
        private void Log(string message) => _ctx.Logger.LogWithTID($"[CallRouter]{message}");
    }
}