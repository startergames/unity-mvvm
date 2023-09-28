using Starter.View;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Inspector {
    [CustomEditor(typeof(DataView))]
    public class DataViewInspector : Editor {
        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();

            var viewModelProperty = serializedObject.FindProperty("viewmodel");
            var pathProperty      = serializedObject.FindProperty(nameof(DataView.path));
            var setterProperty    = serializedObject.FindProperty(nameof(DataView.setter));

            var viewmodelField = new PropertyField(viewModelProperty);
            var pathField      = new PropertyField(pathProperty);
            var setterField    = new PropertyField(setterProperty);

            root.Add(viewmodelField);
            root.Add(pathField);
            root.Add(setterField);

            return root;
        }
    }
}