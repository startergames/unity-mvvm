using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Starter.View;
using Starter.ViewModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(TextView))]
public class TextViewInspector : Editor {
    public override VisualElement CreateInspectorGUI() {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.startergames.mvvm/Editor/Inspector/ViewInspector/TextViewInspector.uxml");

        var root = visualTree.CloneTree();

        var textView          = target as TextView;
        var viewModelProperty = root.Q<ObjectField>("viewmodel");
        var textProperty      = root.Q<TextField>("text");
        var targetProperty    = root.Q<ObjectField>("target");

        textProperty.RegisterValueChangedCallback(evt => { textView.TokenizeText(); });

        var vm = viewModelProperty.value as ViewModel;

        return root;
    }
}