using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using NSubstitute.Community.Diagnostics.Logging;
using NSubstitute.Community.Diagnostics.Utils;
using NSubstitute.Core;

namespace NSubstitute.Community.Diagnostics
{
    internal class DiagContextInternal
    {
        private ConditionalWeakTable<ICallRouter, object> _routerToSubstituteMappings =
            new ConditionalWeakTable<ICallRouter, object>();

        private ConditionalWeakTable<ICallRouter, ICallRouter> _routerToDiagRouterMappings =
            new ConditionalWeakTable<ICallRouter, ICallRouter>();

        private ConditionalWeakTable<object, Type> _substituteToPrimaryType = new ConditionalWeakTable<object, Type>();
        public IDiagnosticsLogger Logger { get; }

        public DiagContextInternal(IDiagnosticsLogger logger)
        {
            Logger = logger;
        }

        public void MapRouterToSubstitute(ICallRouter router, object substitute)
        {
            _routerToSubstituteMappings.Add(router, substitute);
        }

        public void MapRouterToDiagRouter(ICallRouter callRouter, ICallRouter diagRouter)
        {
            _routerToDiagRouterMappings.Add(callRouter, diagRouter);
        }

        public bool TryGetSubstituteForRouter(ICallRouter router, out object substitute)
        {
            return _routerToSubstituteMappings.TryGetValue(router, out substitute);
        }

        public ICallRouter MapToDiagRouter(ICallRouter callRouter)
        {
            return _routerToDiagRouterMappings.TryGetValue(callRouter, out ICallRouter diagRouter)
                ? diagRouter
                : callRouter;
        }

        public void RegisterPrimaryProxyType(object substitute, Type type)
        {
            _substituteToPrimaryType.Add(substitute, type);
        }

        public bool TryGetSubstitutePrimaryType(object substitute, out Type type)
        {
            return _substituteToPrimaryType.TryGetValue(substitute, out type);
        }
    }
}