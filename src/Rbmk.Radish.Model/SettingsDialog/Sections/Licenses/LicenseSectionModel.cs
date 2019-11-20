using System.Reactive;
using System.Reactive.Disposables;
using ReactiveUI;
using Rbmk.Utils.Licenses;

namespace Rbmk.Radish.Model.SettingsDialog.Sections.Licenses
{
    public class LicenseSectionModel : SettingsSectionModel
    {
        public bool HasNoLicense { get; set; }
        public bool HasTrialLicense { get; set; }
        public bool HasLicense { get; set; }
        
        public string LicenseText { get; set; }
        
        public ReactiveCommand<Unit, License> ChangeLicenseCommand { get; set; }
        
        public LicenseSectionModel()
            : base("License")
        {
            var controller = new LicenseSectionController(this);
            
            this.WhenActivated(disposables =>
            {
                controller
                    .BindChangeLicenseCommand()
                    .DisposeWith(disposables);
                
                controller
                    .BindLicenseText()
                    .DisposeWith(disposables);
            });
        }
    }
}