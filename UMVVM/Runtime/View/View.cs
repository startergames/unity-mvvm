using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Starter.View {
    public abstract class View : MonoBehaviour {
        protected static object GetPropertyValue(object currentObject, string path) {
            if (currentObject == null || string.IsNullOrEmpty(path)) return null;

            var memberParts = path.Split('.');

            foreach (var part in memberParts) {
                if (currentObject == null) return null;

                var memberType = currentObject.GetType();
                var match     = Regex.Match(part, @"(^[a-zA-Z_]\w*)\[([^\]])\]$"); // Match array or list accessors

                if (match.Success) {
                    var memberName = match.Groups[1].Value;
                    var key      = match.Groups[2].Value;
                    var index      = int.Parse(key);

                    var member = memberType.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];
                    currentObject = GetMemberValue(member, currentObject);

                    if (currentObject is IList list)
                        currentObject = list[index];
                    else if (currentObject is IDictionary dictionary)
                        currentObject = dictionary[Convert.ChangeType(key, dictionary.GetType().GetGenericArguments()[0])];
                }
                else {
                    var member = memberType.GetMember(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];
                    currentObject = GetMemberValue(member, currentObject);
                }
            }

            return currentObject;
        }

        private static object GetMemberValue(MemberInfo member, object src) {
            switch (member) {
                case PropertyInfo property:
                    return property.GetValue(src, null);
                case FieldInfo field:
                    return field.GetValue(src);
                default:
                    return null;
            }
        }
    }
}