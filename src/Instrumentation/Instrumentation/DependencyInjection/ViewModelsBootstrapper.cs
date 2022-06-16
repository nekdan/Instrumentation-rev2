using Instrumentation.Dialogs;
using Instrumentation.ViewModels;
using Instrumentation.Views;
using ReactiveUI;
using Splat;

namespace Instrumentation.DependencyInjection;

public static class ViewModelsBootstrapper
{
    public static void Register(IMutableDependencyResolver service, IReadonlyDependencyResolver resolver)
    {
        service.RegisterLazySingleton(() => new MainViewModel());
        service.Register(() => new InstrumentsViewModel());
        service.Register(() => new PlayerViewModel());
        service.Register(() => new ImagesCarouselDialogViewModel());

        RegisterViews(service, resolver);
    }

    private static void RegisterViews(IMutableDependencyResolver service, IReadonlyDependencyResolver resolver)
    {
        service.RegisterLazySingleton(() => new MainWindow());
        service.Register(() => new MainView(), typeof(IViewFor<MainViewModel>));
        service.Register(() => new InstrumentsView(), typeof(IViewFor<InstrumentsViewModel>));
        service.Register(() => new PlayerView(), typeof(IViewFor<PlayerViewModel>));
        service.Register(() => new MessageDialogView(), typeof(IViewFor<MessageDialogViewModel>));
        //service.Register(() => new ImagesCarouselDialogView(), typeof(IViewFor<ImagesCarouselDialogViewModel>));
    }
}