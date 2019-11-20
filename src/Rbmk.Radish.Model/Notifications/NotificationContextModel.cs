using System.Reactive;
using Avalonia.Media.Imaging;
using PropertyChanged;
using ReactiveUI;

namespace Rbmk.Radish.Model.Notifications
{
    [AddINotifyPropertyChangedInterface]
    public class NotificationContextModel
    {
        public Bitmap Icon { get; set; }
        
        public string Title { get; set; }
        
        public string Text { get; set; }
        
        public ReactiveCommand<Unit, Unit> OkCommand { get; set; }
        
        public string OkText { get; set; }
        
        public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }
        
        public string CancelText { get; set; }
        
        public ReactiveCommand<Unit, Unit> DefaultCommand { get; set; }
    }
}