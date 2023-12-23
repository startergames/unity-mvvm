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
        [SerializeField]
        private ViewModel.ViewModel viewmodel;

        // Relay를 사용하는 경우 Relay의 ViewModel을 반환합니다.
        public ViewModel.ViewModel ViewModel {
            get => viewmodel is ViewModelRelay relay ? relay.ViewModel : viewmodel;
            set {
                UnregistPropertyChangedCallback();
                viewmodel = value;
                RegistPropertyChangedCallback();
                ViewModelBinded();
            }
        }

        // Relay를 사용하는 경우 Relay 자체를 반환합니다.
        // Relay를 수행하지 않습니다.
        public ViewModel.ViewModel ViewModelSelf => viewmodel;

        public Type                ViewModelType => viewmodel is ViewModelRelay relay ? relay.ViewModelType : viewmodel?.GetType();
        public string              PrefixPath    => viewmodel is ViewModelRelay relay ? relay.PrefixPath : "";

        private readonly List<string>    paths      = new();
        private readonly HashSet<string> pathTokens = new();
        
        // Awake 함수가 부모 Object부터 호출되기 때문에
        // 부모 View의 Initialize 처리 과정에서 자식 View의 동작을 호출하게 되는 경우
        // 자식 View의 Initialize가 이루어지지 않은 상태여서 Exception이 발생하는 경우가 있음.
        private          bool            IsInitialized { get; set; } = false;

        private void Awake() {
            OnPathRegistration();
            RegistPropertyChangedCallback();
            ViewModelBinded();
            IsInitialized             =  true;
        }

        private void OnDestroy() {
            UnregistPropertyChangedCallback();
        }

        private void RegistPropertyChangedCallback() {
            if (viewmodel == null)
                return;
            viewmodel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void UnregistPropertyChangedCallback() {
            if (viewmodel == null)
                return;
            viewmodel.PropertyChanged -= ViewModelOnPropertyChanged;
        }

        // View에서 사용하는 Path들을 RegistPath 함수를 사용하여 미리 등록합니다.
        // OnPropertyChanged 이벤트가 발생했을 때 Path를 확인하여 갱신이 필요한 View인지 판단할 때 사용합니다.
        // 아직은 Path에 대한 처리가 구현되지 않았습니다.
        protected virtual void OnPathRegistration() { }

        protected virtual Task ViewModelBinded() {
            return Task.CompletedTask;
        }

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

        protected abstract void OnPropertyChanged(string propertyName);

        protected async Task WaitViewModelInitialized() {
            while (!IsInitialized) await Task.Delay(10);
            await viewmodel.InitializeAwaiter();
        }

        public object GetPropertyValue(string path) {
            return viewmodel.GetPropertyValue(path);
        }

        public T GetPropertyValue<T>(string path) {
            return (T)viewmodel.GetPropertyValue(path);
        }
        
        public void SetPropertyValue(string path, object value) {
            viewmodel.SetPropertyValue(path, value);
        }
    }
}