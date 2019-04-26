using System;

namespace NSubstitute.Community.Diagnostics.Logging
{
    internal class LoggingScope : IDisposable
    {
        [ThreadStatic]
        private static int _level;

        public LoggingScope()
        {
            _level++;
        }

        public void Dispose()
        {
            _level--;
        }

        public static string GetIndentation() => new string(' ', _level * 4);
    }
}