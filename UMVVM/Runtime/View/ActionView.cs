using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Attributes;
using ExtensionMethod;
using Starter.Util;
using Starter.View;
using Starter.ViewModel;
using UnityEngine;
using UnityEngine.Events;

namespace Command {
    public class ActionView : MonoBehaviour {
        [System.Serializable]
        public class Parameter {
            public ViewModel viewmodel;
            public string    type;

            [SerializeField]
            [HideInInspector]
            private string name;

            [ViewModelPath(nameof(viewmodel))]
            [SerializeField]
            private string path;

            [SerializeReference]
            [SerializeField]
            private string value;

            public object Value => string.IsNullOrWhiteSpace(path) ? value : viewmodel.GetPropertyValue(path);

            public object GetValue(Type parameterType) {
                if (string.IsNullOrWhiteSpace(path)) {
                    if (string.IsNullOrWhiteSpace(value)) {
                        return FormatterServices.GetUninitializedObject(parameterType);
                    }

                    if (parameterType.IsEnum) {
                        return Enum.Parse(parameterType, value);
                    }

                    return Convert.ChangeType(this.value, parameterType);
                }

                return viewmodel.GetPropertyValue(path);
            }
        }

        public ViewModel   viewModel;
        public string      methodData;
        public Parameter[] parameters;
        public UnityEvent  onInvoked;

        private ViewModel ViewModel => viewModel is ViewModelRelay relay ? relay.ViewModel : viewModel;

        public async void Invoke() {
            try {
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

                        var result = methodInfo.Invoke(ViewModel, args);
                        if (result is Task task) {
                            await task;
                        }

                        break;
                }

                onInvoked?.Invoke();
            }
            catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }
}