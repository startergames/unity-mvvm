using Starter.View;
using Starter.ViewModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace PropertyDrawer {
    [CustomPropertyDrawer(typeof(IfView.Condition))]
    public class IfViewConditionDrawer : UnityEditor.PropertyDrawer {
        private PropertyField      _logicalTypeField;
        private PropertyField      _pathField;
        private PropertyField      _typeField;
        private PropertyField      _valueField;
        private SerializedProperty _logicalTypeProperty;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var container           = new VisualElement();
            container.TrackSerializedObjectValue(property.serializedObject, o => {
                Reset(property, container);
            });

            container.Clear();

            _logicalTypeProperty = property.FindPropertyRelative(nameof(IfView.Condition.logicalType));
            var pathProperty        = property.FindPropertyRelative(nameof(IfView.Condition.path));
            var typeProperty        = property.FindPropertyRelative(nameof(IfView.Condition.type));
            var valueProperty       = property.FindPropertyRelative(nameof(IfView.Condition.value));

            _logicalTypeField = new PropertyField(_logicalTypeProperty);
            _pathField        = new PropertyField(pathProperty);
            _typeField        = new PropertyField(typeProperty);
            _valueField       = new PropertyField(valueProperty);
            container.Add(_logicalTypeField);
            container.Add(_pathField);
            container.Add(_typeField);
            container.Add(_valueField);
            
            Reset(property, container);
            return container;
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