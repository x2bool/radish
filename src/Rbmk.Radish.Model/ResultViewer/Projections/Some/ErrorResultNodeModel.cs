using Rbmk.Radish.Services.Redis.Projections;

namespace Rbmk.Radish.Model.ResultViewer.Projections.Some
{
    public class ErrorResultNodeModel : ResultNodeModel
    {
        public string Error { get; }

        public ErrorResultNodeModel(ResultProjectionInfo projectionInfo)
            : base(projectionInfo)
        {
            Error = AsString(projectionInfo.Value);
        }
    }
}