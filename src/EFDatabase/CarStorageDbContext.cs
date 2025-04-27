using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFDatabase;

public class CarStorageDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseInMemoryDatabase(databaseName: "CarStorageDb")
            .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.AccidentalEntityType))
            .UseSeeding((context, _) => DbSeedFactory.Seed(context));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>()
            .HasMany(l => l.Listings)
            .WithOne(l => l.Location)
            .HasForeignKey(l => l.LocationId)
            .HasPrincipalKey(l => l.Id);
    }
    public DbSet<Location> Locations { get; set; }
}
