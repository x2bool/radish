using System.Collections.Generic;

namespace Rbmk.Radish.Services.Redis.Projections
{
    public interface IResultProjector
    {
        IEnumerable<ResultProjectionInfo> Project(params RedisResultInfo[] resultInfo);
    }
}