using System.Reactive;
using System.Reactive.Disposables;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Radish.Services.Redis;
using ReactiveUI;

namespace Rbmk.Radish.Model.ConnectDialog
{
    public class ConnectDialogModel : DialogModel<ConnectResult>
    {
        public RedisConnectionInfo ConnectionInfo { get; set; }
        
        public bool InProgress { get; set; }
        
        public string ConnectionName { get; set; }

        public string ConnectionString { get; set; }
        
        public ReactiveCommand<Unit, ConnectResult> ConnectCommand { get; set; }
        
        public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }
        
        public bool HasError { get; set; }

        public ConnectDialogModel(RedisConnectionInfo connectionInfo)
            : this()
        {
            ConnectionInfo = connectionInfo;
        }

        public ConnectDialogModel()
        {
            var controller = new ConnectDialogController(this);
            
            this.WhenActivated(disposables =>
            {
                controller
                    .BindConnectionName()
                    .DisposeWith(disposables);

                controller
                    .BindConnectionString()
                    .DisposeWith(disposables);
                
                controller
                    .BindConnectCommand()
                    .DisposeWith(disposables);

                controller
                    .BindCancelCommand()
                    .DisposeWith(disposables);
            });
        }
    }
}