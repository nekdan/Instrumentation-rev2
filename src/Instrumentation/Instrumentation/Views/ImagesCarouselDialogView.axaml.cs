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
    public class ImagesCarouselDialogView : ReactiveWindow<ImagesCarouselDialogViewModel>
    {
        private Carousel _carousel;
        private Button _left;
        private Button _right;

        public ImagesCarouselDialogView()
        {
            InitializeComponent();

            _left.Click += (s, e) => _carousel.Previous();
            _right.Click += (s, e) => _carousel.Next();

            this.WhenActivated(disposables =>
            {
                if (DataContext is ImagesCarouselDialogViewModel vm)
                    ViewModel = vm;

                this.WhenAnyValue(x => x.ViewModel.Images)
                    .WhereNotNull()
                    .Select(x => x.Count > 1)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x =>
                    {
                        _left.IsVisible = x;
                        _right.IsVisible = x;
                    })
                    .DisposeWith(disposables);

                this.ViewModel.Close
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.Close())
                    .DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _carousel = this.FindControl<Carousel>("carousel");
            _left = this.FindControl<Button>("left");
            _right = this.FindControl<Button>("right");
        }
    }
}
