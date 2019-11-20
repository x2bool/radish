using PropertyChanged;
using ReactiveUI;

namespace Rbmk.Radish.Model.StructViewer.Projections
{
    [AddINotifyPropertyChangedInterface]
    public class StructProjectionModel : IActivatableViewModel
    {
        public string BadgeText { get; set; }
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}