using System;
using BnsLauncher.Core.Models;
using Caliburn.Micro;

namespace BnsLauncher.ViewModels
{
    public class SelectAccountViewModel : Screen
    {
        private readonly NavigationServices _navigationServices;

        public SelectAccountViewModel(NavigationServices navigationServices)
        {
            _navigationServices = navigationServices;
        }

        public Account[] Accounts { get; set; }
        public Action<Account> OnAccountSelect { get; set; }

        public void SelectAccount(Account account)
        {
            OnAccountSelect?.Invoke(account);

            _navigationServices.Main.NavigateToViewModel<ProfilesViewModel>();
        }
    }
}