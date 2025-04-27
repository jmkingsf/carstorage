using Domain.Models;

namespace Domain.Experimental
{
    public class PermutationGenerator
    {
        private static Dictionary<Guid, List<Location>> _permutations = new Dictionary<Guid, List<Location>>();
        public static List<List<Listing>> GetPermutations(List<Listing> listings)
        {
            var result = new List<List<Listing>>();
            Permute(listings, 0, result);
            return result;
        }

        public static List<Location> GetPermutations(Location location)
        {
            if (_permutations.ContainsKey(location.Id))
            {
                return _permutations[location.Id];
            }

            var result = new List<List<Listing>>();
            Permute(location.Listings, 0, result);

            var permutations = new List<Location>();
            foreach (var listings in result)
            {
                permutations.Add(new Location
                {
                    Id = location.Id,
                    Listings = listings,
                });
            }

            _permutations.Add(location.Id, permutations);

            return permutations;
        }

        private static void Permute(List<Listing> listings, int start, List<List<Listing>> result)
        {
            if (start == listings.Count - 1)
            {
                result.Add(new List<Listing>(listings));
                return;
            }

            for (int i = start; i < listings.Count; i++)
            {
                Swap(listings, start, i);
                Permute(listings, start + 1, result);
                Swap(listings, start, i); // backtrack
            }
        }

        private static void Swap(List<Listing> listings, int i, int j)
        {
            (listings[i], listings[j]) = (listings[j], listings[i]);
        }
    }
}
