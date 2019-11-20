using System;

namespace Rbmk.Radish.Model.FileExplorer
{
	public interface IFileExplorerNodeBuilder
	{
		IObservable<FileExplorerNodeAction> GetActions();

		void HandleAction(FileExplorerNodeAction action);
	}
}
