using System.Text.Json;

using Domain;
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
            .UseSeeding((context, _) =>
            {
                var existListings = context.Set<Listing>().Any();
                if (!existListings)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    };
                    using var filestream = new FileStream("./listings.json", FileMode.Open);
                    var listings = JsonSerializer.Deserialize<List<Listing>>(filestream, options);
                    listings!.ForEach(l => l.Area = l.Width * l.Length);
                    context.Set<Listing>().AddRange(listings!);

                    var locations = listings!.GroupBy(l => l.LocationId).Select(g => new Location
                    {
                        Id = g.First().LocationId,
                        Listings = g.ToList(),
                        TotalSpace = g.ToList().Sum(l => l.Length * l.Width),
                    });
                    context.Set<Location>().AddRange(locations);

                    context.SaveChanges();
                }
            });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>()
            .HasMany(l => l.Listings)
            .WithOne(l => l.Location)
            .HasForeignKey(l => l.LocationId)
            .HasPrincipalKey(l => l.Id);
    }

    public DbSet<Listing> Listings { get; set; }
}
