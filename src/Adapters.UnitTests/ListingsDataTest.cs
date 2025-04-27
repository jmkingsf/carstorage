using System.Text.Json;
using System.Text.Json.Serialization;

using Domain;

namespace Adapters.UnitTests
{
    [TestClass]
    public class ListingsDataTest
    {
        private readonly List<Listing>? _listings;

        public ListingsDataTest()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            using var file = new FileStream("./listings.json", FileMode.Open);
            _listings = JsonSerializer.Deserialize<List<Listing>>(file, options);
        }

        [TestMethod]
        public void ContainsNoDuplicateLocations()
        {

            foreach (var listing in _listings)
            {
                Assert.AreEqual(1, _listings.Count(l => l.LocationId == listing.LocationId), "contains duplicate locations");
            }
        }
    }
}
