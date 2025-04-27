using Domain;
using Domain.DatabasePorts;
using Domain.Models;

using Microsoft.EntityFrameworkCore;

namespace EFDatabase
{
    public class LocationRepository(CarStorageDbContext carStorageDbContext) : ILocationRepository
    {
        public async Task<List<Location>> GetLocationsThatFitCar(int length, int width, int totalSpace)
        {
            return await carStorageDbContext.Set<Location>()
                .Where(location => location.TotalSpace > totalSpace && location.Listings.Any(l => (l.Length >= length && l.Width >= width) || (l.Length >= width && l.Width >= length)))
                .Include(l => l.Listings)
                .ToListAsync();
        }

        public async Task<List<Location>> GetLocationsThatFitAdjustedTotalSpaceRequired(int totalSpaceRequired)
        {
            var adjustedTotalSpaceRequired = totalSpaceRequired * 90;
            return await carStorageDbContext.Set<Location>()
                .Where(location => location.TotalSpace > adjustedTotalSpaceRequired)
                .Include(l => l.Listings)
                .ToListAsync();
        }
    }
}
