using Domain.Experimental;
using Domain.Models;
using Domain.Ports;

namespace Domain
{
    public class VehicleInquiryMatcherGreedy(ILocationRepository locationRepository) : IVehicleInquiryMatcher
    {
        private const double WastePenaltyFactor = 1;
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

            var remainingInquiries = sortedInquiries;
            while(true)
            {
                var inquiry = remainingInquiries.FirstOrDefault();
                if (inquiry == null)
                {
                    break;
                }

                var (bestListing, fits) = FindBestListing(sortedListings, inquiry, remainingInquiries);

                if (bestListing == null)
                    return null; // couldn't fit this car anywhere

                (var anyInquiriesPlaced, remainingInquiries) = fits!.Select(fit => TryPlaceInquiries(remainingInquiries.Select(i => new VehicleInquiry(i.Length, i.Quantity)).ToList(), bestListing, fit)).MinBy(result => result.remainingInquiries.Count);

                // No inquiries could fit
                if (!anyInquiriesPlaced)
                {
                    break;
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

                // Predict upcoming need of 1-2 inquiries
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

        private (bool anyCarPlaced, List<VehicleInquiry> remainingInquiries) TryPlaceInquiries(List<VehicleInquiry> inquiries, Listing listing, Fits fits)
        {
            var widthSpaceRemaining = listing.Width;
            var lengthSpaceRemaining = listing.Length;

            bool anyCarPlaced = false;

            var done = false;
            while (!done)
            {
                var inquiry = inquiries.FirstOrDefault(i => i.Quantity > 0);
                if (inquiry == null)
                    break;

                switch (fits)
                {
                    case Fits.WidthWays:
                        (var widthCanFit, widthSpaceRemaining) = TryFit(inquiry.Length, widthSpaceRemaining);
                        if (widthCanFit)
                        {
                            anyCarPlaced = true;
                            inquiry.Quantity -= 1;
                            continue;
                        }
                        else if (inquiries.Any(i => widthSpaceRemaining >= i.Length))
                        {
                            // cycle through other inquiries to fit in the space
                            anyCarPlaced = true;
                            var inquiryThatCanFit = inquiries.First(i => widthSpaceRemaining - i.Length >= 0);
                            widthSpaceRemaining -= inquiryThatCanFit.Length;
                            inquiryThatCanFit.Quantity -= 1;
                            continue;
                        }
                        else if (lengthSpaceRemaining > Constants.CarWidth)
                        {
                            lengthSpaceRemaining -= Constants.CarWidth;
                            widthSpaceRemaining = inquiry.Length;
                            continue;
                        }
                        done = true;
                        break;

                    case Fits.LengthWays:
                        (var lengthWiseCanFit, lengthSpaceRemaining) = TryFit(inquiry.Length, lengthSpaceRemaining);
                        if (lengthWiseCanFit)
                        {
                            anyCarPlaced = true;
                            inquiry.Quantity -= 1;
                            continue;
                        }
                        else if (inquiries.Any(i => lengthSpaceRemaining >= i.Length))
                        {
                            // cycle through other inquiries to fit in the space
                            anyCarPlaced = true;
                            var inquiryThatCanFit = inquiries.First(i => lengthSpaceRemaining - i.Length >= 0);
                            lengthSpaceRemaining -= inquiryThatCanFit.Length;
                            inquiryThatCanFit.Quantity -= 1;
                            continue;
                        }
                        else if (widthSpaceRemaining > Constants.CarWidth)
                        {
                            widthSpaceRemaining -= Constants.CarWidth;
                            lengthSpaceRemaining = inquiry.Length;
                            continue;
                        }
                        done = true;
                        break;
                    }
            }

            return (anyCarPlaced, inquiries.Where(i => i.Quantity > 0).ToList());
        }

        private (bool canFit, int remainingSpace) TryFit(int spaceNeeded, int spaceRemaining)
        {
            if (spaceRemaining < spaceNeeded)
            {
                return (false, spaceRemaining);
            }
            else
            {
                return (true, spaceRemaining - spaceNeeded);
            }
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
