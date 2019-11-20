using System;
using System.Reactive.Disposables;
using PropertyChanged;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Model.StructViewer.Projections.Hashes
{
    [AddINotifyPropertyChangedInterface]
    public class HashStructItemModel : IActivatableViewModel
    {
        public bool IsChecked { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public bool IsEditing { get; set; }

        public string Key { get; set; }
        
        public string Value { get; set; }
        
        public Action<HashStructItemModel> CheckAction { get; set; }
        
        public Action<HashStructItemModel> EditAction { get; set; }

        public HashStructItemModel()
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