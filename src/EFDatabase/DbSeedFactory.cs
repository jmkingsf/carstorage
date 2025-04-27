using System.Text.Json;

using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EFDatabase;

public static class DbSeedFactory
{
    public static async Task SeedAsync(DbContext context)
    {
        if (await context.Set<Listing>().AnyAsync())
            return;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        using var fileStream = new FileStream("./listings.json", FileMode.Open);
        var listings = await JsonSerializer.DeserializeAsync<List<Listing>>(fileStream, options);
        if (listings == null) return;

        foreach (var listing in listings)
        {
            listing.Area = listing.Width * listing.Length;
        }

        await context.Set<Listing>().AddRangeAsync(listings);

        var locations = listings
            .GroupBy(l => l.LocationId)
            .Select(g => new Location
            {
                Id = g.First().LocationId,
                Listings = g.ToList(),
                TotalSpace = g.Sum(l => l.Width * l.Length),
            });

        await context.Set<Location>().AddRangeAsync(locations);
        await context.SaveChangesAsync();
    }
}
