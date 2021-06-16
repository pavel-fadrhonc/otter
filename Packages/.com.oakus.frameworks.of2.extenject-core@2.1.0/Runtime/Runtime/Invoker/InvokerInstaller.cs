using System.ComponentModel;
using UnityEngine;
using Zenject;

namespace Zenject
{
    public class InvokerInstaller : Installer<Transform, InvokerInstaller>
    {
        [Inject] private Transform parent;
        
        public override void InstallBindings()
        {
            Container.Bind<ManualInvoker>().To<ManualInvoker>().AsSingle();
            Container.BindInterfacesAndSelfTo<WorldInvoker>().AsSingle();
            Container.Bind<MonoUpdater>().ToSelf()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("MonoUpdater")
                .UnderTransform(parent)
                .AsSingle();
        }
    }
}