namespace Rbmk.Radish.Model.Notifications
{
    public interface INotificationManager
    {
        void Show<TContext>(TContext context)
            where TContext : NotificationContextModel;
    }
}