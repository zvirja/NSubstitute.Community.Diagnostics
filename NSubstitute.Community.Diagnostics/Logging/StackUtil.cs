using System.Diagnostics;
using System.Reflection;
using Castle.DynamicProxy;
using NSubstitute.Community.Diagnostics.Utils;

namespace NSubstitute.Community.Diagnostics.Logging
{
    internal static class StackUtil
    {
        private static readonly Assembly NSubstituteAssembly = typeof(Substitute).Assembly;
        private static readonly Assembly CastleProxyAssembly = typeof(ProxyGenerator).Assembly;
        private static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
 
        public static string GetCallerMethodName()
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