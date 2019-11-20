using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.Notifications;
using Splat;

namespace Rbmk.Radish.Views.Notifications
{
    public class NotificationControl : BaseControl<NotificationModel>
    {
        public NotificationControl()
        {
            AvaloniaXamlLoader.Load(this);
            
            Locator.CurrentMutable.RegisterConstant(this);
        }
    }
}