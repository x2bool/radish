using System;
using System.IO;
using System.Reactive.Linq;

namespace Rbmk.Utils.Watcher
{
	public class DirectoryWatcher : IDirectoryWatcher
	{
		private readonly string _path;

		public DirectoryWatcher(string path)
		{
			_path = path;
		}

		public IObservable<WatcherChange> Watch()
		{
			var watcher = CreateWatcher();

			var disposes = Observable.FromEventPattern(
					h => watcher.Disposed += h,
					h => watcher.Disposed -= h)
				.Select(_ => WatcherChange.Error);

			var errors = Observable.FromEventPattern<ErrorEventHandler, ErrorEventArgs>(
					h => watcher.Error += h,
					h => watcher.Error -= h)
				.Select(_ => WatcherChange.Error);

			var creations = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
					h => watcher.Created += h,
					h => watcher.Created -= h)
				.Select(e => new WatcherChange(e.EventArgs.FullPath));

			var deletions = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
					h => watcher.Deleted += h,
					h => watcher.Deleted -= h)
				.Select(e => new WatcherChange(e.EventArgs.FullPath));

			var renames = Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
					h => watcher.Renamed += h,
					h => watcher.Renamed -= h)
				.SelectMany(e => new[]
				{
					new WatcherChange(e.EventArgs.OldFullPath),
					new WatcherChange(e.EventArgs.FullPath)
				});

			return Observable.Defer(() =>
				{
					var observable = Observable.Merge(
						disposes,
						errors,
						creations,
						deletions,
						renames);
	
					watcher.EnableRaisingEvents = true;
					
					return observable;
				})
				.Do(change =>
				{
					if (change == null)
					{
						throw new WatcherException();
					}
				})
				.Finally(() =>
				{
					watcher.Dispose();
				});
		}

		public IObservable<WatcherChange> Scan()
		{
			var directories = Directory.EnumerateDirectories(
					_path, "*.*", SearchOption.AllDirectories)
				.ToObservable();

			var files = Directory.EnumerateFiles(
					_path, "*.*", SearchOption.AllDirectories)
				.ToObservable();

			return directories.Concat(files)
				.Select(path => new WatcherChange(path));
		}

		private FileSystemWatcher CreateWatcher()
		{
			return new FileSystemWatcher(_path)
			{
				InternalBufferSize = ushort.MaxValue,
				IncludeSubdirectories = true,
				NotifyFilter = NotifyFilters.CreationTime |
				               NotifyFilters.FileName |
				               NotifyFilters.DirectoryName
			};
		}
	}
}
