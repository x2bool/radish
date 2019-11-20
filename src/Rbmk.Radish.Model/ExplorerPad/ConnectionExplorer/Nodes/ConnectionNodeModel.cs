using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Avalonia.Interactivity;
using Rbmk.Radish.Services.Redis;
using ReactiveUI;

namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes
{
    public class ConnectionNodeModel : BaseNodeModel
    {
        public bool IsOpen { get; set; }
        
        public string ConnectionName { get; set; }
        
        public string ConnectionString { get; set; }

        public string ConnectionDisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ConnectionName))
                {
                    return $"{ConnectionName} ({ConnectionString})";
                }

                return ConnectionString;
            }
        }
        
        public ReactiveCommand<Unit, RedisConnectionInfo> ReconnectCommand { get; set; }
        
        public ReactiveCommand<Unit, RedisConnectionInfo> DisconnectCommand { get; set; }
        
        public ReactiveCommand<Unit, RedisConnectionInfo> CloseCommand { get; set; }
        
        public RedisConnectionInfo ConnectionInfo { get; set; }

        public ConnectionNodeModel()
        {
            var controller = new ConnectionNodeController(this);
            
            this.WhenActivated(disposables =>
            {
                controller
                    .BindReconnectCommand()
                    .DisposeWith(disposables);
                
                controller
                    .BindDisconnectCommand()
                    .DisposeWith(disposables);

                controller
                    .BindCloseCommand()
                    .DisposeWith(disposables);
            });
        }
    }
}