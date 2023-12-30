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
            var listView = ViewModelCandidateUtility.CreateCandidateListView();

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
                listView.itemsSource = GetMatchingMembers(property, evt.newValue);
                listView.RefreshItems();
            });

            container.Add(_textField);
            container.Add(listView);

            return container;
        }

        private IList GetMatchingMembers(SerializedProperty property, string path) {
            var viewModelPathAttribute = (ViewModelPathAttribute)attribute;
            var member                 = viewModelPathAttribute.member;
            if (!string.IsNullOrEmpty(member)) {
                var propertyPath = property.propertyPath;
                var memberPath   = propertyPath[..propertyPath.LastIndexOf('.')] + "." + member;
                return property.serializedObject.GetMatchingMembers(path, memberPath);
            }
            
            return property.serializedObject.GetMatchingMembers(path);
        }

        private void Reset(SerializedProperty property) { }
    }
}