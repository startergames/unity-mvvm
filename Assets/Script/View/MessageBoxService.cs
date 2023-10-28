using System;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MessageBoxService : IMessageBoxService {
    [SerializeField]
    public GameObject prefab;
    
    readonly IObjectResolver resolver;
    
    public MessageBoxService(IObjectResolver resolver) {
        this.resolver = resolver;
    }
    
    public async Task<bool> Show(string title, string message, string ok, string cancel, Action okAction, Action cancelAction = null) {
        Debug.Log($"MessageBoxService.Show: {title}, {message}, {ok}, {cancel}");
        
        var go = resolver.Instantiate(prefab);
        var view = go.GetComponent<MessageBoxView>();
        view.Title = title;
        view.Message = message;
        view.Ok = ok;
        view.Cancel = cancel;
        view.OkAction = okAction;
        view.CancelAction = cancelAction;

        return await view.WaitForResult();
    }
}