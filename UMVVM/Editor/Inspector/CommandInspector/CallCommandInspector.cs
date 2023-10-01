﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Command;
using Popup;
using Starter.Util;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace Inspector.CommandInspector {
    [CustomEditor(typeof(CallCommand))]
    public class CallCommandInspector : UnityEditor.Editor {
        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();

            var viewModelProperty  = serializedObject.FindProperty(nameof(CallCommand.viewModel));
            var methodProperty     = serializedObject.FindProperty(nameof(CallCommand.methodData));
            var parametersProperty = serializedObject.FindProperty(nameof(CallCommand.parameters));

            var viewModelField = new PropertyField();
            viewModelField.BindProperty(viewModelProperty);

            var methodBtn = new Button() {
                text = "Select method"
            };
            var parameterContainer = new VisualElement() {
                style = {
                    marginLeft = 10
                }
            };
            
            var memberInfo = MemberInfoSerializer.Deserialize(methodProperty.stringValue);
            methodBtn.text = memberInfo?.Name ?? "Select method";
            methodBtn.clicked += () => {
                if (viewModelProperty.objectReferenceValue == null) return;

                var type = viewModelProperty.objectReferenceValue.GetType();
                PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero),
                    new MethodSelectorPopup(
                        type,
                        memberInfo => {
                            var serializedMemberInfo = MemberInfoSerializer.Serialize(memberInfo);
                            methodProperty.stringValue = serializedMemberInfo;
                            methodBtn.text = memberInfo switch {
                                MethodInfo methodInfo => $"{methodInfo.ReturnType.Name} {methodInfo.Name}( {string.Join(", ", methodInfo.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))} )",
                                PropertyInfo propertyInfo => $"{propertyInfo.PropertyType.Name} {propertyInfo.Name}",
                                FieldInfo fieldInfo => $"{fieldInfo.FieldType.Name} {fieldInfo.Name}",
                                _ => methodBtn.text
                            };
                            methodProperty.serializedObject.ApplyModifiedProperties();

                            ResetParameters(memberInfo, parametersProperty, parameterContainer);
                            this.Repaint();
                        }
                    )
                );
            };

            ResetParameters(memberInfo, parametersProperty, parameterContainer);

            root.Add(viewModelField);
            root.Add(methodBtn);
            root.Add(parameterContainer);

            return root;
        }

        private static bool ResetParameters(MemberInfo memberInfo, SerializedProperty parametersProperty, VisualElement parametersContainer) {
            if (memberInfo == null)
                return true;

            parametersContainer.Clear();
            switch (memberInfo) {
                case PropertyInfo propertyInfo: {
                    parametersProperty.arraySize = 1;
                    parametersProperty.serializedObject.ApplyModifiedProperties();
                    var parameterProperty = parametersProperty.GetArrayElementAtIndex(parametersProperty.arraySize - 1);
                    parametersContainer.Add(new PropertyField(parameterProperty));
                    parameterProperty.FindPropertyRelative("name").stringValue           = $"{propertyInfo.PropertyType.Name} {propertyInfo.Name}";
                    parameterProperty.FindPropertyRelative("path").stringValue           = "";
                    break;
                }
                case FieldInfo fieldInfo: {
                    parametersProperty.arraySize = 1;
                    parametersProperty.serializedObject.ApplyModifiedProperties();
                    var parameterProperty = parametersProperty.GetArrayElementAtIndex(parametersProperty.arraySize - 1);
                    parametersContainer.Add(new PropertyField(parameterProperty));
                    parameterProperty.FindPropertyRelative("name").stringValue           = $"{fieldInfo.FieldType.Name} {fieldInfo.Name}";
                    parameterProperty.FindPropertyRelative("path").stringValue           = "";
                    break;
                }
                case MethodInfo methodInfo: {
                    var parameterInfos = methodInfo.GetParameters();
                    parametersProperty.arraySize = parameterInfos.Length;
                    parametersProperty.serializedObject.ApplyModifiedProperties();
                    for (var i = 0; i < parametersProperty.arraySize; i++) {
                        var parameterProperty = parametersProperty.GetArrayElementAtIndex(i);
                        parametersContainer.Add(new PropertyField(parameterProperty));
                        parameterProperty.FindPropertyRelative("name").stringValue           = $"{parameterInfos[i].ParameterType.Name} {parameterInfos[i].Name}";
                        parameterProperty.FindPropertyRelative("path").stringValue           = "";
                    }
                    break;
                }
            }
            parametersContainer.MarkDirtyRepaint();
            return false;
        }
    }
}