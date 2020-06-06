using System;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;

namespace BnsLauncher.Models
{
    public class NavigationItem
    {
        public string Name { get; set; }
        public PackIconKind Icon { get; set; }
        public Func<Task> Action { get; set; }

        public NavigationItem(string name, PackIconKind icon, Func<Task> action)
        {
            Name = name;
            Icon = icon;
            Action = action;
        }
    }
}