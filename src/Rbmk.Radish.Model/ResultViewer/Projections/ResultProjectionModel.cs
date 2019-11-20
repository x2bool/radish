using PropertyChanged;
using ReactiveUI;

namespace Rbmk.Radish.Model.ResultViewer.Projections
{
    [AddINotifyPropertyChangedInterface]
    public abstract class ResultProjectionModel : IActivatableViewModel
    {
        public string Name { get; set; }
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}