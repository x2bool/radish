using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Utils.Licenses;
using ReactiveUI;

namespace Rbmk.Radish.Model.LicenseDialog
{
    public class LicenseDialogModel : DialogModel<bool>
    {
        public ReactiveCommand<Unit, Unit> GetLicenseCommand { get; set; }
        
        public ReactiveCommand<Unit, (FileInfo, License)> OpenCommand { get; set; }
        
        public ReactiveCommand<Unit, Unit> TryForFreeCommand { get; set; }
        
        public ReactiveCommand<Unit, Unit> ActivateCommand { get; set; }
        
        public License CurrentLicense { get; set; }
        
        public bool IsLicenseValid { get; set; }
        
        public bool IsLicenseTrial { get; set; }
        
        public bool IsLicenseSaved { get; set; }
        
        public string FilePath { get; set; }
        
        public string LicenseInfo { get; set; }

        public LicenseDialogModel()
        {
            var controller = new LicenseDialogController(this);
            
            this.WhenActivated(disposables =>
            {
                controller.BindCurrentLicense()
                    .DisposeWith(disposables);

                controller.BindGetLicenseCommand()
                    .DisposeWith(disposables);
                
                controller.BindOpenCommand()
                    .DisposeWith(disposables);
                
                controller.BindTryForFreeCommand()
                    .DisposeWith(disposables);
                
                controller.BindActivateCommand()
                    .DisposeWith(disposables);
            });
        }
    }
}