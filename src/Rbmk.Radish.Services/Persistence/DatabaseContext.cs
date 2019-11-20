using Microsoft.EntityFrameworkCore;
using Rbmk.Radish.Services.Persistence.Entities;

namespace Rbmk.Radish.Services.Persistence
{
    public class DatabaseContext : DbContext
    {
        public DbSet<ConnectionEntity> Connections { get; set; }
        
        public DbSet<SkipUpdateEntity> SkipUpdates { get; set; }
        
        public DbSet<LicenseEntity> Licenses { get; set; }
        
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<ConnectionEntity>(m =>
            {
                m.ToTable("connections");
                
                m.Property(c => c.Id);
                m.Property(c => c.Order).IsRequired();
                m.Property(c => c.Name).IsRequired();
                m.Property(c => c.ConnectionString).IsRequired();
                
                m.HasKey(c => c.Id);
            });

            model.Entity<SkipUpdateEntity>(m =>
            {
                m.ToTable("skip_updates");

                m.Property(s => s.Id);
                m.Property(s => s.DateTime).IsRequired();
                m.Property(s => s.Version).IsRequired();
                
                m.HasKey(s => s.Id);
            });

            model.Entity<LicenseEntity>(m =>
            {
                m.ToTable("licenses");

                m.Property(l => l.Id);
                m.Property(l => l.Base64).IsRequired();

                m.HasKey(l => l.Id);
            });
        }
    }
}