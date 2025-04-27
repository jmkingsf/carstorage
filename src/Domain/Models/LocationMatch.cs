using System.Text.Json.Serialization;

namespace Domain
{
    public class LocationMatch
    {
        [JsonPropertyName("location_id")]
        public Guid LocationId { get; set; }
        [JsonPropertyName("listing_ids")]
        public required List<Guid> ListingIds { get; set; }
        [JsonPropertyName("total_price_in_cents")]
        public long TotalPriceInCents { get; set; }
    }
}
