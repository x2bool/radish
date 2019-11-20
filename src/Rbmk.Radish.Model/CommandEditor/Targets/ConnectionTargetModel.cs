using Rbmk.Radish.Services.Redis;

namespace Rbmk.Radish.Model.CommandEditor.Targets
{
    public class ConnectionTargetModel
    {
        public string ConnectionName { get; set; }
        
        public string ConnectionString { get; set; }
        
        public RedisConnectionInfo ConnectionInfo { get; set; }
    }
}