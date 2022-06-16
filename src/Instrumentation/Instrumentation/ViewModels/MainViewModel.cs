using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData.Binding;
using Instrumentation.Model.Interfaces;
using Instrumentation.Player;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Instrumentation.ViewModels
{
    public class MainViewModel : ViewModelBase, IActivatableViewModel, IScreen
    {
        private readonly IInstrumentationDataProvider _instrumentation = 
            Locator.Current.GetService<IInstrumentationDataProvider>();

        private readonly ISoundPlayer _soundPlayer =
            Locator.Current.GetService<ISoundPlayer>();
        
        public ViewModelActivator Activator { get; } = new();
        public RoutingState Router { get; } = new();

        [Reactive] public ObservableCollectionExtended<CategoryViewModel> Categories { get; private set; } = new();
        
        [Reactive] public CategoryViewModel SelectedCategory { get; set; }
        
        [Reactive] public bool IsLoading { get; private set; }

        [Reactive]
        public PlayerViewModel PlayerViewModel { get; private set; } =
            Locator.Current.GetService<PlayerViewModel>();

        public ReactiveCommand<Unit, IEnumerable<CategoryViewModel>> LoadCategories { get; private set; }

        public MainViewModel()
        {
            LoadCategories = ReactiveCommand.CreateFromTask(GetCategoriesAsync);
            
            this.WhenActivated(disposables =>
            {
                LoadCategories.IsExecuting
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => IsLoading = x)
                    .DisposeWith(disposables);

                LoadCategories
                    .Select(x => new ObservableCollectionExtended<CategoryViewModel>(x))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => Categories = x)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.SelectedCategory)
                    .WhereNotNull()
                    .Select(x => x.ViewModel)
                    .SelectMany(x => Router.Navigate.Execute(x))
                    .Subscribe()
                    .DisposeWith(disposables);
                
                this.WhenAnyValue(x => x.SelectedCategory)
                    .WhereNotNull()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x =>
                    {
                        foreach (var cat in Categories
                                     .Where(c => c.Category.Id != x.Category.Id))
                        {
                            cat.Opacity = 0.5;
                        }

                        x.Opacity = 1;
                    })
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.SelectedCategory)
                    .WhereNotNull()
                    .Subscribe(_ => _soundPlayer.Stop())
                    .DisposeWith(disposables);

                LoadCategories.Execute().Subscribe().DisposeWith(disposables);

                Disposable.Create(() =>
                {
                    _soundPlayer.Dispose();
                }).DisposeWith(disposables);
            });
        }

        private async Task<IEnumerable<CategoryViewModel>> GetCategoriesAsync()
        {
            var data = await _instrumentation.GetInstrumentCategoriesAsync()
                .Select(x => new CategoryViewModel()
                {
                    Category = x,
                    Opacity = 0.7,
                    ViewModel = Locator.Current.GetService<InstrumentsViewModel>().Configure(x)
                })
                .ToListAsync();

            return data;
        }
            
    }
}