using DynamicData.Binding;
using Rbmk.Radish.Services.Redis.Projections;

namespace Rbmk.Radish.Model.ResultViewer.Projections.Some
{
    public class TransactionResultNodeModel : ResultNodeModel
    {
        public TransactionResultNodeModel(ResultProjectionInfo projectionInfo)
            : base(projectionInfo)
        {
            
        }

        public ObservableCollectionExtended<ResultNodeModel> Results { get; set; }
            = new ObservableCollectionExtended<ResultNodeModel>();
    }
}