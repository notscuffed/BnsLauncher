using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Models;
using BnsLauncher.Messages;
using Caliburn.Micro;

namespace BnsLauncher.ViewModels
{
    public class ProfilesViewModel : Screen, IHandle<ReloadProfilesMessage>
    {
        private readonly IProfileLoader _profileLoader;
        private readonly IGameStarter _gameStarter;
        private readonly GameConfig _gameConfig;

        public ProfilesViewModel(IProfileLoader profileLoader, IGameStarter gameStarter, GameConfig gameConfig,
            IEventAggregator eventAggregator)
        {
            _profileLoader = profileLoader;
            _gameStarter = gameStarter;
            _gameConfig = gameConfig;

            eventAggregator.SubscribeOnUIThread(this);
        }

        public ObservableCollection<Profile> Profiles { get; set; } = new ObservableCollection<Profile>();

        public void StartGame(Profile profile) => _gameStarter.Start(profile, _gameConfig);
        public void StopProcess(Process process) => process?.Kill();

        protected override Task OnInitializeAsync(CancellationToken cancellationToken) => LoadProfiles();
        public Task HandleAsync(ReloadProfilesMessage message, CancellationToken cancellationToken) => LoadProfiles();

        private async Task LoadProfiles()
        {
            var profiles = await _profileLoader.LoadProfiles(Constants.ProfilesPath);
            
            Profiles.Clear();

            foreach (var profile in profiles)
            {
                Profiles.Add(profile);
            }
            
            NotifyOfPropertyChange(nameof(Profiles));
        }
    }
}