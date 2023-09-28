using Attributes;

namespace Starter.View {
    public class DataView : View {
        [ViewModelPath]
        public string path;

        public DataSetter setter;

        public async void Start() {
            await WaitViewModelInitialized();
            var value = GetPropertyValue(path);
            setter.Set(value);
        }
    }
}