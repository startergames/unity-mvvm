using System;
using System.Threading.Tasks;
using Starter.View;
using Starter.ViewModel;
using TMPro;
using UnityEngine;

public class MessageBoxView : DialogView<bool> {
    [SerializeField]
    private TextMeshProUGUI title;

    [SerializeField]
    private TextMeshProUGUI message;

    [SerializeField]
    private TextMeshProUGUI ok;

    [SerializeField]
    private TextMeshProUGUI cancel;


    public string Title {
        get => title.text;
        set => title.text = value;
    }

    public string Message {
        get => message.text;
        set => message.text = value;
    }

    public string Ok {
        get => ok.text;
        set => ok.text = value;
    }

    public string Cancel {
        get => cancel.text;
        set => cancel.text = value;
    }

    public Action OkAction     { get; set; }
    public Action CancelAction { get; set; }


    public void OnOk() {
        OkAction?.Invoke();
        DialogViewModel.SetResult(true);
    }

    public void OnCancel() {
        CancelAction?.Invoke();
        DialogViewModel.SetResult(false);
    }

    public async Task<bool> WaitForResult() {
        return await DialogViewModel.WaitForResult();
    }
}

public class DialogView<T> {
    public DialogViewModel DialogViewModel { get; set; }
}

public class DialogViewModel : ViewModel {
    public override Task Initialize() {
        throw new NotImplementedException();
    }

    public override void Finalize() {
        throw new NotImplementedException();
    }

    public void SetResult(bool b) {
        throw new NotImplementedException();
    }

    public async Task<bool> WaitForResult() {
        throw new NotImplementedException();
    }
}