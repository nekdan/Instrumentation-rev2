using Instrumentation.DataAccess;
using Instrumentation.Model.Interfaces;
using Microsoft.EntityFrameworkCore;
using Splat;

namespace Instrumentation.DependencyInjection;

public static class DataAccessBootstrapper
{
    public static void Register(IMutableDependencyResolver service, IReadonlyDependencyResolver resolver)
    {
        var opts = new DbContextOptionsBuilder<InstrumentationContext>();
        opts.UseSqlite("Filename=app.db");
        
        service.Register(() => new InstrumentationContext(opts.Options));
        
        service.Register<IInstrumentationDataProvider>(() => 
            new InstrumentationDataProvider(resolver.GetService<InstrumentationContext>()));
    }
}