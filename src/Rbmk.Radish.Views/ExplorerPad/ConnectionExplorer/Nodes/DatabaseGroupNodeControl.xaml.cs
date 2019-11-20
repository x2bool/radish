using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer;
using Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes;

namespace Rbmk.Radish.Views.ExplorerPad.ConnectionExplorer.Nodes
{
    public class DatabaseGroupNodeControl : BaseControl<DatabaseGroupNodeModel>
    {
        public DatabaseGroupNodeControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}