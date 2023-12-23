using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Starter.ViewModel;
using Util;

namespace ExtensionMethod {
    public static class ViewModelExt {

        public static void SetPropertyValue<T>(this ViewModel viewModel, string path, T value) {
            if (!GetPropertyMemberInfo(viewModel, path, out var currentObject, out var memberInfo))
                throw new ArgumentException("Invalid path");

            var memberType = GetMemberType(memberInfo);
            if (memberType != typeof(T))
                throw new ArgumentException("Invalid value type");

            switch (memberInfo) {
                case PropertyInfo property:
                    property.SetValue(currentObject, value);
                    break;
                case FieldInfo field:
                    field.SetValue(currentObject, value);
                    break;
                case MethodInfo method:
                    var parameters = method.GetParameters();
                    if(parameters.Length == 0)
                        throw new ArgumentException("method has no parameter");
                    
                    var args       = new object[parameters.Length];
                    for (var i = 0; i < parameters.Length; i++) {
                        if (i == 0) {
                            // First parameter receives the provided value
                            args[i] = value;
                        }
                        else {
                            // Other parameters receive their default values if they are optional
                            if(!parameters[i].IsOptional)
                                throw new ArgumentException("method needs more then one parameter");
                            
                            args[i] = parameters[i].DefaultValue;
                        }
                    }

                    method.Invoke(currentObject, args);
                    break;
                default:
                    throw new ArgumentException("Invalid member type on path : " + memberInfo.Name);
            }
        }
        public static object GetPropertyValue(this ViewModel viewModel, string path) {
            if (!GetPropertyMemberInfo(viewModel, path, out var currentObject, out var memberInfo))
                return null;

            return GetMemberValue(memberInfo, currentObject);
        }

        private static bool GetPropertyMemberInfo(ViewModel viewModel, string path, out object currentObject, out MemberInfo memberInfo) {
            memberInfo    = null;
            currentObject = null;
            
            path          = viewModel.GetFullPath(path);
            currentObject = viewModel is ViewModelRelay relay ? relay.ViewModel : viewModel;
            if (currentObject == null || string.IsNullOrEmpty(path))
                return false;

            var memberParts = path.Split('.');

            for (var i = 0; i < memberParts.Length; i++) {
                var part = memberParts[i];
                if (currentObject == null) return false;

                var memberType = currentObject.GetType();
                var match      = ConstInfo.ContainerRegex.Match(part);

                if (match.Groups["number"].Success) {
                    var memberName = match.Groups["var"].Value;
                    var key        = match.Groups["number"].Value;
                    var index      = int.Parse(key);

                    var member = memberType.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase)[0];
                    currentObject = GetMemberValue(member, currentObject);

                    var type = currentObject.GetType();
                    if (type.IsArray) {
                        var array = (Array)currentObject;
                        currentObject = array.Length <= index ? null : array.GetValue(index);
                    }
                    else if (currentObject is IList list)
                        currentObject = list.Count <= index ? null : list[index];
                    else if (currentObject is IDictionary dictionary) {
                        var typedKey = Convert.ChangeType(key, dictionary.GetType().GetGenericArguments()[0]);
                        currentObject = dictionary.Contains(typedKey) ? dictionary[typedKey] : null;
                    }
                }
                else if (match.Groups["key"].Success) {
                    var memberName = match.Groups["var"].Value;
                    var key        = match.Groups["key"].Value;
                    var index      = int.Parse(key);

                    var member = memberType.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase)[0];
                    currentObject = GetMemberValue(member, currentObject);

                    var type = currentObject.GetType();
                    if (currentObject is IDictionary dictionary) {
                        var typedKey = Convert.ChangeType(key, dictionary.GetType().GetGenericArguments()[0]);
                        currentObject = dictionary.Contains(typedKey) ? dictionary[typedKey] : null;
                    }
                }
                else {
                    var members = memberType.GetMember(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (!members.Any()) {
                        currentObject = null;
                        memberInfo    = null;
                        return false;
                    }

                    if (i == memberParts.Length - 1) {
                        memberInfo = members.First();
                        return true;
                    }

                    currentObject = GetMemberValue(members.First(), currentObject);
                }
            }
            return false;
        }

        public static Type GetPropertyType(this ViewModel viewmodel, string path) {
            if (string.IsNullOrWhiteSpace(path)) return null;
            var fullpath = viewmodel.GetFullPath(path);
            var type     = viewmodel is ViewModelRelay relay ? relay.ViewModelType : viewmodel?.GetType();
            if (type == null) return null;

            var memberParts = fullpath.Split('.');
            foreach (var part in memberParts) {
                var match = ConstInfo.ContainerRegex.Match(part);

                if (match.Groups["number"].Success) {
                    var memberName = match.Groups["var"].Value;
                    var key        = match.Groups["number"].Value;
                    var index      = int.Parse(key);

                    var member = type.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase).FirstOrDefault();
                    if (member == null) return null;

                    var memberType = member switch {
                        PropertyInfo property => property.PropertyType, FieldInfo field => field.FieldType, _ => null
                    };

                    if (memberType == null) return null;

                    if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>))
                        type = memberType.GetGenericArguments()[0];
                    else if (memberType.IsArray)
                        type = memberType.GetElementType();
                    else
                        type = memberType;
                }
                else if (match.Groups["key"].Success) {
                    var memberName = match.Groups["var"].Value;
                    var key        = match.Groups["key"].Value;

                    var member = type.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase).FirstOrDefault();
                    if (member == null) return null;

                    var memberType = member switch {
                        PropertyInfo property => property.PropertyType, FieldInfo field => field.FieldType, _ => null
                    };

                    if (memberType == null) return null;

                    if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        type = memberType.GetGenericArguments()[1];
                    else
                        type = memberType;
                }
                else {
                    var members = type.GetMember(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (!members.Any()) return null;
                    var member = members.First();
                    type = GetMemberType(member);
                }
            }

            return type;
        }

        private static Type GetMemberType(MemberInfo member) {
            return member switch {
                PropertyInfo property => property.PropertyType
              , FieldInfo field       => field.FieldType
              , _                     => null
            };
        }

        public static string GetFullPath(this ViewModel currentObject, string path) {
            if (currentObject is ViewModelRelay relay) {
                var relayPrefixPath = relay.PrefixPath;
                return string.IsNullOrWhiteSpace(path)            ? relayPrefixPath :
                       path.StartsWith('[')                       ? relayPrefixPath + path :
                       string.IsNullOrWhiteSpace(relayPrefixPath) ? path :
                                                                    string.Join('.', relayPrefixPath, path)
                    ;
            }

            return path;
        }

        private static object GetMemberValue(MemberInfo member, object src) {
            return member switch {
                PropertyInfo property => property.GetValue(src, null)
              , FieldInfo field => field.GetValue(src)
              , _ => null
            };
        }
    }
}