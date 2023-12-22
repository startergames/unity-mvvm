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

    private ListView  _listView;
    private Toggle    _showSubclassOnlyToggle;
    private TextField _searchField;

    public TypeSearchPopup(System.Action<string> onSelected, [CanBeNull] Type baseType = null) {
        _onSelected = onSelected;
        _baseType   = baseType;

        _searchField = new TextField();
        _searchField.RegisterValueChangedCallback(evt => FilterTypes(evt.newValue));
        _searchField.Focus();
        _showSubclassOnlyToggle = new Toggle("Show Subclass Only") {
            value = false
        };
        _showSubclassOnlyToggle.RegisterValueChangedCallback(evt => FilterTypes(_searchField.value));
        
        FilterTypes("");
    }

    public override void OnGUI(Rect rect) { }

    public override void OnOpen() {
        var root = editorWindow.rootVisualElement;

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
        _listView.RegisterCallback<MouseDownEvent>(e => {
            if (e.clickCount == 2) {
                _onSelected.Invoke(((Type)_listView.itemsSource[_listView.selectedIndex]).AssemblyQualifiedName);
                editorWindow.Close();
            }
        });

        if (_baseType?.IsInterface ?? false) {
            root.Add(_showSubclassOnlyToggle);
        }
        root.Add(_searchField);
        root.Add(_listView);
    }

    private void FilterTypes(string typeName) {
        types ??= new();
        types.Clear();

        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
            foreach (var type in assembly.GetTypes()) {
                if (_baseType != null && ((_baseType.IsInterface && _showSubclassOnlyToggle.value) || !_baseType.IsAssignableFrom(type))) {
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