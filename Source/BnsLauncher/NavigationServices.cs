using Caliburn.Micro;

namespace BnsLauncher
{
    public class NavigationServices
    {
        public NavigationServices(INavigationService side, INavigationService main)
        {
            Side = side;
            Main = main;
        }

        public INavigationService Side { get; }
        public INavigationService Main { get; }
    }
}