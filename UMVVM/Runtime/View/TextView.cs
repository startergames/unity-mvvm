﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Starter.View {
    public class TextView : View {

        public interface IToken {
            public string GetValue(ViewModel.ViewModel viewModel);
        }
        [Serializable]
        public class TextToken : IToken {
            public string text;
            public string GetValue(ViewModel.ViewModel viewModel) => text;
        }

        [Serializable]
        public class PropertyToken : IToken {
            public string path;
            public string format;

            public string GetValue(ViewModel.ViewModel viewModel) {
                var value = GetPropertyValue(viewModel, path);
                return string.IsNullOrEmpty(format)
                           ? value?.ToString()
                           : string.Format("{0:" + format + "}", value);
            }
        }

        public ViewModel.ViewModel viewmodel;
        public string              text;
        public TextMeshProUGUI     target;

        [SerializeField]
        [SerializeReference]
        private List<IToken> tokens;

        private string ResultText => tokens?.Aggregate(string.Empty, (current, token) => current + token.GetValue(viewmodel)) ?? string.Empty;

        private async void Start() {
            await viewmodel.InitializeAwaiter();
            UpdateText();
        }

        private void UpdateText() {
            target.text = ResultText;
        }


#if UNITY_EDITOR
        [ContextMenu("Tokenize Text")]
        public void TokenizeText() {
            tokens.Clear();
            var regex     = new Regex(@"\{([^:}]+)(?::([^}]+))?\}");
            var matches   = regex.Matches(text);
            int lastIndex = 0;

            foreach (Match match in matches) {
                // Add preceding text token
                if (match.Index > lastIndex) {
                    tokens.Add(new TextToken { text = text.Substring(lastIndex, match.Index - lastIndex) });
                }

                // Add property token
                tokens.Add(new PropertyToken { path = match.Groups[1].Value, format = match.Groups[2].Value });
                lastIndex = match.Index + match.Length;
            }

            // Add trailing text token
            if (lastIndex < text.Length) {
                tokens.Add(new TextToken { text = text.Substring(lastIndex) });
            }
        }
#endif
    }
}