namespace Rbmk.Radish.Model.FileExplorer
{
	public abstract class FileExplorerNodeAction
	{
		public FileExplorerNodeModel Model { get; }

		public FileExplorerNodeAction(FileExplorerNodeModel model)
		{
			Model = model;
		}

		public class Delete : FileExplorerNodeAction
		{
			public Delete(FileExplorerNodeModel model)
				: base (model)
			{
				
			}
		}

		public class Add : FileExplorerNodeAction
		{
			public Add(FileExplorerNodeModel model)
				: base(model)
			{

			}
		}
	}
}
