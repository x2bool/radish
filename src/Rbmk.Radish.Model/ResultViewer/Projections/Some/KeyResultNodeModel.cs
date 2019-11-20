using System;
using Rbmk.Radish.Services.Redis.Projections;

namespace Rbmk.Radish.Model.ResultViewer.Projections.Some
{
    public class KeyResultNodeModel : ResultNodeModel
    {
        public string Key { get; }
        
        public KeyResultNodeModel(ResultProjectionInfo projectionInfo)
            : base(projectionInfo)
        {
            Key = AsString(projectionInfo.Value, true);
        }
    }
}