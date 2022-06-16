using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Instrumentation.Dialogs;
using ReactiveUI;

namespace Instrumentation.Views
{
    public class MessageDialogView : ReactiveUserControl<MessageDialogViewModel>
    {
        private Button _okButton;
        private Button _cancelButton;
        private Button _yesButton;
        private Button _noButton;

        private Image _questionIcon;
        private Image _infoIcon;
        private Image _warningIcon;
        private Image _errorIcon;

        public MessageDialogView()
        {
            InitializeComponent();

            ViewModel = (MessageDialogViewModel) DataContext;

            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.ViewModel.Icon)
                    .Select(x => x == MessageDialogIcon.Info)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => _infoIcon.IsVisible = x)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Icon)
                    .Select(x => x == MessageDialogIcon.Warning)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => _warningIcon.IsVisible = x)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Icon)
                    .Select(x => x == MessageDialogIcon.Error)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => _errorIcon.IsVisible = x)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Icon)
                    .Select(x => x == MessageDialogIcon.Question)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => _questionIcon.IsVisible = x)
                    .DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _okButton = this.FindControl<Button>("OkButton");
            _cancelButton = this.FindControl<Button>("CancelButton");
            _yesButton = this.FindControl<Button>("YesButton");
            _noButton = this.FindControl<Button>("NoButton");
            _questionIcon = this.FindControl<Image>("QuestionIcon");
            _infoIcon = this.FindControl<Image>("InfoIcon");
            _warningIcon = this.FindControl<Image>("WarningIcon");
            _errorIcon = this.FindControl<Image>("ErrorIcon");
        }
    }
}
