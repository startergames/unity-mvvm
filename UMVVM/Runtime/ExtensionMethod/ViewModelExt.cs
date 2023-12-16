using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Starter.ViewModel;
using Util;

namespace ExtensionMethod {
    public static class ViewModelExt {
        public static object GetPropertyValue(this ViewModel viewModel, string path) {
            path = viewModel.GetFullPath(path);
            object currentObject = viewModel is ViewModelRelay relay ? relay.ViewModel : viewModel;
            if (currentObject == null || string.IsNullOrEmpty(path)) return null;

            var memberParts = path.Split('.');

            foreach (var part in memberParts) {
                if (currentObject == null) return null;

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
                    if (!members.Any())
                        return null;

                    currentObject = GetMemberValue(members.First(), currentObject);
                }
            }

            return currentObject;
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
                    type = member switch {
                        PropertyInfo property => property.PropertyType, FieldInfo field => field.FieldType, _ => null
                    };
                }
            }

            return type;
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
                PropertyInfo property => property.GetValue(src, null), FieldInfo field => field.GetValue(src), _ => null
            };
        }
    }
}