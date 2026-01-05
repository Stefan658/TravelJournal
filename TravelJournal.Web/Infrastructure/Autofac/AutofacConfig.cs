using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Http;

using TravelJournal.Data.Context;
using TravelJournal.Data.Accessors;
using TravelJournal.Services.Interfaces;
using TravelJournal.Services.Implementations;

namespace TravelJournal.Web.Infrastructure.Autofac
{
    public static class AutofacConfig
    {
        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            // === MVC Controllers ===
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            // === WebAPI Controllers ===
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // === DbContext ===
            builder.RegisterType<TravelJournalDbContext>()
                   .AsSelf()
                   .InstancePerRequest();


            // === ACCESSORS ===
            builder.RegisterAssemblyTypes(typeof(UserAccessor).Assembly)
                   .Where(t => t.Name.EndsWith("Accessor"))
                   .AsImplementedInterfaces()
                   .InstancePerRequest();


            // REGISTER MVC CONTROLLERS
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // REGISTER WEB API CONTROLLERS
            builder.RegisterApiControllers(typeof(MvcApplication).Assembly);


            // === SERVICES ===
            builder.RegisterAssemblyTypes(typeof(JournalService).Assembly)
                   .Where(t => t.Name.EndsWith("Service"))
                   .AsImplementedInterfaces()
                   .InstancePerRequest();

            // === CACHE ===
            builder.RegisterType<MemoryCacheService>()
                   .As<ICache>()
                   .SingleInstance();


            // Optional: Entry accessor/service explicit (deși deja prin assembly scanning se prind)
            builder.RegisterAssemblyTypes(typeof(EntryAccessor).Assembly)
                   .Where(t => t.Name.EndsWith("Accessor"))
                   .AsImplementedInterfaces()
                   .InstancePerRequest();

            builder.RegisterAssemblyTypes(typeof(EntryService).Assembly)
                   .Where(t => t.Name.EndsWith("Service"))
                   .AsImplementedInterfaces()
                   .InstancePerRequest();


            var container = builder.Build();

            // MVC resolver
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // API resolver
            GlobalConfiguration.Configuration.DependencyResolver =
                new AutofacWebApiDependencyResolver(container);
        }
    }
}
