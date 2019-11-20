using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged;
using Rbmk.Radish.Model.WorkspacePad.Workspaces;
using ReactiveUI;
using Rbmk.Utils.Reactive;

namespace Rbmk.Radish.Model.WorkspacePad
{
    [AddINotifyPropertyChangedInterface]
    public class WorkspacePadModel : IActivatableViewModel
    {   
        public string Title { get; set; }

        public ReactiveCommand<Unit, Unit> OpenCommand { get; set; }
        
        public ReactiveCommand<WorkspaceModel, WorkspaceModel> CloseCommand { get; set; }

        public ObservableCollectionExtended<WorkspaceModel> Workspaces { get; set; }
            = new ObservableCollectionExtended<WorkspaceModel>();
        
        public WorkspaceModel SelectedWorkspace { get; set; }

        public bool IsTabStripScrollable { get; set; }
        
        public ReactiveCommand<Unit, Unit> ScrollLeftCommand { get; set; }
        
        public ReactiveCommand<Unit, Unit> ScrollRightCommand { get; set; }

        public WorkspacePadModel()
        {
            var controller = new WorkspacePadController(this);
            
            this.WhenActivated(disposables =>
            {
                controller.BindOpenCommand()
                    .DisposeWith(disposables);

                controller.BindCloseCommand()
                    .DisposeWith(disposables);
                
                controller.BindOpenIntents()
                    .DisposeWith(disposables);

                controller.BindCloseIntents()
                    .DisposeWith(disposables);

                controller.BindWorkspaceTitle()
                    .DisposeWith(disposables);
                
                controller.OpenDefaultWorkspace()
                    .DisposeWith(disposables);

                controller.BindTabTitle()
                    .DisposeWith(disposables);

                controller.BindScrolling()
                    .DisposeWith(disposables);
            });
        }
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}