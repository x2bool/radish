using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Model.Notifications;
using Rbmk.Radish.Services.Licenses;
using Rbmk.Radish.Services.Persistence;
using Rbmk.Radish.Services.Redis.Connections;
using Rbmk.Radish.Services.Redis.Executor;
using Rbmk.Radish.Services.Redis.Parser;
using Rbmk.Radish.Services.Redis.Projections;
using Rbmk.Radish.Services.Updates;
using Rbmk.Radish.Views;
using Rbmk.Radish.Views.Dialogs;
using Rbmk.Radish.Views.Notifications;
using Rbmk.Utils.Apis;
using Rbmk.Utils.Broadcasts;
using Rbmk.Utils.Licenses;
using Rbmk.Utils.Meta;
using Rbmk.Utils.System;
using Serilog;
using Serilog.Core;
using Splat;

namespace Rbmk.Radish
{
    public class Registry
    {
        public void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.Register<Serilog.ILogger>(
                () =>
                {
                    var storage = resolver.GetService<IApplicationStorage>();
                    return new LoggerConfiguration()
                        .WriteTo.Console()
                        .WriteTo.File(Path.Combine(storage.LogDirectory, "application.log"))
                        .CreateLogger();
                });
            
            services.RegisterLazySingleton<IApplicationInfo>(
                () => new ApplicationInfo(
                    Assembly.GetExecutingAssembly()));
            
            services.RegisterLazySingleton<IApiClient>(
                () =>
                {
                    var httpClient = new HttpClient
                    {
                        BaseAddress = new Uri("https://api.rbmk.io")
                    };
                    return new ApiClient(httpClient);
                });
            services.RegisterLazySingleton<IWebBrowser>(
                () => new WebBrowser());
            
            services.RegisterLazySingleton<IBroadcastService>(
                () => new BroadcastService());
            services.RegisterLazySingleton<IUpdateStorage>(
                () => new UpdateStorage(
                    resolver.GetService<IDatabaseContextFactory>()));
            services.RegisterLazySingleton<IUpdateService>(
                () => new UpdateService(
                    resolver.GetService<IApplicationInfo>(),
                    resolver.GetService<IApiClient>(),
                    resolver.GetService<IUpdateStorage>()));
            
            services.RegisterLazySingleton<IDialogProvider>(
                () => new DialogProvider(
                    typeof(MainApplication).Assembly));
            services.RegisterLazySingleton<IDialogManager>(
                () => new DialogManager(
                    resolver.GetService<IDialogProvider>()));
            services.RegisterLazySingleton<INotificationManager>(
                () => new NotificationManager());
            
            services.RegisterLazySingleton<IApplicationStorage>(
                () => ApplicationStorage.Instance);
            services.RegisterLazySingleton<IDatabaseContextFactory>(
                () => new DatabaseContextFactory());
            
            services.RegisterLazySingleton<ILicenseChecker>(
                () => new LicenseChecker());
            services.RegisterLazySingleton<ILicenseStorage>(
                () => new LicenseStorage(
                    resolver.GetService<IDatabaseContextFactory>()));
            services.RegisterLazySingleton<ILicenseService>(
                () => new LicenseService(
                    resolver.GetService<ILicenseStorage>(),
                    resolver.GetService<ILicenseChecker>()));
            
            services.RegisterLazySingleton<IClientAccessor>(
                () => new ClientAccessor());
            services.RegisterLazySingleton<IConnectionStorage>(
                () => new ConnectionStorage(
                    resolver.GetService<IDatabaseContextFactory>()));
            services.RegisterLazySingleton<IConnectionProvider>(
                () => new ConnectionProvider(
                    resolver.GetService<IBroadcastService>(),
                    resolver.GetService<IConnectionStorage>(),
                    resolver.GetService<IClientAccessor>()));
            services.RegisterLazySingleton<ICommandExecutor>(
                () => new CommandExecutor(
                    resolver.GetService<IClientAccessor>()));
            services.RegisterLazySingleton<ICommandParser>(
                () => new CommandParser());
            
            services.RegisterLazySingleton<IResultProjector>(
                () => new ResultProjector());
            services.RegisterLazySingleton<IStructProjector>(
                () => new StructProjector(
                    resolver.GetService<IClientAccessor>()));
        }
    }
}