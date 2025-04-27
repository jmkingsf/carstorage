using Domain.Models;

namespace Domain.Ports
{
    public interface ILocationRepository
    {
        [Obsolete("No longer used in final version")]
        Task<List<Location>> GetLocationsThatFitCar(int length, int width, int totalSpace);
        Task<List<Location>> GetLocationsThatFitAdjustedTotalSpaceRequired(int totalSpaceRequired);
    }
}
