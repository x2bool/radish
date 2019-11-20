using System.Collections;
using System.Collections.Generic;

namespace Rbmk.Radish.Model.FileExplorer
{
    public class SortingComparer : IComparer<FileExplorerNodeModel>
    {
        private static readonly CaseInsensitiveComparer Comparer = new CaseInsensitiveComparer();
        
        public int Compare(FileExplorerNodeModel x, FileExplorerNodeModel y)
        {
            if (x == null)
                return y == null ? 0 : 1;
            
            if (y == null)
                return -1;

            if (x.IsDirectory && y.IsFile)
                return -1;

            if (x.IsFile && y.IsDirectory)
                return 1;

            if (x.Path.Length != y.Path.Length)
                return x.Path.Length > y.Path.Length ? 1 : -1;
            
            for (int i = 0; i < x.Path.Length; i++)
            {
                if (x.Path[i] != y.Path[i])
                {
                    return Comparer.Compare(x.Path[i], y.Path[i]);
                }
            }

            return 0;
        }
    }
}