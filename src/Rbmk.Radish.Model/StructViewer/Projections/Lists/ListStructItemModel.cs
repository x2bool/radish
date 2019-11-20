using System;
using System.Reactive.Disposables;
using PropertyChanged;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Model.StructViewer.Projections.Lists
{
    [AddINotifyPropertyChangedInterface]
    public class ListStructItemModel : IActivatableViewModel
    {
        public bool IsChecked { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public bool IsEditing { get; set; }
        
        public int Index { get; set; }
        
        public string Value { get; set; }
        
        public Action<ListStructItemModel> CheckAction { get; set; }
        
        public Action<ListStructItemModel> EditAction { get; set; }

        public ListStructItemModel()
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