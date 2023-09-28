using System.Collections;
using System.Collections.Generic;
using Starter.ViewModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(ViewModelRelay))]
public class ViewModelRelayEditor : Editor {
    public override VisualElement CreateInspectorGUI() {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.startergames.mvvm/Editor/Inspector/ViewModelRelayInspector.uxml");

        var root = visualTree.CloneTree();

        var errorLabel           = root.Q<Label>("error");
        var relayProperty        = root.Q<ObjectField>("viewmodel");
        var findInParentProperty = root.Q<Toggle>("findInParent");
        var typeContainer = root.Q<VisualElement>("type-container");
        var typeNameProperty     = root.Q<TextField>("type");
        var relayTypeInfoButton  = root.Q<Button>("find-type");

        relayProperty.RegisterValueChangedCallback(evt => {
            var obj = evt.newValue;
            DisplayErrorMessage(obj is not ViewModel, "ViewModel을 할당해야 합니다.");
            if (obj is ViewModel) {
                typeContainer.style.display = DisplayStyle.None;
            }
            else {
                typeContainer.style.display = DisplayStyle.Flex;
            }
        });
        relayProperty.value = relayProperty.value;

        relayTypeInfoButton.clicked += () => {
            var popup = new TypeSearchPopup(typeName => { typeNameProperty.value = typeName; }, typeof(ViewModel));
            UnityEditor.PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero), popup);
        };

        return root;

        void DisplayErrorMessage(bool show, string message) {
            if (show) {
                errorLabel.text          = message;
                errorLabel.style.display = DisplayStyle.Flex;
            }
            else {
                errorLabel.style.display = DisplayStyle.None;
            }
        }
    }
}