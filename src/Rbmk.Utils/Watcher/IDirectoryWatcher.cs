using System;

namespace Rbmk.Utils.Watcher
{
	public interface IDirectoryWatcher
	{
		IObservable<WatcherChange> Watch();
		
		IObservable<WatcherChange> Scan();
	}
}
