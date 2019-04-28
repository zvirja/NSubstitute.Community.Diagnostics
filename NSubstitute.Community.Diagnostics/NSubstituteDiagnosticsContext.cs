using System;
using NSubstitute.Community.Diagnostics.Decorators;
using NSubstitute.Community.Diagnostics.Logging;
using NSubstitute.Community.Diagnostics.Utils;
using NSubstitute.Core;
using NSubstitute.Core.DependencyInjection;

namespace NSubstitute.Community.Diagnostics
{
    public static class NSubstituteDiagnosticsContext
    {
        private static readonly object InstallLock = new object();

        internal static IDisposable Install(IDiagnosticsLogger logger)
        {
            ISubstitutionContext originalContext;

            lock (InstallLock)
            {
                ISubstitutionContext diagnosticsContext = CreateContext(logger);
                originalContext = InstallDiagContext(diagnosticsContext, logger);
            }

            return new DelegatingDisposable(() =>
            {
                lock (InstallLock)
                {
                    UninstallDiagContext(originalContext, logger);
                }
            });
        }

        /// <summary>
        /// Installs tracer which performs detailed NSubstitute monitoring suitable for advanced scenarios troubleshoot.
        /// You should dispose the returned context to resume normal work.
        /// Notice, context cannot be installed/uninstalled concurrently, as it affects static resources.
        /// </summary>
        public static IDisposable CreateTracingContext(Action<string> logger)
        {
            return Install(
                new CallerLogger(
                    new ThreadIdLogger(
                        new IndentationLogger(
                            new FuncLogger(logger, DiagnosticsLogLevel.Tracing)))));
        }

        /// <summary>
        /// Installs logger which performs basic NSubstitute monitoring suitable for basic scenarios troubleshoot.
        /// You should dispose the returned context to resume normal work.
        /// Notice, context cannot be installed/uninstalled concurrently, as it affects static resources.
        /// </summary>
        public static IDisposable CreateLoggingContext(Action<string> logger)
        {
            return Install(
                new CallerLogger(
                    new ThreadIdLogger(
                        new IndentationLogger(
                            new FuncLogger(logger, DiagnosticsLogLevel.Logging)))));
        }
        
        private static ISubstitutionContext CreateContext(IDiagnosticsLogger logger)
        {
            var diagCtx = new DiagContextInternal(logger);
            
            return NSubstituteDefaultFactory.DefaultContainer
                .Customize()
                .Decorate<IThreadLocalContext>((impl, _) => new DiagnosticsThreadLocalContext(impl, diagCtx))
                .Decorate<IProxyFactory>((impl, _) => new DiagnosticsProxyFactory(impl, diagCtx))
                .Resolve<ISubstitutionContext>();
        }

        private static ISubstitutionContext InstallDiagContext(ISubstitutionContext newCtx, IDiagnosticsLogger logger)
        {
            if (!IsDiagContext(newCtx))
                throw new ArgumentException($"Diagnostics context is expected. Actual context type: {newCtx.GetType().FullName}");

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

            SubstitutionContext.Current = newCtx;
            Log("Installed diagnostics context", logger);

            return oldContext;
        }

        private static void UninstallDiagContext(ISubstitutionContext originalCtx, IDiagnosticsLogger logger)
        {
            var current = SubstitutionContext.Current;
            if (!IsDiagContext(current))
                throw new InvalidOperationException(
                    "You are trying to uninstall diagnostics context while current context is not diagnostics. " +
                    "This is clear indication that something goes wrong." +
                    Environment.NewLine +
                    "Notice, the issue might also happen due to concurrency if you run multiple tests with diagnostics in parallel. " +
                    "In that case please ensure that you run tests sequentially or install the hook globally on an assembly level.");

            SubstitutionContext.Current = originalCtx;
            Log("Restored normal context", logger);
        }

        private static bool IsDiagContext(ISubstitutionContext context) =>
            context.ThreadContext.GetType() == typeof(DiagnosticsThreadLocalContext);

        private static void Log(string message, IDiagnosticsLogger logger) =>
            logger.WriteLine($"[DiagnosticsContextInstaller] {message}");
    }
}