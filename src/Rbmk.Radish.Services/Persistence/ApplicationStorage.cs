using System;
using System.IO;

namespace Rbmk.Radish.Services.Persistence
{
    public class ApplicationStorage : IApplicationStorage
    {
        public static ApplicationStorage Instance { get; } = new ApplicationStorage("Rbmk", "Radish");
        
        private ApplicationStorage(string group, string app)
        {
            #if DEBUG
            app += "-dev";
            #endif
            
            BaseDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), group, app);
            Directory.CreateDirectory(BaseDirectory);

            LogDirectory = Path.Combine(BaseDirectory, "logs");
            Directory.CreateDirectory(LogDirectory);

            CacheDirectory = Path.Combine(BaseDirectory, "cache");
            Directory.CreateDirectory(CacheDirectory);
            
            DatabaseFile = Path.Combine(BaseDirectory, "app.db");
        }

        public string BaseDirectory { get; }
        
        public string LogDirectory { get; }
        
        public string CacheDirectory { get; }
        
        public string DatabaseFile { get; }
    }
}