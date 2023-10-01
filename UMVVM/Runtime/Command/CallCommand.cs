using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Attributes;
using Starter.Util;
using Starter.View;
using Starter.ViewModel;
using UnityEngine;

namespace Command {
    public class CallCommand : MonoBehaviour {
        [System.Serializable]
        public class Parameter {
            public View view;

            [SerializeField]
            [HideInInspector]
            private string name;

            [ViewModelPath]
            [SerializeField]
            private string path;

            [SerializeReference]
            [SerializeField]
            private string value;

            public object Value => string.IsNullOrWhiteSpace(path) ? value : view.GetPropertyValue(path);

            public object GetValue(Type parameterType) {
                if (string.IsNullOrWhiteSpace(path)) {
                    if (string.IsNullOrWhiteSpace(value)) {
                        return FormatterServices.GetUninitializedObject(parameterType);
                    }
                    else {
                        return Convert.ChangeType(this.value, parameterType);
                    }
                }

                return view.GetPropertyValue(path);
            }
        }

        public ViewModel   viewModel;
        public string      methodData;
        public Parameter[] parameters;

        private ViewModel ViewModel => viewModel is ViewModelRelay relay ? relay.ViewModel : viewModel;

        public void Invoke() {
            var memberInfo = MemberInfoSerializer.Deserialize(methodData);
            switch (memberInfo) {
                case FieldInfo fieldInfo: {
                    var param = this.parameters.First();
                    fieldInfo.SetValue(ViewModel, param.Value);
                    break;
                }
                case PropertyInfo propertyInfo: {
                    var param = this.parameters.First();
                    propertyInfo.SetValue(ViewModel, param.Value);
                    break;
                }
                case MethodInfo methodInfo:
                    var parameterInfos = methodInfo.GetParameters();
                    var args           = new object[parameterInfos.Length];
                    for (int i = 0; i < parameterInfos.Length; i++) {
                        args[i] = parameters[i].GetValue(parameterInfos[i].ParameterType);
                    }

                    methodInfo.Invoke(ViewModel, args);
                    break;
            }
        }
    }
}