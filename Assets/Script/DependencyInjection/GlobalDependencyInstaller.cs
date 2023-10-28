using Script;
using Starter.ViewModel;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GlobalDependencyInstaller : LifetimeScope {
    [SerializeField]
    public TestBehaviour prefab;
    
    protected override void Configure(IContainerBuilder builder) {
        builder.Register<MessageBoxService>(Lifetime.Singleton)
               .AsImplementedInterfaces();
        builder.RegisterComponentInHierarchy<TestViewModel>();
        builder.RegisterComponentInNewPrefab(prefab, Lifetime.Scoped);
    }
}