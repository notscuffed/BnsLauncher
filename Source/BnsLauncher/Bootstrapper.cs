using System;
using System.IO;
using System.Windows;
using BnsLauncher.Core.Abstractions;
using BnsLauncher.Core.Services;
using BnsLauncher.ViewModels;
using Caliburn.Micro;
using Unity;

namespace BnsLauncher
{
    public class Bootstrapper : BootstrapperBase
    {
        private readonly IUnityContainer _container;

        public Bootstrapper()
        {
            _container = new UnityContainer();

            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();

            Directory.CreateDirectory(Constants.ProfilesPath);
        }

        protected override void Configure()
        {
            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();

            _container.RegisterSingleton<IGameStarter, GameStarter>();
            _container.RegisterSingleton<ILogger, Logger>();
            _container.RegisterSingleton<IProfileLoader, ProfileLoader>();

            _container.RegisterSingleton<SettingsViewModel>();
            
            var gameConfigStorage = new JsonGameConfigStorage {ConfigPath = Constants.ConfigPath};
            var config = gameConfigStorage.LoadConfig();

            if (string.IsNullOrWhiteSpace(config.ClientPath))
                config.ClientPath = "./bin/Client.exe";

            _container.RegisterInstance<IGameConfigStorage>(gameConfigStorage);
            _container.RegisterInstance(config);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            return key == null
                ? _container.Resolve(serviceType)
                : _container.Resolve(serviceType, key);
        }
    }
}