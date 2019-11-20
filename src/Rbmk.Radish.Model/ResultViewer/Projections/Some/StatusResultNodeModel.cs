using Rbmk.Radish.Services.Redis.Projections;

namespace Rbmk.Radish.Model.ResultViewer.Projections.Some
{
    public class StatusResultNodeModel : ResultNodeModel
    {
        public string Status { get; }
        
        public StatusResultNodeModel(ResultProjectionInfo projectionInfo)
            : base(projectionInfo)
        {
            Status = AsString(projectionInfo.Value);
        }
    }
}