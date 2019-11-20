using Rbmk.Radish.Services.Redis;
using Rbmk.Utils.Broadcasts;

namespace Rbmk.Radish.Services.Broadcasts.Tabs
{
    public class OpenTabIntent : Broadcast.Intent
    {
        public RedisServerInfo ServerInfo { get; }
        
        public RedisDatabaseInfo DatabaseInfo { get; }
        
        public int? Index { get; }
        
        public string CommandText { get; }

        public OpenTabIntent(
            int? index = null)
        {
            Index = index;
        }
        
        public OpenTabIntent(
            RedisServerInfo serverInfo,
            string commandText,
            int? index = null)
        {
            ServerInfo = serverInfo;
            Index = index;
            CommandText = commandText;
        }

        public OpenTabIntent(
            RedisDatabaseInfo databaseInfo,
            string commandText,
            int? index = null)
        {
            DatabaseInfo = databaseInfo;
            Index = index;
            CommandText = commandText;
        }
    }
}