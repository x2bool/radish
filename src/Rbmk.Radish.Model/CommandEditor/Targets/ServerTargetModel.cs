using Rbmk.Radish.Services.Redis;

namespace Rbmk.Radish.Model.CommandEditor.Targets
{
    public class ServerTargetModel
    {
        public string Address { get; set; }
        
        public RedisServerInfo ServerInfo { get; set; }
    }
}