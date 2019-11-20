using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer;
using Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes;

namespace Rbmk.Radish.Views.ExplorerPad.ConnectionExplorer.Nodes
{
    public class ServerGroupNodeControl : BaseControl<ServerGroupNodeModel>
    {
        public ServerGroupNodeControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}