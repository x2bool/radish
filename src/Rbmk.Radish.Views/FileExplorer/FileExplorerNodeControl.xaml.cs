using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.FileExplorer;

namespace Rbmk.Radish.Views.FileExplorer
{
    public class FileExplorerNodeControl : BaseControl<FileExplorerNodeModel>
    {
        public FileExplorerNodeControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}