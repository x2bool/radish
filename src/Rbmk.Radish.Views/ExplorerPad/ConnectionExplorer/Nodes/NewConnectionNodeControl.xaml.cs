using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer;
using Rbmk.Radish.Model.ExplorerPad.ConnectionExplorer.Nodes;

namespace Rbmk.Radish.Views.ExplorerPad.ConnectionExplorer.Nodes
{
    public class NewConnectionNodeControl : BaseControl<NewConnectionNodeModel>
    {
        public NewConnectionNodeControl()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void OnTapped(object sender, RoutedEventArgs args)
        {
            ViewModel?.OnTapped();

            args.Handled = true;
        }
    }
}