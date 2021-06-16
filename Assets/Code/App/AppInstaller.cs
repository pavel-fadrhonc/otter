using Zenject;

namespace DefaultNamespace.App
{
    public class AppInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
        }
    }
}