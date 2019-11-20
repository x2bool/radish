namespace Rbmk.Radish.Services.Persistence
{
    public interface IDatabaseContextFactory
    {
        DatabaseContext CreateDbContext();
    }
}