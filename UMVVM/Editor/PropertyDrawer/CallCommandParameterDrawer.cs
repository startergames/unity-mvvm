using System;
using System.Globalization;
using Command;
using Starter.ViewModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Starter {
    [CustomPropertyDrawer(typeof(CallCommand.Parameter))]
    public class CallCommandParameterDrawer : UnityEditor.PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { }

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var root = new VisualElement();

            var label = new Label(property.displayName) {
                style = {
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            root.Add(label);

            var viewModelProperty = property.FindPropertyRelative(nameof(CallCommand.Parameter.viewmodel));
            var nameProperty      = property.FindPropertyRelative("name");
            var pathProperty      = property.FindPropertyRelative("path");
            var valueProperty     = property.FindPropertyRelative("value");
            var typeProperty      = property.FindPropertyRelative("type");

            var nameField = new Label(nameProperty.stringValue);
            nameField.BindProperty(nameProperty);

            var viewModelField = new PropertyField();
            var pathField      = new PropertyField();
            
            viewModelField.BindProperty(viewModelProperty);
            viewModelField.RegisterValueChangeCallback(evt => {
                var viewModel = viewModelProperty.objectReferenceValue as ViewModel.ViewModel;
                pathField.SetEnabled(viewModel != null);
            });

            var viewModel = viewModelProperty.objectReferenceValue as ViewModel.ViewModel;
            pathField.BindProperty(pathProperty);
            pathField.SetEnabled(viewModel != null);

            var _valueContainer = new VisualElement();
            var type            = Type.GetType(typeProperty.stringValue);

            ResetValueContainer(type, valueProperty, _valueContainer);

            if(!string.IsNullOrWhiteSpace(nameField.text))
                root.Add(nameField);
            root.Add(viewModelField);
            root.Add(pathField);
            root.Add(_valueContainer);

            return root;
        }

        private static void ResetValueContainer(Type type, SerializedProperty valueProperty, VisualElement _valueContainer) { // Check if obj is a nullable type and get its underlying type
            if (type == null)
                return;
            
            _valueContainer.Clear();
            
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null) {
                type = underlyingType; // Update obj to its underlying type for further checks
            }

            var typecode = Type.GetTypeCode(type);

            if (type == typeof(bool)) {
                var toggleField = new Toggle("value");
                toggleField.RegisterValueChangedCallback(evt => {
                    valueProperty.stringValue = evt.newValue.ToString();
                    valueProperty.serializedObject.ApplyModifiedProperties();
                });

                toggleField.value = bool.TryParse(valueProperty.stringValue, out var boolValue) && boolValue;
                _valueContainer.Add(toggleField);
            }
            else if (type == typeof(string)) {
                var textField = new TextField("value");
                textField.RegisterValueChangedCallback(evt => {
                    valueProperty.stringValue = evt.newValue;
                    valueProperty.serializedObject.ApplyModifiedProperties();
                });
                textField.value = valueProperty.stringValue;
                _valueContainer.Add(textField);
            }
            else if (type.IsEnum) {
                var enumField = new EnumField("value");
                enumField.RegisterValueChangedCallback(evt => {
                    valueProperty.stringValue = evt.newValue.ToString();
                    valueProperty.serializedObject.ApplyModifiedProperties();
                });

                enumField.Init(
                    (Enum)(Enum.TryParse(type, valueProperty.stringValue, true, out var enumValue)
                               ? enumValue
                               : Enum.GetValues(type).GetValue(0)));
                _valueContainer.Add(enumField);
            }
            else if (typecode
                     is TypeCode.Byte
                     or TypeCode.SByte
                     or TypeCode.UInt16
                     or TypeCode.UInt32
                     or TypeCode.UInt64
                     or TypeCode.Int16
                     or TypeCode.Int32
                     or TypeCode.Int64
                     or TypeCode.Decimal
                     or TypeCode.Double
                     or TypeCode.Single) {
                var numericField = new FloatField("value");
                numericField.RegisterValueChangedCallback(evt => {
                    valueProperty.stringValue = evt.newValue.ToString(CultureInfo.InvariantCulture);
                    valueProperty.serializedObject.ApplyModifiedProperties();
                });
                numericField.value = float.TryParse(valueProperty.stringValue, out var floatValue) ? floatValue : 0f;
                _valueContainer.Add(numericField);
            }
            else if (type.IsClass || underlyingType != null) {
                var propertyField = new PropertyField();
                propertyField.BindProperty(valueProperty);
                _valueContainer.Add(propertyField);
            }
            else {
                var propertyField = new PropertyField();
                propertyField.BindProperty(valueProperty);
                _valueContainer.Add(propertyField);
            }
        }
    }
}