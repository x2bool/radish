using System.Collections.Generic;
using System.Reactive.Disposables;
using DynamicData.Binding;
using Rbmk.Radish.Services.Redis.Projections;
using ReactiveUI;

namespace Rbmk.Radish.Model.ResultViewer.Projections.Some
{
    public class SomeResultProjectionModel : ResultProjectionModel
    {
        public ObservableCollectionExtended<ResultNodeModel> Nodes { get; set; }
            = new ObservableCollectionExtended<ResultNodeModel>();
        
        public ResultNodeModel SelectedNode { get; set; }
        
        public SomeResultProjectionModel(
            List<ResultProjectionInfo> projectInfos)
            : this()
        {
            var controller = new SomeResultProjectionController(this);
            
            this.WhenActivated(disposables =>
            {
                controller.BindNodes(projectInfos)
                    .DisposeWith(disposables);

                controller.BindSelection()
                    .DisposeWith(disposables);
            });
        }

        private SomeResultProjectionModel()
        {
            Name = "Values";
        }
    }
}