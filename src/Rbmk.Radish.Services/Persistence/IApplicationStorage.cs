namespace Rbmk.Radish.Services.Persistence
{
    public interface IApplicationStorage
    {
        string BaseDirectory { get; }
        
        string LogDirectory { get; }
        
        string CacheDirectory { get; }
        
        string DatabaseFile { get; }
    }
}