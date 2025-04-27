using System.Text.Json;

using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EFDatabase;

public static class DbSeedFactory
{
    public static void Seed(DbContext context)
    {
        if (context.Set<Listing>().Any())
            return;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        using var fileStream = new FileStream("./listings.json", FileMode.Open);
        var listings = JsonSerializer.Deserialize<List<Listing>>(fileStream, options);
        if (listings == null) return;

        foreach (var listing in listings)
        {
            listing.Area = listing.Width * listing.Length;
        }

        context.Set<Listing>().AddRange(listings);

        var locations = listings
            .GroupBy(l => l.LocationId)
            .Select(g => new Location
            {
                Id = g.First().LocationId,
                Listings = g.ToList(),
                TotalSpace = g.Sum(l => l.Width * l.Length),
            });

        context.Set<Location>().AddRange(locations);
        context.SaveChanges();
    }
}
