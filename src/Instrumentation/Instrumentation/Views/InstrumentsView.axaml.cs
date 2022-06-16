using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Instrumentation.Model;
using Instrumentation.ViewModels;
using ReactiveUI;

namespace Instrumentation.Views;

public partial class InstrumentsView : ReactiveUserControl<InstrumentsViewModel>
{
    private Carousel _carousel;
    private Button _left;
    private Button _right;
    private Grid _carouselBlock;
    private DockPanel _imageData;

    private Grid _instrumentCarouselBlock;
    private Button _instrumentLeft;
    private Button _instrumentRight;
    private Carousel _instrumentCarousel;
    
    public InstrumentsView()
    {
        InitializeComponent();
        
        _left.Click += (s, e) => _carousel.Previous();
        _right.Click += (s, e) => _carousel.Next();
        _instrumentLeft.Click += (s, e) => _instrumentCarousel.Previous();
        _instrumentRight.Click += (s, e) => _instrumentCarousel.Next();

        this.WhenActivated(disposables =>
        {
            Observable.FromEventPattern(_carousel, nameof(_carousel.Tapped))
                .SelectMany(_ => ViewModel.ShowImages.Execute())
                .Subscribe()
                .DisposeWith(disposables);

            Observable.FromEventPattern(_instrumentCarousel, nameof(_carousel.Tapped))
                .SelectMany(_ => ViewModel.ShowImages.Execute())
                .Subscribe()
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.ViewModel.SoundNoteImages)
                .WhereNotNull()
                .Select(x => x.Count > 1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    _left.IsVisible = x;
                    _right.IsVisible = x;
                })
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.ViewModel.InstrumentImages)
                .WhereNotNull()
                .Select(x => x.Count > 1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    _instrumentLeft.IsVisible = x;
                    _instrumentRight.IsVisible = x;
                })
                .DisposeWith(disposables);
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        _carousel = this.FindControl<Carousel>("carousel");
        _left = this.FindControl<Button>("left");
        _right = this.FindControl<Button>("right");
        _carouselBlock = this.FindControl<Grid>("carouselBlock");
        _imageData = this.FindControl<DockPanel>("ImageData");

        _instrumentCarousel = this.FindControl<Carousel>("instrumentCarousel");
        _instrumentLeft = this.FindControl<Button>("instrumentLeft");
        _instrumentRight = this.FindControl<Button>("instrumentRight");
        _instrumentCarouselBlock = this.FindControl<Grid>("instrumentCarouselBlock");
    }
}