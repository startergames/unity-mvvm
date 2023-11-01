using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Starter;
using Starter.View;
using Starter.ViewModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.NVIDIA;
using UnityEngine.UIElements;

[CustomEditor(typeof(TextView))]
public class TextViewInspector : Editor {
    private static readonly Regex BracketRegex =
        new Regex(@"\{([^\}]*)\}?", RegexOptions.Compiled);

    public override VisualElement CreateInspectorGUI() {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.startergames.mvvm/Editor/Inspector/ViewInspector/TextViewInspector.uxml");

        var root = visualTree.CloneTree();

        var textView       = target as TextView;
        var viewModelField = root.Q<ObjectField>("viewmodel");
        var textField      = root.Q<TextField>("text");
        var candidateField = root.Q<ViewModelCandidateList>("candidate");
        candidateField.style.display = DisplayStyle.None;
        var errorLabel = root.Q<Label>("error");
        errorLabel.style.display = DisplayStyle.None;

        textField.RegisterValueChangedCallback(evt => {
            textView.TokenizeText();
            ToggleCandidate();
            if (ValidateText(textView.text)) {
                errorLabel.style.display = DisplayStyle.None;
            }
            else {
                errorLabel.style.display = DisplayStyle.Flex;
                errorLabel.text          = "{} brackets are not correctly matched!!";
            }
        });
        textField.RegisterCallback<KeyDownEvent>(evt => {
            if (evt.keyCode is KeyCode.UpArrow
                            or KeyCode.DownArrow
                            or KeyCode.LeftArrow
                            or KeyCode.RightArrow) {
                EditorApplication.delayCall += ToggleCandidate;
            }
        });
        textField.RegisterCallback<PointerDownEvent>(evt => { EditorApplication.delayCall += ToggleCandidate; });


        var vm = viewModelField.value as ViewModel;

        return root;

        void ToggleCandidate() {
            if (IsCursorInBrackets(textView.text, textField.cursorIndex)) {
                candidateField.style.display = DisplayStyle.Flex;
                var textCurrentBracket = GetTextCurrentBracket(textView.text, textField.cursorIndex);
                var members = serializedObject.GetMatchingMembers(
                    textCurrentBracket
                );
                candidateField.itemsSource = members;
            }
            else {
                candidateField.style.display = DisplayStyle.None;
            }
        }
    }

    private static string GetTextCurrentBracket(string text, int cursorIndex) {
        var groups = BracketRegex.Matches(text);
        return groups.FirstOrDefault(g => g.Index <= cursorIndex && g.Index + g.Length >= cursorIndex)?.Groups[1].Value;
    }

    private static bool IsCursorInBrackets(string text, int cursorIndex) {
        var groups = BracketRegex.Matches(text);
        return groups.Any(g => g.Index <= cursorIndex && g.Index + g.Length >= cursorIndex);
    }

    private static bool ValidateText(string text) {
        var openBrackets = 0;
        foreach (var c in text) {
            switch (c) {
                case '{': {
                    openBrackets++;
                    if (openBrackets > 1) {
                        // More opening brackets than closing brackets
                        return false;
                    }

                    break;
                }
                case '}': {
                    openBrackets--;
                    if (openBrackets < 0) {
                        // More closing brackets than opening brackets
                        return false;
                    }

                    break;
                }
            }
        }

        // All brackets are correctly matched if openBrackets is zero
        return openBrackets == 0;
    }
}