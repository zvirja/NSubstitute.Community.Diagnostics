using System;
using NSubstitute.Core;
using NSubstitute.Core.DependencyInjection;
using NSubstitute.Proxies;
using NSubstitute.Proxies.CastleDynamicProxy;
using NSubstitute.Proxies.DelegateProxy;

namespace NSubstitute.Community.Diagnostics
{
    public class NSubstituteDiagnosticsContext : IDisposable
    {
        private static readonly object InstallLock = new object();
        
        private readonly ISubstitutionContext _oldContext;
        public IDiagnosticsTracer Tracer { get; }

        public NSubstituteDiagnosticsContext(Action<string> logger) : this(new LoggingFunctionDiagnosticsTracer(logger))
        {
        }

        public NSubstituteDiagnosticsContext(IDiagnosticsTracer tracer)
        {
            Tracer = tracer;
 
            var diagContext = CreateDiagContext(tracer);
            _oldContext = InstallDiagContext(diagContext, tracer);
        }

        private static ISubstitutionContext CreateDiagContext(IDiagnosticsTracer tracer)
        {
            var ctx = new DiagContextInternal(tracer);
            
            return NSubstituteDefaultFactory.DefaultContainer
                .Customize()
                .RegisterPerScope<ThreadLocalContext, ThreadLocalContext>()
                .RegisterPerScope<IThreadLocalContext>(r =>
                    new DiagnosticsThreadLocalContext(
                        r.Resolve<ThreadLocalContext>(),
                        ctx))
                .RegisterPerScope<IProxyFactory>(r =>
                    new DiagnosticsProxyFactory(
                        new ProxyFactory(r.Resolve<DelegateProxyFactory>(), r.Resolve<CastleDynamicProxyFactory>()),
                        ctx))
                .Resolve<ISubstitutionContext>();
            
        }

        private static ISubstitutionContext InstallDiagContext(ISubstitutionContext newContext, IDiagnosticsTracer tracer)
        {
            if(!IsDiagContext(newContext))
                throw new ArgumentException($"Diagnostics context is expected. Actual context type: {newContext.GetType().FullName}");

            lock (InstallLock)
            {
                var oldContext = SubstitutionContext.Current;
                if (IsDiagContext(oldContext))
                {
                    throw new InvalidOperationException(
                        "You are trying to install diagnostics context while other diagnostics context is already installed. " +
                        "This scenario is not supported and might indicate that something is going wrong." +
                        Environment.NewLine +
                        "Notice, the issue might also happen due to concurrency if you run multiple tests with diagnostics in parallel. " +
                        "In that case please ensure that you run tests sequentially or install the hook globally on an assembly level.");
                }

                SubstitutionContext.Current = newContext;
                Log("Installed diagnostics context", tracer);

                return oldContext;
            }
        }

        private static bool IsDiagContext(ISubstitutionContext context)
        {
            return context.ThreadContext.GetType() == typeof(DiagnosticsThreadLocalContext);
        }

        private static void Log(string message, IDiagnosticsTracer tracer) =>
            tracer.WriteLineWithTID($"[DiagnosticsContextInstaller] {message}");

        public void Dispose()
        {
            lock (InstallLock)
            {
                var current = SubstitutionContext.Current;
                if(!IsDiagContext(current))
                    throw new InvalidOperationException(
                        "You are trying to uninstall diagnostics context while current context is not diagnostics. " +
                        "This is clear indication that something goes wrong." +
                        Environment.NewLine +
                        "Notice, the issue might also happen due to concurrency if you run multiple tests with diagnostics in parallel. " +
                        "In that case please ensure that you run tests sequentially or install the hook globally on an assembly level.");

                SubstitutionContext.Current = _oldContext;
                Log("Restored normal context", Tracer);
            }
        }
    }
}