using System.Reactive.Disposables;
using DynamicData.Binding;
using PropertyChanged;
using Rbmk.Radish.Model.StructViewer.Projections;
using Rbmk.Radish.Services.Redis.Projections;
using ReactiveUI;

namespace Rbmk.Radish.Model.StructViewer
{
    [AddINotifyPropertyChangedInterface]
    public class StructViewerModel : IActivatableViewModel
    {   
        public ResultProjectionInfo ResultProjectionInfo { get; set; }
        
        public ObservableCollectionExtended<StructProjectionModel> StructProjections { get; set; }
            = new ObservableCollectionExtended<StructProjectionModel>();
        
        public StructProjectionModel SelectedStructProjection { get; set; }

        public StructViewerModel()
        {
            var controller = new StructViewerController(this);
            
            this.WhenActivated(disposables =>
            {
                controller.BindStructBroadcasts()
                    .DisposeWith(disposables);
                
                controller.BindProjections()
                    .DisposeWith(disposables);
            });
        }
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}