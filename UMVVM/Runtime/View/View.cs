using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExtensionMethod;
using Starter.ViewModel;
using UnityEngine;
using Util;

namespace Starter.View {
    public abstract class View : MonoBehaviour {
        public ViewModel.ViewModel viewmodel;

        public ViewModel.ViewModel ViewModel     => viewmodel is ViewModelRelay relay ? relay.ViewModel : viewmodel;
        public Type                ViewModelType => viewmodel is ViewModelRelay relay ? relay.ViewModelType : viewmodel?.GetType();
        public string              PrefixPath    => viewmodel is ViewModelRelay relay ? relay.PrefixPath : "";

        private readonly List<string>    paths      = new();
        private readonly HashSet<string> pathTokens = new();
        public           bool            IsInitialized { get; private set; } = false;

        private void Awake() {
            OnPathRegistration();
            viewmodel.PropertyChanged += ViewModelOnPropertyChanged;
            IsInitialized             =  true;
        }

        private void OnDestroy() {
            viewmodel.PropertyChanged -= ViewModelOnPropertyChanged;
        } 

        protected abstract void OnPathRegistration();

        protected void RegistePath(string path) {
            path = viewmodel.GetFullPath(path);
            if (paths.Contains(path)) return;
            paths.Add(path);
            pathTokens.UnionWith(path.Split('.'));
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (!IsInitialized) return;
            OnPropertyChanged(e.PropertyName);
        }

        public abstract void OnPropertyChanged(string propertyName);

        protected async Task WaitViewModelInitialized() {
            await viewmodel.InitializeAwaiter();
            while(!IsInitialized) await Task.Delay(10);
        }

        public object GetPropertyValue(string path) {
            return viewmodel.GetPropertyValue(path);
        }

        public T GetPropertyValue<T>(string path) where T : class {
            return viewmodel.GetPropertyValue(path) as T;
        }

    }
}