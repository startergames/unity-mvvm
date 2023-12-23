using System.Collections;
using System.Collections.Generic;
using Attributes;
using Starter.View;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleView : View {
    [ViewModelPath(type: typeof(bool))]
    [SerializeField]
    private string path;
    
    [SerializeField]
    private Toggle toggle;

    void Start() {
        if(toggle == null)
            toggle = GetComponent<Toggle>();
        
        var isOn = GetPropertyValue<bool>(path);
        toggle.isOn = isOn;
        toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(bool isOn) {
        SetPropertyValue(path, isOn);
    }

    protected override void OnPropertyChanged(string propertyName) {
        toggle.onValueChanged.RemoveListener(OnValueChanged);
        var isOn = GetPropertyValue<bool>(path);
        toggle.isOn = isOn;
        toggle.onValueChanged.AddListener(OnValueChanged);
    }
}