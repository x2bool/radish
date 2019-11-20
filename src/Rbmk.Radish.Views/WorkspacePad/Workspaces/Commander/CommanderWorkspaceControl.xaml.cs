using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.WorkspacePad.Workspaces.Commander;

namespace Rbmk.Radish.Views.WorkspacePad.Workspaces.Commander
{
    public class CommanderWorkspaceControl : BaseControl<CommanderWorkspaceModel>
    {
        public CommanderWorkspaceControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}