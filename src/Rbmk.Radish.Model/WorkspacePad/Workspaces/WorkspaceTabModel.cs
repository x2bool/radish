using System.Reactive;
using PropertyChanged;
using ReactiveUI;

namespace Rbmk.Radish.Model.WorkspacePad.Workspaces
{
    [AddINotifyPropertyChangedInterface]
    public class WorkspaceTabModel : IActivatableViewModel
    {
        public string Title { get; set; }
        
        public ReactiveCommand<WorkspaceModel, WorkspaceModel> CloseCommand { get; set; }
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}