using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Instrumentation.ViewModels;
using Instrumentation.Views;
using Splat;

namespace Instrumentation;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop :
            {
                var stop = new CancellationTokenSource();

                async Task<MainWindow> GetMainWindowAsync()
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), stop.Token);
                    var mainWindow = Locator.Current.GetService<MainWindow>();
                    mainWindow.DataContext = Locator.Current.GetService<MainViewModel>();

                    return mainWindow;
                }
            
                var splash = new StartupWindow();
                splash.Show();

                desktop.MainWindow = await GetMainWindowAsync();
                desktop.MainWindow.Closed += (_, _) => stop.Cancel();
                desktop.MainWindow.Show();
                desktop.MainWindow.Activate();
                splash.Close();

                this.Run(stop.Token);
                break;
            }
            
            case ISingleViewApplicationLifetime singleViewPlatform :
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = Locator.Current.GetService<MainViewModel>()
                };
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}