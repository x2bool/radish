using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged;
using Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes;
using ReactiveUI;

namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer
{
    [AddINotifyPropertyChangedInterface]
    public class ConnectionExplorerModel : IActivatableViewModel
    {
        public ObservableCollectionExtended<BaseNodeModel> Items { get; }
            = new ObservableCollectionExtended<BaseNodeModel>();
        
        public SourceList<BaseNodeModel> Nodes { get; }
            = new SourceList<BaseNodeModel>();
        
        public BaseNodeModel SelectedItem { get; set; }
        
        public ConnectionExplorerModel()
        {   
            var controller = new ConnectionExplorerController(this);
            
            this.WhenActivated(disposables =>
            {   
                controller
                    .BindItems()
                    .DisposeWith(disposables);

                controller
                    .BindSelections()
                    .DisposeWith(disposables);

                controller
                    .BindConnectBroadcasts()
                    .DisposeWith(disposables);
                
                controller
                    .BindCloseBroadcasts()
                    .DisposeWith(disposables);

                controller
                    .BindRestoreBroadcasts()
                    .DisposeWith(disposables);
                
                controller
                    .BindDisconnectBroadcasts()
                    .DisposeWith(disposables);

                controller
                    .BindReconnectBroadcasts()
                    .DisposeWith(disposables);

                controller
                    .RestoreConnections()
                    .DisposeWith(disposables);
            });
        }
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}