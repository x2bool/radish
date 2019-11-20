using Rbmk.Radish.Model.CommandEditor;
using Rbmk.Radish.Model.ResultViewer;
using Rbmk.Radish.Model.StructViewer;

namespace Rbmk.Radish.Model.WorkspacePad.Workspaces.Commander
{
    public class CommanderWorkspaceModel : WorkspaceModel
    {   
        public CommandEditorModel CommandEditor { get; set; }
            = new CommandEditorModel();
        
        public ResultViewerModel ResultViewer { get; set; }
            = new ResultViewerModel();
        
        public StructViewerModel StructViewer { get; set; }
            = new StructViewerModel();
    }
}