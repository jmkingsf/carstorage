namespace Domain.Models
{
    public class Location
    {
        public Guid Id { get; set; }
        public List<Listing> Listings { get; set; } = new List<Listing>();
        public int TotalSpace { get; init; }
    }
}
