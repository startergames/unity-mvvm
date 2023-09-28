﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Starter.View;
using Starter.ViewModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace PropertyDrawer {
    [CustomPropertyDrawer(typeof(IfView.Condition))]
    public class IfViewConditionDrawer : UnityEditor.PropertyDrawer {
        private PropertyField                    _logicalTypeField;
        private PropertyField                    _pathField;
        private PopupField<IfView.ConditionType> _typeField;
        private PropertyField                    _valueField;
        private SerializedProperty               _logicalTypeProperty;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var container = new VisualElement();
            container.TrackSerializedObjectValue(property.serializedObject, o => { Reset(property, container); });
            container.Clear();

            _logicalTypeProperty = property.FindPropertyRelative(nameof(IfView.Condition.logicalType));
            var pathProperty  = property.FindPropertyRelative(nameof(IfView.Condition.path));
            var typeProperty  = property.FindPropertyRelative(nameof(IfView.Condition.type));
            var valueProperty = property.FindPropertyRelative(nameof(IfView.Condition.value));

            _logicalTypeField = new PropertyField(_logicalTypeProperty);
            _pathField        = new PropertyField(pathProperty);
            //_typeField        = new PropertyField(typeProperty);
            _valueField       = new PropertyField(valueProperty);
            var valueField = new TextField("value");

            _typeField = new PopupField<IfView.ConditionType>("Type");
            _typeField.RegisterValueChangedCallback(evt => {
                var e = evt.newValue;
            });
            _typeField.BindProperty(typeProperty);
            
            _pathField.RegisterValueChangeCallback(evt => {
                var view         = property.serializedObject.targetObject as View;
                
                var pathProperty = property.FindPropertyRelative(nameof(IfView.Condition.path));
                var obj          = view.GetPropertyType(pathProperty.stringValue);
                if (obj is null) {
                    _typeField.SetEnabled(false);
                    _valueField.SetEnabled(false);
                }
                else {
                    var types = Enum.GetValues(typeof(IfView.ConditionType))
                                    .Cast<IfView.ConditionType>()
                                    // Not attributed enums
                                    .Where(x => !typeof(IfView.ConditionType).GetField(x.ToString()).GetCustomAttributes().Any())
                                    .ToList();

                    if (obj == typeof(string)) {
                        types.AddRange(GetAttributedEnums<ViewConditionTypeForStringAttribute>());
                    }

                    if (obj.IsClass) {
                        types.AddRange(GetAttributedEnums<ViewConditionTypeForObject>());
                    }
                    
                    // If obj convert to numeric type
                    var typecode = Type.GetTypeCode(obj);
                    if (typecode == TypeCode.Byte ||
                        typecode == TypeCode.SByte ||
                        typecode == TypeCode.UInt16 ||
                        typecode == TypeCode.UInt32 ||
                        typecode == TypeCode.UInt64 ||
                        typecode == TypeCode.Int16 ||
                        typecode == TypeCode.Int32 ||
                        typecode == TypeCode.Int64 ||
                        typecode == TypeCode.Decimal ||
                        typecode == TypeCode.Double ||
                        typecode == TypeCode.Single) {
                        types.AddRange(GetAttributedEnums<ViewConditionTypeForNumericAttribute>());
                    }
                    _typeField.choices = types;
                    
                    
                    _typeField.SetEnabled(true);
                    _valueField.SetEnabled(true);
                }
            });

            container.Add(_logicalTypeField);
            container.Add(_pathField);
            //container.Add(_typeField);
            container.Add(_typeField);
            container.Add(_valueField);

            Reset(property, container);
            return container;
        }

        private static List<IfView.ConditionType> GetAttributedEnums<T>() where T : Attribute {
            var enumType = typeof(IfView.ConditionType);
            return Enum.GetValues(enumType)
                       .Cast<IfView.ConditionType>()
                       .Where(x => enumType.GetField(x.ToString()).GetCustomAttribute<T>() != null )
                       .ToList();
        }

        private void Reset(SerializedProperty property, VisualElement container) {
            var view = property.serializedObject.targetObject as View;
            if (view?.ViewModel is null) {
                _logicalTypeField.SetEnabled(false);
                _pathField.SetEnabled(false);
                _typeField.SetEnabled(false);
                _valueField.SetEnabled(false);
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
                _valueField.SetEnabled(true);
            }
        }
    }
}