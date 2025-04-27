namespace Domain.DatabasePorts
{
    public interface IListingRepository
    {
        Task<List<Listing>> GetAll();
        Task<IEnumerable<Listing>> GetLocationsThatFitCar(int maxLength, int maxWidth, int totalSpace);
    }
}
