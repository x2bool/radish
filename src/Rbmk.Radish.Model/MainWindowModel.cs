using System;
using System.Reactive;
using System.Reactive.Disposables;
using PropertyChanged;
using Rbmk.Radish.Model.ExplorerPad;
using Rbmk.Radish.Model.Notifications;
using Rbmk.Radish.Model.WorkspacePad;
using ReactiveUI;

namespace Rbmk.Radish.Model
{
    [AddINotifyPropertyChangedInterface]
    public class MainWindowModel : IActivatableViewModel
    {
        public bool IsInitialized { get; set; }
        
        public string Title { get; set; } = "123";
        
        public ReactiveCommand<Unit, Unit> ExitCommand { get; set; }
        
        public ExplorerPadModel ExplorerPad { get; set; }
        
        public WorkspacePadModel WorkspacePad { get; set; }
        
        public NotificationModel Notification { get; set; }

        public MainWindowModel()
        {
            var controller = new MainWindowController(this);
            
            this.WhenActivated(disposables =>
            {
                controller
                    .BindExitCommand()
                    .DisposeWith(disposables);
                
                controller
                    .BindInitialization()
                    .DisposeWith(disposables);
                
                controller
                    .BindWindowTitle()
                    .DisposeWith(disposables);

                controller
                    .BindUpdateManager()
                    .DisposeWith(disposables);
            });
        }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}