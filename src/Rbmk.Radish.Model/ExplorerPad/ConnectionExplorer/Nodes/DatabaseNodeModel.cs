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
    public class DatabaseNodeModel : BaseNodeModel
    {
        public int Number { get; set; }
        
        public RedisDatabaseInfo DatabaseInfo { get; set; }

        public ObservableCollectionExtended<MenuItemModel> MenuItems { get; set; }
            = new ObservableCollectionExtended<MenuItemModel>();
        
        public ReactiveCommand<Unit, Unit> ActivateCommand { get; set; }
        
        public DatabaseNodeModel()
        {
            var controller = new DatabaseNodeController(this);
            
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