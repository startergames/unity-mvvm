using System;
using Attributes;

namespace Starter.View {
    public class DataView : View {
        [ViewModelPath]
        public string path;

        public DataSetter setter;

        public async void Start() {
            await WaitViewModelInitialized();
            SetData();
        }

        private void SetData() {
            var value = GetPropertyValue(path);
            setter.Set(value);
        }

        protected override void OnPathRegistration() {
            RegistePath(path);
        }
        
        public override void OnPropertyChanged(string propertyName) {
            SetData();
        }
    }
}