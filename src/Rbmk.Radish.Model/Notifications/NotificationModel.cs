using PropertyChanged;

namespace Rbmk.Radish.Model.Notifications
{
    [AddINotifyPropertyChangedInterface]
    public class NotificationModel
    {
        public bool IsVisible { get; set; }
        
        public NotificationContextModel NotificationContext { get; set; }
    }
}