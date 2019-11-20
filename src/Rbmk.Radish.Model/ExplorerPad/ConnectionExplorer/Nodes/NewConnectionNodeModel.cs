using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using Rbmk.Radish.Services.Redis;
using ReactiveUI;

namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes
{
    public class NewConnectionNodeModel : BaseNodeModel
    {
        public ReactiveCommand<Unit, RedisConnectionInfo> ConnectCommand { get; set; }

        public NewConnectionNodeModel()
        {
            var controller = new NewConnectionNodeController(this);
            
            this.WhenActivated(disposables =>
            {
                controller
                    .BindConnectCommand()
                    .DisposeWith(disposables);
            });
        }
        
        public async void OnTapped()
        {
            await ConnectCommand.Execute().ToTask();
        }
    }
}