using Avalonia.Markup.Xaml;
using Rbmk.Radish.Model.WorkspacePad.Workspaces;

namespace Rbmk.Radish.Views.WorkspacePad.Workspaces
{
    public class WorkspaceTabControl : BaseControl<WorkspaceTabModel>
    {
        public WorkspaceTabControl()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}