using Rbmk.Radish.Services.Redis;

namespace Rbmk.Radish.Model.SelectTargetDialog.Items
{
    public class ServerItemModel
    {
        public string Address { get; set; }
        
        public RedisServerInfo ServerInfo { get; set; }
    }
}