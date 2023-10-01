using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Popup {
    public class DataSettableMemberSelectorPopup : PopupWindowContent {
        private readonly Action<MemberInfo> _onSelected;
        private readonly Type               _type;
        private readonly Type               _targetType;
        private          List<MemberInfo>   _members;
        private          List<MemberInfo>   _filteredMembers;
        private          ListView           _listView;

        public DataSettableMemberSelectorPopup(Type type, Type targetType, Action<MemberInfo> onSelected) {
            _type            = type;
            _targetType      = targetType;
            _onSelected      = onSelected;
            _members         = GetSettableMembers(_type, _targetType);
            _filteredMembers = new List<MemberInfo>(_members);
        }

        public override void OnGUI(Rect rect) { }

        public override void OnOpen() {
            var root = editorWindow.rootVisualElement;

            var searchField = new TextField();
            searchField.RegisterValueChangedCallback(evt => FilterMembers(evt.newValue));
            searchField.Focus();

            _listView = new ListView {
                itemsSource     = _filteredMembers,
                fixedItemHeight = 20,
                makeItem        = () => new Label(),
                bindItem = (element, index) => {
                    var label          = element as Label;
                    var memberInfo = _filteredMembers[index];
                    label.text = memberInfo switch {
                        FieldInfo fieldInfo => $"{fieldInfo.FieldType.Name} {fieldInfo.Name}",
                        PropertyInfo propertyInfo => $"{propertyInfo.PropertyType.Name} {propertyInfo.Name}",
                        MethodInfo methodInfo => $"{methodInfo.ReturnType.Name} {methodInfo.Name}( {string.Join(", ", methodInfo.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))} )",
                        _ => $"unknown - {memberInfo.Name}"
                    };
                }
            };
            _listView.onItemsChosen += objects => {
                _onSelected.Invoke((MemberInfo)objects.First());
                editorWindow.Close();
            };

            root.Add(searchField);
            root.Add(_listView);
        }

        private void FilterMembers(string searchText) {
            _filteredMembers = string.IsNullOrEmpty(searchText)
                                   ? new List<MemberInfo>(_members)
                                   : _members.Where(member => member.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            _listView.itemsSource = _filteredMembers;
        }

        private static List<MemberInfo> GetSettableMembers(Type type, Type targetType) {
            var members = new List<MemberInfo>();
            members.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                 .Where(f => targetType.IsAssignableFrom(f.FieldType)));
            members.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                 .Where(prop => prop.CanWrite && targetType.IsAssignableFrom(prop.PropertyType)));
            members.AddRange(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                 .Where(m => {
                                     var p = m.GetParameters();
                                     var optionalCount = p.Count(r => r.IsOptional);
                                     if (p.Length - optionalCount > 1) return false;
                                     var paramInfo = p.FirstOrDefault(r => targetType.IsAssignableFrom(r.ParameterType));
                                     if (paramInfo == null) return false;
                                     if (paramInfo.IsOptional && p.Length != optionalCount) return false;
                                     return true;
                                 }));
            return members;
        }
    }
}