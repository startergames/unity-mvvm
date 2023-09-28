﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Attributes;
using Starter.View;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PropertyDrawer {
    [CustomPropertyDrawer(typeof(ViewModelPathAttribute))]
    public class ViewModelPathDrawer : UnityEditor.PropertyDrawer {
        private TextField _textField;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            // Ensure the property is a string
            if (property.propertyType != SerializedPropertyType.String)
                return new Label("ViewModelPath can only be used with strings.");

            var view = property.serializedObject.targetObject as View;
            
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
                var label = element as Label;
                var memberInfo  = (MemberInfo)listView.itemsSource[index];
                var type = memberInfo switch {
                    System.Reflection.PropertyInfo propertyInfo => propertyInfo.PropertyType,
                    System.Reflection.FieldInfo fieldInfo => fieldInfo.FieldType,
                    _ => null
                };
                label.text = $"{memberInfo.Name} ({type.Name})";
            };
            // _listView.onItemsChosen += items => {
            //     var memberInfo = (MemberInfo)items.First();
            //     var path       = _textField.value;
            //     var lastDot = path.LastIndexOf('.');
            //     var prefixPath = lastDot == -1 ? "" : path.Substring(0, lastDot);
            //     _textField.value = prefixPath == "" ? memberInfo.Name : $"{prefixPath}.{memberInfo.Name}";
            //     _textField.Focus();
            // };

            // Update the ListView items when the TextField receives focus
            _textField.RegisterCallback<FocusInEvent>(evt => {
                listView.itemsSource   = GetMatchingMembers(property, _textField.value);
                listView.RefreshItems();
                listView.style.display = DisplayStyle.Flex;
            });

            // Hide the ListView when the TextField loses focus
            _textField.RegisterCallback<FocusOutEvent>(evt => {
                listView.style.display = DisplayStyle.None;
            });

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

        private IList GetMatchingMembers(SerializedProperty property, string textFieldValue) {
            var view = property.serializedObject.targetObject as View;
            if (view?.ViewModel is null) return null;

            var type = view.ViewModel.GetType();

            var splited = textFieldValue.Split('.');
            if (splited.Length == 0)
                return type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                           .Where(member => member.DeclaringType.Assembly != typeof(MonoBehaviour).Assembly)
                           .Where(member => member.MemberType is MemberTypes.Property or MemberTypes.Field)
                           .OrderBy(member => member.Name)
                           .ToList();


            for (int i = 0; i < splited.Length; ++i) {
                var memberName = splited[i];
                
                if (string.IsNullOrWhiteSpace(memberName)) {
                    return type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                               .Where(member => member.DeclaringType.Assembly != typeof(MonoBehaviour).Assembly)
                               .Where(member => member.MemberType is MemberTypes.Property or MemberTypes.Field)
                               .OrderBy(member => member.Name)
                               .ToList();
                }
                
                if (i == splited.Length - 1) {
                    return type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                               .Where(member => member.DeclaringType.Assembly != typeof(MonoBehaviour).Assembly)
                               .Where(member => member.MemberType is MemberTypes.Property or MemberTypes.Field)
                               .Where(member => member.Name.Contains(memberName, StringComparison.OrdinalIgnoreCase))
                               .OrderBy(member => member.Name)
                               .ToList();
                }

                var memberInfo = type.GetMember(memberName,  BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase).First();
                type = memberInfo switch {
                    System.Reflection.PropertyInfo propertyInfo => propertyInfo.PropertyType,
                    System.Reflection.FieldInfo fieldInfo => fieldInfo.FieldType,
                    _ => null
                };

                if (type == null)
                    return null;
            }

            return null;
        }

        private void Reset(SerializedProperty property) { }
    }
}