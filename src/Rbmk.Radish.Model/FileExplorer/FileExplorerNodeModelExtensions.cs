using System.IO;
using System.Linq;

namespace Rbmk.Radish.Model.FileExplorer
{
    public static class FileExplorerNodeModelExtensions
    {
        public static string GetFullPath(
            this FileExplorerNodeModel model)
        {
            var separator = Path.DirectorySeparatorChar.ToString();
            if (separator == "/")
            {
                return "/" + string.Join(separator, model.Path);
            }
            return string.Join(separator, model.Path);
        }

        public static string GetFilename(
            this FileExplorerNodeModel model)
        {
            return model.Path.LastOrDefault();
        }
    }
}