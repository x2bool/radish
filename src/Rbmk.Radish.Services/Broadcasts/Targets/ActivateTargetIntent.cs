using Rbmk.Radish.Services.Redis;
using Rbmk.Utils.Broadcasts;

namespace Rbmk.Radish.Services.Broadcasts.Targets
{
    public class ActivateTargetIntent : Broadcast.Intent
    {
        public RedisServerInfo ServerInfo { get; }
        
        public RedisDatabaseInfo DatabaseInfo { get; }
        
        public ActivateTargetIntent(
            RedisServerInfo serverInfo)
        {
            ServerInfo = serverInfo;
        }

        public ActivateTargetIntent(
            RedisDatabaseInfo databaseInfo)
        {
            DatabaseInfo = databaseInfo;
        }
    }
}