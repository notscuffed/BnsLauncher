using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BnsLauncher.Messages;
using Caliburn.Micro;

namespace BnsLauncher.ViewModels
{
    public class SideViewModel : Screen
    {
        private readonly NavigationServices _navigationServices;
        private readonly IEventAggregator _eventAggregator;

        public SideViewModel(NavigationServices navigationServices, IEventAggregator eventAggregator)
        {
            _navigationServices = navigationServices;
            _eventAggregator = eventAggregator;
        }

        public async Task TabClicked(string tabName)
        {
            switch (tabName)
            {
                case "exit":
                    Environment.Exit(0);
                    break;

                case "reload_profiles":
                    await _eventAggregator.PublishOnUIThreadAsync(new ReloadProfilesMessage());
                    break;

                case "settings":
                    _navigationServices.Main.NavigateToViewModel<SettingsViewModel>();
                    break;
                
                case "about":
                    _navigationServices.Main.NavigateToViewModel<AboutViewModel>();
                    break;
                
                case "profiles_folder":
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = Path.GetFullPath(Constants.ProfilesPath)
                    });

                    break;
                default:
                    _navigationServices.Main.NavigateToViewModel<ProfilesViewModel>();
                    break;
            }
        }
    }
}