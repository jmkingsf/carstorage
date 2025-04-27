using Domain;
using Domain.DatabasePorts;

using Microsoft.EntityFrameworkCore;

namespace EFDatabase
{
    public class ListingRepository(CarStorageDbContext dbContext) : IListingRepository
    {
        public async Task<List<Listing>> GetAll()
        {
            return await dbContext.Set<Listing>().ToListAsync();
        }

        public async Task<IEnumerable<Listing>> GetLocationsThatFitCar(int maxLength, int maxWidth, int totalSpace)
        {
            var adjustedTotalSpace = totalSpace * .90;
            return await dbContext
                .Set<Listing>()
                .Where(l => (l.Length >= maxLength && l.Width >= maxWidth) || (l.Length >= maxWidth && l.Width >= maxLength) && l.Location.TotalSpace >= adjustedTotalSpace)
                .ToListAsync();
        }
    }
}
