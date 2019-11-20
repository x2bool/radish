using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ExplorerPad;

namespace Rbmk.Radish.Views.ExplorerPad
{
    public class ExplorerPadControl : BaseControl<ExplorerPadModel>
    {
        public ExplorerPadControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}