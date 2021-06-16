using Zenject;

namespace of2.VFX
{
    public class VFXInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var vfxManager = gameObject.GetComponentInChildren<VFXManager>();
            Container.Bind<IVFXManager>().To<VFXManager>().FromInstance(vfxManager).AsSingle();
        }
    }
}