using System;
using System.Reflection;

namespace NSubstitute.Community.Diagnostics
{
    internal static class ReflectionReader
    {
        public static string ReadFieldValue<TRaw>(object obj, string memberName, Func<TRaw, string> reader, string defaultValue)
        {
            var fieldInfo = obj.GetType().GetField(memberName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo != null && fieldInfo.GetValue(obj) is TRaw rawValue)
            {
                return reader(rawValue);
            }
            
            return defaultValue;
        }
    }
}