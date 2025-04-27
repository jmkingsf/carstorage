using Domain.Models;

namespace Domain.DatabasePorts
{
    public interface ILocationRepository
    {
        Task<List<Location>> GetLocationsThatFitCar(int length, int width, int totalSpace);
        Task<List<Location>> GetLocationsThatFitAdjustedTotalSpaceRequired(int totalSpaceRequired);
    }
}
