using DynamicData.Binding;
using Rbmk.Radish.Services.Redis.Projections;

namespace Rbmk.Radish.Model.ResultViewer.Projections.Some
{
    public class ArrayResultNodeModel : ResultNodeModel
    {
        public int Length { get; set; }

        public string FormattedLength => $"({Length})";
        
        public ArrayResultNodeModel(ResultProjectionInfo projectionInfo)
            : base(projectionInfo)
        {
            Length = projectionInfo.Children.Count;
        }

        public ObservableCollectionExtended<ResultNodeModel> Results { get; set; }
            = new ObservableCollectionExtended<ResultNodeModel>();
    }
}