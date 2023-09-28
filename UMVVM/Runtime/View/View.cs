﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Starter.ViewModel;
using UnityEngine;

namespace Starter.View {
    public abstract class View : MonoBehaviour {
        [SerializeField]
        private ViewModel.ViewModel viewmodel;
        
        public ViewModel.ViewModel ViewModel => GetFullPath(viewmodel, "").obj;

        protected Task WaitViewModelInitialized() {
            return viewmodel.InitializeAwaiter();
        }

        public object GetPropertyValue(string path) {
            return GetPropertyValue(viewmodel, path);
        }
        public T GetPropertyValue<T>(string path) where T : class {
            return GetPropertyValue(viewmodel, path) as T;
        }
        private static object GetPropertyValue(ViewModel.ViewModel viewModel, string path) {
            (viewModel, path) = GetFullPath(viewModel, path);
            object currentObject = viewModel;
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

                    var member = memberType.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase)[0];
                    currentObject = GetMemberValue(member, currentObject);

                    if (currentObject is IList list)
                        currentObject = list[index];
                    else if (currentObject is IDictionary dictionary)
                        currentObject = dictionary[Convert.ChangeType(key, dictionary.GetType().GetGenericArguments()[0])];
                }
                else {
                    var member = memberType.GetMember(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase)[0];
                    currentObject = GetMemberValue(member, currentObject);
                }
            }

            return currentObject;
        }

        private static (ViewModel.ViewModel obj, string path) GetFullPath(ViewModel.ViewModel currentObject, string path) {
            while (true) {
                if (currentObject is ViewModelRelay relay) {
                    var appendedPath = !string.IsNullOrWhiteSpace(path) ? relay.prefixPath + "." + path : relay.prefixPath;
                    currentObject = relay.viewmodel;
                    path          = appendedPath;
                    continue;
                }

                return (currentObject, path);
            }
        }

        private static object GetMemberValue(MemberInfo member, object src) {
            return member switch {
                PropertyInfo property => property.GetValue(src, null),
                FieldInfo field => field.GetValue(src),
                _ => null
            };
        }
    }
}