using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
        private readonly IProfileAccountsGetter _profileAccountsGetter;
        private readonly NavigationServices _navigationServices;
        private readonly GlobalConfig _globalConfig;

        public ProfilesViewModel(IProfileLoader profileLoader, IGameStarter gameStarter, GlobalConfig globalConfig,
            IEventAggregator eventAggregator, IProfileAccountsGetter profileAccountsGetter, NavigationServices navigationServices)
        {
            _profileLoader = profileLoader;
            _gameStarter = gameStarter;
            _globalConfig = globalConfig;
            _profileAccountsGetter = profileAccountsGetter;
            _navigationServices = navigationServices;

            gameStarter.OnProcessExit += GameStarterOnOnProcessExit;

            eventAggregator.SubscribeOnUIThread(this);
        }

        public ObservableCollection<Profile> Profiles { get; set; } = new ObservableCollection<Profile>();

        public void StartGame(Profile profile)
        {
            var matchedAccounts = _profileAccountsGetter.GetAccountsForProfile(profile, _globalConfig.Accounts.ToArray());

            // If no accounts then just start the game
            if (matchedAccounts.Length == 0)
            {
                _gameStarter.Start(profile, _globalConfig, null);
                return;
            }

            // Otherwise select account
            _navigationServices.Main.NavigateToViewModel<SelectAccountViewModel>(new Dictionary<string, object>
            {
                [nameof(SelectAccountViewModel.Accounts)] = matchedAccounts,
                [nameof(SelectAccountViewModel.OnAccountSelect)] =
                    (Action<Account>) (account => _gameStarter.Start(profile, _globalConfig, account))
            });
        }

        public void StopProcess(ProcessInfo processInfo) => processInfo?.Process.Kill();

        protected override Task OnInitializeAsync(CancellationToken cancellationToken) => LoadProfiles();
        public Task HandleAsync(ReloadProfilesMessage message, CancellationToken cancellationToken) => LoadProfiles();

        private async Task LoadProfiles()
        {
            var oldProfiles = Profiles.ToDictionary(x => x.ProfilePath, x => x.ProcessInfos);
            var profiles = await _profileLoader.LoadProfiles(Constants.ProfilesPath);

            // Migrate old process infos
            foreach (var profile in profiles)
            {
                if (!oldProfiles.TryGetValue(profile.ProfilePath, out var oldProcessInfos))
                    continue;

                foreach (var processInfo in oldProcessInfos)
                {
                    profile.AddProcessInfo(processInfo);
                }
            }

            Profiles.Clear();

            foreach (var profile in profiles)
            {
                Profiles.Add(profile);
            }

            NotifyOfPropertyChange(nameof(Profiles));
        }

        private void GameStarterOnOnProcessExit(Process process)
        {
            var pid = process.Id;
            var profile = Profiles.FirstOrDefault(x => x.HasProcessWithId(pid));
            profile?.RemoveProcessWithId(pid);
        }
    }
}