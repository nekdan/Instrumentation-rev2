using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DialogHost;
using Instrumentation.Dialogs;
using Instrumentation.ViewModels;
using ReactiveUI;
using Splat;

namespace Instrumentation.Views;

public class MainView : ReactiveUserControl<MainViewModel>
{
    private Button _infoButton;
    private ViewModelViewHost _dialog;

    public MainView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            Observable.FromEventPattern(_infoButton, nameof(Button.Click))
                .Select(_ => new AppInformationView())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => x.ShowDialog(Locator.Current.GetService<MainWindow>()));

            Interactions.ShowMessageBox.RegisterHandler(async interaction =>
            {
                var dialog = new MessageDialogView
                {
                    DataContext = interaction.Input
                };

                await DialogHost.DialogHost.Show(dialog,
                    delegate(object sender, DialogOpenedEventArgs args)
                    {
                        interaction.Input.Close
                            .Where(x => x is true)
                            .Subscribe(_ => args.Session.Close())
                            .DisposeWith(disposables);
                    });

                interaction.SetOutput(((MessageDialogViewModel) dialog.DataContext)!.Result ??
                                      MessageDialogResult.Cancel);
            }).DisposeWith(disposables);
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _infoButton = this.FindControl<Button>("InfoButton");
        _dialog = this.FindControl<ViewModelViewHost>("Dialog");
    }
}