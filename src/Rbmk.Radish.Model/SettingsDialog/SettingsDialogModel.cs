using System.Reactive;
using System.Reactive.Disposables;
using DynamicData.Binding;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Model.SettingsDialog.Sections;
using ReactiveUI;

namespace Rbmk.Radish.Model.SettingsDialog
{
    public class SettingsDialogModel : DialogModel<Unit>
    {
        public ObservableCollectionExtended<SettingsSectionModel> Sections { get; set; }
            = new ObservableCollectionExtended<SettingsSectionModel>();
        
        public SettingsSectionModel SelectedSection { get; set; }
        
        public ReactiveCommand<Unit, Unit> InfoCommand { get; set; }
        
        public ReactiveCommand<Unit, Unit> CloseCommand { get; set; }

        public SettingsDialogModel()
        {
            var controller = new SettingsDialogController(this);
            
            this.WhenActivated(disposables =>
            {
                controller.BindSections()
                    .DisposeWith(disposables);

                controller.BindInfoCommand()
                    .DisposeWith(disposables);

                controller.BindCloseCommand()
                    .DisposeWith(disposables);
            });
        }
    }
}