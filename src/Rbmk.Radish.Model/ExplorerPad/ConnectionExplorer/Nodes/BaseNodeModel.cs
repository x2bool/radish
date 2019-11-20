using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged;
using ReactiveUI;
using Rbmk.Utils.Reactive;

namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes
{
    [AddINotifyPropertyChangedInterface]
    public abstract class BaseNodeModel : IActivatableViewModel
    {
        public bool IsExpanded { get; set; }
        
        public ObservableCollectionExtended<BaseNodeModel> Items { get; }
            = new ObservableCollectionExtended<BaseNodeModel>();
        
        public SourceList<BaseNodeModel> Nodes { get; }
            = new SourceList<BaseNodeModel>();
        
        public BaseNodeModel()
        {
            this.WhenActivated(disposables =>
            {
                BindNodes()
                    .DisposeWith(disposables);
            });
        }

        private IDisposable BindNodes()
        {
            return Nodes.Connect()
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(Items)
                .SubscribeWithLog();
        }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}