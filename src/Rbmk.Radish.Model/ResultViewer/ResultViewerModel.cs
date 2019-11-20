using System.Reactive.Disposables;
using DynamicData.Binding;
using PropertyChanged;
using Rbmk.Radish.Model.ResultViewer.Projections;
using Rbmk.Radish.Services.Redis;
using ReactiveUI;

namespace Rbmk.Radish.Model.ResultViewer
{
    [AddINotifyPropertyChangedInterface]
    public class ResultViewerModel : IActivatableViewModel
    {
        public RedisResultInfo[] CurrentResultInfos { get; set; }
        
        public ResultProjectionModel ResultProjection { get; set; }
        
        public ObservableCollectionExtended<ResultProjectionModel> ResultProjections { get; set; }
            = new ObservableCollectionExtended<ResultProjectionModel>();
        
        public ResultViewerModel()
        {
            var controller = new ResultViewerController(this);
            
            this.WhenActivated(disposables =>
            {
                controller.BindResultBroadcasts()
                    .DisposeWith(disposables);

                controller.BindProjections()
                    .DisposeWith(disposables);
            });
        }
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}