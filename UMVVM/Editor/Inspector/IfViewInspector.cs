using System.Collections;
using System.Collections.Generic;
using Starter.ViewModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(IfView))]
public class IfViewInspector : Editor {
    public override VisualElement CreateInspectorGUI() {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.startergames.mvvm/Editor/Inspector/IfViewInspector.uxml");

        var root = visualTree.CloneTree();

        var errorLabel           = root.Q<Label>("error");
        var relayProperty        = root.Q<ObjectField>("viewmodel");
        return root;
    }
}