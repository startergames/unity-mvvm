using System;
using System.Threading.Tasks;
using Attributes;

namespace Starter.View {
    public class DataView : View {
        [ViewModelPath]
        public string path;

        public DataSetter setter;

        protected override async Task ViewModelBinded() {
            SetData();
        }

        private void SetData() {
            var value = GetPropertyValue(path);
            setter.Set(value);
        }

        protected override void OnPathRegistration() {
            RegistePath(path);
        }

        protected override void OnPropertyChanged(string propertyName) {
            SetData();
        }
    }
}