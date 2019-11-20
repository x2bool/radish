using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.FileExplorer;

namespace Rbmk.Radish.Views.FileExplorer
{
    public class FileExplorerControl : BaseControl<FileExplorerModel>
    {
        public FileExplorerControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}