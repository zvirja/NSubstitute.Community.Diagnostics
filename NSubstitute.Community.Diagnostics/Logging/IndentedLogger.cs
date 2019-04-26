using System;
using System.Threading;

namespace NSubstitute.Community.Diagnostics.Logging
{
    internal class IndentedLogger : IDiagnosticsLogger
    {
        private readonly ThreadLocal<int> _level = new ThreadLocal<int>();

        private readonly IDiagnosticsLogger _impl;
        public IndentedLogger(IDiagnosticsLogger impl)
        {
            _impl = impl;
        }

        public DiagnosticsLogLevel Level => _impl.Level;

        public void WriteLine(string line) => _impl.WriteLine(new string(' ', _level.Value * 4) + line);

        public IDisposable Scope() => new ScopeCounter(this);

        private class ScopeCounter : IDisposable
        {
            private readonly IndentedLogger _owner;

            public ScopeCounter(IndentedLogger owner)
            {
                _owner = owner;
                _owner._level.Value++;
            }

            public void Dispose() => _owner._level.Value--;
        }
    }
}