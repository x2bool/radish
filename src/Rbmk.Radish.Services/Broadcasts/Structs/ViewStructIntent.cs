using Rbmk.Radish.Services.Redis;
using Rbmk.Radish.Services.Redis.Projections;
using Rbmk.Utils.Broadcasts;

namespace Rbmk.Radish.Services.Broadcasts.Structs
{
    public class ViewStructIntent : Broadcast.Intent
    {
        public ResultProjectionInfo ResultProjectionInfo { get; }

        public ViewStructIntent(ResultProjectionInfo resultProjectionInfo)
        {
            ResultProjectionInfo = resultProjectionInfo;
        }
    }
}