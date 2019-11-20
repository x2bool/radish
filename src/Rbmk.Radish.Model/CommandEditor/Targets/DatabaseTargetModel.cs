using Rbmk.Radish.Services.Redis;

namespace Rbmk.Radish.Model.CommandEditor.Targets
{
    public class DatabaseTargetModel
    {
        public string Number { get; set; }
        
        public RedisDatabaseInfo DatabaseInfo { get; set; }
    }
}