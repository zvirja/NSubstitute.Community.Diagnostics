using System.Diagnostics;
using System.Reflection;
using Castle.DynamicProxy;
using NSubstitute.Community.Diagnostics.Utils;

namespace NSubstitute.Community.Diagnostics.Logging
{
    internal class CallerLogger : IDiagnosticsLogger
    {
        private static readonly Assembly NSubstituteAssembly = typeof(Substitute).Assembly;
        private static readonly Assembly CastleProxyAssembly = typeof(ProxyGenerator).Assembly;
        private static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
        private readonly IDiagnosticsLogger _impl;

        public CallerLogger(IDiagnosticsLogger impl)
        {
            _impl = impl;
        }

        public DiagnosticsLogLevel Level => _impl.Level;

        public void WriteLine(string line) => _impl.WriteLine($"[Caller: {GetCallerMethodName()}]{line}");
 
        private static string GetCallerMethodName()
        {
            const string Unknown = "<unknown>";
            
            var stack = new StackTrace();
            var frames = stack.GetFrames();

            if (frames == null)
                return Unknown;
            
            
            foreach (var stackFrame in frames)
            {
                var method = stackFrame.GetMethod();
                var declaringAssembly = method.DeclaringType?.Assembly;
                if(declaringAssembly == null)
                    continue;

                if (declaringAssembly == NSubstituteAssembly ||
                    declaringAssembly == CurrentAssembly ||
                    declaringAssembly == CastleProxyAssembly ||
                    declaringAssembly.IsDynamic)
                {
                    continue;
                }

                return $"{method.DeclaringType.DiagName()}.{method.Name}";
            }

            return Unknown;
        }
    }
}