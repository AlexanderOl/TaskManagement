using Microsoft.EntityFrameworkCore;
using Common.DbEntities;

namespace Common.Services
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<TaskEntity> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskEntity>().HasKey(t => t.Id);

            modelBuilder.Entity<TaskEntity>()
                .Property(t => t.Status)
                .HasConversion<int>();

        }
    }
}
