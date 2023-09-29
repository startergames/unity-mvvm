using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Starter.ViewModel;
using UnityEngine;

namespace Starter.View {
    public abstract class View : MonoBehaviour {
        [SerializeField]
        private ViewModel.ViewModel viewmodel;

        private static readonly Regex containerRegex = new Regex(@"(^[a-zA-Z_]\w*)\[([^\]])\]$", RegexOptions.Compiled);

        public ViewModel.ViewModel ViewModel  => GetFullPath(viewmodel, "").obj;
        public Type                 ViewModelType => viewmodel is ViewModelRelay relay ? relay.ViewModelType : viewmodel?.GetType();
        public string              PrefixPath => GetFullPath(viewmodel, "").path;

        protected Task WaitViewModelInitialized() {
            return viewmodel.InitializeAwaiter();
        }

        public object GetPropertyValue(string path) {
            return GetPropertyValue(viewmodel, path);
        }

        public Type GetPropertyType(string path) {
            if (string.IsNullOrWhiteSpace(path)) return null;
            var (_, fullpath) = GetFullPath(viewmodel, path);
            var type = ViewModelType;
            if (type == null) return null;
            
            var memberParts = fullpath.Split('.');
            foreach (var part in memberParts) {
                var match = containerRegex.Match(part);

                if (match.Success) {
                    var memberName = match.Groups[1].Value;
                    var key        = match.Groups[2].Value;
                    var index      = int.Parse(key);

                    var member = type.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase).FirstOrDefault();
                    if (member == null) return null;
                    
                    var memberType = member switch {
                        PropertyInfo property => property.PropertyType,
                        FieldInfo field => field.FieldType,
                        _ => null
                    };
                    
                    if (memberType == null) return null;
                    
                    if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>))
                        type = memberType.GetGenericArguments()[0];
                    else if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        type = memberType.GetGenericArguments()[1];
                    else
                        type = memberType;
                }
                else {
                    var members = type.GetMember(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (!members.Any()) return null;
                    var member = members.First();
                    type = member switch {
                        PropertyInfo property => property.PropertyType,
                        FieldInfo field => field.FieldType,
                        _ => null
                    };
                }
            }

            return type;
        }

        public T GetPropertyValue<T>(string path) where T : class {
            return GetPropertyValue(viewmodel, path) as T;
        }

        private static object GetPropertyValue(ViewModel.ViewModel viewModel, string path) {
            (viewModel, path) = GetFullPath(viewModel, path);
            object currentObject = viewModel;
            if (currentObject == null || string.IsNullOrEmpty(path)) return null;

            var memberParts = path.Split('.');

            foreach (var part in memberParts) {
                if (currentObject == null) return null;

                var memberType = currentObject.GetType();
                var match      = containerRegex.Match(part);

                if (match.Success) {
                    var memberName = match.Groups[1].Value;
                    var key        = match.Groups[2].Value;
                    var index      = int.Parse(key);

                    var member = memberType.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase)[0];
                    currentObject = GetMemberValue(member, currentObject);

                    if (currentObject is IList list)
                        currentObject = list.Count <= index ? null : list[index];
                    else if (currentObject is IDictionary dictionary) {
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

        private static (ViewModel.ViewModel obj, string path) GetFullPath(ViewModel.ViewModel currentObject, string path) {
            if (currentObject is ViewModelRelay relay)
                return (relay.ViewModel, string.IsNullOrWhiteSpace(path) ? relay.PrefixPath : string.Join('.', relay.PrefixPath, path));

            return (currentObject, path);
        }

        private static object GetMemberValue(MemberInfo member, object src) {
            return member switch {
                PropertyInfo property => property.GetValue(src, null),
                FieldInfo field => field.GetValue(src),
                _ => null
            };
        }
    }
}