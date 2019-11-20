using NStack;
using Rbmk.Radish.Services.Redis.Projections;

namespace Rbmk.Radish.Model.ResultViewer.Projections.Some
{
    public class ValueResultNodeModel : ResultNodeModel
    {
        public string Value { get; }

        public bool IsNull => Value == null;
        
        public ValueResultNodeModel(ResultProjectionInfo projectionInfo)
            : base(projectionInfo)
        {
            Value = AsString(projectionInfo.Value, true);
        }
    }
}