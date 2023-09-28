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
        var prefixProperty       = root.Q<TextField>("path");
        var findInParentProperty = root.Q<Toggle>("findInParent");
        var typeNameProperty     = root.Q<TextField>("type");
        var findTypeButton       = root.Q<Button>("findType");

        relayProperty.RegisterValueChangedCallback(evt => {
            if (evt.newValue != null && evt.newValue is not ViewModel) {
                errorLabel.text          = "ViewModel을 할당해야 합니다.";
                errorLabel.style.display = DisplayStyle.Flex;
            }
            else {
                errorLabel.style.display = DisplayStyle.None;
            }
        });

        findInParentProperty.RegisterValueChangedCallback(evt => {
            typeNameProperty.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            findTypeButton.style.display   = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
        });

        findTypeButton.clicked += () => {
            var popup = new TypeSearchPopup(typeName => {
                typeNameProperty.value = typeName;
            }, typeof(ViewModel));
            UnityEditor.PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero), popup);
        };

        return root;
    }
}