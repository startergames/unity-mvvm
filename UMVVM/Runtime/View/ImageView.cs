using UnityEngine;
using UnityEngine.UI;

namespace Starter.View {
    public class ImageView : View {
        public ViewModel.ViewModel ViewModel;
        public string              path;
        public Image               target;

        private async void Start() {
            await ViewModel.InitializeAwaiter();
            var image = GetPropertyValue(ViewModel, path) as Sprite;

            if (image == null) {
                Debug.LogError("The specified path is invalid");
                return;
            }
            
            target.sprite = image;
        }
    }
}