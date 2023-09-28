using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Starter.View {
    public class TextView : TextViewBase {

        public string              text;
        public TextMeshProUGUI     target;


        private async void Start() {
            if (target == null) target = GetComponent<TextMeshProUGUI>();
            
            await WaitViewModelInitialized();
            UpdateText();
        }

        private void UpdateText() {
            target.text = ResultText;
        }

        [ContextMenu("Tokenize Text")]
        public void TokenizeText() {
            TokenizeText(text);
        }
    }
}