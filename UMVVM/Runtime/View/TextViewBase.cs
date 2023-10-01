using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Starter.View {
    public abstract class TextViewBase : View {
        public interface IToken {
            public string GetValue(View view);
        }

        [Serializable]
        public class TextToken : IToken {
            public string text;
            public string GetValue(View view) => text;
        }

        [Serializable]
        public class PropertyToken : IToken {
            public string path;
            public string format;

            public string GetValue(View view) {
                var value = view.GetPropertyValue(path);
                return string.IsNullOrEmpty(format)
                           ? value?.ToString()
                           : string.Format("{0:" + format + "}", value);
            }
        }

        [SerializeField, HideInInspector]
        [SerializeReference]
        protected List<IToken> tokens = new();

        public void TokenizeText(string text) {
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

        protected string ResultText => tokens?.Aggregate(string.Empty, (current, token) => current + token.GetValue(this)) ?? string.Empty;
    }
}