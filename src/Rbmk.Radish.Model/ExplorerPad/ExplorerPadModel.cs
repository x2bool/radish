using System.Reactive;
using System.Reactive.Disposables;
using DynamicData.Binding;
using PropertyChanged;
using Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer;
using ReactiveUI;

namespace Rbmk.Radish.Model.ExplorerPad
{
    [AddINotifyPropertyChangedInterface]
    public class ExplorerPadModel : IActivatableViewModel
    {
        public ConnectionExplorerModel ConnectionExplorer { get; set; }
            = new ConnectionExplorerModel();
        
        public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; set; }

        public ExplorerPadModel()
        {
            var controller = new ExplorerPadController(this);
            
            this.WhenActivated(disposables =>
            {
                controller.BindOpenSettingsCommand()
                    .DisposeWith(disposables);
            });
        }
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}