using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Popup {
    public class MethodSelectorPopup : PopupWindowContent {
        private readonly Action<MemberInfo> _onSelected;
        private readonly Type               _type;
        private          List<MemberInfo>   _members;
        private          List<MemberInfo>   _filteredMembers;

        public MethodSelectorPopup(Type type, Action<MemberInfo> onSelected) {
            _type            = type;
            _onSelected      = onSelected;
            _members = _type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                            .Where(member => member is MethodInfo or PropertyInfo)
                            .Where(member => member switch {
                                FieldInfo fieldInfo => fieldInfo.DeclaringType.Assembly == _type.Assembly,
                                PropertyInfo propertyInfo => propertyInfo.DeclaringType.Assembly == _type.Assembly,
                                MethodInfo methodInfo => methodInfo.DeclaringType.Assembly == _type.Assembly,
                                _ => false
                            })
                            .ToList();
            _filteredMembers = new List<MemberInfo>(_members);
        }
        
        public override void OnGUI(Rect rect) { }

        public override void OnOpen() {
            var root = editorWindow.rootVisualElement;

            var searchField = new TextField();

            var _listView = new ListView {
                itemsSource     = _filteredMembers,
                fixedItemHeight = 20,
                makeItem        = () => new Label(),
                bindItem = (element, index) => {
                    var label      = element as Label;
                    var memberInfo = _filteredMembers[index];
                    label.text = memberInfo switch {
                        PropertyInfo propertyInfo => $"{propertyInfo.PropertyType.Name} {propertyInfo.Name}",
                        MethodInfo methodInfo => $"{methodInfo.ReturnType.Name} {methodInfo.Name}( {string.Join(", ", methodInfo.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))} )",
                        _ => $"unknown - {memberInfo.Name}"
                    };
                },
            };

            searchField.RegisterValueChangedCallback(evt => FilterMembers(_listView, evt.newValue));
            searchField.Focus();
            
            _listView.onItemsChosen += objects => {
                _onSelected.Invoke((MemberInfo)objects.First());
                editorWindow.Close();
            };

            root.Add(searchField);
            root.Add(_listView);
        }

        private void FilterMembers(ListView listView, string evtNewValue) {
            _filteredMembers = _members
                               .Where(member => member.Name.IndexOf(evtNewValue, StringComparison.OrdinalIgnoreCase) >= 0)
                               .ToList();
            
            listView.itemsSource = _filteredMembers;
            listView.RefreshItems();
        }
    }
}