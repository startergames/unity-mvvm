using Command;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PropertyDrawer {
    [CustomPropertyDrawer(typeof(CallCommand.Parameter))]
    public class CallCommandParameterDrawer : UnityEditor.PropertyDrawer{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { }

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var root = new VisualElement();

            var viewProperty = property.FindPropertyRelative(nameof(CallCommand.Parameter.view));
            var nameProperty = property.FindPropertyRelative("name");
            var pathProperty = property.FindPropertyRelative("path");
            var valueProperty = property.FindPropertyRelative("value");
            
            var nameField = new Label(nameProperty.stringValue);
            nameField.BindProperty(nameProperty);

            var viewField = new PropertyField();
            viewField.BindProperty(viewProperty);
            
            var pathField = new PropertyField();
            pathField.BindProperty(pathProperty);
            
            var valueField = new PropertyField();
            valueField.BindProperty(valueProperty);
            
            root.Add(nameField);
            root.Add(viewField);
            root.Add(pathField);
            root.Add(valueField);
            
            return root;
        }
    }
}