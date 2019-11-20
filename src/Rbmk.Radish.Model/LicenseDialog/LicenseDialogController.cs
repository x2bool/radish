using System;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Services.Licenses;
using Rbmk.Utils.Licenses;
using Rbmk.Utils.Reactive;
using Rbmk.Utils.System;
using ReactiveUI;
using Splat;

namespace Rbmk.Radish.Model.LicenseDialog
{
    public class LicenseDialogController
    {
        private readonly IDialogManager _dialogManager;
        private readonly ILicenseChecker _licenseChecker;
        private readonly ILicenseService _licenseService;
        private readonly IWebBrowser _webBrowser;

        private readonly LicenseDialogModel _model;

        public LicenseDialogController(
            LicenseDialogModel model)
            : this(
                Locator.Current.GetService<IDialogManager>(),
                Locator.Current.GetService<ILicenseChecker>(),
                Locator.Current.GetService<ILicenseService>(),
                Locator.Current.GetService<IWebBrowser>())
        {
            _model = model;
        }

        private LicenseDialogController(
            IDialogManager dialogManager,
            ILicenseChecker licenseChecker,
            ILicenseService licenseService,
            IWebBrowser webBrowser)
        {
            _dialogManager = dialogManager;
            _licenseChecker = licenseChecker;
            _licenseService = licenseService;
            _webBrowser = webBrowser;
        }

        public IDisposable BindCurrentLicense()
        {
            var changeSubscription = _model.WhenAnyValue(m => m.CurrentLicense)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(license =>
                {
                    if (license != null)
                    {
                        _model.LicenseInfo = MakeInfoText(license);
                        
                        _model.IsLicenseValid = CheckLicenseSignature(license);
                        _model.IsLicenseTrial = license.Type == LicenseType.Trial;
                    }
                    else
                    {
                        _model.LicenseInfo = "";
                        
                        _model.IsLicenseValid = true;
                        _model.IsLicenseTrial = false;
                    }
                });

            var existingSubscription = _licenseService.GetCurrentLicenseAsync()
                .ToObservable()
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(license =>
                {
                    _model.IsLicenseSaved = true;
                    _model.CurrentLicense = license;
                });
            
            return new CompositeDisposable(
                existingSubscription,
                changeSubscription);
        }

        public IDisposable BindGetLicenseCommand()
        {
            _model.GetLicenseCommand = ReactiveCommand.Create(
                () => _webBrowser.OpenUrl("https://rbmk.io"), null, RxApp.MainThreadScheduler);

            return _model.GetLicenseCommand
                .SubscribeWithLog();
        }

        public IDisposable BindOpenCommand()
        {
            _model.OpenCommand = ReactiveCommand.CreateFromObservable(
                () => OpenLicense(), null, RxApp.MainThreadScheduler);

            return _model.OpenCommand
                .SubscribeWithLog(tuple =>
                {
                    var (file, license) = tuple;

                    _model.FilePath = file.FullName;
                    _model.IsLicenseSaved = false;
                    _model.CurrentLicense = license;
                });
        }

        public IDisposable BindActivateCommand()
        {
            var canApply = _model.WhenAnyValue(m => m.CurrentLicense)
                .Select(license => license != null
                   && !_model.IsLicenseSaved
                   && CheckLicenseSignature(license)
                   && license.ValidUntilDate > DateTimeOffset.UtcNow);
            
            _model.ActivateCommand = ReactiveCommand.CreateFromObservable(
                () => ApplyLicense(_model.CurrentLicense), canApply, RxApp.MainThreadScheduler);
            
            return _model.ActivateCommand
                .SubscribeWithLog(_ =>
                {
                    _model.Close(true);
                });
        }

        public IDisposable BindTryForFreeCommand()
        {
            var isLicenseNotTrial = _model.WhenAnyValue(m => m.IsLicenseTrial)
                .Select(v => !v);
            
            _model.TryForFreeCommand = ReactiveCommand.CreateFromObservable(
                () => ApplyLicense(License.Trial), isLicenseNotTrial, RxApp.MainThreadScheduler);
            
            return _model.TryForFreeCommand
                .SubscribeWithLog(_ =>
                {
                    _model.Close(true);
                });
        }

        private IObservable<(FileInfo, License)> OpenLicense()
        {
            return _dialogManager.OpenFile()
                .Select(fileName =>
                {
                    var file = new FileInfo(fileName);
                    
                    try
                    {
                        var text = File.ReadAllText(fileName);
                        var license = License.FromBase64(text);

                        return (file, license);
                    }
                    catch
                    {
                        return (file, null);
                    }
                });
        }

        private string MakeInfoText(License license)
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(license.FullName))
            {
                builder.Append($"Licensed to {license.FullName}");
                
                if (!string.IsNullOrWhiteSpace(license.Company))
                {
                    builder.Append($" ({license.Company})");
                }
                
                builder.Append(Environment.NewLine);
                builder.Append(Environment.NewLine);
            }

            if (license.ValidUntilDate != default(DateTimeOffset))
            {
                if (license.ValidUntilDate < DateTimeOffset.UtcNow)
                {
                    builder.Append(license.Type == LicenseType.Trial
                        ? "Free trial period has expired"
                        : $"Subscription has expired on {license.ValidUntilDate:D}");
                }
                else
                {
                    builder.Append($"Subscription is active until {license.ValidUntilDate:D}");
                }
                
                builder.Append(Environment.NewLine);
                builder.Append(Environment.NewLine);
            }
            
            return builder.ToString();
        }

        private bool CheckLicenseSignature(License license)
        {
            if (license.Type != LicenseType.Trial)
            {
                return _licenseChecker.CheckSignature(license);
            }

            return true;
        }

        private IObservable<Unit> ApplyLicense(License license)
        {
            return _licenseService.SetCurrentLicenseAsync(license)
                .ToObservable();
        }
    }
}