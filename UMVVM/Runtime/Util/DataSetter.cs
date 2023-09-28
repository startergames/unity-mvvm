using System.Reflection;
using Starter.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Starter.View {
    [System.Serializable]
    public class DataSetter {
        public enum MemberType {
            Field,
            Property,
            Method,
        }

        public MonoBehaviour target;

        [FormerlySerializedAs("methodInfo")]
        [SerializeField, HideInInspector]
        private string memberName;

        public void Set(object value) {
            var memberInfo = MemberInfoSerializer.Deserialize(memberName);
            switch (memberInfo) {
                case FieldInfo fieldInfo:
                    fieldInfo.SetValue(target, value);
                    break;
                case PropertyInfo propertyInfo:
                    propertyInfo.SetValue(target, value);
                    break;
                case MethodInfo methodInfo:
                    var      parameters = methodInfo.GetParameters();
                    var args       = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++) {
                        if (i == 0) {
                            // First parameter receives the provided value
                            args[i] = value;
                        }
                        else {
                            // Other parameters receive their default values if they are optional
                            args[i] = parameters[i].IsOptional ? parameters[i].DefaultValue : null;
                        }
                    }

                    methodInfo.Invoke(target, args);
                    break;
            }
        }
    }
}