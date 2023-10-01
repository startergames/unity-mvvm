using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TypeSearchPopup : PopupWindowContent {
    private          List<Type>     types;
    private readonly Action<string> _onSelected;

    [CanBeNull]
    private readonly Type _baseType;

    private ListView _listView;

    public TypeSearchPopup(System.Action<string> onSelected, [CanBeNull] Type baseType = null) {
        _onSelected = onSelected;
        _baseType   = baseType;
        FilterTypes("");
    }

    public override void OnGUI(Rect rect) { }

    public override void OnOpen() {
        var root = editorWindow.rootVisualElement;

        var searchField = new TextField();
        searchField.RegisterValueChangedCallback(evt => FilterTypes(evt.newValue));
        searchField.Focus();

        _listView = new ListView() {
            itemsSource     = types,
            fixedItemHeight = 20,
            makeItem        = () => new Label(),
            bindItem = (element, index) => {
                var label = element as Label;
                label.text = types[index].Name;
            }
        };
        _listView.onItemsChosen += objects => {
            _onSelected.Invoke(((Type)objects.First()).AssemblyQualifiedName);
            editorWindow.Close();
        };

        root.Add(searchField);
        root.Add(_listView);
    }

    private void FilterTypes(string typeName) {
        types ??= new();
        types.Clear();

        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
            foreach (var type in assembly.GetTypes()) {
                if (_baseType != null && !type.IsSubclassOf(_baseType)) {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(typeName) || type.AssemblyQualifiedName.Contains(typeName, StringComparison.OrdinalIgnoreCase)) {
                    types.Add(type);
                }
            }
        }

        if (_listView != null) {
            _listView.itemsSource = types;
            _listView.RefreshItems();
        }
    }
}