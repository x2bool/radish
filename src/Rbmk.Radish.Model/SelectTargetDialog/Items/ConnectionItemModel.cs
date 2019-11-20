using Rbmk.Radish.Services.Redis;

namespace Rbmk.Radish.Model.SelectTargetDialog.Items
{
    public class ConnectionItemModel
    {   
        public string ConnectionName { get; set; }
        
        public string ConnectionString { get; set; }
        
        public RedisConnectionInfo ConnectionInfo { get; set; }
    }
}