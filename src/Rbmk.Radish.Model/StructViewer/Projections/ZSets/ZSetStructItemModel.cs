using System;
using System.Reactive.Disposables;
using PropertyChanged;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Model.StructViewer.Projections.ZSets
{
    [AddINotifyPropertyChangedInterface]
    public class ZSetStructItemModel : IActivatableViewModel
    {
        public bool IsChecked { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public bool IsEditing { get; set; }

        public string Score { get; set; }
        
        public string Value { get; set; }
        
        public Action<ZSetStructItemModel> CheckAction { get; set; }
        
        public Action<ZSetStructItemModel> EditAction { get; set; }

        public ZSetStructItemModel()
        {   
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(item => item.IsChecked)
                    .SubscribeWithLog(_ => { CheckAction?.Invoke(this); })
                    .DisposeWith(disposables);
            });
        }
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}