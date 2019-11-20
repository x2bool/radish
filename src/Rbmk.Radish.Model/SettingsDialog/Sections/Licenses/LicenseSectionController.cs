using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Model.LicenseDialog;
using Rbmk.Radish.Services.Licenses;
using Rbmk.Utils.Licenses;
using Rbmk.Utils.Reactive;
using ReactiveUI;
using Splat;

namespace Rbmk.Radish.Model.SettingsDialog.Sections.Licenses
{
    public class LicenseSectionController
    {
        private readonly LicenseSectionModel _model;
        private readonly ILicenseService _licenseService;
        private readonly IDialogManager _dialogManager;

        public LicenseSectionController(
            LicenseSectionModel model)
            : this(
                Locator.Current.GetService<ILicenseService>(),
                Locator.Current.GetService<IDialogManager>())
        {
            _model = model;
        }

        private LicenseSectionController(
            ILicenseService licenseService,
            IDialogManager dialogManager)
        {
            _licenseService = licenseService;
            _dialogManager = dialogManager;
        }

        public IDisposable BindLicenseText()
        {
            return LoadLicense()
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(ShowLicense);
        }

        public IDisposable BindChangeLicenseCommand()
        {
            _model.ChangeLicenseCommand = ReactiveCommand.CreateFromObservable<Unit, License>(
                _ => OpenDialog().SelectMany(wasSelected => LoadLicense()), null, RxApp.MainThreadScheduler);

            return _model.ChangeLicenseCommand
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeWithLog(ShowLicense);
        }

        private IObservable<bool> OpenDialog()
        {
            return _dialogManager.Open(new LicenseDialogModel());
        }

        private IObservable<License> LoadLicense()
        {
            return _licenseService.GetCurrentLicenseAsync()
                .ToObservable();
        }

        private void ShowLicense(License license)
        {
            switch (license.Type)
            {
                case LicenseType.Trial:
                    _model.HasTrialLicense = true;
                    _model.HasLicense = false;
                    _model.HasNoLicense = false;
                    _model.LicenseText = $"Trial license until {license.ValidUntilDate:D}";
                    break;
                        
                case LicenseType.Alpha:
                case LicenseType.Beta:
                    _model.HasTrialLicense = false;
                    _model.HasLicense = true;
                    _model.HasNoLicense = false;
                    _model.LicenseText = $"Subscription is active until {license.ValidUntilDate:D}";
                    break;
                        
                default:
                    _model.HasTrialLicense = false;
                    _model.HasLicense = false;
                    _model.HasNoLicense = true;
                    _model.LicenseText = "Subscription is not active";
                    break;
            }
        }
    }
}