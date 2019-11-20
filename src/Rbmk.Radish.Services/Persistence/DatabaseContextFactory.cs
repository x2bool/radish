using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Rbmk.Radish.Services.Persistence
{
    public class DatabaseContextFactory
        : IDatabaseContextFactory, IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var storage = ApplicationStorage.Instance;
            var options = new DbContextOptionsBuilder<DatabaseContext>();
            options.UseSqlite($"Data Source={storage.DatabaseFile};");
            return new DatabaseContext(options.Options);
        }

        public DatabaseContext CreateDbContext() => CreateDbContext(new string[0]);
    }
}