using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged;
using Rbmk.Radish.Model.Dialogs;
using Rbmk.Utils.Reactive;
using Rbmk.Utils.Watcher;
using ReactiveUI;
using Splat;

namespace Rbmk.Radish.Model.FileExplorer
{
	[AddINotifyPropertyChangedInterface]
	public class FileExplorerModel : IActivatableViewModel
	{
		public ObservableCollectionExtended<string> Roots { get; set; }
			= new ObservableCollectionExtended<string>();

		public SourceList<FileExplorerNodeModel> Nodes { get; set; }
			= new SourceList<FileExplorerNodeModel>();
		
		public ObservableCollectionExtended<FileExplorerNodeModel> Items { get; set; }
			= new ObservableCollectionExtended<FileExplorerNodeModel>();
		
		public FileExplorerNodeModel SelectedItem { get; set; }
		
		public ReactiveCommand<Unit, string> OpenFolderCommand { get; set; }
		
		private readonly Dictionary<string, FileExplorerNodeModel> _roots
			= new Dictionary<string, FileExplorerNodeModel>();
		
		private readonly Dictionary<string, IDisposable> _disposables
			= new Dictionary<string, IDisposable>();

		public FileExplorerModel(
			IDialogManager dialogManager)
		{
			this.WhenActivated(disposables =>
			{
				BindNodes()
					.DisposeWith(disposables);

				BindCommands(dialogManager)
					.DisposeWith(disposables);

				BindAdditions()
					.DisposeWith(disposables);

				BindRemovals()
					.DisposeWith(disposables);

				BindCleanup()
					.DisposeWith(disposables);
			});
		}

		public FileExplorerModel()
			: this(
				Locator.Current.GetService<IDialogManager>())
		{
		}

		private IDisposable BindNodes()
		{
			return Nodes.Connect()
				.SubscribeOn(RxApp.TaskpoolScheduler)
				.ObserveOn(RxApp.MainThreadScheduler)
				.Bind(Items)
				.SubscribeWithLog();
		}

		private IDisposable BindCommands(IDialogManager dialogManager)
		{
			OpenFolderCommand = ReactiveCommand.CreateFromObservable(
				() => dialogManager.OpenFolder(null), null, RxApp.MainThreadScheduler);

			return OpenFolderCommand
				.SubscribeOn(RxApp.TaskpoolScheduler)
				.ObserveOn(RxApp.MainThreadScheduler)
				.SubscribeWithLog(directory =>
				{
					var path = Path.GetFullPath(directory);
					
					if (Roots.All(r => r != path))
					{
						Roots.Add(path);
					}
				});
		}

		private IDisposable BindAdditions()
		{
			return Roots.ObserveAdditions()
				.SubscribeOn(RxApp.TaskpoolScheduler)
				.ObserveOn(RxApp.MainThreadScheduler)
				.SubscribeWithLog(path =>
				{
					var watcher = new DirectoryWatcher(path);
					var builder = new FileExplorerNodeBuilder(watcher, path);
					
					var subscription = builder.GetActions()
						.SubscribeOn(RxApp.TaskpoolScheduler)
						.ObserveOn(RxApp.MainThreadScheduler)
						.SubscribeWithLog(builder.HandleAction);
					
					Nodes.Add(builder.Root);
					
					_roots.Add(path, builder.Root);
					_disposables.Add(path, subscription);
				});
		}

		private IDisposable BindRemovals()
		{
			return Roots.ObserveRemovals()
				.SubscribeOn(RxApp.TaskpoolScheduler)
				.ObserveOn(RxApp.MainThreadScheduler)
				.SubscribeWithLog(path =>
				{
					if (_roots.TryGetValue(path, out var node))
					{
						_roots.Remove(path);
						Nodes.Remove(node);
					}

					if (_disposables.TryGetValue(path, out var subscription))
					{
						_disposables.Remove(path);
						subscription.Dispose();
					}
				});
		}

		private IDisposable BindCleanup()
		{
			return Disposable.Create(() =>
			{
				foreach (var pair in _disposables)
				{
					pair.Value.Dispose();
				}
				_disposables.Clear();
				_roots.Clear();
			});
		}

		public ViewModelActivator Activator { get; } = new ViewModelActivator();
	}
}
