using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Instrumentation.Views
{
    public partial class AppInformationView : Window
    {
        public AppInformationView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
