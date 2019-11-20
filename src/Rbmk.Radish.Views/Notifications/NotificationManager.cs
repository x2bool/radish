using System;
using Rbmk.Radish.Model.Notifications;
using Rbmk.Utils.Reactive;
using Splat;

namespace Rbmk.Radish.Views.Notifications
{
    public class NotificationManager : INotificationManager
    {
        private IDisposable _okCommandSubscription;
        private IDisposable _cancelCommandSubscription;
        private IDisposable _defaultCommandSubscription;
        
        public void Show<TContext>(TContext context)
            where TContext : NotificationContextModel
        {
            var control = Locator.Current.GetService<NotificationControl>();

            if (control?.ViewModel != null)
            {
                control.ViewModel.NotificationContext = context;
                control.ViewModel.IsVisible = true;
            }
            
            _okCommandSubscription?.Dispose();
            _okCommandSubscription = context.OkCommand
                .SubscribeWithLog(_ => { ReleaseSubscriptions(control); });

            _cancelCommandSubscription?.Dispose();
            _cancelCommandSubscription = context.CancelCommand
                .SubscribeWithLog(_ => { ReleaseSubscriptions(control); });
            
            _defaultCommandSubscription?.Dispose();
            _defaultCommandSubscription = context.DefaultCommand
                .SubscribeWithLog(_ => { ReleaseSubscriptions(control); });
        }

        private void ReleaseSubscriptions(NotificationControl control)
        {
            _okCommandSubscription?.Dispose();
            _okCommandSubscription = null;

            _cancelCommandSubscription?.Dispose();
            _cancelCommandSubscription = null;

            _defaultCommandSubscription?.Dispose();
            _defaultCommandSubscription = null;

            if (control?.ViewModel != null)
            {
                control.ViewModel.NotificationContext = null;
                control.ViewModel.IsVisible = false;
            }
        }
    }
}