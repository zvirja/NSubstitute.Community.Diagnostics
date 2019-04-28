using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NSubstitute.Community.Diagnostics.Logging;
using NSubstitute.Community.Diagnostics.Utils;
using NSubstitute.Exceptions;

namespace NSubstitute.Community.Diagnostics
{
    public static class NSubstituteExceptionDiagnostics
    {
        private static readonly object InstallLock = new object();

        [ThreadStatic] private static Stack<string> _logEntries;

        private static IDisposable _diagContext;

        /// <summary>
        /// Installs global NSubstitute logger and exception listener.
        /// Whenever NSubstitute exception is thrown, NSubstitute internal log is attached.
        /// </summary>
        public static IDisposable Install()
        {
            lock (InstallLock)
            {
                if (IsDiagnosticsContextInstalled())
                    throw new InvalidOperationException(
                        "Exception diagnostics is already installed. " +
                        "Ensure that hook is installed only once globally on assembly level.");

                InstallDiagnosticsContext();
                InstallExceptionHook();
            }

            return new DelegatingDisposable(() =>
            {
                lock (InstallLock)
                {
                    if (!IsDiagnosticsContextInstalled())
                        throw new InvalidOperationException("Exception diagnostics is not installed.");

                    UninstallDiagnosticsContext();
                    UninstallExceptionHook();
                }
            });
        }

        private static bool IsDiagnosticsContextInstalled() => _diagContext != null;

        private static void InstallDiagnosticsContext()
        {
            // Installer throws in case of concurrency, so if method returns - everything is fine.
            _diagContext = NSubstituteDiagnosticsContext.Install(
                new CallerLogger(
                    new IndentationLogger(
                        new FuncLogger(
                            msg =>
                            {
                                if (_logEntries == null)
                                {
                                    _logEntries = new Stack<string>();
                                }

                                _logEntries.Push(msg);
                            },
                            DiagnosticsLogLevel.Logging))));
        }

        private static void UninstallDiagnosticsContext()
        {
            _diagContext.Dispose();
            _diagContext = null;
        }

        private static void InstallExceptionHook()
        {
            AppDomain.CurrentDomain.FirstChanceException += OnException;
        }
        
        private static void UninstallExceptionHook()
        {
            AppDomain.CurrentDomain.FirstChanceException -= OnException;
        }

        private static void OnException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (e.Exception is SubstituteException substituteException)
            {
                var currentThreadEntries = _logEntries?.ToArray() ?? new string[0];
                if (currentThreadEntries.Length > 0)
                {
                    var newMessage = BuildMessage(currentThreadEntries, substituteException);
                    PatchExceptionMessage(substituteException, newMessage);
                }
            }
        }

        private static string BuildMessage(string[] logEntries, Exception originalException)
        {
            return $"{originalException.Message}" +
                   Environment.NewLine +
                   "************************************************************************" +
                   Environment.NewLine +
                   "***************************  LOG (reversed)  ***************************" +
                   Environment.NewLine +
                   "************************************************************************" +
                   Environment.NewLine +
                   Environment.NewLine +
                   string.Join(Environment.NewLine, logEntries) +
                   Environment.NewLine +
                   Environment.NewLine +
                   "************************************************************************" +
                   Environment.NewLine +
                   "************************************************************************" +
                   Environment.NewLine +
                   Environment.NewLine;
        }

        private static void PatchExceptionMessage(Exception ex, string newMessage)
        {
            // It's a dirty way of patching the existing exception.
            // There is no public API for that, so we are using reflection.
            // Given that this code is used for very exceptional situations, let's hope it's OK to do it.
            var field = ex.GetType().GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(ex, newMessage);
            }
        }
    }
}