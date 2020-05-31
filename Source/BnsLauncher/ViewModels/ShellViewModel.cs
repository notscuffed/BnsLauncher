using System.Windows.Controls;
using Caliburn.Micro;
using Unity;

namespace BnsLauncher.ViewModels
{
    public class ShellViewModel : Screen
    {
        private readonly IUnityContainer _container;
        private Frame _main;
        private Frame _side;

        public ShellViewModel(IUnityContainer container)
        {
            _container = container;
        }

        public void RegisterSideFrame(Frame frame)
        {
            _side = frame;
            TryRegisterFrames();
        }

        public void RegisterMainFrame(Frame frame)
        {
            _main = frame;
            TryRegisterFrames();
        }

        private void TryRegisterFrames()
        {
            if (_main == null || _side == null)
                return;
            
            var navigationServices = new NavigationServices(
                new FrameAdapter(_side),
                new FrameAdapter(_main));

            _container.RegisterInstance(navigationServices);

            navigationServices.Side.NavigateToViewModel(typeof(SideViewModel));
            navigationServices.Main.NavigateToViewModel(typeof(ProfilesViewModel));
        }
    }
}