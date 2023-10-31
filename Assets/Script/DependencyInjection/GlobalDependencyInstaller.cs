using Script;
using Starter.ViewModel;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GlobalDependencyInstaller : LifetimeScope {
    [SerializeField]
    public TestBehaviour prefab;

    [SerializeField]
    private MessageBoxService messageServicePrefab;

    protected override void Configure(IContainerBuilder builder) {
        builder.RegisterComponentInHierarchy<TestViewModel>();
        builder.RegisterComponentInNewPrefab(prefab, Lifetime.Scoped);
        builder.RegisterComponentInNewPrefab(messageServicePrefab, Lifetime.Scoped)
               .As<IMessageBoxService>();
    }
}