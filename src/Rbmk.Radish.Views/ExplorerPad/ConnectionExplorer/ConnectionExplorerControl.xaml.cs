using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer;

namespace Rbmk.Radish.Views.ExplorerPad.ConnectionExplorer
{
    public class ConnectionExplorerControl
        : BaseControl<ConnectionExplorerModel>
    {
        public ConnectionExplorerControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}