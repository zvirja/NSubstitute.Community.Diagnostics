using System;

namespace NSubstitute.Community.Diagnostics.Utils
{
    public class DelegatingDisposable : IDisposable
    {
        private readonly Action _onDispose;

        public DelegatingDisposable(Action onDispose)
        {
            _onDispose = onDispose;
        }
        
        public void Dispose() => _onDispose.Invoke();
    }
}