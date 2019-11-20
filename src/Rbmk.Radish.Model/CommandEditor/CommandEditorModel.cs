using System.Reactive;
using System.Reactive.Disposables;
using DynamicData.Binding;
using PropertyChanged;
using Rbmk.Radish.Model.CommandEditor.Targets;
using Rbmk.Radish.Model.Highlighting;
using Rbmk.Radish.Model.SelectTargetDialog;
using Rbmk.Radish.Services.Redis;
using ReactiveUI;

namespace Rbmk.Radish.Model.CommandEditor
{
    [AddINotifyPropertyChangedInterface]
    public class CommandEditorModel : IActivatableViewModel
    {
        public HighlightingSyntax Highlighting { get; set; } = HighlightingSyntax.Redis;
        
        public bool IsQuickAccessEnabled { get; set; }
        
        public string QuickAccessText { get; set; }
        
        public ObservableCollectionExtended<string> QuickAccessItems { get; set; }
            = new ObservableCollectionExtended<string>();
        
        public string SelectedQuickAccessItem { get; set; }
        
        public string CommandText { get; set; }
        
        public ReactiveCommand<string, RedisResultInfo[]> ExecuteCommand { get; set; }
        
        public ReactiveCommand<string, RedisResultInfo[]> ExploreCommand { get; set; }
        
        public ReactiveCommand<Unit, SelectTargetResult> SelectTargetCommand { get; set; }
        
        public RedisConnectionInfo SelectedConnectionInfo { get; set; }
        
        public ConnectionTargetModel SelectedConnectionTarget { get; set; }
        
        public RedisServerInfo SelectedServerInfo { get; set; }
        
        public ServerTargetModel SelectedServerTarget { get; set; }
        
        public RedisDatabaseInfo SelectedDatabaseInfo { get; set; }
        
        public DatabaseTargetModel SelectedDatabaseTarget { get; set; }
        
        public bool IsConnectionTargetSelected { get; set; }
        
        public bool IsServerTargetSelected { get; set; }
        
        public bool IsDatabaseTargetSelected { get; set; }
        
        public CommandEditorModel()
        {
            this.WhenActivated(disposables =>
            {
                var controller = new CommandEditorController(this);
                
                controller.BindSelectedConnection()
                    .DisposeWith(disposables);

                controller.BindSelectedServer()
                    .DisposeWith(disposables);

                controller.BindSelectedDatabase()
                    .DisposeWith(disposables);
                
                controller.BindTargetSelection()
                    .DisposeWith(disposables);

                controller.BindFlags()
                    .DisposeWith(disposables);

                controller.BindExecuteCommand()
                    .DisposeWith(disposables);

                controller.BindDisconnects()
                    .DisposeWith(disposables);

                controller.BindTargetActivation()
                    .DisposeWith(disposables);

                controller.BindQuickAccess()
                    .DisposeWith(disposables);

                controller.BindQuickAccessItems()
                    .DisposeWith(disposables);

                controller.BindExploreCommand()
                    .DisposeWith(disposables);

                controller.BindSelectedQuickAccessItem()
                    .DisposeWith(disposables);
            });
        }
        
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}