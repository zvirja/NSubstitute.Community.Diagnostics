using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NSubstitute.Core;
using NSubstitute.Core.Arguments;

namespace NSubstitute.Community.Diagnostics
{
    internal static class DiagnosticsExtensions
    {
        private static readonly IMethodInfoFormatter CallFormatter = new CallFormatter();

        public static string DiagName(this Delegate @delegate) => $"<{@delegate?.Method.Name ?? "null"}>";

        public static string DiagName(this MatchArgs matchArgs)
        {
            if (matchArgs == MatchArgs.Any) return "<any>";
            if (matchArgs == MatchArgs.AsSpecifiedInCall) return "<as specified>";
            return "<unknown>";
        }

        public static string DiagName(this ICallRouter callRouter, DiagContextInternal ctx)
        {
            var name = $"<{callRouter.GetObjectId(ctx)}";
            if (ctx.TryGetSubstituteForRouter(callRouter, out object substitute))
            {
                name += $"#{substitute.SubstituteId(ctx)}";
            }

            return $"{name}>";
        }

        public static string DiagName(this IReturn value, DiagContextInternal ctx)
        {
            var unknown = $"<UNKNOWN|{value.GetType().DiagName()}>";
            
            if (value is ReturnValue rvs)
                return ReflectionReader.ReadFieldValue(
                    rvs,
                    "_value",
                    (object o) => o.GetObjectId(ctx),
                    "<VALUE|CANNOT_READ>");

            var valueType = value.GetType();
            if (!valueType.IsGenericType)
                return unknown;

            var gtd = valueType.GetGenericTypeDefinition();

            if (gtd == typeof(ReturnMultipleValues<>))
            {
                return ReflectionReader.ReadFieldValue(
                    value,
                    "_valuesToReturn",
                    (IEnumerable<object> objs) => objs.Print(o => o.GetObjectId(ctx)),
                    "<MVALUES|CANNOT_READ>");
            }
            
            if (gtd == typeof(ReturnValueFromFunc<>))
            {
                return ReflectionReader.ReadFieldValue(
                    value,
                    "_funcToReturnValue",
                    (Delegate d) => $"<FUNC|{d.DiagName()}>",
                    "<FVALUE|CANNOT_READ>");
            }

            if (gtd == typeof(ReturnMultipleFuncsValues<>))
            {
                return ReflectionReader.ReadFieldValue(
                    value,
                    "_funcsToReturn",
                    (IEnumerable<Delegate> ds) => $"<FUNCS|{ds.Print(DiagName)}>",
                    "<FMVALUES|CANNOT_READ>");
            }

            return unknown;
        }

        public static string DiagName(this IArgumentSpecification argSpec)
        {
            return $"<{argSpec}>";
        }

        public static string DiagName(this ICallSpecification argSpec)
        {
            return $"<{argSpec} -> {argSpec.GetMethodInfo().ReturnType.DiagName()}>";
        }

        public static string DiagName(this ICall call, DiagContextInternal ctx)
        {
            return
                $"<{call.Target().SubstituteId(ctx)}>.{CallFormatter.Format(call.GetMethodInfo(), call.GetOriginalArguments().Select(a => a.GetObjectId(ctx)))} -> {call.GetMethodInfo().ReturnType.DiagName()}";
        }

        public static string DiagName(this Type type)
        {
            return type.Name;
        }

        public static string DiagName<T>(this T value) where T : struct, Enum
        {
            return $"<{value}>";
        }

        public static string DiagName(this PendingSpecificationInfo pendingSpecification, DiagContextInternal ctx)
        {
            string innerInfo = pendingSpecification.Handle(
                spec => $"SPEC|{spec.DiagName()}",
                call => $"CALL|{call.DiagName(ctx)}");

            return $"<{innerInfo}>";
        }

        public static string SubstituteId(this object substitute, DiagContextInternal ctx)
        {
            string proxyType = "";
            if (ctx.TryGetSubstitutePrimaryType(substitute, out Type primaryType))
            {
                proxyType = $".{primaryType.Name}";
            }

            return $"Substitute{proxyType}|{RuntimeHelpers.GetHashCode(substitute):x8}";
        }

        public static string Print<T>(this IEnumerable<T> sequence, Func<T, string> formatter)
        {
            return "[" + string.Join(", ", sequence.Select(formatter)) + "]";
        }

        public static string GetObjectId(this object obj, DiagContextInternal ctx)
        {
            if (obj is null)
            {
                return "<null>";
            }

            if (obj is ICallRouterProvider)
            {
                return obj.SubstituteId(ctx);
            }
            
            var type = obj.GetType();
            var typeName = type.Name;
            // Trim "Diagnostics" prefix from type to make output more clear.
            // It doesn't matter to end user whether we have wrapper - we don't alter the logic.
            if (type.Assembly == Assembly.GetExecutingAssembly())
                typeName = typeName.Substring("Diagnostics".Length);

            string id;
            if (type == typeof(string))
            {
                id = $"\"{obj}\"";
            }
            else if (type.IsEnum)
            {
                id = $"<{obj}>";
            }
            else if (type.IsPrimitive)
            {
                id = obj.ToString();
            }
            else if(obj is ICallRouter || obj is IPendingSpecification || obj is IProxyFactory || obj is IThreadLocalContext)
            {
                id = obj.GetHashCode().ToString("x8");
            }
            // It's unsafe to call `.ToString()` on unknown types, as this method might be overridden and it will
            // lead to a substitute member invocation. In rare cases it might lead to the stack overflow.
            // To mitigate that we invoke method which doesn't invoke custom client code.
            else 
            {
                id = RuntimeHelpers.GetHashCode(obj).ToString("x8");
            }

            return $"{typeName}|{id}";
        }
    }
}