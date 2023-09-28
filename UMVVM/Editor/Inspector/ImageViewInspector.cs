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

[CustomEditor(typeof(ImageView))]
public class ImageViewInspector : Editor {
    public override VisualElement CreateInspectorGUI() {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.startergames.mvvm/Editor/Inspector/ImageViewInspector.uxml");

        var root = visualTree.CloneTree();

        var imageView          = target as ImageView;

        return root;
    }
}