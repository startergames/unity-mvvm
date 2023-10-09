using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Attributes;
using Starter.View;
using Starter.ViewModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Starter {
    [CustomPropertyDrawer(typeof(ViewModelPathAttribute))]
    public class ViewModelPathDrawer : UnityEditor.PropertyDrawer {
        private                 TextField _textField;
        private static readonly Regex     ContainerRegex = new Regex(@"(?<var>\w+)\[(?<number>[0-9]+)\]|\[""?(?<key>\w+)""?\]");
        private static readonly Regex     TokenizeRegex  = new Regex(@"(?<dot>\.)?(?<var>\w+)|(?<dot>\.)?(?<index>\[[0-9]+\])|(?<dot>\.)?(?<key>\[""?\w+""?\])|(?<dot>\.)", RegexOptions.Compiled);

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            // Ensure the property is a string
            if (property.propertyType != SerializedPropertyType.String)
                return new Label("ViewModelPath can only be used with strings.");

            var view = property.serializedObject.targetObject as View.View;

            // Create a VisualElement that will contain our controls
            var container = new VisualElement();
            container.TrackSerializedObjectValue(property.serializedObject, o => { Reset(property); });

            // Create a TextField for string input
            _textField = new TextField("ViewModel Path") {
                value = property.stringValue
            };
            _textField.BindProperty(property);

            // Create a ListView to show available ViewModel members
            var listView = new ListView {
                style = {
                    display = DisplayStyle.None
                },
                fixedItemHeight = 20,
            };
            listView.makeItem = () => new Label();
            listView.bindItem = (element, index) => {
                var label      = element as Label;
                var memberInfo = (MemberInfo)listView.itemsSource[index];
                var type = memberInfo switch {
                    System.Reflection.PropertyInfo propertyInfo => propertyInfo.PropertyType,
                    System.Reflection.FieldInfo fieldInfo => fieldInfo.FieldType,
                    _ => null
                };
                label.text = $"{memberInfo.Name} ({type.Name})";
            };

            // Update the ListView items when the TextField receives focus
            _textField.RegisterCallback<FocusInEvent>(evt => {
                listView.itemsSource = GetMatchingMembers(property, _textField.value);
                listView.RefreshItems();
                listView.style.display = DisplayStyle.Flex;
            });

            // Hide the ListView when the TextField loses focus
            _textField.RegisterCallback<FocusOutEvent>(evt => { listView.style.display = DisplayStyle.None; });

            // Update the property value when the TextField value changes
            _textField.RegisterValueChangedCallback(evt => {
                //property.stringValue  = evt.newValue;
                listView.itemsSource = GetMatchingMembers(property, _textField.value);
                listView.RefreshItems();
            });


            container.Add(_textField);
            container.Add(listView);

            return container;
        }

        private IList GetMatchingMembers(SerializedProperty property, string path) {
            Type type;
            var  viewModelPathAttribute = (ViewModelPathAttribute)attribute;
            if (property.serializedObject.targetObject is View.View view) {
                if (view.ViewModelType is null) return null;

                type = view.ViewModelType;
                var prefixPath = view.PrefixPath;
                if (!string.IsNullOrWhiteSpace(prefixPath)) {
                    path = string.Join('.', prefixPath, path);
                }
            }
            else if (property.serializedObject.targetObject is ViewModelRelay relay) {
                type = GetViewModelTypeAndPath(relay, ref path);
                if (type == null)
                    return null;
            }
            else if (!string.IsNullOrEmpty(viewModelPathAttribute.member)) {
                var siblingPropertyPath = property.propertyPath.Substring(0, property.propertyPath.LastIndexOf('.')) + "." + viewModelPathAttribute.member;
                var siblingProperty     = property.serializedObject.FindProperty(siblingPropertyPath);
                if (siblingProperty == null)
                    return null;

                var siblingValue = siblingProperty.objectReferenceValue;
                if (siblingValue is not ViewModel.ViewModel viewModel)
                    return null;
                
                if(viewModel is ViewModelRelay viewModelRelay) {
                    type = GetViewModelTypeAndPath(viewModelRelay, ref path, false);
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
                        type = GetMemberType(memberInfo);
                    }
                    else {
                        return type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                                   .Where(member => member.DeclaringType.Assembly != typeof(MonoBehaviour).Assembly)
                                   .Where(member => member.MemberType is MemberTypes.Property or MemberTypes.Field)
                                   .Where(member => member.Name.Contains(memberName, StringComparison.OrdinalIgnoreCase))
                                   .OrderBy(member => member.Name)
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
                       .Where(member => member.DeclaringType.Assembly != typeof(MonoBehaviour).Assembly)
                       .Where(member => member.MemberType is MemberTypes.Property or MemberTypes.Field)
                       .OrderBy(member => member.Name)
                       .ToList();
        }

        private static Type GetViewModelTypeAndPath(ViewModelRelay relay, ref string path, bool exceptLast = true) {
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

        private Type GetMemberType(MemberInfo memberInfo) {
            return memberInfo switch {
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                FieldInfo fieldInfo => fieldInfo.FieldType,
                _ => null
            };
        }

        private void Reset(SerializedProperty property) { }
    }
}