using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Starter.View {
    public class TextView : TextViewBase {

        public string              text;
        public TextMeshProUGUI     target;

        protected override async Task ViewModelBinded() {
            if (target == null) target = GetComponent<TextMeshProUGUI>();
            UpdateText();
        }

        private void UpdateText() {
            target.text = ResultText;
        }

        [ContextMenu("Tokenize Text")]
        public void TokenizeText() {
            TokenizeText(text);
        }

        protected override void OnPropertyChanged(string propertyName) {
            UpdateText();
        }

        protected override void OnPathRegistration() {
            TokenizeText();
            foreach (var token in tokens) {
                if (token is PropertyToken propertyToken) {
                    RegistePath(propertyToken.path);
                }
            }
        }
    }
}