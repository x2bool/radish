using Rbmk.Radish.Services.Redis;
using Rbmk.Utils.Broadcasts;

namespace Rbmk.Radish.Services.Broadcasts.Results
{
    public class ViewResultsIntent : Broadcast.Intent
    {   
        public RedisResultInfo[] ResultInfos { get; }

        public ViewResultsIntent(params RedisResultInfo[] resultInfos)
        {
            ResultInfos = resultInfos;
        }
    }
}