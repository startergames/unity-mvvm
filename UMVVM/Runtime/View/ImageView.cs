using System;
using System.Threading.Tasks;
using Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Starter.View {
    public class ImageView : View {
        [ViewModelPath(type: typeof(Sprite))]
        public string path;

        public Image target;

        protected override async Task ViewModelBinded() {
            if (target == null) target = GetComponent<Image>();
            SetImage();
        }

        private void SetImage() {
            var image = GetPropertyValue<Sprite>(path);

            if (image == null) {
                Debug.LogError("The specified path is invalid : " + path);
                return;
            }

            target.sprite = image;
        }

        protected override void OnPathRegistration() {
            RegistePath(path);
        }

        protected override void OnPropertyChanged(string propertyName) {
            SetImage();
        }
    }
}