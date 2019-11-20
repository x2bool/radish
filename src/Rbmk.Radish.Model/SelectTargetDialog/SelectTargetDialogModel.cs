using System.Reactive;
using System.Reactive.Disposables;
using DynamicData.Binding;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Model.SelectTargetDialog.Items;
using Rbmk.Radish.Services.Redis;
using ReactiveUI;

namespace Rbmk.Radish.Model.SelectTargetDialog
{
    public class SelectTargetDialogModel : DialogModel<SelectTargetResult>
    {
        public ReactiveCommand<Unit, Unit> SelectCommand { get; set; }
        
        public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }
        
        public ObservableCollectionExtended<ConnectionItemModel> Connections { get; set; }
            = new ObservableCollectionExtended<ConnectionItemModel>();
        
        public ConnectionItemModel SelectedConnection { get; set; }
        
        public RedisConnectionInfo InitialConnectionInfo { get; set; }
        
        public ObservableCollectionExtended<ServerItemModel> Servers { get; set; }
            = new ObservableCollectionExtended<ServerItemModel>();
        
        public ServerItemModel SelectedServer { get; set; }
        
        public RedisServerInfo InitialServerInfo { get; set; }
        
        public ObservableCollectionExtended<DatabaseItemModel> Databases { get; set; }
            = new ObservableCollectionExtended<DatabaseItemModel>();
        
        public DatabaseItemModel SelectedDatabase { get; set; }
        
        public RedisDatabaseInfo InitialDatabaseInfo { get; set; }

        public SelectTargetDialogModel()
        {
            var controller = new SelectTargetDialogController(this);
            
            this.WhenActivated(disposables =>
            {
                controller
                    .BindConnections()
                    .DisposeWith(disposables);

                controller
                    .BindServers()
                    .DisposeWith(disposables);

                controller
                    .BindDatabases()
                    .DisposeWith(disposables);
                
                controller
                    .BindSelect()
                    .DisposeWith(disposables);

                controller
                    .BindCancel()
                    .DisposeWith(disposables);
            });
        }
    }
}