using Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Starter.View {
    public class ImageView : View {
        [ViewModelPath(type: typeof(Sprite))]
        public string              path;
        public Image               target;

        private async void Start() {
            if (target == null) target = GetComponent<Image>();
            
            await WaitViewModelInitialized();
            var image = GetPropertyValue<Sprite>(path);

            if (image == null) {
                Debug.LogError("The specified path is invalid");
                return;
            }
            
            target.sprite = image;
        }
    }
}