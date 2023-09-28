using System;
using System.Linq;
using System.Reflection;

namespace Starter.Util{
    public class MemberInfoSerializer {
        public static string Serialize(MemberInfo memberInfo) {
            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null) {
                var parameterTypes = methodInfo.GetParameters()
                                               .Select(p => p.ParameterType.AssemblyQualifiedName)
                                               .ToArray();
                var parameterTypesString = string.Join("#", parameterTypes);
                return $"{memberInfo.DeclaringType.AssemblyQualifiedName}|{memberInfo.Name}|{memberInfo.MemberType}|{parameterTypesString}";
            }

            return $"{memberInfo.DeclaringType.AssemblyQualifiedName}|{memberInfo.Name}|{memberInfo.MemberType}";
        }

        public static MemberInfo Deserialize(string serializedMemberInfo) {
            var parts = serializedMemberInfo.Split('|');
            if (parts.Length < 3) return null;

            var type = Type.GetType(parts[0]);
            if (type == null) throw new TypeLoadException("Type not found");

            var memberName = parts[1];
            var memberType = (MemberTypes)Enum.Parse(typeof(MemberTypes), parts[2]);

            switch (memberType) {
                case MemberTypes.Field:
                    return type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                case MemberTypes.Property:
                    return type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                case MemberTypes.Method:
                    if (parts.Length == 4) {
                        var parameterTypeNames = parts[3].Split('#');
                        var parameterTypes     = parameterTypeNames.Select(Type.GetType).ToArray();
                        return type.GetMethod(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, parameterTypes, null);
                    }
                    return type.GetMethod(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                default:
                    throw new NotSupportedException("Member type not supported");
            }
        }
    }
}