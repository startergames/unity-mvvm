using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Command;
using Popup;
using Starter.Util;
using Starter.ViewModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace Inspector.CommandInspector {
    [CustomEditor(typeof(ActionView))]
    public class ActionViewInspector : UnityEditor.Editor {
        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();

            var viewModelProperty  = serializedObject.FindProperty(nameof(ActionView.viewModel));
            var methodProperty     = serializedObject.FindProperty(nameof(ActionView.methodData));
            var parametersProperty = serializedObject.FindProperty(nameof(ActionView.parameters));

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
                if (viewModelProperty.objectReferenceValue is not ViewModel viewModel)
                    return;

                var type = viewModel is ViewModelRelay relay ? relay.ViewModelType : viewModel.GetType();
                if (type == null) {
                    EditorUtility.DisplayDialog("Error", "ViewModel type is null", "OK");
                }
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
                    parameterProperty.FindPropertyRelative("name").stringValue = $"{propertyInfo.PropertyType.Name} {propertyInfo.Name}";
                    parameterProperty.FindPropertyRelative("type").stringValue = propertyInfo.PropertyType.AssemblyQualifiedName;
                    parameterProperty.serializedObject.ApplyModifiedProperties();
                    
                    var propertyField     = new PropertyField();
                    propertyField.BindProperty(parameterProperty);
                    parametersContainer.Add(propertyField);
                    break;
                }
                case FieldInfo fieldInfo: {
                    parametersProperty.arraySize = 1;
                    parametersProperty.serializedObject.ApplyModifiedProperties();
                    var parameterProperty = parametersProperty.GetArrayElementAtIndex(parametersProperty.arraySize - 1);
                    parameterProperty.FindPropertyRelative("name").stringValue = $"{fieldInfo.FieldType.Name} {fieldInfo.Name}";
                    parameterProperty.FindPropertyRelative("type").stringValue = fieldInfo.FieldType.AssemblyQualifiedName;
                    parameterProperty.serializedObject.ApplyModifiedProperties();
                    
                    var propertyField     = new PropertyField();
                    propertyField.BindProperty(parameterProperty);
                    parametersContainer.Add(propertyField);
                    break;
                }
                case MethodInfo methodInfo: {
                    var parameterInfos = methodInfo.GetParameters();
                    parametersProperty.arraySize = parameterInfos.Length;
                    parametersProperty.serializedObject.ApplyModifiedProperties();
                    for (var i = 0; i < parametersProperty.arraySize; i++) {
                        var parameterProperty = parametersProperty.GetArrayElementAtIndex(i);
                        parameterProperty.FindPropertyRelative("name").stringValue = $"{parameterInfos[i].ParameterType.Name} {parameterInfos[i].Name}";
                        parameterProperty.FindPropertyRelative("type").stringValue = parameterInfos[i].ParameterType.AssemblyQualifiedName;
                        parameterProperty.serializedObject.ApplyModifiedProperties();
                        
                        var propertyField     = new PropertyField();
                        propertyField.BindProperty(parameterProperty);
                        parametersContainer.Add(propertyField);
                    }

                    break;
                }
            }

            parametersContainer.MarkDirtyRepaint();
            return false;
        }
    }
}