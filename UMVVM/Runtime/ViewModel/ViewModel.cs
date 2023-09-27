using System.Threading.Tasks;
using UnityEngine;

namespace Starter.ViewModel {
    public abstract class ViewModel : MonoBehaviour {
        private         bool IsInitialized { get; set; }
        public abstract Task Initialize();

        public async Task InitializeAwaiter() {
            while (!IsInitialized)
                await Task.Delay(10);
        }

        private async void Start() {
            await Initialize();
            IsInitialized = true;
        }
    }
}