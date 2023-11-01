using System.Collections;
using System.Reflection;
using UnityEngine.UIElements;

public class ViewModelCandidateList : VisualElement {
    private readonly ListView _listView;
    public IList itemsSource {
        get => _listView.itemsSource;
        set {
            _listView.itemsSource = value; 
            _listView.RefreshItems();
        }
    }

    public new class UxmlFactory : UxmlFactory<ViewModelCandidateList, UxmlTraits> {
    }

    public ViewModelCandidateList() {
        _listView = new ListView {
            fixedItemHeight = 20,
            makeItem        = () => new Label()
        };
        _listView.bindItem = (element, index) => {
            var label      = element as Label;
            var memberInfo = (MemberInfo)_listView.itemsSource[index];
            var type = memberInfo switch {
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                FieldInfo fieldInfo => fieldInfo.FieldType,
                _ => null
            };
            label.text = $"{memberInfo.Name} ({type.Name})";
        };
        Add(_listView);
    }
}