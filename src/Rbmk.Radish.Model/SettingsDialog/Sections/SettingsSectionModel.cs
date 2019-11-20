using PropertyChanged;
using ReactiveUI;

namespace Rbmk.Radish.Model.SettingsDialog.Sections
{
    [AddINotifyPropertyChangedInterface]
    public abstract class SettingsSectionModel : IActivatableViewModel
    {
        public string Name { get; set; }

        public SettingsSectionModel(string name)
        {
            Name = name;
        }
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}