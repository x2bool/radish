using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using DynamicData.Binding;
using Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Menus;
using Rbmk.Radish.Services.Redis;
using ReactiveUI;

namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes
{
    public class ServerNodeModel : BaseNodeModel
    {
        public string Address { get; set; }
        
        public RedisServerInfo ServerInfo { get; set; }

        public ObservableCollectionExtended<MenuItemModel> MenuItems { get; set; }
            = new ObservableCollectionExtended<MenuItemModel>();
        
        public ReactiveCommand<Unit, Unit> ActivateCommand { get; set; }

        public bool IsAvailable { get; set; } = true;

        public ServerNodeModel()
        {
            var controller = new ServerNodeController(this);
            
            this.WhenActivated(disposables =>
            {
                controller.BindActivateCommand()
                    .DisposeWith(disposables);
                
                controller.BindCommands()
                    .DisposeWith(disposables);
            });
        }
    }
}