using System;
using System.IO.Abstractions;
using System.Windows;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Services;
using BnsLauncher.Logging;
using BnsLauncher.ViewModels;
using Caliburn.Micro;
using Unity;

namespace BnsLauncher
{
    public class Bootstrapper : BootstrapperBase
    {
        private readonly IUnityContainer _container;
        private readonly IFileSystem _fs = new FileSystem();

        public Bootstrapper()
        {
            _container = new UnityContainer();

            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();

            _fs.Directory.CreateDirectory(Constants.ProfilesPath);
        }

        protected override void Configure()
        {
            _container.RegisterInstance(_fs);
            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();

            InitializeLogging();
            
            _container.RegisterSingleton<IGameStarter, GameStarter>();
            _container.RegisterSingleton<IProfileLoader, ProfileLoader>();
            _container.RegisterSingleton<ISampleProfileWriter, SampleProfileWriter>();

            _container.RegisterSingleton<ProfilesViewModel>();
            _container.RegisterSingleton<SettingsViewModel>();
            
            var gameConfigStorage = new JsonGlobalConfigStorage(_fs) {ConfigPath = Constants.ConfigPath};
            var config = gameConfigStorage.LoadConfig();

            if (string.IsNullOrWhiteSpace(config.ClientPath))
                config.ClientPath = "./bin/Client.exe";

            _container.RegisterInstance<IGlobalConfigStorage>(gameConfigStorage);
            _container.RegisterInstance(config);

            var profileWatcher = new ProfilesWatcher(Constants.ProfilesPath, _fs);
            _container.RegisterInstance<IProfilesWatcher>(profileWatcher);
            profileWatcher.WatchForChanges();
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            return key == null
                ? _container.Resolve(serviceType)
                : _container.Resolve(serviceType, key);
        }

        private void InitializeLogging()
        {
            var logVM = new LogViewModel();
            
            _container.RegisterInstance(logVM);
            _container.RegisterInstance<ILogger>(new Logger(logVM.LogEntries));
        }
    }
}