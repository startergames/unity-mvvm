using UnityEditor;
using UnityEngine.UIElements;

namespace PropertyDrawer {
    [CustomPropertyDrawer(typeof(IfView.Condition))]
    public class IfViewConditionDrawer : UnityEditor.PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var container = new VisualElement();
            
            return container;
        }
    }
}