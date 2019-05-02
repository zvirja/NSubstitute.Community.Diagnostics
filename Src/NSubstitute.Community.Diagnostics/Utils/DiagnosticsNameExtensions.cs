using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using NSubstitute.Core;
using NSubstitute.Core.Arguments;

namespace NSubstitute.Community.Diagnostics.Utils
{
    internal static class DiagnosticsNameExtensions
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
            return $"<{argSpec} Signature: {argSpec.GetMethodInfo().DiagName()}>";
        }

        public static string DiagName(this ICall call, DiagContextInternal ctx)
        {
            var substituteId = call.Target().SubstituteId(ctx);
            var callArgs = call.FormatArgs(ctx);
            var signature = call.GetMethodInfo().DiagName();
 
            return $"<[{substituteId}].{callArgs} Signature: {signature}>";
        }

        public static string FormatArgs(this ICall call, DiagContextInternal ctx)
        {
            var methodInfo = call.GetMethodInfo();
            return CallFormatter.Format(methodInfo, call.GetOriginalArguments().Select(a => a.GetObjectId(ctx)));
        }

        public static string DiagName(this Type type)
        {
            return type.GetNonMangledTypeName();
        }

        public static string DiagName(this MethodInfo methodInfo)
        {
            var nameAndArgs = CallFormatter.Format(methodInfo, FormatMethodParameterTypes(methodInfo.GetParameters()));
            var retType = methodInfo.ReturnType.DiagName();

            return $"{nameAndArgs} -> {retType}";

            IEnumerable<string> FormatMethodParameterTypes(IEnumerable<ParameterInfo> parameters)
            {
                return parameters.Select(p =>
                {
                    var type = p.ParameterType;

                    if (p.IsOut)
                        return "out " + type.GetElementType().GetNonMangledTypeName();

                    if (type.IsByRef)
                        return "ref " + type.GetElementType().GetNonMangledTypeName();

                    if (p.IsParams())
                        return "params " + type.GetNonMangledTypeName();

                    return type.GetNonMangledTypeName();
                });
            }
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
                proxyType = $".{primaryType.DiagName()}";
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
            var typeName = type.GetNonMangledTypeName();
            // Trim "Diagnostics" prefix from type to make output more clear.
            // It doesn't matter to end user whether we have wrapper - we don't alter the logic.
            if (type.Assembly == Assembly.GetExecutingAssembly() &&
                typeName.StartsWith("Diagnostics", StringComparison.Ordinal))
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
            else if (obj is ICallRouter || obj is IPendingSpecification || obj is IProxyFactory ||
                     obj is IThreadLocalContext)
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

        private static string GetNonMangledTypeName(this Type type)
        {
            // Implementation copied from NSubstitute:
            // https://github.com/nsubstitute/NSubstitute/blob/d02239799fdfdca8332fa2600eecd8c8df5ec784/src/NSubstitute/Core/Extensions.cs#L50
            
            // Handle simple case without invoking more complex logic.
            if (!type.GetTypeInfo().IsGenericType && !type.IsNested)
            {
                return type.Name;
            }

            Type[] genericTypeArguments = type.GetGenericArguments();
            int alreadyHandledGenericArgumentsCount = 0;
            var resultTypeName = new StringBuilder();

            void AppendTypeNameRecursively(Type currentType)
            {
                Type declaringType = currentType.DeclaringType;
                if (declaringType != null)
                {
                    AppendTypeNameRecursively(declaringType);
                    resultTypeName.Append("+");
                }

                resultTypeName.Append(GetTypeNameWithoutGenericArity(currentType));

                // When you take the generic type arguments for a nested type, the type arguments from parent types
                // are included as well. We don't want to include them again, so simply skip all the already
                // handled arguments.
                // Notice, we expect generic type arguments order to always be parent to child, left to right.
                string[] ownGenericArguments = genericTypeArguments
                    .Take(currentType.GetGenericArguments().Length)
                    .Skip(alreadyHandledGenericArgumentsCount)
                    .Select(t => t.GetNonMangledTypeName())
                    .ToArray();

                if (ownGenericArguments.Length == 0)
                {
                    return;
                }

                alreadyHandledGenericArgumentsCount += ownGenericArguments.Length;

                resultTypeName.Append("<");
                resultTypeName.Append(string.Join(", ", ownGenericArguments));
                resultTypeName.Append(">");
            }

            AppendTypeNameRecursively(type);
            return resultTypeName.ToString();

            string GetTypeNameWithoutGenericArity(Type t)
            {
                var tn = t.Name;
                var indexOfBacktick = tn.IndexOf('`');
                // For nested generic types the back stick symbol might be missing.
                return indexOfBacktick > -1 ? tn.Substring(0, indexOfBacktick) : tn;
            }
        }
    }
}