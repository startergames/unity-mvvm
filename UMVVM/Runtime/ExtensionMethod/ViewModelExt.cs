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
            if (!GetPropertyMemberInfo(viewModel, path, out _, out var setter))
                throw new ArgumentException("Invalid path");
            
            setter.Invoke(value);
        }

        public static object GetPropertyValue(this ViewModel viewModel, string path) {
            if (!GetPropertyMemberInfo(viewModel, path, out var getter, out var setter))
                return null;

            return getter.Invoke();
        }

        private static bool GetPropertyMemberInfo(ViewModel viewModel, string path, out Func<object> getter, out Action<object> setter) {
            object     currentObject = null;
            getter = null;
            setter = null;

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

                    var member = memberType.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase)[0];
                    currentObject = GetMemberValue(member, currentObject);

                    getter = () => {
                        if (currentObject is IList list) {
                            if (!int.TryParse(key, out var index)) {
                                throw new ArgumentException("Invalid index on path : " + key);
                            }

                            return list.Count <= index ? null : list[index];
                        }
                        if (currentObject is IDictionary dict) {
                            var typedKey = Convert.ChangeType(key, dict.GetType().GetGenericArguments()[0]);
                            return dict.Contains(typedKey) ? dict[typedKey] : null;
                        }

                        throw new ArgumentException("Invalid member type on path : " + memberName);
                    };
                    setter = value => {
                        if (currentObject is IList list) {
                            if (!int.TryParse(key, out var index)) {
                                throw new ArgumentException("Invalid index on path : " + key);
                            }

                            if (list.Count <= index) {
                                throw new ArgumentException("Invalid index on path : " + key);
                            }

                            list[index] = value;
                        }
                        else {
                            throw new ArgumentException("Invalid member type on path : " + memberName);
                        }
                    };
                    
                    if (i == memberParts.Length - 1) {
                        return true;
                    }

                    currentObject = getter.Invoke();
                }
                else if (match.Groups["key"].Success) {
                    var memberName = match.Groups["var"].Value;
                    var key        = match.Groups["key"].Value;

                    var member = memberType.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase)[0];
                    currentObject = GetMemberValue(member, currentObject);

                    getter = () => {
                        if (currentObject is IDictionary dict) {
                            var typedKey = Convert.ChangeType(key, dict.GetType().GetGenericArguments()[0]);
                            return dict.Contains(typedKey) ? dict[typedKey] : null;
                        }

                        throw new ArgumentException("Invalid member type on path : " + memberName);
                    };
                    setter = value => {
                        if (currentObject is IDictionary dict) {
                            var typedKey = Convert.ChangeType(key, dict.GetType().GetGenericArguments()[0]);
                            dict[typedKey] = value;
                        }
                        else {
                            throw new ArgumentException("Invalid member type on path : " + memberName);
                        }
                    };

                    if (i == memberParts.Length - 1) {
                        return true;
                    }

                    currentObject = getter.Invoke();
                }
                else {
                    var members = memberType.GetMember(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (!members.Any()) {
                        currentObject = null;
                        return false;
                    }

                    var memberInfo = members.First();
                    getter = () => GetMemberValue(memberInfo, currentObject);
                    setter = value => {
                        switch (memberInfo) {
                            case PropertyInfo property:
                                property.SetValue(currentObject, value);
                                break;
                            case FieldInfo field:
                                field.SetValue(currentObject, value);
                                break;
                            case MethodInfo method:
                                var parameters = method.GetParameters();
                                if (parameters.Length == 0)
                                    throw new ArgumentException("method has no parameter");

                                var args = new object[parameters.Length];
                                for (var i = 0; i < parameters.Length; i++) {
                                    if (i == 0) {
                                        // First parameter receives the provided value
                                        args[i] = value;
                                    }
                                    else {
                                        // Other parameters receive their default values if they are optional
                                        if (!parameters[i].IsOptional)
                                            throw new ArgumentException("method needs more then one parameter");

                                        args[i] = parameters[i].DefaultValue;
                                    }
                                }

                                method.Invoke(currentObject, args);
                                break;
                            default:
                                throw new ArgumentException("Invalid member type on path : " + memberInfo.Name);
                        }
                    };

                    if (i == memberParts.Length - 1) {
                        return true;
                    }

                    currentObject = getter.Invoke();
                }
            }

            return false;
        }

        public static Type GetPropertyType(this ViewModel viewmodel, string path) {
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
              , FieldInfo field       => field.GetValue(src)
              , MethodInfo method     => method.Invoke(src, null)
              , _                     => null
            };
        }
    }
}