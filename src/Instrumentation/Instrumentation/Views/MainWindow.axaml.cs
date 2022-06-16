using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Instrumentation.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            Interactions.ShowImagesCarousel.RegisterHandler(async interaction =>
            {
                var dialog = new ImagesCarouselDialogView
                {
                    DataContext = interaction.Input
                };

                // await DialogHost.DialogHost.Show(dialog,
                //     delegate(object sender, DialogOpenedEventArgs args)
                //     {
                //         interaction.Input.Close
                //             .Where(x => x is true)
                //             .Subscribe(_ => args.Session.Close())
                //             .DisposeWith(disposables);
                //     });
                await dialog.ShowDialog(this);

                interaction.SetOutput(Unit.Default);
            });

            Interactions.ShowSaveFileDialog.RegisterHandler(async interaction =>
            {
                var dialog = new SaveFileDialog
                {
                    InitialFileName = interaction.Input,
                    DefaultExtension = "jpg",
                };
                var result = await dialog.ShowAsync(this);
                
                interaction.SetOutput(result);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}