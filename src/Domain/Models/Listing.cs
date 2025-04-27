using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class Listing
    {
        public Guid Id { get; set; }
        [JsonPropertyName("location_id")]
        public Guid LocationId { get; set; }
        public Location Location { get; set; } = null!;
        public int Length { get; set; }
        public int Width { get; set; }
        public int Area { get; set; }
        [JsonPropertyName("price_in_cents")]
        public long PriceInCents { get; set; }

        public bool IsUsed { get; set; } = false;

    }
}
