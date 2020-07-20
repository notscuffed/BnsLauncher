using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BnsLauncher.Core.Models;
using Caliburn.Micro;

namespace BnsLauncher.ViewModels
{
    public class AccountsViewModel : Screen
    {
        private readonly NavigationServices _navigationServices;
        private readonly GlobalConfig _globalConfig;

        public AccountsViewModel(NavigationServices navigationServices, GlobalConfig globalConfig)
        {
            _navigationServices = navigationServices;
            _globalConfig = globalConfig;
        }

        public ObservableCollection<Account> Accounts => _globalConfig.Accounts;

        public void EditAccount(Account account)
        {
            _navigationServices.Main.NavigateToViewModel<AccountViewModel>(new Dictionary<string, object>
            {
                [nameof(AccountViewModel.Account)] = account,
                [nameof(AccountViewModel.TitleText)] = "Editing account",
                [nameof(AccountViewModel.ActionButtonText)] = "Back",
                [nameof(AccountViewModel.OnAction)] = (Action<Account>) BackAction,
            });
        }

        public void CreateAccount()
        {
            var account = new Account();
            
            _navigationServices.Main.NavigateToViewModel<AccountViewModel>(new Dictionary<string, object>
            {
                [nameof(AccountViewModel.Account)] = account,
                [nameof(AccountViewModel.TitleText)] = "Adding account",
                [nameof(AccountViewModel.ActionButtonText)] = "Add",
                [nameof(AccountViewModel.OnAction)] = (Action<Account>) CreateAction,
            });
        }

        public void RemoveAccount(Account account)
        {
            Accounts.Remove(account);
        }

        private void BackAction(Account account)
        {
            _navigationServices.Main.NavigateToViewModel<AccountsViewModel>();
        }

        private void CreateAction(Account account)
        {
            Accounts.Add(account);
            _navigationServices.Main.NavigateToViewModel<AccountsViewModel>();
        }
    }
}