using System.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Menus
{
    public abstract class MenuItemModel
    {
        public string Name { get; set; }
        
        public ReactiveCommand<MenuItemModel, Unit> SelectCommand { get; set; }
    }
}