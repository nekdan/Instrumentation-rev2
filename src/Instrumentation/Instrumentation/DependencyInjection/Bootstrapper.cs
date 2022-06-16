using Instrumentation.Player;
using LazyCache;
using Splat;

namespace Instrumentation.DependencyInjection;

public static class Bootstrapper
{
    public static void Register(IMutableDependencyResolver service, IReadonlyDependencyResolver resolver)
    {
        DataAccessBootstrapper.Register(service, resolver);
        ViewModelsBootstrapper.Register(service, resolver);
        
        service.RegisterLazySingleton<ISoundPlayer>(() => new SoundPlayer());
        service.RegisterLazySingleton<IAppCache>(() => new CachingService());
    }
}