using Domain.DatabasePorts;
using Domain.Models;

namespace Domain
{
    public interface IVehicleInquiryMatcher
    {
        Task<IEnumerable<LocationMatch>> Match(List<VehicleInquiry> vehicleInquiry);
    }

    public class VehicleInquiryMatcher(ILocationRepository locationRepository, IListingRepository listingRepository) : IVehicleInquiryMatcher
    {
        private const int Width = 10;

        // listing state could tell me about of space per line remaining by both width and length
        // for anything with a similar stat or better I can automatically say yes without continuing processing
        public async Task<IEnumerable<LocationMatch>> Match(List<VehicleInquiry> vehicleInquiry)
        {
            List<LocationMatch> locationMatches = new List<LocationMatch>();
            // foreach distinct location
            // check if the vehicle inquery can fit
            // How would recursion would here? Check if vehicle fits in the listing and check if space exists at the same listing or next location
            foreach (var location in await locationRepository.GetLocationsThatFitCar(vehicleInquiry.MaxBy(v => v.Length).Length, Width, vehicleInquiry.Sum(v => v.Length * Width)))
            {
                foreach (var permutation in PermutationGenerator.GetPermutations(location))
                {
                    var matchByWidth = MatchesAtLocation(vehicleInquiry, permutation, 0, 0, 1, permutation.Listings[0].Width, permutation.Listings[0].Length, Fits.WidthWays);
                    var matchByLength = MatchesAtLocation(vehicleInquiry, permutation, 0, 0, 1, permutation.Listings[0].Width, permutation.Listings[0].Length, Fits.LengthWays);

                    if (matchByWidth != null)
                        locationMatches.Add(matchByWidth);

                    if (matchByLength != null)
                        locationMatches.Add(matchByLength);
                }
            }


            return locationMatches.OrderBy(l => l.TotalPriceInCents).DistinctBy(l => l.LocationId).ToList().OrderBy(l => l.TotalPriceInCents);
        }

        private LocationMatch? MatchesAtLocation(List<VehicleInquiry> vehicleInquiry, Location location, int inquiryIndex, int listingIndex, int carIndex, int? widthSpaceRemaining, int? lengthSpaceRemaining, Fits fits)
        {
            if (widthSpaceRemaining is < 0 || lengthSpaceRemaining is < 0)
                return null;

            if (inquiryIndex == vehicleInquiry.Count)
                return new LocationMatch
                {
                    LocationId = location.Id,
                    ListingIds = location.Listings.Slice(0, ++listingIndex).Select(l => l.Id).ToList(),
                    TotalPriceInCents = location.Listings.Slice(0, listingIndex).Select(l => l.PriceInCents).Sum()
                };

            if (listingIndex == location.Listings.Count)
                return null;

            var inquiry = vehicleInquiry[inquiryIndex];
            var listing = location.Listings[listingIndex];

            switch (fits)
            {
                case Fits.WidthWays:
                    widthSpaceRemaining -= inquiry.Length;
                    if (widthSpaceRemaining < 0)
                    {
                        lengthSpaceRemaining -= Width;
                        widthSpaceRemaining = listing.Width - inquiry.Length;
                    }
                    break;
                case Fits.LengthWays:
                    lengthSpaceRemaining -= inquiry.Length;
                    if (lengthSpaceRemaining < 0)
                    {
                        widthSpaceRemaining -= Width;
                        lengthSpaceRemaining = listing.Length - inquiry.Length;
                    }
                    break;
            }

            if (carIndex < inquiry.Quantity)
            {
                carIndex++;
            }
            else
            {
                carIndex = 1;
                inquiryIndex++;
            }

            return MatchesAtLocation(vehicleInquiry, location, inquiryIndex, listingIndex, carIndex, widthSpaceRemaining, lengthSpaceRemaining, fits);
        }
    }
}
