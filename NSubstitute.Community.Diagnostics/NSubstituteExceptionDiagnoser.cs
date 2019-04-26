using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using NSubstitute.Community.Diagnostics.Logging;
using NSubstitute.Exceptions;

namespace NSubstitute.Community.Diagnostics
{
    public static class NSubstituteExceptionDiagnoser
    {
        private static int _isInstalled;

        [ThreadStatic] private static Stack<string> _logEntries;

        /// <summary>
        /// Installs global NSubstitute logger and exception listener.
        /// Whenever NSubstitute exception is thrown, NSubstitute log is attached.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Install()
        {
            if (Interlocked.CompareExchange(ref _isInstalled, 1, 0) != 0)
            {
                throw new InvalidOperationException("Diagnoser is already installed.");
            }

            InstallDiagnosticsContext();
            InstallExceptionHook();
        }

        private static void InstallDiagnosticsContext()
        {
            // Context is installed on creation, so we just do nothing with the object.
            // Current API doesn't allow to uninstall context, so we forget the object.
            new NSubstituteDiagnosticsContext(
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

        private static void InstallExceptionHook()
        {
            AppDomain.CurrentDomain.FirstChanceException += OnException;
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