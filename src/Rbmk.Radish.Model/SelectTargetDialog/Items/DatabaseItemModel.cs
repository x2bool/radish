using Rbmk.Radish.Services.Redis;

namespace Rbmk.Radish.Model.SelectTargetDialog.Items
{
    public class DatabaseItemModel
    {
        public int Number { get; set; }
        
        public RedisDatabaseInfo DatabaseInfo { get; set; }
    }
}