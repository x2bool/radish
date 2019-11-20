using System;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using Rbmk.Utils.Watcher;

namespace Rbmk.Radish.Model.FileExplorer
{
	public class FileExplorerNodeBuilder : IFileExplorerNodeBuilder
	{
		private readonly IDirectoryWatcher _directoryWatcher;

		public FileExplorerNodeModel Root { get; }

		public FileExplorerNodeBuilder(
			IDirectoryWatcher directoryWatcher,
			string path)
		{
			_directoryWatcher = directoryWatcher;

			Root = new FileExplorerNodeModel
			{
				IsRoot = true,
				IsFile = false,
				IsDirectory = true,
				Path = GetPath(path)
			};
		}

		public IObservable<FileExplorerNodeAction> GetActions()
		{
			return _directoryWatcher.Scan()
				.Concat(_directoryWatcher.Watch())
				.Select(HandleChange);
		}

		public void HandleAction(FileExplorerNodeAction action)
		{
			switch (action)
			{
				case FileExplorerNodeAction.Add add:
					HandleAddAction(add);
					break;

				case FileExplorerNodeAction.Delete del:
					HandleDeleteAction(del);
					break;
			}
		}

		private FileExplorerNodeAction HandleChange(WatcherChange change)
		{
			var node = new FileExplorerNodeModel
			{
				IsFile = change.IsFile,
				IsDirectory = change.IsDirectory,
				Path = GetPath(change.Path)
			};

			if (change.IsFile || change.IsDirectory)
			{
				return new FileExplorerNodeAction.Add(node);
			}

			return new FileExplorerNodeAction.Delete(node);
		}

		private void HandleAddAction(FileExplorerNodeAction.Add add)
		{
			var node = add.Model;
			AddNode(node, Root);
		}

		private void HandleDeleteAction(FileExplorerNodeAction.Delete del)
		{
			var node = del.Model;
			DeleteNode(node, Root);
		}

		private void AddNode(
			FileExplorerNodeModel node,
			FileExplorerNodeModel target)
		{
			if (IsSameNode(node, target))
			{
				// update node
				node.IsFile = target.IsFile;
				node.IsDirectory = target.IsDirectory;
			}
			else if (IsChildNode(node, target))
			{
				if (!target.Nodes.Items.Any(n => IsSameNode(n, node)))
				{
					target.Nodes.Add(node);
				}
			}
			else if (IsDescendantNode(node, target))
			{
				var nextNode = GetNextNode(node, target);

				if (nextNode == null)
				{
					nextNode = new FileExplorerNodeModel
					{
						IsFile = false,
						IsDirectory = true,
						Path = node.Path.Take(target.Path.Length + 1).ToArray()
					};

					target.Nodes.Add(nextNode);
				}
				
				AddNode(node, nextNode);
			}
		}

		private void DeleteNode(
			FileExplorerNodeModel node,
			FileExplorerNodeModel target)
		{
			if (IsSameNode(node, target))
			{
				// delete children nodes
				if (node.Nodes != null)
				{
					foreach (var child in node.Nodes.Items)
					{
						DeleteNode(child, node);
					}
				}
			}
			else if (IsChildNode(node, target))
			{
				if (target.Nodes != null)
				{
					// delete children nodes
					DeleteNode(node, node);

					// and remove node from target
					var child = target.Nodes.Items.FirstOrDefault(n => IsSameNode(node, n));
					if (child != null)
					{
						target.Nodes.Remove(child);
					}
				}
			}
			else if (IsDescendantNode(node, target))
			{
				// navigate to node and delete it
				var nextNode = GetNextNode(node, target);
				if (nextNode != null)
				{
					DeleteNode(node, nextNode);
				}
			}
		}
		
		private bool IsSameNode(
			FileExplorerNodeModel node,
			FileExplorerNodeModel target)
		{
			if (node == target) return true;
			if (node.Path.Length != target.Path.Length) return false;

			for (int i = 0; i < node.Path.Length; i++)
			{
				if (node.Path[i] != target.Path[i]) return false;
			}

			return true;
		}

		private bool IsChildNode(
			FileExplorerNodeModel node,
			FileExplorerNodeModel target)
		{
			if (node.Path.Length - target.Path.Length == 1)
			{
				for (int i = 0; i < target.Path.Length; i++)
				{
					if (node.Path[i] != target.Path[i]) return false;
				}
				return true;
			}

			return false;
		}

		private bool IsDescendantNode(
			FileExplorerNodeModel node,
			FileExplorerNodeModel target)
		{
			if (node.Path.Length - target.Path.Length > 0)
			{
				for (int i = 0; i < target.Path.Length; i++)
				{
					if (node.Path[i] != target.Path[i]) return false;
				}
				return true;
			}

			return false;
		}

		private FileExplorerNodeModel GetNextNode(
			FileExplorerNodeModel node,
			FileExplorerNodeModel target)
		{
			foreach (var child in target.Nodes.Items)
			{
				if (IsDescendantNode(node, child))
				{
					return child;
				}
			}

			return null;
		}

		private string[] GetPath(string path)
		{
			return path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
