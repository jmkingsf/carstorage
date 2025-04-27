using Domain.DatabasePorts;
using Domain.Models;

using NSubstitute;

namespace Domain.UnitTests;

[TestClass]
public class VehicleInquiryMatcherTests
{
    private readonly ILocationRepository _repository = Substitute.For<ILocationRepository>();
    private VehicleInquiryMatcherGreedy _vehicleInquiryMatcher;

    public VehicleInquiryMatcherTests()
    {
        _vehicleInquiryMatcher = new VehicleInquiryMatcherGreedy(_repository);
    }

    [TestMethod]
    public async Task Match_2CarsFitInListingByLength()
    {
        // arrange
        List<VehicleInquiry> inqueries =
        [
            new VehicleInquiry(20, 2)
        ];
        _repository.GetLocationsThatFitAdjustedTotalSpaceRequired(0).ReturnsForAnyArgs(Task.FromResult<List<Location>>([
            new Location
            {
                Id = Guid.NewGuid(),
                Listings =
                [
                    new Listing
                    {
                        Length = 40,
                        Width = 10
                    }
                ]
            }
        ]));

        // act
        var locationMatchResult = await _vehicleInquiryMatcher.Match(inqueries);

        // assert
        Assert.AreEqual(1, locationMatchResult.Count());
    }

    [TestMethod]
    public async Task Match_2CarsFitInListingByWidth()
    {
        // arrange
        List<VehicleInquiry> inqueries =
        [
            new VehicleInquiry(20, 2)
        ];
        _repository.GetLocationsThatFitAdjustedTotalSpaceRequired(0).ReturnsForAnyArgs(Task.FromResult<List<Location>>([
            new Location
            {
                Id = Guid.NewGuid(),
                Listings =
                [
                    new Listing
                    {
                        Length = 10,
                        Width = 40
                    }
                ]
            }
        ]));

        // act
        var locationMatchResult = await _vehicleInquiryMatcher.Match(inqueries);

        // assert
        Assert.AreEqual(1, locationMatchResult.Count());
    }

    [TestMethod]
    public async Task Match_4CarsFitInListingByWidthOrLength()
    {
        // arrange
        List<VehicleInquiry> inqueries =
        [
            new VehicleInquiry(20, 4)
        ];
        _repository.GetLocationsThatFitAdjustedTotalSpaceRequired(0).ReturnsForAnyArgs(Task.FromResult<List<Location>>([
            new Location
            {
                Id = Guid.NewGuid(),
                Listings =
                [
                    new Listing
                    {
                        Length = 20,
                        Width = 40
                    }
                ]
            }
        ]));

        // act
        var locationMatchResult = await _vehicleInquiryMatcher.Match(inqueries);

        // assert
        Assert.AreEqual(1, locationMatchResult.Count());
    }

    [TestMethod]
    public async Task Match_MultipleListingsFitMultipleVehicles()
    {
        // arrange
        List<VehicleInquiry> inquiries =
        [
            new VehicleInquiry(10, 2), // Two small cars
            new VehicleInquiry(15, 1)  // One medium car
        ];

        _repository.GetLocationsThatFitAdjustedTotalSpaceRequired(0).ReturnsForAnyArgs(Task.FromResult<List<Location>>([
            new Location
            {
                Id = Guid.NewGuid(),
                Listings =
                [
                    new Listing { Length = 20, Width = 10 }, // can fit two 10-length cars
                    new Listing { Length = 20, Width = 15 }  // can fit the 15-length car
                ]
            }
        ]));

        // act
        var locationMatchResult = await _vehicleInquiryMatcher.Match(inquiries);

        // assert
        Assert.AreEqual(1, locationMatchResult.Count());
        var match = locationMatchResult.First();
        Assert.AreEqual(2, match.ListingIds.Count); // Used both listings
    }

    [TestMethod]
    public async Task Match_SingleListingFitsDifferentSizedCars()
    {
        // arrange
        List<VehicleInquiry> inquiries =
        [
            new VehicleInquiry(12, 1),
            new VehicleInquiry(8, 1)
        ];

        _repository.GetLocationsThatFitAdjustedTotalSpaceRequired(0).ReturnsForAnyArgs(Task.FromResult<List<Location>>([
            new Location
            {
                Id = Guid.NewGuid(),
                Listings =
                [
                    new Listing { Length = 25, Width = 10 } // Should be able to fit both vehicles end-to-end
                ]
            }
        ]));

        // act
        var locationMatchResult = await _vehicleInquiryMatcher.Match(inquiries);

        // assert
        Assert.AreEqual(1, locationMatchResult.Count());
        var match = locationMatchResult.First();
        Assert.AreEqual(1, match.ListingIds.Count); // Only one listing needed
    }

