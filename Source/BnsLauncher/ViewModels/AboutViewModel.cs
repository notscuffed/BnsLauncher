using System.Collections.Generic;
using System.Diagnostics;
using Caliburn.Micro;

namespace BnsLauncher.ViewModels
{
    public class AboutViewModel : Screen
    {
        public static IEnumerable<Attribution> Attributions { get; } = new[]
        {
            Add("bnspatch - by zeffy @ ", "https://github.com/bnsmodpolice/bnspatch"),
            Add("pluginloader - by zeffy @ ", "https://github.com/bnsmodpolice/pluginloader"),
            Add("UnityContainer @ ", "https://github.com/unitycontainer/unity"),
            Add("Fody.PropertyChanged @ ", "https://github.com/Fody/PropertyChanged"),
            Add("Newtonsoft.Json @ ", "https://github.com/JamesNK/Newtonsoft.Json"),
            Add("MaterialDesignInXAML @ ", "https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit"),
            Add("Caliburn.Micro @ ", "https://github.com/Caliburn-Micro/Caliburn.Micro"),
            Add("FluentWPF @ ", "https://github.com/sourcechord/FluentWPF"),
        };

        public class Attribution
        {
            public string Text { get; set; }
            public string Link { get; set; }
        }

        public void OpenUri(object input)
        {
            switch (input)
            {
                case Attribution attribution:
                    Process.Start(new ProcessStartInfo(attribution.Link));
                    break;
                
                case string link:
                    Process.Start(new ProcessStartInfo(link));
                    break;
            }
        }

        private static Attribution Add(string text, string link)
        {
            return new Attribution {Text = text, Link = link};
        }
    }
}