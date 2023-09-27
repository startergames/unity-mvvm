using System.Threading.Tasks;

namespace Starter.ViewModel {
    public class ViewModelRelay : ViewModel {
        public ViewModel viewmodel;
        public string    prefixPath;

        public                bool   findInParent = false;
        public                string typeName;

        public override async Task Initialize() {
            await viewmodel.InitializeAwaiter();
        }
    }
}