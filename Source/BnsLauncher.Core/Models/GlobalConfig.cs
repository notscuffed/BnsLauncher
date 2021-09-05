using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace BnsLauncher.Core.Models
{
    public class GlobalConfig : PropertyChangedBase
    {
        public string ClientPath { get; set; }
        public string Arguments { get; set; }
        public bool Unattended { get; set; } = true;
        public bool NoTextureStreaming { get; set; } = true;
        public ObservableCollection<Account> Accounts { get; set; } = new ObservableCollection<Account>();

        public bool ShowPrivateServerIp { get; set; } = true;

        public GlobalConfig()
        {
            Accounts.CollectionChanged += AccountsOnCollectionChanged;
        }

        private void AccountsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Subscribe(e.NewItems.OfType<Account>());
                    break;
                
                case NotifyCollectionChangedAction.Remove:
                    Unsubscribe(e.OldItems.OfType<Account>());
                    break;
                
                case NotifyCollectionChangedAction.Replace:
                    Unsubscribe(e.OldItems.OfType<Account>());
                    Subscribe(e.NewItems.OfType<Account>());
                    break;
                
                case NotifyCollectionChangedAction.Move:
                    // Do nothing
                    break;
                
                case NotifyCollectionChangedAction.Reset:
                    Unsubscribe(e.OldItems.OfType<Account>());
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Unsubscribe(IEnumerable<Account> accounts)
        {
            foreach (var account in accounts)
            {
                account.PropertyChanged -= AccountOnPropertyChanged;
            }
        }
        
        private void Subscribe(IEnumerable<Account> accounts)
        {
            foreach (var account in accounts)
            {
                account.PropertyChanged += AccountOnPropertyChanged;
            }
        }

        private void AccountOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged($"{nameof(Account)}In{nameof(Accounts)}");
        }
    }
}