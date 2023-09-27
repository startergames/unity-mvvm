using UnityEngine;
using UnityEngine.UI;

namespace Starter.View {
    public class ImageView : View {
        public string              path;
        public Image               target;

        private async void Start() {
            await viewmodel.InitializeAwaiter();
            var image = GetPropertyValue<Sprite>(path);

            if (image == null) {
                Debug.LogError("The specified path is invalid");
                return;
            }
            
            target.sprite = image;
        }
    }
}