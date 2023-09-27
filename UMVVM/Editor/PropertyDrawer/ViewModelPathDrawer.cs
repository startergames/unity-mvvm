using Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PropertyDrawer {
    [CustomPropertyDrawer(typeof(ViewModelPathAttribute))]
    public class ViewModelPathDrawer : UnityEditor.PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            // Ensure the property is a string
            if (property.propertyType != SerializedPropertyType.String)
                return new Label("ViewModelPath can only be used with strings.");

            // Create a VisualElement that will contain our controls
            var container = new VisualElement();

            // Create a TextField for string input
            var textField = new TextField("ViewModel Path") {
                value = property.stringValue
            };
            container.Add(textField);

            // Create a ListView to show available ViewModel members
            var listView = new ListView {
                style = {
                    height  = 100,
                    display = DisplayStyle.None
                }
            };
            container.Add(listView);

            // Update the ListView items when the TextField receives focus
            textField.RegisterCallback<FocusInEvent>(evt => {
                //listView.itemsSource   = GetMatchingMembers(property, textField.value);
                listView.style.display = DisplayStyle.Flex;
            });

            // Hide the ListView when the TextField loses focus
            textField.RegisterCallback<FocusOutEvent>(evt => listView.style.display = DisplayStyle.None);

            // Update the property value when the TextField value changes
            textField.RegisterValueChangedCallback(evt => property.stringValue = evt.newValue);

            return container;
        }
    }
}