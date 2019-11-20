using System;

namespace Rbmk.Utils.Watcher
{
	public class WatcherException : Exception
	{
		public WatcherException()
			: base("An error occured in file system watcher")
		{
		}
	}
}
