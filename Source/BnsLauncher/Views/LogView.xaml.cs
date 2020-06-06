using System.Windows.Controls;

namespace BnsLauncher.Views
{
    public partial class LogView
    {
        public LogView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!(e.Source is ScrollViewer scrollViewer))
                return;
            
            scrollViewer.ScrollToBottom();
        }
    }
}