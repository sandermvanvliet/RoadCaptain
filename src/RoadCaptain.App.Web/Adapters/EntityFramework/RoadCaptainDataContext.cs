using Microsoft.EntityFrameworkCore;

namespace RoadCaptain.App.Web.Adapters.EntityFramework
{
    public class RoadCaptainDataContext : DbContext
    {
        public DbSet<Route> Routes { get; set; }
        public DbSet<User> Users { get; set; }

        private readonly string _databasePath;

#pragma warning disable CS8618 // Non-nullable properties are initialized by EF Core
        public RoadCaptainDataContext()
#pragma warning restore CS8618
        {
            _databasePath = Path.Combine(Environment.CurrentDirectory, "database.sqlite3");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite($"Data Source={_databasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder
                .Entity<User>()
                .Property(u => u.Id)
                .IsRequired();

            modelBuilder
                .Entity<User>()
                .Property(u => u.Name)
                .IsRequired();

            modelBuilder
                .Entity<User>()
                .Property(u => u.ZwiftSubject)
                .IsRequired();

            modelBuilder
                .Entity<Route>()
                .HasKey(r => r.Id);

            modelBuilder
                .Entity<Route>()
                .Property(r => r.Id)
                .IsRequired();

            modelBuilder
                .Entity<Route>()
                .Property(r => r.Serialized)
                .IsRequired();

            modelBuilder
                .Entity<Route>()
                .Property(r => r.UserId)
                .IsRequired();

            modelBuilder
                .Entity<Route>()
                .HasOne(r => r.User)
                .WithMany(u => u.Routes);
        }
    }
}