    [TestMethod]
    public async Task Match_FailsWhenNotEnoughSpace()
    {
        // arrange
        List<VehicleInquiry> inquiries =
        [
            new VehicleInquiry(20, 2) // Needs more space than provided
        ];

        _repository.GetLocationsThatFitAdjustedTotalSpaceRequired(0).ReturnsForAnyArgs(Task.FromResult<List<Location>>([
            new Location
            {
                Id = Guid.NewGuid(),
                Listings =
                [
                    new Listing { Length = 30, Width = 10 } // Too small to fit two 20-length cars
                ]
            }
        ]));

        // act
        var locationMatchResult = await _vehicleInquiryMatcher.Match(inquiries);

        // assert
        Assert.AreEqual(0, locationMatchResult.Count());
    }

    [TestMethod]
    public async Task Match_FitsAcrossMultipleListings()
    {
        // arrange
        List<VehicleInquiry> inquiries =
        [
            new VehicleInquiry(15, 1),
            new VehicleInquiry(15, 1),
            new VehicleInquiry(15, 1)
        ];

        _repository.GetLocationsThatFitAdjustedTotalSpaceRequired(0).ReturnsForAnyArgs(Task.FromResult<List<Location>>([
            new Location
            {
                Id = Guid.NewGuid(),
                Listings =
                [
                    new Listing { Length = 20, Width = 10 },
                    new Listing { Length = 20, Width = 10 },
                    new Listing { Length = 20, Width = 10 }
                ]
            }
        ]));

        // act
        var locationMatchResult = await _vehicleInquiryMatcher.Match(inquiries);

        // assert
        Assert.AreEqual(1, locationMatchResult.Count());
        var match = locationMatchResult.First();
        Assert.AreEqual(3, match.ListingIds.Count); // Each car needed a separate listing
    }

    [TestMethod]
    public async Task Match_FitsAcrossMultipleListings_MoreWidth()
    {
        // arrange
        List<VehicleInquiry> inquiries =
        [
            new VehicleInquiry(15, 1),
            new VehicleInquiry(15, 1),
            new VehicleInquiry(15, 1)
        ];

        _repository.GetLocationsThatFitAdjustedTotalSpaceRequired(0).ReturnsForAnyArgs(Task.FromResult<List<Location>>([
            new Location
            {
                Id = Guid.NewGuid(),
                Listings =
                [
                    new Listing { Length = 20, Width = 15 },
                    new Listing { Length = 20, Width = 15 },
                    new Listing { Length = 20, Width = 15 }
                ]
            }
        ]));

        // act
        var locationMatchResult = await _vehicleInquiryMatcher.Match(inquiries);

        // assert
        Assert.AreEqual(1, locationMatchResult.Count());
        var match = locationMatchResult.First();
        Assert.AreEqual(2, match.ListingIds.Count); // Each car needed a separate listing
    }

    [TestMethod]
    public async Task Match_FitsAcrossMultipleListings_PreferLengthFit()
    {
        // arrange
        List<VehicleInquiry> inquiries =
        [
            new VehicleInquiry(15, 1),
            new VehicleInquiry(15, 1),
            new VehicleInquiry(15, 1)
        ];

        _repository.GetLocationsThatFitAdjustedTotalSpaceRequired(0).ReturnsForAnyArgs(Task.FromResult<List<Location>>([
            new Location
            {
                Id = Guid.NewGuid(),
                Listings =
                [
                    new Listing { Length = 15, Width = 20 },
                    new Listing { Length = 15, Width = 20 },
                    new Listing { Length = 15, Width = 20 }
                ]
            }
        ]));

        // act
        var locationMatchResult = await _vehicleInquiryMatcher.Match(inquiries);

        // assert
        Assert.AreEqual(1, locationMatchResult.Count());
        var match = locationMatchResult.First();
        Assert.AreEqual(2, match.ListingIds.Count); // Each car needed a separate listing
    }

    [TestMethod]
    public async Task Match_FitSmallCarInSpace()
    {
        // arrange
        List<VehicleInquiry> inquiries =
        [
            new VehicleInquiry(20, 1),
            new VehicleInquiry(15, 1),
            new VehicleInquiry(5, 1)
        ];

        _repository.GetLocationsThatFitAdjustedTotalSpaceRequired(0).ReturnsForAnyArgs(Task.FromResult<List<Location>>([
            new Location
            {
                Id = Guid.NewGuid(),
                Listings =
                [
                    new Listing { Length = 25, Width = 10 },
                    new Listing { Length = 15, Width = 10 }
                ]
            }
        ]));

        // act
        var locationMatchResult = await _vehicleInquiryMatcher.Match(inquiries);

        // assert
        Assert.AreEqual(1, locationMatchResult.Count());
        var match = locationMatchResult.First();
        Assert.AreEqual(2, match.ListingIds.Count); // Each car needed a separate listing
    }
}
