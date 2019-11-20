using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using Rbmk.Utils.Reactive;
using ReactiveUI;

namespace Rbmk.Radish.Model.FileExplorer
{
	public class FileExplorerNodeModel : IActivatableViewModel
	{
		public string[] Path { get; set; }
		
		public bool IsRoot { get; set; }

		public bool IsFile { get; set; }

		public bool IsDirectory { get; set; }

		public SourceList<FileExplorerNodeModel> Nodes { get; set; }
			= new SourceList<FileExplorerNodeModel>();

		public ObservableCollectionExtended<FileExplorerNodeModel> Items { get; set; }
			= new ObservableCollectionExtended<FileExplorerNodeModel>();
		
		public FileExplorerNodeModel()
		{
			this.WhenActivated(disposables =>
			{
				BindNodes()
					.DisposeWith(disposables);
			});
		}

		private IDisposable BindNodes()
		{
			return Nodes.Connect()
				.Filter(GetFilter())
				.Sort(GetSorting())
				.SubscribeOn(RxApp.TaskpoolScheduler)
				.ObserveOn(RxApp.MainThreadScheduler)
				.Bind(Items)
				.SubscribeWithLog();
		}

		private IComparer<FileExplorerNodeModel> GetSorting()
		{
			return new SortingComparer();
		}

		private Func<FileExplorerNodeModel, bool> GetFilter()
		{
			return new FilterLogic().Filter;
		}
		
		public override string ToString() => "/" + string.Join("/", Path);
		
		public ViewModelActivator Activator { get; } = new ViewModelActivator();
	}
}
