using System.Reflection;
using ExtensionMethod;
using Popup;
using Starter.Util;
using Starter.View;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Starter {
    using UnityEditor;

    [CustomPropertyDrawer(typeof(DataSetter))]
    public class DataSetterDrawer : PropertyDrawer {
        private System.Type _targetType;
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var root = new VisualElement();
            
            

            var targetProperty     = property.FindPropertyRelative(nameof(DataSetter.target));
            var memberName         = property.FindPropertyRelative("memberName");

            var targetField     = new PropertyField();
            var memberSelectBtn = new Button();

            targetField.RegisterValueChangeCallback(evt => {
                if (targetProperty.objectReferenceValue is null) {
                    memberSelectBtn.style.display = DisplayStyle.None;
                }
                else {
                    memberSelectBtn.style.display = DisplayStyle.Flex;
                }
            });
            targetField.BindProperty(targetProperty);
            var memberInfo = MemberInfoSerializer.Deserialize(memberName.stringValue);
            memberSelectBtn.text = memberInfo?.Name ?? "Select member";

            memberSelectBtn.clicked += () => {
                if (targetProperty.objectReferenceValue == null) return;

                var type = targetProperty.objectReferenceValue.GetType();
                PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero),
                    new DataSettableMemberSelectorPopup(
                        type,
                        _targetType,
                        memberInfo => {
                            var serializedMemberInfo = MemberInfoSerializer.Serialize(memberInfo);
                            memberName.stringValue = serializedMemberInfo;
                            memberSelectBtn.text    = memberInfo.Name;
                            memberName.serializedObject.ApplyModifiedProperties();
                        }
                    )
                );
            };

            root.TrackSerializedObjectValue(property.serializedObject, so => { TargetChanged(); });
            TargetChanged();
            
            root.Add(targetField);
            root.Add(memberSelectBtn);

            return root;

            void TargetChanged() {
                if (property.serializedObject.targetObject is not DataView view) return;
                _targetType = view.ViewModelSelf.GetPropertyType(view.path);

                if (_targetType == null) {
                    memberSelectBtn.style.display = DisplayStyle.None;
                }
                else {
                    memberSelectBtn.style.display = DisplayStyle.Flex;
                }
            }
        }
    }
}