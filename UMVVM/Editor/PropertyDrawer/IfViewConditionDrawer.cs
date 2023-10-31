using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ExtensionMethod;
using Starter.View;
using Starter.ViewModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Starter {
    [CustomPropertyDrawer(typeof(IfView.Condition))]
    public class IfViewConditionDrawer : UnityEditor.PropertyDrawer {
        private PropertyField                    _logicalTypeField;
        private PropertyField                    _pathField;
        private PopupField<IfView.ConditionType> _typeField;

        private VisualElement _valueContainer;

        private SerializedProperty _logicalTypeProperty;

        public override bool CanCacheInspectorGUI(SerializedProperty property) {
            return false;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var container = new VisualElement();
            container.TrackSerializedObjectValue(property.serializedObject, o => { Reset(property, container); });
            container.Clear();

            _logicalTypeProperty = property.FindPropertyRelative(nameof(IfView.Condition.logicalType));
            var pathProperty  = property.FindPropertyRelative(nameof(IfView.Condition.path));
            var typeProperty  = property.FindPropertyRelative(nameof(IfView.Condition.type));
            var valueProperty = property.FindPropertyRelative(nameof(IfView.Condition.value));

            _logicalTypeField = new PropertyField(_logicalTypeProperty);
            _pathField        = new PropertyField();
            //_typeField        = new PropertyField(typeProperty);

            _valueContainer = new VisualElement();
            var valueField = new TextField("value");

            _typeField = new PopupField<IfView.ConditionType>("Type");
            _typeField.RegisterValueChangedCallback(evt => {
                var e = evt.newValue;
                typeProperty.enumValueIndex = (int)e;
                typeProperty.serializedObject.ApplyModifiedProperties();
                RefreshValueField();
            });
            _typeField.value = (IfView.ConditionType)typeProperty.enumValueIndex;

            _pathField.RegisterValueChangeCallback(_ => { RefreshValueField(); });
            _pathField.BindProperty(pathProperty);
            RefreshValueField();

            container.Add(_logicalTypeField);
            container.Add(_pathField);
            //container.Add(_typeField);
            container.Add(_typeField);
            container.Add(_valueContainer);

            Reset(property, container);
            return container;

            void RefreshValueField() {
                var view = property.serializedObject.targetObject as View.View;

                var pathProperty = property.FindPropertyRelative(nameof(IfView.Condition.path));
                var obj          = view.ViewModelSelf.GetPropertyType(pathProperty.stringValue);
                if (obj is null) {
                    _typeField.SetEnabled(false);
                    _valueContainer.SetEnabled(false);
                }
                else {
                    var types = Enum.GetValues(typeof(IfView.ConditionType))
                                    .Cast<IfView.ConditionType>()
                                    // Not attributed enums
                                    .Where(x => !typeof(IfView.ConditionType).GetField(x.ToString()).GetCustomAttributes().Any())
                                    .ToList();

                    _valueContainer.Clear();
                    if (obj == typeof(string)) {
                        types.AddRange(GetAttributedEnums<ViewConditionTypeForStringAttribute>());
                    }

                    // Check if obj is a nullable type and get its underlying type
                    var underlyingType = Nullable.GetUnderlyingType(obj);
                    if (underlyingType != null) {
                        obj = underlyingType; // Update obj to its underlying type for further checks
                    }

                    if (obj.IsClass || underlyingType != null) {
                        types.AddRange(GetAttributedEnums<ViewConditionTypeForObject>());
                    }

                    // If obj convert to numeric type
                    var typecode = Type.GetTypeCode(obj);
                    if (typecode
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
                        types.AddRange(GetAttributedEnums<ViewConditionTypeForNumericAttribute>());
                    }

                    if (obj == typeof(bool)) {
                        var toggleField = new Toggle("value");
                        toggleField.RegisterValueChangedCallback(evt => {
                            valueProperty.stringValue = evt.newValue.ToString();
                            valueProperty.serializedObject.ApplyModifiedProperties();
                        });

                        toggleField.value = bool.TryParse(valueProperty.stringValue, out var boolValue) && boolValue;
                        _valueContainer.Add(toggleField);
                    }
                    else if (obj == typeof(string)) {
                        var textField = new TextField("value");
                        textField.RegisterValueChangedCallback(evt => {
                            valueProperty.stringValue = evt.newValue;
                            valueProperty.serializedObject.ApplyModifiedProperties();
                        });
                        textField.value = valueProperty.stringValue;
                        _valueContainer.Add(textField);
                    }
                    else if (obj.IsEnum) {
                        var enumField = new EnumField("value");
                        enumField.RegisterValueChangedCallback(evt => {
                            valueProperty.stringValue = evt.newValue.ToString();
                            valueProperty.serializedObject.ApplyModifiedProperties();
                        });

                        enumField.Init(
                            (Enum)(Enum.TryParse(obj, valueProperty.stringValue, true, out var enumValue)
                                       ? enumValue
                                       : Enum.GetValues(obj).GetValue(0)));
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
                    else if (obj.IsClass || obj.IsInterface || underlyingType != null) {
                        if (_typeField.value == IfView.ConditionType.Is) {
                            var typeField   = new TextField("Type") {
                                value = valueProperty.stringValue
                            };
                            var typeFindBtn = new Button {
                                text = "Find"
                            };
                            typeFindBtn.clicked += () => {
                                // var type = Type.GetType(valueProperty.stringValue);
                                var popup = new TypeSearchPopup(typeName => {
                                    typeField.value = typeName;
                                    valueProperty.stringValue = typeName;
                                    valueProperty.serializedObject.ApplyModifiedProperties();
                                }, typeof(object));
                                UnityEditor.PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero), popup);
                            };
                            _valueContainer.Add(typeField);
                            _valueContainer.Add(typeFindBtn);
                        }
                        else if (_typeField.value != IfView.ConditionType.IsNull) {
                            var propertyField = new PropertyField();
                            propertyField.BindProperty(valueProperty);
                            _valueContainer.Add(propertyField);
                        }
                    }
                    else {
                        var propertyField = new PropertyField();
                        propertyField.BindProperty(valueProperty);
                        _valueContainer.Add(propertyField);
                    }

                    _typeField.choices = types;

                    _typeField.SetEnabled(true);
                    _valueContainer.SetEnabled(true);
                }
            }
        }

        private static List<IfView.ConditionType> GetAttributedEnums<T>() where T : Attribute {
            var enumType = typeof(IfView.ConditionType);
            return Enum.GetValues(enumType)
                       .Cast<IfView.ConditionType>()
                       .Where(x => enumType.GetField(x.ToString()).GetCustomAttribute<T>() != null)
                       .ToList();
        }

        private void Reset(SerializedProperty property, VisualElement container) {
            var view = property.serializedObject.targetObject as View.View;
            if (view?.ViewModelType is null) {
                _logicalTypeField.SetEnabled(false);
                _pathField.SetEnabled(false);
                _typeField.SetEnabled(false);
                _valueContainer.SetEnabled(false);
            }
            else {
                if (_logicalTypeProperty.propertyPath.EndsWith($"[0].{nameof(IfView.Condition.logicalType)}")) {
                    _logicalTypeField.style.display = DisplayStyle.None;
                    _logicalTypeField.SetEnabled(false);
                }
                else {
                    _logicalTypeField.style.display = DisplayStyle.Flex;
                    _logicalTypeField.SetEnabled(true);
                }

                _pathField.SetEnabled(true);
                _typeField.SetEnabled(true);
                _valueContainer.SetEnabled(true);
            }
        }
    }
}