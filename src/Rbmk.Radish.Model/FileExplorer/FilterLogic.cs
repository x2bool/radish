using System.Linq;

namespace Rbmk.Radish.Model.FileExplorer
{
    public class FilterLogic
    {
        public bool Filter(FileExplorerNodeModel model)
        {
            var name = model.Path.Last();
            
            if (model.IsFile)
            {
                return name.EndsWith(".http");
            }

            if (model.IsDirectory)
            {
                return !name.StartsWith(".");
            }

            return false;
        }
    }
}