using System;
using NSubstitute.Core;

namespace NSubstitute.Community.Diagnostics
{
    internal class DiagnosticsProxyFactory : IProxyFactory
    {
        private readonly IProxyFactory _impl;
        private readonly DiagContextInternal _ctx;

        public DiagnosticsProxyFactory(IProxyFactory impl, DiagContextInternal ctx)
        {
            _impl = impl;
            _ctx = ctx;
        }

        public object GenerateProxy(ICallRouter callRouter, Type typeToProxy, Type[] additionalInterfaces, object[] constructorArguments)
        {
            var diagCallRouter = new DiagnosticsCallRouter(callRouter, _ctx);
            var result = _impl.GenerateProxy(diagCallRouter, typeToProxy, additionalInterfaces, constructorArguments);
 
            _ctx.RegisterPrimaryProxyType(result, typeToProxy);

            Log(
                $"GenerateProxy(callRouter: {callRouter.DiagName(_ctx)}, " +
                $"typeToProxy: {typeToProxy.FullName}, " +
                $"additionalInterfaces: {additionalInterfaces.Print(x => x.FullName)}, " +
                $"constructorArguments: {constructorArguments.Print(a => a.GetObjectId(_ctx))}) " +
                $"=> {result.SubstituteId(_ctx)}");
 
            _ctx.MapRouterToSubstitute(callRouter, result);
            _ctx.MapRouterToSubstitute(diagCallRouter, result);
            _ctx.MapRouterToDiagRouter(callRouter, diagCallRouter);
            
            return result;
        }
 
        public override int GetHashCode() => _impl.GetHashCode();
 
        private void Log(string message) => _ctx.Tracer.WriteLineWithTID($"[ProxyFactory] {message}");
    }
}