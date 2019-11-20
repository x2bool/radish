using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Model.ExplorerPad;
using Rbmk.Radish.Model.LicenseDialog;
using Rbmk.Radish.Model.Notifications;
using Rbmk.Radish.Model.WorkspacePad;
using Rbmk.Radish.Services.Licenses;
using Rbmk.Radish.Services.Persistence;
using Rbmk.Radish.Services.Persistence.Entities;
using Rbmk.Radish.Services.Updates;
using ReactiveUI;
using Rbmk.Utils.Reactive;
using Rbmk.Utils.System;
using Splat;

namespace Rbmk.Radish.Model
{
    public class MainWindowController
    {
        private readonly INotificationManager _notificationManager;
        private readonly IDialogManager _dialogManager;
        private readonly IUpdateService _updateService;
        private readonly ILicenseService _licenseService;
        private readonly IWebBrowser _webBrowser;
        private readonly IDatabaseContextFactory _databaseContextFactory;

        private readonly MainWindowModel _model;

        public MainWindowController(
            MainWindowModel model)
            : this (
                Locator.Current.GetService<INotificationManager>(),
                Locator.Current.GetService<IDialogManager>(),
                Locator.Current.GetService<IUpdateService>(),
                Locator.Current.GetService<ILicenseService>(),
                Locator.Current.GetService<IWebBrowser>(),
                Locator.Current.GetService<IDatabaseContextFactory>())
        {
            _model = model;
        }

        public MainWindowController(
            INotificationManager notificationManager,
            IDialogManager dialogManager,
            IUpdateService updateService,
            ILicenseService licenseService,
            IWebBrowser webBrowser,
            IDatabaseContextFactory databaseContextFactory)
        {
            _notificationManager = notificationManager;
            _dialogManager = dialogManager;
            _updateService = updateService;
            _licenseService = licenseService;
            _webBrowser = webBrowser;
            _databaseContextFactory = databaseContextFactory;
        }

        public IDisposable BindExitCommand()
        {
            _model.ExitCommand = ReactiveCommand.Create(
                () => { }, null, RxApp.MainThreadScheduler);

            return _model.ExitCommand.SubscribeWithLog();
        }

        public IDisposable BindInitialization()
        {
            return MigrateDatabaseAsync().ToObservable()
                .SelectMany(_ => CheckLicense())
                .SubscribeWithLog(isLicenseValid =>
                {
                    _model.Notification = new NotificationModel();
                    
                    if (isLicenseValid)
                    {
                        _model.ExplorerPad = new ExplorerPadModel();
                        _model.WorkspacePad = new WorkspacePadModel();
                        
                        _model.IsInitialized = true;
                    }
                    else
                    {
                        _model.ExitCommand.Execute()
                            .SubscribeWithLog();
                    }
                });
        }

        public IDisposable BindWindowTitle()
        {
            return _model.WhenAnyValue(m => m.WorkspacePad.Title)
                .SubscribeWithLog(title =>
                {
                    if (string.IsNullOrWhiteSpace(title) || title == "…")
                    {
                        _model.Title = "Radish";
                    }
                    else
                    {
                        _model.Title = "Radish - " + title;
                    }
                });
        }

        public IDisposable BindUpdateManager()
        {   
            return CheckUpdate() // TODO: use event from view instead of constant delay
                .ObserveOn(AvaloniaScheduler.Instance)
                .SubscribeWithLog(updateInfo =>
                {
                    if (updateInfo != null)
                    {
                        var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();
                        var icon = new Bitmap(assetLoader.Open(
                            new Uri("resm:Rbmk.Radish.Views.Icons.X96.Download.png?assembly=Rbmk.Radish.Views")));
                        
                        _notificationManager.Show(new NotificationContextModel
                        {
                            Icon = icon,
                            Title = $"Update to version {updateInfo.Version.ToSemverString()}",
                            Text = "A newer version is available",
                            OkCommand = ReactiveCommand.Create(
                                () => DownloadUpdate(updateInfo), null, RxApp.MainThreadScheduler),
                            OkText = "Download",
                            CancelCommand = ReactiveCommand.CreateFromTask(
                                () => SkipUpdate(updateInfo), null, RxApp.MainThreadScheduler),
                            CancelText = "Skip",
                            DefaultCommand = ReactiveCommand.Create(
                                () => DownloadUpdate(updateInfo), null, RxApp.MainThreadScheduler)
                        });
                    }
                });
        }

        private Task MigrateDatabaseAsync()
        {
            using (var db = _databaseContextFactory.CreateDbContext())
            {
                return db.Database.MigrateAsync();
            }
        }

        private IObservable<bool> CheckLicense()
        {
            return _licenseService.CheckCurrentLicenseAsync()
                .ToObservable()
                .SelectMany(isLicenseValid =>
                {
                    if (isLicenseValid)
                    {
                        return Observable.Return(true);
                    }
                    
                    return _dialogManager.Open(new LicenseDialogModel());
                })
                .SelectMany(_ => _licenseService.CheckCurrentLicenseAsync());
        }

        private IObservable<UpdateInfo> CheckUpdate()
        {
            return _updateService.CheckUpdate()
                .ToObservable()
                .Catch<UpdateInfo, Exception>(e => Observable.Return<UpdateInfo>(null))
                .Delay(TimeSpan.FromSeconds(20)); // TODO: use event from view instead of constant delay
        }

        private void DownloadUpdate(UpdateInfo updateInfo)
        {
            var url = updateInfo.DownloadUrl;
            _webBrowser.OpenUrl(url);
        }

        private async Task SkipUpdate(UpdateInfo updateInfo)
        {
            var version = updateInfo.Version;
            await _updateService.SkipUpdate(version);
        }
    }
}