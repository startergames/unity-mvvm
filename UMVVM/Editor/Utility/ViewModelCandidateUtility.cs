using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Starter.ViewModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Starter {
    public static class ViewModelCandidateUtility {
        private static readonly Regex TokenizeRegex =
            new(@"(?<dot>\.)?(?<var>\w+)|(?<dot>\.)?(?<index>\[[0-9]+\])|(?<dot>\.)?(?<key>\[""?\w+""?\])|(?<dot>\.)"
              , RegexOptions.Compiled
            );

        public static ListView CreateCandidateListView() {
            var listView = new ListView {
                style = {
                    display = DisplayStyle.None
                },
                fixedItemHeight = 20,
                makeItem        = () => new Label()
            };
            listView.bindItem = (element, index) => {
                var label      = element as Label;
                var memberInfo = (MemberInfo)listView.itemsSource[index];
                var type = memberInfo switch {
                    PropertyInfo propertyInfo => propertyInfo.PropertyType,
                    FieldInfo fieldInfo => fieldInfo.FieldType,
                    _ => null
                };
                label.text = $"{memberInfo.Name} ({type.Name})";
            };
            return listView;
        }

        public static IList GetMatchingMembers(this SerializedObject so, string path, string member = null) {
            Type type;
            if (so.targetObject is View.View view) {
                if (view.ViewModelType is null) return null;

                type = view.ViewModelType;
                var prefixPath = view.PrefixPath;
                if (!string.IsNullOrWhiteSpace(prefixPath)) {
                    path = string.Join('.', prefixPath, path);
                }
            }
            else if (so.targetObject is ViewModelRelay relay) {
                type = relay.GetViewModelTypeAndPath(ref path);
                if (type == null)
                    return null;
            }
            else {
                if (!string.IsNullOrEmpty(member)) {
                    var siblingPropertyPath = path[..path.LastIndexOf('.')] + "." + member;
                    var siblingProperty     = so.FindProperty(siblingPropertyPath);

                    var siblingValue = siblingProperty?.objectReferenceValue;
                    if (siblingValue is not ViewModel.ViewModel viewModel)
                        return null;

                    if (viewModel is ViewModelRelay viewModelRelay) {
                        type = viewModelRelay.GetViewModelTypeAndPath(ref path, false);
                        if (type == null)
                            return null;
                    }
                    else {
                        type = viewModel.GetType();
                    }
                }
                else {
                    return null;
                }
            }

            var matches = TokenizeRegex.Matches(path);
            for (int i = 0; i < matches.Count(); ++i) {
                var m      = matches[i];
                var isLast = i == matches.Count() - 1;

                if (m.Groups["var"].Success) {
                    var memberName = m.Groups["var"].Value;
                    if (!isLast) {
                        var memberInfo = type.GetMember(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase).FirstOrDefault();
                        if (memberInfo == null)
                            return null;
                        type = memberInfo.GetMemberType();
                    }
                    else {
                        return type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                                   .Where(mi => mi.DeclaringType != null && mi.DeclaringType.Assembly != typeof(MonoBehaviour).Assembly)
                                   .Where(mi => mi.MemberType is MemberTypes.Property or MemberTypes.Field)
                                   .Where(mi => mi.Name.Contains(memberName, StringComparison.OrdinalIgnoreCase))
                                   .OrderBy(mi => mi.Name)
                                   .ToList();
                    }
                }
                else if (m.Groups["index"].Success) {
                    type = type switch {
                        { IsArray: true } => type.GetElementType(),
                        { IsGenericType: true } => type.GetGenericArguments()[0],
                        _ => null
                    };
                }
                else if (m.Groups["key"].Success) {
                    type = type.IsGenericType ? type.GetGenericArguments()[1] : null;
                }
                else if (m.Groups["dot"].Success && isLast) {
                    break;
                }

                if (type == null)
                    return null;
            }

            return type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                       .Where(mi => mi.DeclaringType != null && mi.DeclaringType.Assembly != typeof(MonoBehaviour).Assembly)
                       .Where(mi => mi.MemberType is MemberTypes.Property or MemberTypes.Field)
                       .OrderBy(mi => mi.Name)
                       .ToList();
        }

        private static Type GetMemberType(this MemberInfo memberInfo) {
            return memberInfo switch {
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                FieldInfo fieldInfo => fieldInfo.FieldType,
                _ => null
            };
        }

        private static Type GetViewModelTypeAndPath(this ViewModelRelay relay, ref string path, bool exceptLast = true) {
            if (relay.ViewModelType is null)
                return null;

            var type       = relay.ViewModelType;
            var prefixPath = exceptLast ? relay.PrefixPathExceptLast : relay.PrefixPath;
            if (!string.IsNullOrWhiteSpace(prefixPath)) {
                if (path.StartsWith('[')) {
                    path = prefixPath + path;
                }
                else {
                    path = string.Join('.', prefixPath, path);
                }
            }

            return type;
        }
    }
}