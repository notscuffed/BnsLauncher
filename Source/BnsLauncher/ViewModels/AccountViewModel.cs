using System;
using BnsLauncher.Core.Models;
using Caliburn.Micro;

namespace BnsLauncher.ViewModels
{
    public class AccountViewModel : Screen
    {
        public Account Account { get; set; }
        public string TitleText { get; set; }
        public string ActionButtonText { get; set; }
        public Action<Account> OnAction { get; set; }

        public void ExecuteAction()
        {
            OnAction?.Invoke(Account);
        }
    }
}