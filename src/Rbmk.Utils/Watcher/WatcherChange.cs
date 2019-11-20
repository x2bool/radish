using System.IO;

namespace Rbmk.Utils.Watcher
{
	public class WatcherChange
	{
		public string Path { get; }

		public bool IsFile { get; }

		public bool IsDirectory { get; }

		public WatcherChange(string path, bool isFile, bool isDirectory)
		{
			Path = path;
			IsFile = isFile;
			IsDirectory = isDirectory;
		}

		public WatcherChange(string path)
		{
			Path = path;
			IsFile = File.Exists(path);
			IsDirectory = Directory.Exists(path);
		}

		public static WatcherChange Error { get; } = null;
	}
}
