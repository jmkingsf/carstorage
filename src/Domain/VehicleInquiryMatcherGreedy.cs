using Domain.Experimental;
using Domain.Models;
using Domain.Ports;

namespace Domain
{
    public class VehicleInquiryMatcherGreedy(ILocationRepository locationRepository) : IVehicleInquiryMatcher
    {
        private const double WastePenaltyFactor = 1;
        private const double MaxWidthDifferential = 9;
        public async Task<IEnumerable<LocationMatch>> Match(List<VehicleInquiry> vehicleInquiry)
        {
            var matches = new List<LocationMatch>();
            foreach (var location in await locationRepository.GetLocationsThatFitAdjustedTotalSpaceRequired(vehicleInquiry.Sum(v => v.Length)))
            {
                var match = PredictiveGreedyFit(location, vehicleInquiry);
                if (match != null)
                {
                    matches.Add(match);
                }
            }

            return matches.OrderBy(t => t.TotalPriceInCents);
        }

        private LocationMatch? PredictiveGreedyFit(Location location, List<VehicleInquiry> vehicleInquiries)
        {
            var sortedListings = location.Listings
                .OrderBy(l => l.PriceInCents)
                .ToList();

            var sortedInquiries = vehicleInquiries
                .OrderByDescending(v => v.Length)
                .ToList();

            var usedListingIds = new List<Guid>();
            long totalPrice = 0;

            var remainingInquiries = sortedInquiries.Slice(0, sortedInquiries.Count);
            foreach (var inquiry in sortedInquiries)
            {
                if (!remainingInquiries.Contains(inquiry))
                {
                    continue;
                }

                var (bestListing, fits) = FindBestListing(sortedListings, inquiry, remainingInquiries);

                if (bestListing == null)
                    return null; // couldn't fit this car anywhere

                (var anyInquiriesPlaced, remainingInquiries) = fits!.Select(fit => TryPlaceInquiries(remainingInquiries, bestListing, fit)).MinBy(result => result.remainingInquiries.Count);

                // No inquiries could fit
                if (!anyInquiriesPlaced)
                {
                    continue;
                }

                bestListing.IsUsed = true;
                usedListingIds.Add(bestListing.Id);
                totalPrice += bestListing.PriceInCents;
            }

            // Not able to place all inquiries or no listings were used
            if (usedListingIds.Count == 0 || remainingInquiries.Count != 0)
                return null;

            return new LocationMatch
            {
                LocationId = location.Id,
                ListingIds = usedListingIds,
                TotalPriceInCents = totalPrice
            };
        }

        private (Listing?, List<Fits>?) FindBestListing(
            List<Listing> listings,
            VehicleInquiry currentInquiry,
            List<VehicleInquiry> upcomingInquiries)
        {
            Listing? bestListing = null;
            double bestScore = double.MaxValue;
            List<Fits>? bestFits = null;

            foreach (var listing in listings.Where(l => !l.IsUsed))
            {
                List<Fits> fits = new List<Fits>();
                if (!CanFit(listing, currentInquiry, fits))
                    continue;

                int carArea = Constants.CarWidth * currentInquiry.Length;
                int remainingArea = listing.Area - carArea;

                // Predict upcoming need: next 1–2 cars' areas
                int nextNeedArea = upcomingInquiries
                    .Skip(1)
                    .Take(2)
                    .Sum(v => v.Length);

                int predictedWaste = Math.Max(remainingArea - nextNeedArea, 0);

                // Calculate score
                double score = listing.PriceInCents + (predictedWaste * WastePenaltyFactor);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestListing = listing;
                    bestFits = fits;
                }
            }

            return (bestListing, bestFits);
        }

        private (bool anyCarPlaced, List<VehicleInquiry> remainingInquiries) TryPlaceInquiries(List<VehicleInquiry> originalInquiries, Listing listing, Fits fits)
        {
            var widthSpaceRemaining = listing.Width;
            var lengthSpaceRemaining = listing.Length;
            bool anyCarPlaced = false;

            var remainingInquiries = new List<VehicleInquiry>();

            foreach (var inquiry in originalInquiries)
            {
                int carsPlaced = 0;

                for (var carIndex = 0; carIndex < inquiry.Quantity; carIndex++)
                {
                    bool fitSuccessful = false;

                    switch (fits)
                    {
                        case Fits.WidthWays:
                            widthSpaceRemaining -= inquiry.Length;
                            if (widthSpaceRemaining < 0 && lengthSpaceRemaining > Constants.CarWidth + MaxWidthDifferential)
                            {
                                lengthSpaceRemaining -= Constants.CarWidth;
                                widthSpaceRemaining = listing.Width - inquiry.Length;
                            }
                            break;

                        case Fits.LengthWays:
                            lengthSpaceRemaining -= inquiry.Length;
                            if (lengthSpaceRemaining < 0 && widthSpaceRemaining > Constants.CarWidth + MaxWidthDifferential)
                            {
                                widthSpaceRemaining -= Constants.CarWidth;
                                lengthSpaceRemaining = listing.Length - inquiry.Length;
                            }
                            break;
                    }

                    fitSuccessful = widthSpaceRemaining >= 0 && lengthSpaceRemaining >= 0;

                    if (!fitSuccessful)
                    {
                        // Revert decrement because car didn't actually fit
                        switch (fits)
                        {
                            case Fits.WidthWays:
                                widthSpaceRemaining += inquiry.Length;
                                break;
                            case Fits.LengthWays:
                                lengthSpaceRemaining += inquiry.Length;
                                break;
                        }
                        break; // can't fit more of this inquiry
                    }

                    carsPlaced++;
                    anyCarPlaced = true;
                }

                if (carsPlaced < inquiry.Quantity)
                {
                    // Some or all cars from this inquiry still remain
                    inquiry.Quantity -= carsPlaced;
                    remainingInquiries.Add(inquiry);
                }
            }

            return (anyCarPlaced, remainingInquiries);
        }

        private bool CanFit(Listing listing, VehicleInquiry inquiry, List<Fits> fits)
        {
            if (listing.Width >= Constants.CarWidth && listing.Length >= inquiry.Length)
            {
                fits.Add(Fits.LengthWays);
            }
            if (listing.Width >= inquiry.Length && listing.Length >= Constants.CarWidth)
            {
                fits.Add(Fits.WidthWays);
            }

            return fits.Any();
        }
    }
}
