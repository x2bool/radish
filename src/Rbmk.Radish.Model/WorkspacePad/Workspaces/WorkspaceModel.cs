using PropertyChanged;
using ReactiveUI;

namespace Rbmk.Radish.Model.WorkspacePad.Workspaces
{
    [AddINotifyPropertyChangedInterface]
    public class WorkspaceModel : IActivatableViewModel
    {
        public WorkspaceTabModel Tab { get; set; }
            = new WorkspaceTabModel();
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}