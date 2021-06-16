using System.Collections;
using UnityEngine;

namespace Zenject
{
    public class CoroutineRunnerInstaller : Installer<Transform, CoroutineRunnerInstaller>
    {
        [Inject] private Transform parent;
        public override void InstallBindings()
        {
            Container.BindFactory<IEnumerator, ofCoroutine, ofCoroutine.Factory>().FromPoolableMemoryPool();
            
            Container.Bind<CoroutineRunner>().ToSelf()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("CoroutineRunner")
                .UnderTransform(parent)
                .AsSingle();
        }
    }
}