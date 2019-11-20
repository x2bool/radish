using Rbmk.Radish.Services.Redis;

namespace Rbmk.Radish.Model.SelectTargetDialog
{
    public abstract class SelectTargetResult
    {
        public class Selected : SelectTargetResult
        {
            public RedisConnectionInfo ConnectionInfo { get; }
            
            public RedisServerInfo ServerInfo { get; }
            
            public RedisDatabaseInfo DatabaseInfo { get; }

            public Selected(
                RedisConnectionInfo connectionInfo,
                RedisServerInfo serverInfo,
                RedisDatabaseInfo databaseInfo)
            {
                ConnectionInfo = connectionInfo;
                ServerInfo = serverInfo;
                DatabaseInfo = databaseInfo;
            }
        }
        
        public class NoConnectionsAvailable : SelectTargetResult
        {
        }
        
        public class Cancelled : SelectTargetResult
        {
        }
    }
}